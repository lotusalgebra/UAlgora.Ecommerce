using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Audit Log operations.
/// </summary>
public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByStoreAsync(Guid? storeId, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.StoreId == storeId)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByActionAsync(AuditAction action, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByCategoryAsync(AuditCategory category, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.Category == category)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetFailedAsync(int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => !a.IsSuccess)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> SearchAsync(
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
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(a => a.StoreId == storeId.Value);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        if (action.HasValue)
        {
            query = query.Where(a => a.Action == action.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        if (isSuccess.HasValue)
        {
            query = query.Where(a => a.IsSuccess == isSuccess.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountAsync(
        Guid? storeId = null,
        string? entityType = null,
        AuditAction? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(a => a.StoreId == storeId.Value);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (action.HasValue)
        {
            query = query.Where(a => a.Action == action.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        return await query.CountAsync(ct);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        var logsToDelete = await DbSet
            .Where(a => a.Timestamp < cutoffDate)
            .ToListAsync(ct);

        if (logsToDelete.Count > 0)
        {
            DbSet.RemoveRange(logsToDelete);
            await Context.SaveChangesAsync(ct);
        }

        return logsToDelete.Count;
    }
}
