using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for payment operations.
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>
    /// Gets payments by order ID.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets a payment by transaction ID.
    /// </summary>
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default);

    /// <summary>
    /// Gets a payment by payment intent ID.
    /// </summary>
    Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default);

    /// <summary>
    /// Gets payments by status.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets payments by provider.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByProviderAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Gets refunds for a payment.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetRefundsAsync(Guid paymentId, CancellationToken ct = default);

    /// <summary>
    /// Gets payments within a date range.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Gets total captured amount for an order.
    /// </summary>
    Task<decimal> GetTotalCapturedAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets total refunded amount for an order.
    /// </summary>
    Task<decimal> GetTotalRefundedAsync(Guid orderId, CancellationToken ct = default);
}

/// <summary>
/// Repository interface for stored payment method operations.
/// </summary>
public interface IStoredPaymentMethodRepository : IRepository<StoredPaymentMethod>
{
    /// <summary>
    /// Gets stored payment methods for a customer.
    /// </summary>
    Task<IReadOnlyList<StoredPaymentMethod>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets the default payment method for a customer.
    /// </summary>
    Task<StoredPaymentMethod?> GetDefaultAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets a payment method by provider method ID.
    /// </summary>
    Task<StoredPaymentMethod?> GetByProviderMethodIdAsync(string providerMethodId, CancellationToken ct = default);

    /// <summary>
    /// Sets a payment method as default.
    /// </summary>
    Task SetDefaultAsync(Guid customerId, Guid paymentMethodId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all payment methods for a customer.
    /// </summary>
    Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
}
