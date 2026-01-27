using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// Service interface for Razorpay payment operations.
/// </summary>
public interface IRazorpayPaymentService
{
    /// <summary>
    /// Creates a Razorpay order for payment.
    /// </summary>
    Task<RazorpayOrderResponse> CreateOrderAsync(
        LicenseType tier,
        string customerEmail,
        string customerName,
        string? companyName,
        string? domain,
        string currency = "INR");

    /// <summary>
    /// Verifies the payment signature.
    /// </summary>
    bool VerifyPaymentSignature(string orderId, string paymentId, string signature);

    /// <summary>
    /// Gets payment details from Razorpay.
    /// </summary>
    Task<Razorpay.Api.Payment?> GetPaymentAsync(string paymentId);

    /// <summary>
    /// Validates a webhook signature.
    /// </summary>
    bool ValidateWebhookSignature(string payload, string signature);

    /// <summary>
    /// Gets the price for a license tier in INR.
    /// </summary>
    decimal GetPriceForTierInr(LicenseType tier);

    /// <summary>
    /// Cancels a subscription in Razorpay.
    /// </summary>
    Task CancelSubscriptionAsync(string subscriptionId);
}
