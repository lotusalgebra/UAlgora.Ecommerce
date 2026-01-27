using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;
using UAlgora.Ecommerce.LicensePortal.Services;

namespace UAlgora.Ecommerce.LicensePortal.Controllers;

public class CheckoutController : Controller
{
    private readonly IStripePaymentService _stripeService;
    private readonly IRazorpayPaymentService _razorpayService;
    private readonly ILicenseGenerationService _licenseService;
    private readonly IEmailService _emailService;
    private readonly ILicenseSubscriptionRepository _subscriptionRepository;
    private readonly ILicensePaymentRepository _paymentRepository;
    private readonly LicensePortalOptions _options;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        IStripePaymentService stripeService,
        IRazorpayPaymentService razorpayService,
        ILicenseGenerationService licenseService,
        IEmailService emailService,
        ILicenseSubscriptionRepository subscriptionRepository,
        ILicensePaymentRepository paymentRepository,
        IOptions<LicensePortalOptions> options,
        ILogger<CheckoutController> logger)
    {
        _stripeService = stripeService;
        _razorpayService = razorpayService;
        _licenseService = licenseService;
        _emailService = emailService;
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _options = options.Value;
        _logger = logger;
    }

    [HttpGet("checkout/{tier}")]
    public IActionResult Index(string tier)
    {
        if (!Enum.TryParse<LicenseType>(tier, true, out var licenseType) ||
            licenseType == LicenseType.Trial)
        {
            return RedirectToAction("Index", "Home");
        }

        var pricing = licenseType == LicenseType.Enterprise
            ? _options.Pricing.Enterprise
            : _options.Pricing.Standard;

        var viewModel = new CheckoutViewModel
        {
            SelectedTier = licenseType,
            TierName = licenseType.ToString(),
            Price = pricing.AnnualPrice,
            Currency = pricing.Currency,
            Features = pricing.Features,
            StripePublishableKey = _options.Stripe.PublishableKey,
            RazorpayKeyId = _options.Razorpay.KeyId
        };

        return View(viewModel);
    }

    [HttpPost("checkout/stripe")]
    public async Task<IActionResult> CreateStripeSession([FromBody] StripeCheckoutRequest request)
    {
        try
        {
            var successUrl = $"{_options.BaseUrl}/checkout/success";
            var cancelUrl = $"{_options.BaseUrl}/checkout/{request.Tier.ToString().ToLower()}";

            var session = await _stripeService.CreateCheckoutSessionAsync(
                request.Tier,
                request.CustomerEmail,
                request.CustomerName,
                request.CompanyName,
                request.Domain,
                successUrl,
                cancelUrl);

            return Json(new { sessionId = session.Id, url = session.Url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Stripe checkout session");
            return BadRequest(new { error = "Failed to create checkout session" });
        }
    }

    [HttpPost("checkout/razorpay/create-order")]
    public async Task<IActionResult> CreateRazorpayOrder([FromBody] RazorpayOrderRequest request)
    {
        try
        {
            var order = await _razorpayService.CreateOrderAsync(
                request.Tier,
                request.CustomerEmail,
                request.CustomerName,
                request.CompanyName,
                request.Domain);

            // Store order details in session for later verification
            HttpContext.Session.SetString($"razorpay_order_{order.OrderId}", System.Text.Json.JsonSerializer.Serialize(request));

            return Json(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Razorpay order");
            return BadRequest(new { error = "Failed to create order" });
        }
    }

    [HttpPost("checkout/razorpay/verify")]
    public async Task<IActionResult> VerifyRazorpayPayment([FromBody] RazorpayVerifyRequest request)
    {
        try
        {
            // Verify signature
            if (!_razorpayService.VerifyPaymentSignature(request.OrderId, request.PaymentId, request.Signature))
            {
                return BadRequest(new { error = "Invalid payment signature" });
            }

            // Get payment details
            var payment = await _razorpayService.GetPaymentAsync(request.PaymentId);
            if (payment == null)
            {
                return BadRequest(new { error = "Payment not found" });
            }

            // Generate license
            var license = await _licenseService.GenerateAndActivateLicenseAsync(
                request.Tier,
                request.CustomerEmail,
                request.CustomerName,
                request.CompanyName,
                request.Domain,
                "Razorpay");

            // Create subscription record
            var subscription = new LicenseSubscription
            {
                Id = Guid.NewGuid(),
                LicenseId = license.Id,
                CustomerEmail = request.CustomerEmail,
                CustomerName = request.CustomerName,
                PaymentProvider = "Razorpay",
                ProviderSubscriptionId = request.PaymentId,
                Status = LicenseSubscriptionStatus.Active,
                Amount = _razorpayService.GetPriceForTierInr(request.Tier),
                Currency = "INR",
                BillingInterval = "year",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = license.ValidUntil ?? DateTime.UtcNow.AddYears(1),
                AutoRenew = false, // Razorpay one-time payments don't auto-renew by default
                LicenseType = request.Tier,
                LicensedDomain = request.Domain,
                PaymentCount = 1,
                LastPaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _subscriptionRepository.AddAsync(subscription);

            // Create payment record
            var licensePayment = new LicensePayment
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                LicenseId = license.Id,
                PaymentProvider = "Razorpay",
                ProviderPaymentId = request.PaymentId,
                Status = LicensePaymentStatus.Succeeded,
                Amount = subscription.Amount,
                Currency = "INR",
                CustomerEmail = request.CustomerEmail,
                CustomerName = request.CustomerName,
                PaidAt = DateTime.UtcNow,
                PaymentType = "subscription",
                PeriodStart = subscription.CurrentPeriodStart,
                PeriodEnd = subscription.CurrentPeriodEnd,
                LicenseType = request.Tier,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(licensePayment);

            // Send confirmation email
            await _emailService.SendLicensePurchasedEmailAsync(license);

            return Json(new
            {
                success = true,
                licenseKey = license.Key,
                redirectUrl = $"/checkout/success?key={license.Key}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Razorpay payment");
            return BadRequest(new { error = "Payment verification failed" });
        }
    }

    [HttpGet("checkout/success")]
    public async Task<IActionResult> Success(string? session_id, string? key)
    {
        License? license = null;

        // Handle Stripe success
        if (!string.IsNullOrEmpty(session_id))
        {
            // License will be created by webhook, just show success page
            // We'll retrieve it later when webhook processes
            return View(new SuccessViewModel
            {
                LicenseKey = "Processing...",
                CustomerEmail = "Check your email for license details",
                ValidUntil = DateTime.UtcNow.AddYears(1),
                NextSteps =
                [
                    "Check your email for your license key",
                    "Copy the license key from your email",
                    "Go to your Umbraco backoffice",
                    "Navigate to Settings → Algora Commerce → License",
                    "Enter your license key and click Activate"
                ]
            });
        }

        // Handle direct key (Razorpay)
        if (!string.IsNullOrEmpty(key))
        {
            return View(new SuccessViewModel
            {
                LicenseKey = key,
                CustomerEmail = "",
                ValidUntil = DateTime.UtcNow.AddYears(1),
                NextSteps =
                [
                    "Copy your license key below",
                    "Go to your Umbraco backoffice",
                    "Navigate to Settings → Algora Commerce → License",
                    "Enter your license key and click Activate"
                ]
            });
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("checkout/cancel")]
    public IActionResult Cancel()
    {
        return View();
    }
}
