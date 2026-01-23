using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for Audit Log operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry.
    /// </summary>
    Task LogAsync(AuditLog entry, CancellationToken ct = default);

    /// <summary>
    /// Logs an action on an entity.
    /// </summary>
    Task LogAsync(
        AuditAction action,
        AuditCategory category,
        string entityType,
        Guid? entityId = null,
        string? description = null,
        object? oldValues = null,
        object? newValues = null,
        Guid? storeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets audit logs by entity.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);

    /// <summary>
    /// Gets audit logs by store.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByStoreAsync(Guid? storeId, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Gets audit logs by user.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Gets audit logs by date range.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 100, CancellationToken ct = default);

    /// <summary>
    /// Searches audit logs.
    /// </summary>
    Task<AuditSearchResult> SearchAsync(AuditSearchCriteria criteria, CancellationToken ct = default);

    /// <summary>
    /// Deletes old audit logs.
    /// </summary>
    Task<int> PurgeAsync(DateTime olderThan, CancellationToken ct = default);

    /// <summary>
    /// Exports audit logs to a file.
    /// </summary>
    Task<byte[]> ExportAsync(AuditSearchCriteria criteria, ExportFormat format = ExportFormat.Csv, CancellationToken ct = default);
}

/// <summary>
/// Audit log search criteria.
/// </summary>
public class AuditSearchCriteria
{
    public Guid? StoreId { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public AuditCategory? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsSuccess { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 100;
}

/// <summary>
/// Audit search result.
/// </summary>
public class AuditSearchResult
{
    public IReadOnlyList<AuditLog> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    Csv,
    Json,
    Excel
}
