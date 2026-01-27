using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// Stripe payment service implementation.
/// </summary>
public class StripePaymentService : IStripePaymentService
{
    private readonly LicensePortalOptions _options;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IOptions<LicensePortalOptions> options,
        ILogger<StripePaymentService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Configure Stripe API key
        StripeConfiguration.ApiKey = _options.Stripe.SecretKey;
    }

    public async Task<Session> CreateCheckoutSessionAsync(
        LicenseType tier,
        string customerEmail,
        string customerName,
        string? companyName,
        string? domain,
        string successUrl,
        string cancelUrl)
    {
        var pricing = tier == LicenseType.Enterprise
            ? _options.Pricing.Enterprise
            : _options.Pricing.Standard;

        var sessionOptions = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            Mode = "subscription",
            CustomerEmail = customerEmail,
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = pricing.Currency.ToLower(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Algora Commerce {tier} License",
                            Description = $"Annual subscription for Algora Commerce {tier} tier"
                        },
                        UnitAmount = (long)(pricing.AnnualPrice * 100), // Stripe uses cents
                        Recurring = new SessionLineItemPriceDataRecurringOptions
                        {
                            Interval = "year"
                        }
                    },
                    Quantity = 1
                }
            ],
            Metadata = new Dictionary<string, string>
            {
                ["tier"] = tier.ToString(),
                ["customer_name"] = customerName,
                ["company_name"] = companyName ?? "",
                ["domain"] = domain ?? ""
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["tier"] = tier.ToString(),
                    ["customer_name"] = customerName,
                    ["company_name"] = companyName ?? "",
                    ["domain"] = domain ?? ""
                }
            },
            AllowPromotionCodes = true,
            BillingAddressCollection = "required"
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions);

        _logger.LogInformation(
            "Created Stripe checkout session {SessionId} for {Email}, tier {Tier}",
            session.Id, customerEmail, tier);

        return session;
    }

    public async Task<Subscription?> GetSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var subscriptionService = new SubscriptionService();
            return await subscriptionService.GetAsync(subscriptionId);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get subscription {SubscriptionId}", subscriptionId);
            return null;
        }
    }

    public async Task<Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false)
    {
        var subscriptionService = new SubscriptionService();

        if (cancelImmediately)
        {
            return await subscriptionService.CancelAsync(subscriptionId);
        }

        // Cancel at period end
        var updateOptions = new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        };

        return await subscriptionService.UpdateAsync(subscriptionId, updateOptions);
    }

    public Event? ValidateWebhook(string payload, string signature)
    {
        try
        {
            return EventUtility.ConstructEvent(
                payload,
                signature,
                _options.Stripe.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to validate Stripe webhook signature");
            return null;
        }
    }

    public decimal GetPriceForTier(LicenseType tier)
    {
        return tier switch
        {
            LicenseType.Standard => _options.Pricing.Standard.AnnualPrice,
            LicenseType.Enterprise => _options.Pricing.Enterprise.AnnualPrice,
            _ => 0
        };
    }
}
