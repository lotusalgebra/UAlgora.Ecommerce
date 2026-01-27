using Stripe.Checkout;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// Service interface for Stripe payment operations.
/// </summary>
public interface IStripePaymentService
{
    /// <summary>
    /// Creates a Stripe Checkout session for subscription purchase.
    /// </summary>
    Task<Session> CreateCheckoutSessionAsync(
        LicenseType tier,
        string customerEmail,
        string customerName,
        string? companyName,
        string? domain,
        string successUrl,
        string cancelUrl);

    /// <summary>
    /// Gets subscription details from Stripe.
    /// </summary>
    Task<Stripe.Subscription?> GetSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Cancels a subscription at period end.
    /// </summary>
    Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false);

    /// <summary>
    /// Validates a webhook signature.
    /// </summary>
    Stripe.Event? ValidateWebhook(string payload, string signature);

    /// <summary>
    /// Gets the price for a license tier.
    /// </summary>
    decimal GetPriceForTier(LicenseType tier);
}
