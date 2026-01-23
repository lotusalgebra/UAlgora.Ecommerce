using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Store operations.
/// </summary>
public class StoreRepository : SoftDeleteRepository<Store>, IStoreRepository
{
    public StoreRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Store?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.Code == code, ct);
    }

    public async Task<Store?> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s =>
                s.Domain == domain ||
                s.AlternateDomains.Contains(domain), ct);
    }

    public async Task<Store?> GetByUmbracoNodeIdAsync(int nodeId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.UmbracoNodeId == nodeId, ct);
    }

    public async Task<IReadOnlyList<Store>> GetActiveAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Status == StoreStatus.Active)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Store>> GetByStatusAsync(StoreStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Status == status)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Store>> GetExpiringTrialsAsync(int daysUntilExpiry, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        return await DbSet
            .Where(s => s.LicenseType == LicenseType.Trial)
            .Where(s => s.TrialExpiresAt.HasValue && s.TrialExpiresAt.Value <= cutoffDate)
            .OrderBy(s => s.TrialExpiresAt)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(s => s.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<bool> DomainExistsAsync(string domain, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(s =>
            s.Domain == domain ||
            s.AlternateDomains.Contains(domain));
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<string> GetNextOrderNumberAsync(Guid storeId, CancellationToken ct = default)
    {
        var store = await GetByIdAsync(storeId, ct);
        if (store == null)
        {
            throw new InvalidOperationException($"Store with ID {storeId} not found.");
        }

        var sequence = ++store.OrderNumberSequence;
        await Context.SaveChangesAsync(ct);

        return $"{store.OrderNumberPrefix}{sequence:D6}";
    }
}
