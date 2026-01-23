using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Audit Log operations.
/// Note: Audit logs are never soft-deleted, they are permanent records.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Get audit logs by store.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByStoreAsync(Guid? storeId, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Get audit logs for a specific entity.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);

    /// <summary>
    /// Get audit logs by user.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Get audit logs by action type.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByActionAsync(AuditAction action, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Get audit logs by category.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByCategoryAsync(AuditCategory category, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Get audit logs within a date range.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Get failed audit logs.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetFailedAsync(int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Search audit logs.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> SearchAsync(
        Guid? storeId = null,
        string? entityType = null,
        string? userId = null,
        AuditAction? action = null,
        AuditCategory? category = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool? isSuccess = null,
        int skip = 0,
        int take = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Get audit log count by criteria.
    /// </summary>
    Task<int> GetCountAsync(
        Guid? storeId = null,
        string? entityType = null,
        AuditAction? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Delete old audit logs (for cleanup).
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken ct = default);
}
