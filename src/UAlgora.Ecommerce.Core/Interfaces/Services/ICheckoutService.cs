using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for checkout operations.
/// </summary>
public interface ICheckoutService
{
    /// <summary>
    /// Initializes a checkout session.
    /// </summary>
    Task<CheckoutSession> InitializeAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a checkout session by ID.
    /// </summary>
    Task<CheckoutSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Updates the shipping address for checkout.
    /// </summary>
    Task<CheckoutSession> UpdateShippingAddressAsync(Guid sessionId, Address address, CancellationToken ct = default);

    /// <summary>
    /// Updates the billing address for checkout.
    /// </summary>
    Task<CheckoutSession> UpdateBillingAddressAsync(Guid sessionId, Address address, CancellationToken ct = default);

    /// <summary>
    /// Updates the shipping method for checkout.
    /// </summary>
    Task<CheckoutSession> UpdateShippingMethodAsync(Guid sessionId, string shippingMethodId, CancellationToken ct = default);

    /// <summary>
    /// Gets available shipping options for checkout.
    /// </summary>
    Task<IReadOnlyList<ShippingOption>> GetShippingOptionsAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets available payment methods for checkout.
    /// </summary>
    Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Creates a payment intent for the checkout.
    /// </summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Completes the checkout and creates an order.
    /// </summary>
    Task<Order> CompleteAsync(Guid sessionId, CompleteCheckoutRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validates the checkout session.
    /// </summary>
    Task<CheckoutValidationResult> ValidateAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Cancels/abandons a checkout session.
    /// </summary>
    Task CancelAsync(Guid sessionId, CancellationToken ct = default);
}

/// <summary>
/// Represents a checkout session.
/// </summary>
public class CheckoutSession
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid? CustomerId { get; set; }

    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    public Address? ShippingAddress { get; set; }
    public Address? BillingAddress { get; set; }
    public bool BillingSameAsShipping { get; set; } = true;

    public string? SelectedShippingMethod { get; set; }
    public string? SelectedPaymentMethod { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public string? PaymentIntentId { get; set; }
    public string? PaymentClientSecret { get; set; }

    public CheckoutStep CurrentStep { get; set; } = CheckoutStep.Information;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Checkout steps.
/// </summary>
public enum CheckoutStep
{
    Information = 0,
    Shipping = 1,
    Payment = 2,
    Review = 3,
    Complete = 4
}

/// <summary>
/// Available payment method.
/// </summary>
public class PaymentMethod
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Payment intent result.
/// </summary>
public class PaymentIntentResult
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request to complete checkout.
/// </summary>
public class CompleteCheckoutRequest
{
    public string? PaymentIntentId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? CustomerNote { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool CreateAccount { get; set; }
    public string? Password { get; set; }
}

/// <summary>
/// Checkout validation result.
/// </summary>
public class CheckoutValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = [];
    public CheckoutStep? FailedAtStep { get; set; }

    public static CheckoutValidationResult Success() => new();

    public static CheckoutValidationResult Failure(string error, CheckoutStep? step = null) => new()
    {
        Errors = [error],
        FailedAtStep = step
    };
}
