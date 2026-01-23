using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for License operations.
/// </summary>
public class LicenseRepository : Repository<License>, ILicenseRepository
{
    public LicenseRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<License?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(l => l.Key == key, ct);
    }

    public async Task<IReadOnlyList<License>> GetByStatusAsync(LicenseStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(l => l.Status == status)
            .OrderBy(l => l.CustomerName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<License>> GetByTypeAsync(LicenseType type, CancellationToken ct = default)
    {
        return await DbSet
            .Where(l => l.Type == type)
            .OrderBy(l => l.CustomerName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<License>> GetByCustomerEmailAsync(string email, CancellationToken ct = default)
    {
        return await DbSet
            .Where(l => l.CustomerEmail == email)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<License>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(l => l.Status == LicenseStatus.Active)
            .Where(l => l.IsLifetime || !l.ValidUntil.HasValue || l.ValidUntil.Value >= now)
            .OrderBy(l => l.CustomerName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<License>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var cutoffDate = now.AddDays(daysUntilExpiry);
        return await DbSet
            .Where(l => l.Status == LicenseStatus.Active)
            .Where(l => !l.IsLifetime)
            .Where(l => l.ValidUntil.HasValue && l.ValidUntil.Value <= cutoffDate && l.ValidUntil.Value >= now)
            .OrderBy(l => l.ValidUntil)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<License>> GetExpiredActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(l => l.Status == LicenseStatus.Active)
            .Where(l => !l.IsLifetime)
            .Where(l => l.ValidUntil.HasValue && l.ValidUntil.Value < now)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<License>> GetRequiringValidationAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(l => l.Status == LicenseStatus.Active)
            .Where(l =>
                !l.LastValidatedAt.HasValue ||
                l.LastValidatedAt.Value.AddHours(l.ValidationIntervalHours) < now)
            .ToListAsync(ct);
    }

    public async Task UpdateValidationStatusAsync(Guid licenseId, LicenseValidationResult result, string? error = null, CancellationToken ct = default)
    {
        var license = await GetByIdAsync(licenseId, ct);
        if (license != null)
        {
            license.LastValidatedAt = DateTime.UtcNow;
            license.LastValidationResult = result;
            license.LastValidationError = error;

            if (result == LicenseValidationResult.Valid)
            {
                license.ConsecutiveValidationFailures = 0;
            }
            else
            {
                license.ConsecutiveValidationFailures++;
            }

            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> IncrementActivationCountAsync(Guid licenseId, CancellationToken ct = default)
    {
        var license = await GetByIdAsync(licenseId, ct);
        if (license == null)
        {
            return false;
        }

        if (license.ActivationCount >= license.MaxActivations)
        {
            return false;
        }

        license.ActivationCount++;
        if (!license.FirstActivatedAt.HasValue)
        {
            license.FirstActivatedAt = DateTime.UtcNow;
        }
        license.LastActivatedAt = DateTime.UtcNow;

        await Context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> KeyExistsAsync(string key, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(l => l.Key == key);
        if (excludeId.HasValue)
        {
            query = query.Where(l => l.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<License?> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        return await DbSet
            .Where(l => l.Status == LicenseStatus.Active)
            .Where(l =>
                l.LicensedDomains != null &&
                l.LicensedDomains.Contains(domain))
            .FirstOrDefaultAsync(ct);
    }
}
