using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for LicenseSubscription operations.
/// </summary>
public class LicenseSubscriptionRepository : Repository<LicenseSubscription>, ILicenseSubscriptionRepository
{
    public LicenseSubscriptionRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<LicenseSubscription?> GetByProviderSubscriptionIdAsync(string providerSubscriptionId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.License)
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == providerSubscriptionId, ct);
    }

    public async Task<LicenseSubscription?> GetByLicenseIdAsync(Guid licenseId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.License)
            .FirstOrDefaultAsync(s => s.LicenseId == licenseId, ct);
    }

    public async Task<IReadOnlyList<LicenseSubscription>> GetByCustomerEmailAsync(string email, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.License)
            .Where(s => s.CustomerEmail == email)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicenseSubscription>> GetByStatusAsync(LicenseSubscriptionStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.License)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicenseSubscription>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(s => s.License)
            .Where(s => s.Status == LicenseSubscriptionStatus.Active)
            .Where(s => s.CurrentPeriodEnd >= now)
            .OrderBy(s => s.CurrentPeriodEnd)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicenseSubscription>> GetDueForRenewalAsync(int withinDays, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var cutoffDate = now.AddDays(withinDays);
        return await DbSet
            .Include(s => s.License)
            .Where(s => s.Status == LicenseSubscriptionStatus.Active)
            .Where(s => s.AutoRenew)
            .Where(s => s.NextPaymentDate.HasValue && s.NextPaymentDate.Value <= cutoffDate)
            .OrderBy(s => s.NextPaymentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicenseSubscription>> GetPastDueAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.License)
            .Where(s => s.Status == LicenseSubscriptionStatus.PastDue)
            .OrderBy(s => s.CurrentPeriodEnd)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicenseSubscription>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var cutoffDate = now.AddDays(daysUntilExpiry);
        return await DbSet
            .Include(s => s.License)
            .Where(s => s.Status == LicenseSubscriptionStatus.Active)
            .Where(s => s.CurrentPeriodEnd <= cutoffDate && s.CurrentPeriodEnd >= now)
            .OrderBy(s => s.CurrentPeriodEnd)
            .ToListAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid subscriptionId, LicenseSubscriptionStatus status, CancellationToken ct = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, ct);
        if (subscription != null)
        {
            subscription.Status = status;
            subscription.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdatePeriodAsync(Guid subscriptionId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, ct);
        if (subscription != null)
        {
            subscription.CurrentPeriodStart = periodStart;
            subscription.CurrentPeriodEnd = periodEnd;
            subscription.NextPaymentDate = periodEnd;
            subscription.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task CancelAsync(Guid subscriptionId, bool cancelAtPeriodEnd = true, CancellationToken ct = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, ct);
        if (subscription != null)
        {
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.AutoRenew = false;

            if (cancelAtPeriodEnd)
            {
                subscription.CancelAtPeriodEnd = subscription.CurrentPeriodEnd;
            }
            else
            {
                subscription.Status = LicenseSubscriptionStatus.Cancelled;
            }

            subscription.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<LicenseSubscription?> GetWithPaymentsAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.License)
            .Include(s => s.Payments.OrderByDescending(p => p.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);
    }

    public async Task<IReadOnlyList<LicenseSubscription>> GetByPaymentProviderAsync(string provider, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.License)
            .Where(s => s.PaymentProvider == provider)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task IncrementPaymentCountAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, ct);
        if (subscription != null)
        {
            subscription.PaymentCount++;
            subscription.LastPaymentDate = DateTime.UtcNow;
            subscription.FailureCount = 0;
            subscription.LastFailureReason = null;
            subscription.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task RecordPaymentFailureAsync(Guid subscriptionId, string reason, CancellationToken ct = default)
    {
        var subscription = await GetByIdAsync(subscriptionId, ct);
        if (subscription != null)
        {
            subscription.FailureCount++;
            subscription.LastFailureReason = reason;
            subscription.UpdatedAt = DateTime.UtcNow;

            // If too many failures, mark as past due
            if (subscription.FailureCount >= 3)
            {
                subscription.Status = LicenseSubscriptionStatus.PastDue;
            }

            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<PagedResult<LicenseSubscription>> SearchAsync(
        string? searchTerm = null,
        LicenseSubscriptionStatus? status = null,
        string? paymentProvider = null,
        LicenseType? licenseType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = DbSet.Include(s => s.License).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.CustomerEmail.ToLower().Contains(term) ||
                s.CustomerName.ToLower().Contains(term) ||
                (s.LicensedDomain != null && s.LicensedDomain.ToLower().Contains(term)) ||
                s.ProviderSubscriptionId.ToLower().Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(paymentProvider))
        {
            query = query.Where(s => s.PaymentProvider == paymentProvider);
        }

        if (licenseType.HasValue)
        {
            query = query.Where(s => s.LicenseType == licenseType.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<LicenseSubscription>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
