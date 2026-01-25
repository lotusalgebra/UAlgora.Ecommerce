using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for checkout step configuration operations.
/// </summary>
public class CheckoutStepRepository : ICheckoutStepRepository
{
    private readonly EcommerceDbContext _context;

    public CheckoutStepRepository(EcommerceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CheckoutStepConfiguration>> GetAllAsync(Guid? storeId = null, CancellationToken ct = default)
    {
        var query = _context.CheckoutSteps.AsQueryable();

        if (storeId.HasValue)
            query = query.Where(s => s.StoreId == storeId.Value || s.StoreId == null);

        return await query.OrderBy(s => s.SortOrder).ToListAsync(ct);
    }

    public async Task<IEnumerable<CheckoutStepConfiguration>> GetEnabledAsync(Guid? storeId = null, CancellationToken ct = default)
    {
        var query = _context.CheckoutSteps.Where(s => s.IsEnabled);

        if (storeId.HasValue)
            query = query.Where(s => s.StoreId == storeId.Value || s.StoreId == null);

        return await query.OrderBy(s => s.SortOrder).ToListAsync(ct);
    }

    public async Task<CheckoutStepConfiguration?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CheckoutSteps.FindAsync(new object[] { id }, ct);
    }

    public async Task<CheckoutStepConfiguration?> GetByCodeAsync(string code, Guid? storeId = null, CancellationToken ct = default)
    {
        var query = _context.CheckoutSteps.Where(s => s.Code == code);

        if (storeId.HasValue)
            query = query.Where(s => s.StoreId == storeId.Value || s.StoreId == null);

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<CheckoutStepConfiguration> CreateAsync(CheckoutStepConfiguration step, CancellationToken ct = default)
    {
        step.CreatedAt = DateTime.UtcNow;
        step.UpdatedAt = DateTime.UtcNow;

        _context.CheckoutSteps.Add(step);
        await _context.SaveChangesAsync(ct);

        return step;
    }

    public async Task<CheckoutStepConfiguration> UpdateAsync(CheckoutStepConfiguration step, CancellationToken ct = default)
    {
        step.UpdatedAt = DateTime.UtcNow;

        _context.CheckoutSteps.Update(step);
        await _context.SaveChangesAsync(ct);

        return step;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var step = await GetByIdAsync(id, ct);
        if (step != null)
        {
            _context.CheckoutSteps.Remove(step);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task ReorderAsync(IEnumerable<(Guid Id, int SortOrder)> orders, CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in orders)
        {
            var step = await GetByIdAsync(id, ct);
            if (step != null)
            {
                step.SortOrder = sortOrder;
                step.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
