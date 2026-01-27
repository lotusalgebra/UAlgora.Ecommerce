using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for country operations.
/// </summary>
public class CountryRepository : Repository<Country>, ICountryRepository
{
    public CountryRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<IReadOnlyList<Country>> GetActiveAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Country>> GetAllWithStatesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.States.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ThenBy(s => s.Name))
            .OrderByDescending(c => c.IsFeatured)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Country?> GetWithStatesAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.States.OrderBy(s => s.SortOrder).ThenBy(s => s.Name))
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Country?> GetWithStatesByCodeAsync(string code, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.States.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ThenBy(s => s.Name))
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<IReadOnlyList<Country>> GetShippingCountriesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.States.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ThenBy(s => s.Name))
            .Where(c => c.IsActive && c.AllowShipping)
            .OrderByDescending(c => c.IsFeatured)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Country>> GetBillingCountriesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.States.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ThenBy(s => s.Name))
            .Where(c => c.IsActive && c.AllowBilling)
            .OrderByDescending(c => c.IsFeatured)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Country>> GetFeaturedAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.IsActive && c.IsFeatured)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StateProvince>> GetStatesAsync(Guid countryId, CancellationToken ct = default)
    {
        return await Context.Set<StateProvince>()
            .Where(s => s.CountryId == countryId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StateProvince>> GetStatesByCountryCodeAsync(string countryCode, CancellationToken ct = default)
    {
        var country = await DbSet.FirstOrDefaultAsync(c => c.Code == countryCode, ct);
        if (country == null) return [];

        return await Context.Set<StateProvince>()
            .Where(s => s.CountryId == country.Id && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<StateProvince?> GetStateByIdAsync(Guid stateId, CancellationToken ct = default)
    {
        return await Context.Set<StateProvince>()
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.Id == stateId, ct);
    }

    public async Task<StateProvince?> GetStateByCodeAsync(string countryCode, string stateCode, CancellationToken ct = default)
    {
        return await Context.Set<StateProvince>()
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.Country!.Code == countryCode && s.Code == stateCode, ct);
    }

    public async Task<StateProvince> AddStateAsync(StateProvince state, CancellationToken ct = default)
    {
        await Context.Set<StateProvince>().AddAsync(state, ct);
        await Context.SaveChangesAsync(ct);
        return state;
    }

    public async Task<StateProvince> UpdateStateAsync(StateProvince state, CancellationToken ct = default)
    {
        state.UpdatedAt = DateTime.UtcNow;
        Context.Set<StateProvince>().Update(state);
        await Context.SaveChangesAsync(ct);
        return state;
    }

    public async Task DeleteStateAsync(Guid stateId, CancellationToken ct = default)
    {
        var state = await Context.Set<StateProvince>().FindAsync([stateId], ct);
        if (state != null)
        {
            Context.Set<StateProvince>().Remove(state);
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(c => c.Code == code && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<bool> StateCodeExistsAsync(Guid countryId, string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await Context.Set<StateProvince>()
            .AnyAsync(s => s.CountryId == countryId && s.Code == code && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
    }

    // Explicit implementation for ICountryRepository.CountAsync
    async Task<int> ICountryRepository.CountAsync(CancellationToken ct)
    {
        return await DbSet.CountAsync(ct);
    }
}
