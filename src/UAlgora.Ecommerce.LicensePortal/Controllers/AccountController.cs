using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;
using UAlgora.Ecommerce.LicensePortal.Services;

namespace UAlgora.Ecommerce.LicensePortal.Controllers;

/// <summary>
/// Controller for customer account management.
/// Handles license viewing, subscription management, and payment history.
/// </summary>
public class AccountController : Controller
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly ILicenseSubscriptionRepository _subscriptionRepository;
    private readonly ILicensePaymentRepository _paymentRepository;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IRazorpayPaymentService _razorpayPaymentService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    private const string EmailSessionKey = "CustomerEmail";
    private const string LoginCodeSessionKey = "LoginCode";
    private const string LoginCodeExpirySessionKey = "LoginCodeExpiry";

    public AccountController(
        ILicenseRepository licenseRepository,
        ILicenseSubscriptionRepository subscriptionRepository,
        ILicensePaymentRepository paymentRepository,
        IStripePaymentService stripePaymentService,
        IRazorpayPaymentService razorpayPaymentService,
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _licenseRepository = licenseRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _stripePaymentService = stripePaymentService;
        _razorpayPaymentService = razorpayPaymentService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Login page - email-based authentication.
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (IsLoggedIn())
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    /// <summary>
    /// Send login code to email.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendLoginCode([FromForm] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            TempData["Error"] = "Please enter your email address.";
            return RedirectToAction(nameof(Login));
        }

        // Check if email has any licenses
        var licenses = await _licenseRepository.GetByCustomerEmailAsync(request.Email);
        if (licenses.Count == 0)
        {
            TempData["Error"] = "No licenses found for this email address. Please use the email you used during purchase.";
            return RedirectToAction(nameof(Login));
        }

        // Generate a 6-digit code
        var code = GenerateLoginCode();
        var expiry = DateTime.UtcNow.AddMinutes(10);

        // Store in session
        HttpContext.Session.SetString(LoginCodeSessionKey, code);
        HttpContext.Session.SetString(LoginCodeExpirySessionKey, expiry.ToString("O"));
        HttpContext.Session.SetString(EmailSessionKey + "_Pending", request.Email);

        // Send email with code
        try
        {
            await _emailService.SendLoginCodeAsync(request.Email, code);
            _logger.LogInformation("Login code sent to {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send login code to {Email}", request.Email);
            TempData["Error"] = "Failed to send login code. Please try again.";
            return RedirectToAction(nameof(Login));
        }

        TempData["Success"] = $"A login code has been sent to {request.Email}. Please check your inbox.";
        TempData["PendingEmail"] = request.Email;
        return RedirectToAction(nameof(VerifyCode));
    }

    /// <summary>
    /// Verify login code page.
    /// </summary>
    [HttpGet]
    public IActionResult VerifyCode()
    {
        var pendingEmail = TempData["PendingEmail"]?.ToString() ?? HttpContext.Session.GetString(EmailSessionKey + "_Pending");
        if (string.IsNullOrEmpty(pendingEmail))
        {
            return RedirectToAction(nameof(Login));
        }

        ViewBag.Email = pendingEmail;
        TempData.Keep("PendingEmail");
        return View();
    }

    /// <summary>
    /// Verify the login code.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyCode([FromForm] LoginVerifyRequest request)
    {
        var storedCode = HttpContext.Session.GetString(LoginCodeSessionKey);
        var storedExpiry = HttpContext.Session.GetString(LoginCodeExpirySessionKey);
        var pendingEmail = HttpContext.Session.GetString(EmailSessionKey + "_Pending");

        if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(storedExpiry) || string.IsNullOrEmpty(pendingEmail))
        {
            TempData["Error"] = "Login session expired. Please start over.";
            return RedirectToAction(nameof(Login));
        }

        if (DateTime.TryParse(storedExpiry, out var expiry) && expiry < DateTime.UtcNow)
        {
            TempData["Error"] = "Login code has expired. Please request a new one.";
            return RedirectToAction(nameof(Login));
        }

        if (!string.Equals(request.Code.Trim(), storedCode, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Invalid login code. Please try again.";
            TempData["PendingEmail"] = pendingEmail;
            return RedirectToAction(nameof(VerifyCode));
        }

        // Login successful
        HttpContext.Session.SetString(EmailSessionKey, pendingEmail);
        HttpContext.Session.Remove(LoginCodeSessionKey);
        HttpContext.Session.Remove(LoginCodeExpirySessionKey);
        HttpContext.Session.Remove(EmailSessionKey + "_Pending");

        _logger.LogInformation("Customer {Email} logged in successfully", pendingEmail);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Logout.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        var email = HttpContext.Session.GetString(EmailSessionKey);
        HttpContext.Session.Clear();
        _logger.LogInformation("Customer {Email} logged out", email ?? "unknown");
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Account dashboard - shows overview of licenses, subscriptions, and recent payments.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!TryGetCustomerEmail(out var email))
        {
            return RedirectToAction(nameof(Login));
        }

        var licenses = await _licenseRepository.GetByCustomerEmailAsync(email);
        var subscriptions = await _subscriptionRepository.GetByCustomerEmailAsync(email);
        var payments = await _paymentRepository.GetByCustomerEmailAsync(email);

        var customerName = licenses.FirstOrDefault()?.CustomerName ?? email;

        var viewModel = new AccountDashboardViewModel
        {
            CustomerEmail = email,
            CustomerName = customerName,
            Licenses = licenses.Select(MapLicense).ToList(),
            Subscriptions = subscriptions.Select(s => MapSubscription(s, licenses)).ToList(),
            RecentPayments = payments.OrderByDescending(p => p.CreatedAt).Take(5).Select(MapPayment).ToList(),
            Stats = new AccountStats
            {
                TotalLicenses = licenses.Count,
                ActiveLicenses = licenses.Count(l => l.Status == LicenseStatus.Active),
                ActiveSubscriptions = subscriptions.Count(s => s.Status == LicenseSubscriptionStatus.Active),
                TotalSpent = payments.Where(p => p.Status == LicensePaymentStatus.Succeeded).Sum(p => p.Amount),
                Currency = payments.FirstOrDefault()?.Currency ?? "USD"
            }
        };

        return View(viewModel);
    }

    /// <summary>
    /// View all licenses.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Licenses()
    {
        if (!TryGetCustomerEmail(out var email))
        {
            return RedirectToAction(nameof(Login));
        }

        var licenses = await _licenseRepository.GetByCustomerEmailAsync(email);

        var viewModel = new LicensesPageViewModel
        {
            CustomerEmail = email,
            Licenses = licenses.Select(MapLicense).ToList(),
            TotalCount = licenses.Count
        };

        return View(viewModel);
    }

    /// <summary>
    /// View license details.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LicenseDetails(Guid id)
    {
        if (!TryGetCustomerEmail(out var email))
        {
            return RedirectToAction(nameof(Login));
        }

        var license = await _licenseRepository.GetByIdAsync(id);
        if (license == null || !string.Equals(license.CustomerEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        return View(MapLicense(license));
    }

    /// <summary>
    /// View all subscriptions.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Subscriptions()
    {
        if (!TryGetCustomerEmail(out var email))
        {
            return RedirectToAction(nameof(Login));
        }

        var licenses = await _licenseRepository.GetByCustomerEmailAsync(email);
        var subscriptions = await _subscriptionRepository.GetByCustomerEmailAsync(email);

        var viewModel = new SubscriptionsPageViewModel
        {
            CustomerEmail = email,
            Subscriptions = subscriptions.Select(s => MapSubscription(s, licenses)).ToList(),
            TotalCount = subscriptions.Count
        };

        return View(viewModel);
    }

    /// <summary>
    /// Cancel a subscription.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelSubscription([FromForm] CancelSubscriptionRequest request)
    {
        if (!TryGetCustomerEmail(out var email))
        {
            return RedirectToAction(nameof(Login));
        }

        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId);
        if (subscription == null || !string.Equals(subscription.CustomerEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Subscription not found.";
            return RedirectToAction(nameof(Subscriptions));
        }

        if (subscription.Status != LicenseSubscriptionStatus.Active)
        {
            TempData["Error"] = "This subscription cannot be cancelled.";
            return RedirectToAction(nameof(Subscriptions));
        }

        try
        {
            // Cancel with the payment provider
            if (subscription.PaymentProvider == "Stripe")
            {
                // Stripe's cancelImmediately is the inverse of CancelAtPeriodEnd
                await _stripePaymentService.CancelSubscriptionAsync(subscription.ProviderSubscriptionId, cancelImmediately: !request.CancelAtPeriodEnd);
            }
            else if (subscription.PaymentProvider == "Razorpay")
            {
                await _razorpayPaymentService.CancelSubscriptionAsync(subscription.ProviderSubscriptionId);
            }

            // Update local subscription
            await _subscriptionRepository.CancelAsync(subscription.Id, request.CancelAtPeriodEnd);

            _logger.LogInformation("Subscription {SubscriptionId} cancelled by {Email}", subscription.Id, email);

            if (request.CancelAtPeriodEnd)
            {
                TempData["Success"] = "Your subscription has been cancelled. You will retain access until the end of the current billing period.";
            }
            else
            {
                TempData["Success"] = "Your subscription has been cancelled immediately.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel subscription {SubscriptionId}", subscription.Id);
            TempData["Error"] = "Failed to cancel subscription. Please contact support.";
        }

        return RedirectToAction(nameof(Subscriptions));
    }

    /// <summary>
    /// View all invoices/payments.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Invoices()
    {
        if (!TryGetCustomerEmail(out var email))
        {
            return RedirectToAction(nameof(Login));
        }

        var payments = await _paymentRepository.GetByCustomerEmailAsync(email);
        var successfulPayments = payments.Where(p => p.Status == LicensePaymentStatus.Succeeded).ToList();

        var viewModel = new InvoicesPageViewModel
        {
            CustomerEmail = email,
            Payments = payments.OrderByDescending(p => p.CreatedAt).Select(MapPayment).ToList(),
            TotalCount = payments.Count,
            TotalSpent = successfulPayments.Sum(p => p.Amount),
            Currency = payments.FirstOrDefault()?.Currency ?? "USD"
        };

        return View(viewModel);
    }

    /// <summary>
    /// Download a receipt/invoice.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DownloadReceipt(Guid id)
    {
        if (!TryGetCustomerEmail(out var email))
        {
            return RedirectToAction(nameof(Login));
        }

        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null || !string.Equals(payment.CustomerEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(payment.ReceiptUrl))
        {
            return Redirect(payment.ReceiptUrl);
        }

        if (!string.IsNullOrEmpty(payment.InvoiceUrl))
        {
            return Redirect(payment.InvoiceUrl);
        }

        TempData["Error"] = "Receipt not available for this payment.";
        return RedirectToAction(nameof(Invoices));
    }

    #region Helper Methods

    private bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString(EmailSessionKey));
    }

    private bool TryGetCustomerEmail(out string email)
    {
        email = HttpContext.Session.GetString(EmailSessionKey) ?? string.Empty;
        return !string.IsNullOrEmpty(email);
    }

    private static string GenerateLoginCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private static LicenseViewModel MapLicense(License license)
    {
        var statusClass = license.Status switch
        {
            LicenseStatus.Active => "success",
            LicenseStatus.Expired => "danger",
            LicenseStatus.Suspended => "warning",
            LicenseStatus.GracePeriod => "warning",
            LicenseStatus.Revoked => "danger",
            LicenseStatus.PendingActivation => "info",
            _ => "secondary"
        };

        var enabledFeatures = new List<string>();
        if (!string.IsNullOrEmpty(license.EnabledFeaturesJson))
        {
            try
            {
                enabledFeatures = JsonSerializer.Deserialize<List<string>>(license.EnabledFeaturesJson) ?? [];
            }
            catch { }
        }

        return new LicenseViewModel
        {
            Id = license.Id,
            Key = license.Key,
            MaskedKey = MaskLicenseKey(license.Key),
            Type = license.Type,
            TypeName = license.Type.ToString(),
            Status = license.Status,
            StatusName = license.Status.ToString(),
            StatusClass = statusClass,
            ValidFrom = license.ValidFrom,
            ValidUntil = license.ValidUntil,
            LicensedDomains = license.LicensedDomains,
            Company = license.Company,
            IsExpired = license.IsExpired,
            IsInGracePeriod = license.IsInGracePeriod,
            DaysUntilExpiration = license.DaysUntilExpiration,
            AutoRenew = license.AutoRenew,
            CreatedAt = license.CreatedAt,
            EnabledFeatures = enabledFeatures
        };
    }

    private static SubscriptionViewModel MapSubscription(LicenseSubscription subscription, IReadOnlyList<License> licenses)
    {
        var license = licenses.FirstOrDefault(l => l.Id == subscription.LicenseId);

        var statusClass = subscription.Status switch
        {
            LicenseSubscriptionStatus.Active => "success",
            LicenseSubscriptionStatus.PastDue => "warning",
            LicenseSubscriptionStatus.Cancelled => "secondary",
            LicenseSubscriptionStatus.Expired => "danger",
            LicenseSubscriptionStatus.Trialing => "info",
            LicenseSubscriptionStatus.Paused => "warning",
            _ => "secondary"
        };

        return new SubscriptionViewModel
        {
            Id = subscription.Id,
            LicenseId = subscription.LicenseId,
            LicenseKey = license != null ? MaskLicenseKey(license.Key) : "N/A",
            LicenseType = subscription.LicenseType,
            LicenseTypeName = subscription.LicenseType.ToString(),
            PaymentProvider = subscription.PaymentProvider,
            ProviderSubscriptionId = subscription.ProviderSubscriptionId,
            Status = subscription.Status,
            StatusName = subscription.Status.ToString(),
            StatusClass = statusClass,
            Amount = subscription.Amount,
            Currency = subscription.Currency,
            FormattedAmount = FormatCurrency(subscription.Amount, subscription.Currency),
            BillingInterval = subscription.BillingInterval,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            NextPaymentDate = subscription.NextPaymentDate,
            AutoRenew = subscription.AutoRenew,
            CanCancel = subscription.Status == LicenseSubscriptionStatus.Active && !subscription.CancelledAt.HasValue,
            IsCancelled = subscription.CancelledAt.HasValue,
            CancelledAt = subscription.CancelledAt,
            DaysUntilRenewal = subscription.DaysUntilRenewal,
            PaymentCount = subscription.PaymentCount
        };
    }

    private static PaymentViewModel MapPayment(LicensePayment payment)
    {
        var statusClass = payment.Status switch
        {
            LicensePaymentStatus.Succeeded => "success",
            LicensePaymentStatus.Failed => "danger",
            LicensePaymentStatus.Refunded => "secondary",
            LicensePaymentStatus.Pending => "warning",
            LicensePaymentStatus.Processing => "info",
            _ => "secondary"
        };

        var paymentMethod = "Card";
        if (!string.IsNullOrEmpty(payment.CardBrand) && !string.IsNullOrEmpty(payment.CardLast4))
        {
            paymentMethod = $"{payment.CardBrand} ****{payment.CardLast4}";
        }

        return new PaymentViewModel
        {
            Id = payment.Id,
            PaymentProvider = payment.PaymentProvider,
            ProviderPaymentId = payment.ProviderPaymentId,
            Status = payment.Status,
            StatusName = payment.Status.ToString(),
            StatusClass = statusClass,
            Amount = payment.Amount,
            Currency = payment.Currency,
            FormattedAmount = FormatCurrency(payment.Amount, payment.Currency),
            LicenseType = payment.LicenseType,
            LicenseTypeName = payment.LicenseType.ToString(),
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt,
            ReceiptUrl = payment.ReceiptUrl,
            InvoiceUrl = payment.InvoiceUrl,
            CardBrand = payment.CardBrand,
            CardLast4 = payment.CardLast4,
            PaymentMethod = paymentMethod,
            PeriodStart = payment.PeriodStart,
            PeriodEnd = payment.PeriodEnd
        };
    }

    private static string MaskLicenseKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8)
            return key;

        return key[..4] + new string('*', key.Length - 8) + key[^4..];
    }

    private static string FormatCurrency(decimal amount, string currency)
    {
        return currency.ToUpperInvariant() switch
        {
            "USD" => $"${amount:N2}",
            "EUR" => $"\u20ac{amount:N2}",
            "GBP" => $"\u00a3{amount:N2}",
            "INR" => $"\u20b9{amount:N2}",
            _ => $"{currency} {amount:N2}"
        };
    }

    #endregion
}
