using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Services;

namespace UAlgora.Ecommerce.LicensePortal.Controllers;

[ApiController]
[Route("webhook")]
public class WebhookController : ControllerBase
{
    private readonly IStripePaymentService _stripeService;
    private readonly IRazorpayPaymentService _razorpayService;
    private readonly ILicenseGenerationService _licenseService;
    private readonly IEmailService _emailService;
    private readonly ILicenseSubscriptionRepository _subscriptionRepository;
    private readonly ILicensePaymentRepository _paymentRepository;
    private readonly ILicenseRepository _licenseRepository;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IStripePaymentService stripeService,
        IRazorpayPaymentService razorpayService,
        ILicenseGenerationService licenseService,
        IEmailService emailService,
        ILicenseSubscriptionRepository subscriptionRepository,
        ILicensePaymentRepository paymentRepository,
        ILicenseRepository licenseRepository,
        ILogger<WebhookController> logger)
    {
        _stripeService = stripeService;
        _razorpayService = razorpayService;
        _licenseService = licenseService;
        _emailService = emailService;
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _licenseRepository = licenseRepository;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without signature");
            return BadRequest("Missing signature");
        }

        var stripeEvent = _stripeService.ValidateWebhook(json, signature);
        if (stripeEvent == null)
        {
            _logger.LogWarning("Stripe webhook signature validation failed");
            return BadRequest("Invalid signature");
        }

        _logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);

        try
        {
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutCompleted(stripeEvent);
                    break;

                case "invoice.paid":
                    await HandleInvoicePaid(stripeEvent, json);
                    break;

                case "invoice.payment_failed":
                    await HandlePaymentFailed(stripeEvent, json);
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionCancelled(stripeEvent);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook: {EventType}", stripeEvent.Type);
            return StatusCode(500);
        }
    }

    [HttpPost("razorpay")]
    public async Task<IActionResult> RazorpayWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Razorpay webhook received without signature");
            return BadRequest("Missing signature");
        }

        if (!_razorpayService.ValidateWebhookSignature(json, signature))
        {
            _logger.LogWarning("Razorpay webhook signature validation failed");
            return BadRequest("Invalid signature");
        }

        try
        {
            var webhook = JsonSerializer.Deserialize<RazorpayWebhookPayload>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (webhook == null)
            {
                return BadRequest("Invalid payload");
            }

            _logger.LogInformation("Razorpay webhook received: {Event}", webhook.Event);

            switch (webhook.Event)
            {
                case "payment.authorized":
                case "payment.captured":
                    // Already handled in verify endpoint
                    break;

                case "subscription.charged":
                    await HandleRazorpaySubscriptionCharged(webhook);
                    break;

                default:
                    _logger.LogInformation("Unhandled Razorpay event: {Event}", webhook.Event);
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Razorpay webhook");
            return StatusCode(500);
        }
    }

    private async Task HandleCheckoutCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null) return;

        var metadata = session.Metadata;
        if (!Enum.TryParse<LicenseType>(metadata.GetValueOrDefault("tier"), out var tier))
        {
            _logger.LogWarning("Invalid tier in checkout session metadata");
            return;
        }

        var customerEmail = session.CustomerEmail ?? session.CustomerDetails?.Email;
        var customerName = metadata.GetValueOrDefault("customer_name") ?? "Customer";
        var companyName = metadata.GetValueOrDefault("company_name");
        var domain = metadata.GetValueOrDefault("domain");

        if (string.IsNullOrEmpty(customerEmail))
        {
            _logger.LogWarning("No customer email in checkout session");
            return;
        }

        // Generate license
        var license = await _licenseService.GenerateAndActivateLicenseAsync(
            tier,
            customerEmail,
            customerName,
            companyName,
            domain,
            "Stripe",
            session.SubscriptionId);

        // Create subscription record
        var subscription = new LicenseSubscription
        {
            Id = Guid.NewGuid(),
            LicenseId = license.Id,
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            PaymentProvider = "Stripe",
            ProviderSubscriptionId = session.SubscriptionId ?? session.Id,
            ProviderCustomerId = session.CustomerId,
            Status = LicenseSubscriptionStatus.Active,
            Amount = _stripeService.GetPriceForTier(tier),
            Currency = "USD",
            BillingInterval = "year",
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = license.ValidUntil ?? DateTime.UtcNow.AddYears(1),
            AutoRenew = true,
            LicenseType = tier,
            LicensedDomain = domain,
            PaymentCount = 1,
            LastPaymentDate = DateTime.UtcNow,
            NextPaymentDate = license.ValidUntil,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _subscriptionRepository.AddAsync(subscription);

        // Create payment record
        var payment = new LicensePayment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            LicenseId = license.Id,
            PaymentProvider = "Stripe",
            ProviderPaymentId = session.PaymentIntentId ?? session.Id,
            ProviderCustomerId = session.CustomerId,
            Status = LicensePaymentStatus.Succeeded,
            Amount = subscription.Amount,
            Currency = "USD",
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            PaidAt = DateTime.UtcNow,
            PaymentType = "subscription",
            PeriodStart = subscription.CurrentPeriodStart,
            PeriodEnd = subscription.CurrentPeriodEnd,
            LicenseType = tier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);

        // Send confirmation email
        await _emailService.SendLicensePurchasedEmailAsync(license);

        _logger.LogInformation(
            "Created license {LicenseKey} for checkout session {SessionId}",
            license.Key, session.Id);
    }

    private async Task HandleInvoicePaid(Event stripeEvent, string json)
    {
        // Parse invoice data from JSON to get subscription ID (avoids SDK version issues)
        var invoiceData = ExtractInvoiceData(json);
        if (invoiceData == null || string.IsNullOrEmpty(invoiceData.SubscriptionId)) return;

        // Find the subscription
        var subscription = await _subscriptionRepository.GetByProviderSubscriptionIdAsync(invoiceData.SubscriptionId);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for invoice: {SubscriptionId}", invoiceData.SubscriptionId);
            return;
        }

        // Check if this is a renewal (not the first payment)
        if (subscription.PaymentCount > 0 && invoiceData.BillingReason == "subscription_cycle")
        {
            // Extend the license
            var license = await _licenseService.ExtendLicenseAsync(subscription.LicenseId);
            if (license == null) return;

            // Update subscription period
            await _subscriptionRepository.UpdatePeriodAsync(
                subscription.Id,
                DateTime.UtcNow,
                license.ValidUntil ?? DateTime.UtcNow.AddYears(1));

            // Increment payment count
            await _subscriptionRepository.IncrementPaymentCountAsync(subscription.Id);

            // Create payment record
            var payment = new LicensePayment
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                LicenseId = subscription.LicenseId,
                PaymentProvider = "Stripe",
                ProviderPaymentId = invoiceData.PaymentIntentId ?? invoiceData.InvoiceId,
                ProviderInvoiceId = invoiceData.InvoiceId,
                ProviderCustomerId = invoiceData.CustomerId,
                Status = LicensePaymentStatus.Succeeded,
                Amount = invoiceData.AmountPaid / 100m,
                Currency = invoiceData.Currency?.ToUpper() ?? "USD",
                CustomerEmail = subscription.CustomerEmail,
                CustomerName = subscription.CustomerName,
                PaidAt = DateTime.UtcNow,
                ReceiptUrl = invoiceData.HostedInvoiceUrl,
                InvoiceUrl = invoiceData.InvoicePdf,
                PaymentType = "subscription",
                PeriodStart = DateTime.UtcNow,
                PeriodEnd = license.ValidUntil ?? DateTime.UtcNow.AddYears(1),
                LicenseType = subscription.LicenseType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            // Send renewal email
            await _emailService.SendLicenseRenewedEmailAsync(license);

            _logger.LogInformation(
                "Renewed license {LicenseId} for subscription {SubscriptionId}",
                license.Id, subscription.ProviderSubscriptionId);
        }
    }

    private async Task HandlePaymentFailed(Event stripeEvent, string json)
    {
        var invoiceData = ExtractInvoiceData(json);
        if (invoiceData == null || string.IsNullOrEmpty(invoiceData.SubscriptionId)) return;

        var subscription = await _subscriptionRepository.GetByProviderSubscriptionIdAsync(invoiceData.SubscriptionId);
        if (subscription == null) return;

        var reason = invoiceData.ErrorMessage ?? "Payment failed";

        // Record failure
        await _subscriptionRepository.RecordPaymentFailureAsync(subscription.Id, reason);

        // Get license for email
        var license = await _licenseRepository.GetByIdAsync(subscription.LicenseId);
        if (license != null)
        {
            await _emailService.SendPaymentFailedEmailAsync(license, reason);
        }

        _logger.LogWarning(
            "Payment failed for subscription {SubscriptionId}: {Reason}",
            subscription.ProviderSubscriptionId, reason);
    }

    private async Task HandleSubscriptionCancelled(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (stripeSubscription == null) return;

        var subscription = await _subscriptionRepository.GetByProviderSubscriptionIdAsync(stripeSubscription.Id);
        if (subscription == null) return;

        // Cancel the subscription
        await _subscriptionRepository.CancelAsync(subscription.Id, cancelAtPeriodEnd: false);

        // Get license
        var license = await _licenseRepository.GetByIdAsync(subscription.LicenseId);
        if (license != null)
        {
            license.AutoRenew = false;
            await _licenseRepository.UpdateAsync(license);

            await _emailService.SendSubscriptionCancelledEmailAsync(license);
        }

        _logger.LogInformation(
            "Subscription cancelled: {SubscriptionId}",
            subscription.ProviderSubscriptionId);
    }

    private async Task HandleRazorpaySubscriptionCharged(RazorpayWebhookPayload webhook)
    {
        // Handle Razorpay subscription renewal
        // This would be implemented similarly to Stripe's invoice.paid handler
        _logger.LogInformation("Razorpay subscription charged event received");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Extracts invoice data from raw JSON to avoid Stripe SDK version compatibility issues.
    /// </summary>
    private InvoiceData? ExtractInvoiceData(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var dataObj = root.GetProperty("data").GetProperty("object");

            return new InvoiceData
            {
                InvoiceId = dataObj.TryGetProperty("id", out var id) ? id.GetString() : null,
                SubscriptionId = dataObj.TryGetProperty("subscription", out var sub) ? sub.GetString() : null,
                PaymentIntentId = dataObj.TryGetProperty("payment_intent", out var pi) ? pi.GetString() : null,
                CustomerId = dataObj.TryGetProperty("customer", out var cust) ? cust.GetString() : null,
                AmountPaid = dataObj.TryGetProperty("amount_paid", out var amt) ? amt.GetInt64() : 0,
                Currency = dataObj.TryGetProperty("currency", out var curr) ? curr.GetString() : null,
                BillingReason = dataObj.TryGetProperty("billing_reason", out var br) ? br.GetString() : null,
                HostedInvoiceUrl = dataObj.TryGetProperty("hosted_invoice_url", out var hiu) ? hiu.GetString() : null,
                InvoicePdf = dataObj.TryGetProperty("invoice_pdf", out var pdf) ? pdf.GetString() : null,
                ErrorMessage = dataObj.TryGetProperty("last_finalization_error", out var err) && err.ValueKind != JsonValueKind.Null
                    ? err.TryGetProperty("message", out var msg) ? msg.GetString() : null
                    : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract invoice data from JSON");
            return null;
        }
    }

    private class InvoiceData
    {
        public string? InvoiceId { get; set; }
        public string? SubscriptionId { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? CustomerId { get; set; }
        public long AmountPaid { get; set; }
        public string? Currency { get; set; }
        public string? BillingReason { get; set; }
        public string? HostedInvoiceUrl { get; set; }
        public string? InvoicePdf { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

public class RazorpayWebhookPayload
{
    public string Event { get; set; } = string.Empty;
    public RazorpayWebhookEntity? Payload { get; set; }
}

public class RazorpayWebhookEntity
{
    public RazorpayPaymentEntity? Payment { get; set; }
    public RazorpaySubscriptionEntity? Subscription { get; set; }
}

public class RazorpayPaymentEntity
{
    public RazorpayEntityData? Entity { get; set; }
}

public class RazorpaySubscriptionEntity
{
    public RazorpayEntityData? Entity { get; set; }
}

public class RazorpayEntityData
{
    public string? Id { get; set; }
    public string? OrderId { get; set; }
    public long? Amount { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public string? Email { get; set; }
}
