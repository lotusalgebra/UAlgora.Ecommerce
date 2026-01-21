using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Providers;

/// <summary>
/// Provider interface for payment gateway integrations.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets supported payment methods.
    /// </summary>
    IReadOnlyList<string> SupportedMethods { get; }

    /// <summary>
    /// Checks if the provider is available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a payment intent.
    /// </summary>
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Confirms a payment.
    /// </summary>
    Task<PaymentConfirmationResponse> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default);

    /// <summary>
    /// Captures a payment.
    /// </summary>
    Task<PaymentCaptureResponse> CapturePaymentAsync(string paymentIntentId, decimal? amount = null, CancellationToken ct = default);

    /// <summary>
    /// Cancels a payment intent.
    /// </summary>
    Task<PaymentCancelResponse> CancelPaymentAsync(string paymentIntentId, CancellationToken ct = default);

    /// <summary>
    /// Refunds a payment.
    /// </summary>
    Task<RefundResponse> RefundAsync(RefundRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets payment status.
    /// </summary>
    Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentIntentId, CancellationToken ct = default);

    /// <summary>
    /// Creates a customer in the payment provider.
    /// </summary>
    Task<ProviderCustomerResponse> CreateCustomerAsync(CreateProviderCustomerRequest request, CancellationToken ct = default);

    /// <summary>
    /// Saves a payment method for a customer.
    /// </summary>
    Task<SavedPaymentMethodResponse> SavePaymentMethodAsync(SavePaymentMethodRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets saved payment methods for a customer.
    /// </summary>
    Task<IReadOnlyList<SavedPaymentMethodResponse>> GetSavedPaymentMethodsAsync(string providerCustomerId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a saved payment method.
    /// </summary>
    Task DeletePaymentMethodAsync(string paymentMethodId, CancellationToken ct = default);

    /// <summary>
    /// Handles a webhook from the payment provider.
    /// </summary>
    Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default);

    /// <summary>
    /// Gets the client secret for frontend integration.
    /// </summary>
    Task<string> GetClientSecretAsync(string paymentIntentId, CancellationToken ct = default);
}

/// <summary>
/// Request to create a payment intent.
/// </summary>
public class CreatePaymentIntentRequest
{
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Description { get; set; }
    public Guid OrderId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? ProviderCustomerId { get; set; }
    public string? PaymentMethodId { get; set; }
    public bool CaptureImmediately { get; set; } = true;
    public Dictionary<string, string> Metadata { get; set; } = [];
}

/// <summary>
/// Payment intent response.
/// </summary>
public class PaymentIntentResponse
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment confirmation response.
/// </summary>
public class PaymentConfirmationResponse
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Status { get; set; }
    public string? TransactionId { get; set; }
    public decimal? AmountCaptured { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment capture response.
/// </summary>
public class PaymentCaptureResponse
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public decimal AmountCaptured { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment cancel response.
/// </summary>
public class PaymentCancelResponse
{
    public bool Success { get; set; }
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Refund request.
/// </summary>
public class RefundRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}

/// <summary>
/// Refund response.
/// </summary>
public class RefundResponse
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal AmountRefunded { get; set; }
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment status response.
/// </summary>
public class PaymentStatusResponse
{
    public string? PaymentIntentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AmountCaptured { get; set; }
    public decimal AmountRefunded { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool IsCaptured { get; set; }
    public bool IsRefunded { get; set; }
    public bool IsCancelled { get; set; }
}

/// <summary>
/// Request to create a provider customer.
/// </summary>
public class CreateProviderCustomerRequest
{
    public Guid CustomerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}

/// <summary>
/// Provider customer response.
/// </summary>
public class ProviderCustomerResponse
{
    public bool Success { get; set; }
    public string? ProviderCustomerId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request to save a payment method.
/// </summary>
public class SavePaymentMethodRequest
{
    public string ProviderCustomerId { get; set; } = string.Empty;
    public string PaymentMethodToken { get; set; } = string.Empty;
    public bool SetAsDefault { get; set; }
}

/// <summary>
/// Saved payment method response.
/// </summary>
public class SavedPaymentMethodResponse
{
    public bool Success { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? Type { get; set; }
    public string? Last4 { get; set; }
    public string? Brand { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Webhook processing result.
/// </summary>
public class WebhookProcessingResult
{
    public bool Success { get; set; }
    public string? EventType { get; set; }
    public string? PaymentIntentId { get; set; }
    public Guid? OrderId { get; set; }
    public string? ErrorMessage { get; set; }
    public WebhookAction Action { get; set; }
}

/// <summary>
/// Action to take after webhook processing.
/// </summary>
public enum WebhookAction
{
    None,
    MarkAsPaid,
    MarkAsFailed,
    ProcessRefund,
    UpdatePaymentMethod,
    ChargeDisputed
}
