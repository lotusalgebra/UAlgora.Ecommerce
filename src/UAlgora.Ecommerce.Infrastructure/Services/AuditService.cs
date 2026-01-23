using System.Text;
using System.Text.Json;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for Audit Log operations.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(AuditLog entry, CancellationToken ct = default)
    {
        entry.Timestamp = DateTime.UtcNow;
        await _auditLogRepository.AddAsync(entry, ct);
    }

    public async Task LogAsync(
        AuditAction action,
        AuditCategory category,
        string entityType,
        Guid? entityId = null,
        string? description = null,
        object? oldValues = null,
        object? newValues = null,
        Guid? storeId = null,
        CancellationToken ct = default)
    {
        var entry = new AuditLog
        {
            Action = action,
            Category = category,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            StoreId = storeId,
            Timestamp = DateTime.UtcNow,
            IsSuccess = true,
            IsSystemAction = true // Default to system action when no user context
        };

        if (oldValues != null)
        {
            entry.OldValuesJson = JsonSerializer.Serialize(oldValues);
        }

        if (newValues != null)
        {
            entry.NewValuesJson = JsonSerializer.Serialize(newValues);
        }

        // Calculate changed properties
        if (oldValues != null && newValues != null)
        {
            var changedProperties = GetChangedProperties(oldValues, newValues);
            if (changedProperties.Count > 0)
            {
                entry.ChangedPropertiesJson = JsonSerializer.Serialize(changedProperties);
            }
        }

        await _auditLogRepository.AddAsync(entry, ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default)
    {
        return await _auditLogRepository.GetByEntityAsync(entityType, entityId, ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByStoreAsync(Guid? storeId, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await _auditLogRepository.GetByStoreAsync(storeId, skip, take, ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await _auditLogRepository.GetByUserAsync(userId, skip, take, ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await _auditLogRepository.GetByDateRangeAsync(startDate, endDate, skip, take, ct);
    }

    public async Task<AuditSearchResult> SearchAsync(AuditSearchCriteria criteria, CancellationToken ct = default)
    {
        var items = await _auditLogRepository.SearchAsync(
            storeId: criteria.StoreId,
            entityType: criteria.EntityType,
            userId: criteria.UserId,
            action: criteria.Action,
            category: criteria.Category,
            startDate: criteria.StartDate,
            endDate: criteria.EndDate,
            isSuccess: criteria.IsSuccess,
            skip: criteria.Skip,
            take: criteria.Take,
            ct: ct);

        var totalCount = await _auditLogRepository.GetCountAsync(
            storeId: criteria.StoreId,
            entityType: criteria.EntityType,
            action: criteria.Action,
            startDate: criteria.StartDate,
            endDate: criteria.EndDate,
            ct: ct);

        return new AuditSearchResult
        {
            Items = items,
            TotalCount = totalCount,
            Skip = criteria.Skip,
            Take = criteria.Take
        };
    }

    public async Task<int> PurgeAsync(DateTime olderThan, CancellationToken ct = default)
    {
        return await _auditLogRepository.DeleteOlderThanAsync(olderThan, ct);
    }

    public async Task<byte[]> ExportAsync(AuditSearchCriteria criteria, ExportFormat format = ExportFormat.Csv, CancellationToken ct = default)
    {
        // Get all matching records for export
        var items = await _auditLogRepository.SearchAsync(
            storeId: criteria.StoreId,
            entityType: criteria.EntityType,
            userId: criteria.UserId,
            action: criteria.Action,
            category: criteria.Category,
            startDate: criteria.StartDate,
            endDate: criteria.EndDate,
            isSuccess: criteria.IsSuccess,
            skip: 0,
            take: int.MaxValue,
            ct: ct);

        var result = new AuditSearchResult { Items = items };

        return format switch
        {
            ExportFormat.Csv => ExportToCsv(result.Items),
            ExportFormat.Json => ExportToJson(result.Items),
            ExportFormat.Excel => ExportToExcel(result.Items),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    private static List<string> GetChangedProperties(object oldValues, object newValues)
    {
        var changes = new List<string>();

        var oldDict = ConvertToDictionary(oldValues);
        var newDict = ConvertToDictionary(newValues);

        foreach (var key in oldDict.Keys.Union(newDict.Keys))
        {
            var oldVal = oldDict.TryGetValue(key, out var ov) ? ov : null;
            var newVal = newDict.TryGetValue(key, out var nv) ? nv : null;

            if (!Equals(oldVal, newVal))
            {
                changes.Add(key);
            }
        }

        return changes;
    }

    private static Dictionary<string, object?> ConvertToDictionary(object obj)
    {
        if (obj is Dictionary<string, object?> dict)
            return dict;

        var result = new Dictionary<string, object?>();
        foreach (var prop in obj.GetType().GetProperties())
        {
            result[prop.Name] = prop.GetValue(obj);
        }
        return result;
    }

    private static byte[] ExportToCsv(IReadOnlyList<AuditLog> logs)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Timestamp,Action,Category,EntityType,EntityId,UserId,UserName,Description,IsSuccess,IpAddress");

        // Data rows
        foreach (var log in logs)
        {
            sb.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.Action}\",\"{log.Category}\",\"{EscapeCsv(log.EntityType)}\",\"{log.EntityId}\",\"{EscapeCsv(log.UserId)}\",\"{EscapeCsv(log.UserName)}\",\"{EscapeCsv(log.Description)}\",\"{log.IsSuccess}\",\"{EscapeCsv(log.IpAddress)}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static byte[] ExportToJson(IReadOnlyList<AuditLog> logs)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(logs, options);
        return Encoding.UTF8.GetBytes(json);
    }

    private static byte[] ExportToExcel(IReadOnlyList<AuditLog> logs)
    {
        // For a real implementation, you'd use a library like EPPlus or ClosedXML
        // For now, we'll return CSV with .xlsx-friendly format
        return ExportToCsv(logs);
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Escape quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}
