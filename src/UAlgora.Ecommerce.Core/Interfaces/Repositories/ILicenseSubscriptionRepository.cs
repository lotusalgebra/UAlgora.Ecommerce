using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for LicenseSubscription operations.
/// </summary>
public interface ILicenseSubscriptionRepository : IRepository<LicenseSubscription>
{
    /// <summary>
    /// Get subscription by provider subscription ID.
    /// </summary>
    Task<LicenseSubscription?> GetByProviderSubscriptionIdAsync(string providerSubscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Get subscription by license ID.
    /// </summary>
    Task<LicenseSubscription?> GetByLicenseIdAsync(Guid licenseId, CancellationToken ct = default);

    /// <summary>
    /// Get subscriptions by customer email.
    /// </summary>
    Task<IReadOnlyList<LicenseSubscription>> GetByCustomerEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Get subscriptions by status.
    /// </summary>
    Task<IReadOnlyList<LicenseSubscription>> GetByStatusAsync(LicenseSubscriptionStatus status, CancellationToken ct = default);

    /// <summary>
    /// Get active subscriptions.
    /// </summary>
    Task<IReadOnlyList<LicenseSubscription>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get subscriptions due for renewal within the specified days.
    /// </summary>
    Task<IReadOnlyList<LicenseSubscription>> GetDueForRenewalAsync(int withinDays, CancellationToken ct = default);

    /// <summary>
    /// Get subscriptions that are past due.
    /// </summary>
    Task<IReadOnlyList<LicenseSubscription>> GetPastDueAsync(CancellationToken ct = default);

    /// <summary>
    /// Get subscriptions expiring within the specified days.
    /// </summary>
    Task<IReadOnlyList<LicenseSubscription>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default);

    /// <summary>
    /// Update subscription status.
    /// </summary>
    Task UpdateStatusAsync(Guid subscriptionId, LicenseSubscriptionStatus status, CancellationToken ct = default);

    /// <summary>
    /// Update subscription period after renewal.
    /// </summary>
    Task UpdatePeriodAsync(Guid subscriptionId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);

    /// <summary>
    /// Cancel subscription.
    /// </summary>
    Task CancelAsync(Guid subscriptionId, bool cancelAtPeriodEnd = true, CancellationToken ct = default);

    /// <summary>
    /// Get subscription with payments included.
    /// </summary>
    Task<LicenseSubscription?> GetWithPaymentsAsync(Guid subscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Get subscriptions by payment provider.
    /// </summary>
    Task<IReadOnlyList<LicenseSubscription>> GetByPaymentProviderAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Increment payment count.
    /// </summary>
    Task IncrementPaymentCountAsync(Guid subscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Record payment failure.
    /// </summary>
    Task RecordPaymentFailureAsync(Guid subscriptionId, string reason, CancellationToken ct = default);

    /// <summary>
    /// Search subscriptions with pagination.
    /// </summary>
    Task<PagedResult<LicenseSubscription>> SearchAsync(
        string? searchTerm = null,
        LicenseSubscriptionStatus? status = null,
        string? paymentProvider = null,
        LicenseType? licenseType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);
}
