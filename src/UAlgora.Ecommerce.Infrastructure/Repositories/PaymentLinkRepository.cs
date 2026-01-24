using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Payment Link operations.
/// </summary>
public class PaymentLinkRepository : SoftDeleteRepository<PaymentLink>, IPaymentLinkRepository
{
    public PaymentLinkRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<PaymentLink?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.Code == code, ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.StoreId == storeId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetByStatusAsync(PaymentLinkStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(p => p.Status == PaymentLinkStatus.Active)
            .Where(p => p.IsActive)
            .Where(p => !p.ExpiresAt.HasValue || p.ExpiresAt.Value > now)
            .Where(p => !p.ValidFrom.HasValue || p.ValidFrom.Value <= now)
            .Where(p => !p.MaxUses.HasValue || p.UsageCount < p.MaxUses.Value)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        return await DbSet
            .Where(p => p.Status == PaymentLinkStatus.Active)
            .Where(p => p.ExpiresAt.HasValue && p.ExpiresAt.Value <= cutoffDate && p.ExpiresAt.Value > DateTime.UtcNow)
            .OrderBy(p => p.ExpiresAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetExpiredActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(p => p.Status == PaymentLinkStatus.Active)
            .Where(p => p.ExpiresAt.HasValue && p.ExpiresAt.Value <= now)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetByTypeAsync(PaymentLinkType type, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Type == type)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(p => p.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentLinkPayment>> GetPaymentsAsync(Guid paymentLinkId, CancellationToken ct = default)
    {
        return await Context.PaymentLinkPayments
            .Where(p => p.PaymentLinkId == paymentLinkId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PaymentLinkPayment> AddPaymentAsync(PaymentLinkPayment payment, CancellationToken ct = default)
    {
        payment.Id = Guid.NewGuid();
        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        await Context.PaymentLinkPayments.AddAsync(payment, ct);
        await Context.SaveChangesAsync(ct);
        return payment;
    }

    public async Task<PaymentLinkPayment> UpdatePaymentAsync(PaymentLinkPayment payment, CancellationToken ct = default)
    {
        payment.UpdatedAt = DateTime.UtcNow;
        Context.PaymentLinkPayments.Update(payment);
        await Context.SaveChangesAsync(ct);
        return payment;
    }

    public async Task<PaymentLinkStatistics> GetStatisticsAsync(Guid paymentLinkId, CancellationToken ct = default)
    {
        var payments = await Context.PaymentLinkPayments
            .Where(p => p.PaymentLinkId == paymentLinkId)
            .ToListAsync(ct);

        var successful = payments.Where(p => p.Status == PaymentLinkPaymentStatus.Completed).ToList();

        return new PaymentLinkStatistics
        {
            TotalPayments = payments.Count,
            SuccessfulPayments = successful.Count,
            FailedPayments = payments.Count(p => p.Status == PaymentLinkPaymentStatus.Failed),
            PendingPayments = payments.Count(p => p.Status == PaymentLinkPaymentStatus.Pending || p.Status == PaymentLinkPaymentStatus.Processing),
            TotalCollected = successful.Sum(p => p.Amount),
            TotalTips = successful.Sum(p => p.TipAmount),
            AverageAmount = successful.Any() ? successful.Average(p => p.Amount) : 0,
            ConversionRate = payments.Any() ? (decimal)successful.Count / payments.Count * 100 : 0,
            LastPaymentAt = payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt
        };
    }

    public async Task IncrementUsageAsync(Guid paymentLinkId, decimal amount, CancellationToken ct = default)
    {
        var paymentLink = await GetByIdAsync(paymentLinkId, ct);
        if (paymentLink != null)
        {
            paymentLink.UsageCount++;
            paymentLink.TotalCollected += amount;
            paymentLink.UpdatedAt = DateTime.UtcNow;

            // Check if max uses reached
            if (paymentLink.MaxUses.HasValue && paymentLink.UsageCount >= paymentLink.MaxUses.Value)
            {
                paymentLink.Status = PaymentLinkStatus.Completed;
            }

            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<PagedResult<PaymentLink>> SearchAsync(
        string? searchTerm = null,
        PaymentLinkStatus? status = null,
        PaymentLinkType? type = null,
        Guid? storeId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Code.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                (p.ReferenceNumber != null && p.ReferenceNumber.ToLower().Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        if (storeId.HasValue)
        {
            query = query.Where(p => p.StoreId == storeId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PaymentLink>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
