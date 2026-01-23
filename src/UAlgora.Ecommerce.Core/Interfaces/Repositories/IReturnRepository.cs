using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Return operations.
/// </summary>
public interface IReturnRepository : ISoftDeleteRepository<Return>
{
    /// <summary>
    /// Get a return by its return number.
    /// </summary>
    Task<Return?> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// Get returns by store.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Get returns by customer.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Get returns by order.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByOrderAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Get returns by status.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken ct = default);

    /// <summary>
    /// Get pending returns (awaiting approval).
    /// </summary>
    Task<IReadOnlyList<Return>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>
    /// Get returns with items to be restocked.
    /// </summary>
    Task<IReadOnlyList<Return>> GetForRestockingAsync(CancellationToken ct = default);

    /// <summary>
    /// Get return items for a return.
    /// </summary>
    Task<IReadOnlyList<ReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default);

    /// <summary>
    /// Update return status.
    /// </summary>
    Task<bool> UpdateStatusAsync(Guid returnId, ReturnStatus newStatus, string? processedBy = null, string? notes = null, CancellationToken ct = default);

    /// <summary>
    /// Get returns within a date range.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Get total refund amount by store for a date range.
    /// </summary>
    Task<decimal> GetTotalRefundAmountAsync(Guid? storeId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
}
