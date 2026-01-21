using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for discount operations.
/// </summary>
public class DiscountRepository : SoftDeleteRepository<Discount>, IDiscountRepository
{
    public DiscountRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Discount?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(d => d.Code == code, ct);
    }

    public async Task<IReadOnlyList<Discount>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(d => d.IsActive)
            .Where(d => !d.StartDate.HasValue || d.StartDate.Value <= now)
            .Where(d => !d.EndDate.HasValue || d.EndDate.Value >= now)
            .Where(d => !d.TotalUsageLimit.HasValue || d.UsageCount < d.TotalUsageLimit.Value)
            .OrderBy(d => d.Priority)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetActiveAutomaticAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(d => d.IsActive)
            .Where(d => d.Code == null) // Non-coupon discounts
            .Where(d => !d.StartDate.HasValue || d.StartDate.Value <= now)
            .Where(d => !d.EndDate.HasValue || d.EndDate.Value >= now)
            .Where(d => !d.TotalUsageLimit.HasValue || d.UsageCount < d.TotalUsageLimit.Value)
            .OrderBy(d => d.Priority)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetByTypeAsync(DiscountType type, CancellationToken ct = default)
    {
        return await DbSet
            .Where(d => d.Type == type)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetApplicableToProductAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(d => d.IsActive)
            .Where(d => !d.StartDate.HasValue || d.StartDate.Value <= now)
            .Where(d => !d.EndDate.HasValue || d.EndDate.Value >= now)
            .Where(d => !d.TotalUsageLimit.HasValue || d.UsageCount < d.TotalUsageLimit.Value)
            .Where(d =>
                d.Scope == DiscountScope.Order ||
                d.ApplicableProductIds.Contains(productId) ||
                !d.ApplicableProductIds.Any()) // If no specific products, applies to all
            .Where(d => !d.ExcludedProductIds.Contains(productId))
            .OrderBy(d => d.Priority)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetApplicableToCategoryAsync(
        Guid categoryId,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(d => d.IsActive)
            .Where(d => !d.StartDate.HasValue || d.StartDate.Value <= now)
            .Where(d => !d.EndDate.HasValue || d.EndDate.Value >= now)
            .Where(d => !d.TotalUsageLimit.HasValue || d.UsageCount < d.TotalUsageLimit.Value)
            .Where(d =>
                d.Scope == DiscountScope.Order ||
                d.ApplicableCategoryIds.Contains(categoryId) ||
                !d.ApplicableCategoryIds.Any())
            .Where(d => !d.ExcludedCategoryIds.Contains(categoryId))
            .OrderBy(d => d.Priority)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetValidForDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(d =>
                (!d.StartDate.HasValue || d.StartDate.Value <= endDate) &&
                (!d.EndDate.HasValue || d.EndDate.Value >= startDate))
            .OrderBy(d => d.StartDate)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(d => d.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task IncrementUsageCountAsync(Guid discountId, CancellationToken ct = default)
    {
        var discount = await GetByIdAsync(discountId, ct);
        if (discount != null)
        {
            discount.UsageCount++;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<int> GetCustomerUsageCountAsync(
        Guid discountId,
        Guid customerId,
        CancellationToken ct = default)
    {
        return await Context.DiscountUsages
            .Where(u => u.DiscountId == discountId && u.CustomerId == customerId)
            .CountAsync(ct);
    }

    public async Task RecordUsageAsync(DiscountUsage usage, CancellationToken ct = default)
    {
        await Context.DiscountUsages.AddAsync(usage, ct);
        await Context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(d => d.IsActive)
            .Where(d => d.EndDate.HasValue && d.EndDate.Value < now)
            .ToListAsync(ct);
    }

    public async Task DeactivateExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var expiredDiscounts = await DbSet
            .Where(d => d.IsActive)
            .Where(d => d.EndDate.HasValue && d.EndDate.Value < now)
            .ToListAsync(ct);

        foreach (var discount in expiredDiscounts)
        {
            discount.IsActive = false;
        }

        await Context.SaveChangesAsync(ct);
    }
}
