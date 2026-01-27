using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for LicensePayment operations.
/// </summary>
public interface ILicensePaymentRepository : IRepository<LicensePayment>
{
    /// <summary>
    /// Get payment by provider payment ID.
    /// </summary>
    Task<LicensePayment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken ct = default);

    /// <summary>
    /// Get payments by subscription ID.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Get payments by license ID.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetByLicenseIdAsync(Guid licenseId, CancellationToken ct = default);

    /// <summary>
    /// Get payments by customer email.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetByCustomerEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Get payments by status.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetByStatusAsync(LicensePaymentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Get successful payments within a date range.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetSuccessfulInRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>
    /// Get total revenue within a date range.
    /// </summary>
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to, string? currency = null, CancellationToken ct = default);

    /// <summary>
    /// Get total revenue by license type.
    /// </summary>
    Task<Dictionary<LicenseType, decimal>> GetRevenueByLicenseTypeAsync(DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>
    /// Get payment count by status.
    /// </summary>
    Task<Dictionary<LicensePaymentStatus, int>> GetCountByStatusAsync(DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>
    /// Update payment status.
    /// </summary>
    Task UpdateStatusAsync(Guid paymentId, LicensePaymentStatus status, string? failureReason = null, CancellationToken ct = default);

    /// <summary>
    /// Mark payment as paid.
    /// </summary>
    Task MarkAsPaidAsync(Guid paymentId, DateTime paidAt, string? receiptUrl = null, CancellationToken ct = default);

    /// <summary>
    /// Mark payment as refunded.
    /// </summary>
    Task MarkAsRefundedAsync(Guid paymentId, decimal refundAmount, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Get payments by payment provider.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetByPaymentProviderAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Get failed payments for retry.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetFailedForRetryAsync(int maxAgeDays = 7, CancellationToken ct = default);

    /// <summary>
    /// Search payments with pagination.
    /// </summary>
    Task<PagedResult<LicensePayment>> SearchAsync(
        string? searchTerm = null,
        LicensePaymentStatus? status = null,
        string? paymentProvider = null,
        LicenseType? licenseType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Get recent payments.
    /// </summary>
    Task<IReadOnlyList<LicensePayment>> GetRecentAsync(int count = 10, CancellationToken ct = default);
}
