using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Payment Link operations.
/// </summary>
public interface IPaymentLinkRepository : ISoftDeleteRepository<PaymentLink>
{
    /// <summary>
    /// Get a payment link by its code.
    /// </summary>
    Task<PaymentLink?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Get payment links by store.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Get payment links by status.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetByStatusAsync(PaymentLinkStatus status, CancellationToken ct = default);

    /// <summary>
    /// Get active payment links.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get payment links expiring soon.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default);

    /// <summary>
    /// Get expired payment links that are still active.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetExpiredActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get payment links by type.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetByTypeAsync(PaymentLinkType type, CancellationToken ct = default);

    /// <summary>
    /// Check if a payment link code already exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Get payments for a payment link.
    /// </summary>
    Task<IReadOnlyList<PaymentLinkPayment>> GetPaymentsAsync(Guid paymentLinkId, CancellationToken ct = default);

    /// <summary>
    /// Add a payment record for a payment link.
    /// </summary>
    Task<PaymentLinkPayment> AddPaymentAsync(PaymentLinkPayment payment, CancellationToken ct = default);

    /// <summary>
    /// Update a payment record.
    /// </summary>
    Task<PaymentLinkPayment> UpdatePaymentAsync(PaymentLinkPayment payment, CancellationToken ct = default);

    /// <summary>
    /// Get payment link statistics.
    /// </summary>
    Task<PaymentLinkStatistics> GetStatisticsAsync(Guid paymentLinkId, CancellationToken ct = default);

    /// <summary>
    /// Increment usage count and update total collected.
    /// </summary>
    Task IncrementUsageAsync(Guid paymentLinkId, decimal amount, CancellationToken ct = default);

    /// <summary>
    /// Search payment links.
    /// </summary>
    Task<PagedResult<PaymentLink>> SearchAsync(
        string? searchTerm = null,
        PaymentLinkStatus? status = null,
        PaymentLinkType? type = null,
        Guid? storeId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);
}

/// <summary>
/// Statistics for a payment link.
/// </summary>
public class PaymentLinkStatistics
{
    /// <summary>
    /// Total number of payments.
    /// </summary>
    public int TotalPayments { get; set; }

    /// <summary>
    /// Number of successful payments.
    /// </summary>
    public int SuccessfulPayments { get; set; }

    /// <summary>
    /// Number of failed payments.
    /// </summary>
    public int FailedPayments { get; set; }

    /// <summary>
    /// Number of pending payments.
    /// </summary>
    public int PendingPayments { get; set; }

    /// <summary>
    /// Total amount collected.
    /// </summary>
    public decimal TotalCollected { get; set; }

    /// <summary>
    /// Total tip amount collected.
    /// </summary>
    public decimal TotalTips { get; set; }

    /// <summary>
    /// Average payment amount.
    /// </summary>
    public decimal AverageAmount { get; set; }

    /// <summary>
    /// Conversion rate (successful / total * 100).
    /// </summary>
    public decimal ConversionRate { get; set; }

    /// <summary>
    /// Last payment date.
    /// </summary>
    public DateTime? LastPaymentAt { get; set; }
}
