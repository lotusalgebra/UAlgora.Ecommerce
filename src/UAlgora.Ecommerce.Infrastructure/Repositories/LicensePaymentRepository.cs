using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for LicensePayment operations.
/// </summary>
public class LicensePaymentRepository : Repository<LicensePayment>, ILicensePaymentRepository
{
    public LicensePaymentRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<LicensePayment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Subscription)
            .Include(p => p.License)
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == providerPaymentId, ct);
    }

    public async Task<IReadOnlyList<LicensePayment>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicensePayment>> GetByLicenseIdAsync(Guid licenseId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.LicenseId == licenseId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicensePayment>> GetByCustomerEmailAsync(string email, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.CustomerEmail == email)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicensePayment>> GetByStatusAsync(LicensePaymentStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicensePayment>> GetSuccessfulInRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Status == LicensePaymentStatus.Succeeded)
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= from && p.PaidAt.Value <= to)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to, string? currency = null, CancellationToken ct = default)
    {
        var query = DbSet
            .Where(p => p.Status == LicensePaymentStatus.Succeeded)
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= from && p.PaidAt.Value <= to);

        if (!string.IsNullOrWhiteSpace(currency))
        {
            query = query.Where(p => p.Currency == currency);
        }

        return await query.SumAsync(p => p.Amount - (p.RefundedAmount ?? 0), ct);
    }

    public async Task<Dictionary<LicenseType, decimal>> GetRevenueByLicenseTypeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Status == LicensePaymentStatus.Succeeded)
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= from && p.PaidAt.Value <= to)
            .GroupBy(p => p.LicenseType)
            .Select(g => new { LicenseType = g.Key, Revenue = g.Sum(p => p.Amount - (p.RefundedAmount ?? 0)) })
            .ToDictionaryAsync(x => x.LicenseType, x => x.Revenue, ct);
    }

    public async Task<Dictionary<LicensePaymentStatus, int>> GetCountByStatusAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
    }

    public async Task UpdateStatusAsync(Guid paymentId, LicensePaymentStatus status, string? failureReason = null, CancellationToken ct = default)
    {
        var payment = await GetByIdAsync(paymentId, ct);
        if (payment != null)
        {
            payment.Status = status;
            payment.FailureReason = failureReason;
            payment.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAsPaidAsync(Guid paymentId, DateTime paidAt, string? receiptUrl = null, CancellationToken ct = default)
    {
        var payment = await GetByIdAsync(paymentId, ct);
        if (payment != null)
        {
            payment.Status = LicensePaymentStatus.Succeeded;
            payment.PaidAt = paidAt;
            payment.ReceiptUrl = receiptUrl;
            payment.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAsRefundedAsync(Guid paymentId, decimal refundAmount, string? reason = null, CancellationToken ct = default)
    {
        var payment = await GetByIdAsync(paymentId, ct);
        if (payment != null)
        {
            payment.RefundedAmount = (payment.RefundedAmount ?? 0) + refundAmount;
            payment.RefundedAt = DateTime.UtcNow;
            payment.RefundReason = reason;
            payment.Status = payment.RefundedAmount >= payment.Amount
                ? LicensePaymentStatus.Refunded
                : LicensePaymentStatus.PartiallyRefunded;
            payment.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<LicensePayment>> GetByPaymentProviderAsync(string provider, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.PaymentProvider == provider)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LicensePayment>> GetFailedForRetryAsync(int maxAgeDays = 7, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-maxAgeDays);
        return await DbSet
            .Where(p => p.Status == LicensePaymentStatus.Failed)
            .Where(p => p.CreatedAt >= cutoffDate)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<LicensePayment>> SearchAsync(
        string? searchTerm = null,
        LicensePaymentStatus? status = null,
        string? paymentProvider = null,
        LicenseType? licenseType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Include(p => p.Subscription)
            .Include(p => p.License)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.CustomerEmail.ToLower().Contains(term) ||
                (p.CustomerName != null && p.CustomerName.ToLower().Contains(term)) ||
                p.ProviderPaymentId.ToLower().Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(paymentProvider))
        {
            query = query.Where(p => p.PaymentProvider == paymentProvider);
        }

        if (licenseType.HasValue)
        {
            query = query.Where(p => p.LicenseType == licenseType.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<LicensePayment>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<LicensePayment>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Subscription)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }
}
