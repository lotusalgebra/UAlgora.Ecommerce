using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for tax configuration and calculation operations.
/// </summary>
public class TaxService : ITaxService
{
    private readonly EcommerceDbContext _context;

    public TaxService(EcommerceDbContext context)
    {
        _context = context;
    }

    #region Tax Categories

    public async Task<TaxCategory?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TaxCategories
            .Include(c => c.Rates)
            .ThenInclude(r => r.TaxZone)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<TaxCategory?> GetCategoryByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.TaxCategories
            .Include(c => c.Rates)
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<IReadOnlyList<TaxCategory>> GetAllCategoriesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.TaxCategories.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaxCategory>> GetActiveCategoriesAsync(CancellationToken ct = default)
    {
        return await GetAllCategoriesAsync(false, ct);
    }

    public async Task<TaxCategory?> GetDefaultCategoryAsync(CancellationToken ct = default)
    {
        return await _context.TaxCategories
            .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive, ct);
    }

    public async Task<TaxCategory> CreateCategoryAsync(TaxCategory category, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (category.IsDefault)
        {
            await UnsetDefaultCategoryAsync(ct);
        }

        _context.TaxCategories.Add(category);
        await _context.SaveChangesAsync(ct);
        return category;
    }

    public async Task<TaxCategory> UpdateCategoryAsync(TaxCategory category, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (category.IsDefault)
        {
            await UnsetDefaultCategoryAsync(category.Id, ct);
        }

        _context.TaxCategories.Update(category);
        await _context.SaveChangesAsync(ct);
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _context.TaxCategories.FindAsync([id], ct);
        if (category != null)
        {
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task SetDefaultCategoryAsync(Guid id, CancellationToken ct = default)
    {
        await UnsetDefaultCategoryAsync(ct);

        var category = await _context.TaxCategories.FindAsync([id], ct);
        if (category != null)
        {
            category.IsDefault = true;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateCategorySortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in sortOrders)
        {
            var category = await _context.TaxCategories.FindAsync([id], ct);
            if (category != null)
            {
                category.SortOrder = sortOrder;
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<TaxCategory> ToggleCategoryStatusAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _context.TaxCategories.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Tax category {id} not found");

        category.IsActive = !category.IsActive;
        await _context.SaveChangesAsync(ct);
        return category;
    }

    private async Task UnsetDefaultCategoryAsync(CancellationToken ct = default)
    {
        var defaultCategories = await _context.TaxCategories
            .Where(c => c.IsDefault)
            .ToListAsync(ct);

        foreach (var c in defaultCategories)
        {
            c.IsDefault = false;
        }
    }

    private async Task UnsetDefaultCategoryAsync(Guid exceptId, CancellationToken ct = default)
    {
        var defaultCategories = await _context.TaxCategories
            .Where(c => c.IsDefault && c.Id != exceptId)
            .ToListAsync(ct);

        foreach (var c in defaultCategories)
        {
            c.IsDefault = false;
        }
    }

    #endregion

    #region Tax Zones

    public async Task<TaxZone?> GetZoneByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TaxZones
            .Include(z => z.Rates)
            .ThenInclude(r => r.TaxCategory)
            .FirstOrDefaultAsync(z => z.Id == id, ct);
    }

    public async Task<TaxZone?> GetZoneByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.TaxZones
            .Include(z => z.Rates)
            .FirstOrDefaultAsync(z => z.Code == code, ct);
    }

    public async Task<IReadOnlyList<TaxZone>> GetAllZonesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.TaxZones.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(z => z.IsActive);
        }

        return await query
            .OrderByDescending(z => z.Priority)
            .ThenBy(z => z.SortOrder)
            .ThenBy(z => z.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaxZone>> GetActiveZonesAsync(CancellationToken ct = default)
    {
        return await GetAllZonesAsync(false, ct);
    }

    public async Task<TaxZone?> GetDefaultZoneAsync(CancellationToken ct = default)
    {
        return await _context.TaxZones
            .FirstOrDefaultAsync(z => z.IsDefault && z.IsActive, ct);
    }

    public async Task<IReadOnlyList<TaxZone>> GetZonesForAddressAsync(Address address, CancellationToken ct = default)
    {
        var zones = await GetActiveZonesAsync(ct);
        return zones.Where(z => z.MatchesAddress(address)).ToList();
    }

    public async Task<TaxZone> CreateZoneAsync(TaxZone zone, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (zone.IsDefault)
        {
            await UnsetDefaultZoneAsync(ct);
        }

        _context.TaxZones.Add(zone);
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    public async Task<TaxZone> UpdateZoneAsync(TaxZone zone, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (zone.IsDefault)
        {
            await UnsetDefaultZoneAsync(zone.Id, ct);
        }

        _context.TaxZones.Update(zone);
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    public async Task DeleteZoneAsync(Guid id, CancellationToken ct = default)
    {
        var zone = await _context.TaxZones.FindAsync([id], ct);
        if (zone != null)
        {
            zone.IsDeleted = true;
            zone.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task SetDefaultZoneAsync(Guid id, CancellationToken ct = default)
    {
        await UnsetDefaultZoneAsync(ct);

        var zone = await _context.TaxZones.FindAsync([id], ct);
        if (zone != null)
        {
            zone.IsDefault = true;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateZoneSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in sortOrders)
        {
            var zone = await _context.TaxZones.FindAsync([id], ct);
            if (zone != null)
            {
                zone.SortOrder = sortOrder;
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<TaxZone> ToggleZoneStatusAsync(Guid id, CancellationToken ct = default)
    {
        var zone = await _context.TaxZones.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Tax zone {id} not found");

        zone.IsActive = !zone.IsActive;
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    private async Task UnsetDefaultZoneAsync(CancellationToken ct = default)
    {
        var defaultZones = await _context.TaxZones
            .Where(z => z.IsDefault)
            .ToListAsync(ct);

        foreach (var z in defaultZones)
        {
            z.IsDefault = false;
        }
    }

    private async Task UnsetDefaultZoneAsync(Guid exceptId, CancellationToken ct = default)
    {
        var defaultZones = await _context.TaxZones
            .Where(z => z.IsDefault && z.Id != exceptId)
            .ToListAsync(ct);

        foreach (var z in defaultZones)
        {
            z.IsDefault = false;
        }
    }

    #endregion

    #region Tax Rates

    public async Task<TaxRate?> GetRateByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TaxRates
            .Include(r => r.TaxZone)
            .Include(r => r.TaxCategory)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<TaxRate>> GetRatesForZoneAsync(Guid zoneId, CancellationToken ct = default)
    {
        return await _context.TaxRates
            .Include(r => r.TaxCategory)
            .Where(r => r.TaxZoneId == zoneId)
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaxRate>> GetRatesForCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _context.TaxRates
            .Include(r => r.TaxZone)
            .Where(r => r.TaxCategoryId == categoryId)
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<TaxRate?> GetRateAsync(Guid zoneId, Guid categoryId, CancellationToken ct = default)
    {
        return await _context.TaxRates
            .FirstOrDefaultAsync(r => r.TaxZoneId == zoneId && r.TaxCategoryId == categoryId, ct);
    }

    public async Task<IReadOnlyList<TaxRate>> GetAllRatesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.TaxRates
            .Include(r => r.TaxZone)
            .Include(r => r.TaxCategory)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        return await query
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<TaxRate> CreateRateAsync(TaxRate rate, CancellationToken ct = default)
    {
        _context.TaxRates.Add(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task<TaxRate> UpdateRateAsync(TaxRate rate, CancellationToken ct = default)
    {
        _context.TaxRates.Update(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task DeleteRateAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await _context.TaxRates.FindAsync([id], ct);
        if (rate != null)
        {
            rate.IsDeleted = true;
            rate.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<TaxRate>> CreateRatesForZoneAsync(Guid zoneId, decimal defaultRate, CancellationToken ct = default)
    {
        var categories = await GetActiveCategoriesAsync(ct);
        var zone = await GetZoneByIdAsync(zoneId, ct);
        var rates = new List<TaxRate>();

        foreach (var category in categories)
        {
            // Skip if rate already exists
            var existingRate = await GetRateAsync(zoneId, category.Id, ct);
            if (existingRate != null) continue;

            var rate = new TaxRate
            {
                Name = $"{zone?.Name ?? "Zone"} - {category.Name}",
                TaxZoneId = zoneId,
                TaxCategoryId = category.Id,
                Rate = category.IsTaxExempt ? 0 : defaultRate,
                RateType = TaxRateType.Percentage,
                IsActive = true
            };

            _context.TaxRates.Add(rate);
            rates.Add(rate);
        }

        await _context.SaveChangesAsync(ct);
        return rates;
    }

    public async Task<TaxRate> ToggleRateStatusAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await _context.TaxRates.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Tax rate {id} not found");

        rate.IsActive = !rate.IsActive;
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    #endregion

    #region Tax Calculation

    public async Task<TaxCalculationResult> CalculateTaxAsync(
        TaxCalculationContext context,
        CancellationToken ct = default)
    {
        var result = new TaxCalculationResult { Success = true };

        // Check for tax exemption
        if (context.IsTaxExempt || !string.IsNullOrEmpty(context.ExemptionNumber))
        {
            result.IsExempt = true;
            result.ExemptionReason = !string.IsNullOrEmpty(context.ExemptionNumber)
                ? $"Exempt: {context.ExemptionNumber}"
                : "Customer is tax exempt";
            result.ExemptAmount = context.Amount;
            return result;
        }

        // Find matching zones
        var matchingZones = await GetZonesForAddressAsync(context.Address, ct);
        if (!matchingZones.Any())
        {
            // Try default zone
            var defaultZone = await GetDefaultZoneAsync(ct);
            if (defaultZone != null)
            {
                matchingZones = [defaultZone];
            }
            else
            {
                result.TaxableAmount = context.Amount;
                return result; // No tax zones configured
            }
        }

        // Find category
        TaxCategory? category = null;
        if (!string.IsNullOrEmpty(context.TaxClass))
        {
            category = await GetCategoryByCodeAsync(context.TaxClass, ct);
        }
        category ??= await GetDefaultCategoryAsync(ct);

        if (category?.IsTaxExempt == true)
        {
            result.IsExempt = true;
            result.ExemptionReason = $"Category '{category.Name}' is tax exempt";
            result.ExemptAmount = context.Amount;
            return result;
        }

        // Get applicable rates
        var taxableAmount = context.Amount;
        if (context.IncludesShipping)
        {
            taxableAmount += context.ShippingAmount;
        }

        result.TaxableAmount = taxableAmount;
        decimal totalTax = 0;
        decimal compoundBase = 0;

        foreach (var zone in matchingZones)
        {
            var rates = await GetRatesForZoneAsync(zone.Id, ct);
            if (category != null)
            {
                rates = rates.Where(r => r.TaxCategoryId == category.Id).ToList();
            }

            foreach (var rate in rates.Where(r => r.IsActive && r.IsCurrentlyEffective))
            {
                var tax = rate.CalculateTax(taxableAmount, compoundBase);
                if (tax > 0)
                {
                    totalTax += tax;
                    if (rate.IsCompound)
                    {
                        compoundBase += tax;
                    }

                    result.Breakdown.Add(new TaxBreakdown
                    {
                        JurisdictionType = rate.JurisdictionType ?? zone.Name,
                        JurisdictionName = rate.JurisdictionName ?? rate.Name,
                        Rate = rate.Rate,
                        Amount = tax,
                        IsCompound = rate.IsCompound
                    });
                }
            }
        }

        result.TaxAmount = Math.Round(totalTax, 2);
        result.EffectiveRate = taxableAmount > 0
            ? Math.Round((totalTax / taxableAmount) * 100, 4)
            : 0;

        return result;
    }

    public async Task<TaxSummary> CalculateOrderTaxAsync(
        Address address,
        IEnumerable<TaxableItem> items,
        decimal shippingAmount = 0,
        CancellationToken ct = default)
    {
        var summary = new TaxSummary();
        var jurisdictionTotals = new Dictionary<string, TaxBreakdown>();

        foreach (var item in items)
        {
            var context = new TaxCalculationContext
            {
                Address = address,
                Amount = item.Amount * item.Quantity,
                TaxClass = item.TaxClass,
                IsTaxExempt = item.IsTaxExempt
            };

            var result = await CalculateTaxAsync(context, ct);

            summary.ItemResults.Add(new TaxItemResult
            {
                ItemId = item.ItemId,
                TaxableAmount = result.TaxableAmount,
                TaxAmount = result.TaxAmount,
                EffectiveRate = result.EffectiveRate,
                IsExempt = result.IsExempt
            });

            if (result.IsExempt)
            {
                summary.TotalExempt += result.ExemptAmount;
            }
            else
            {
                summary.TotalTaxable += result.TaxableAmount;
                summary.TotalTax += result.TaxAmount;
            }

            // Aggregate jurisdiction breakdown
            foreach (var breakdown in result.Breakdown)
            {
                var key = $"{breakdown.JurisdictionType}|{breakdown.JurisdictionName}";
                if (jurisdictionTotals.TryGetValue(key, out var existing))
                {
                    existing.Amount += breakdown.Amount;
                }
                else
                {
                    jurisdictionTotals[key] = new TaxBreakdown
                    {
                        JurisdictionType = breakdown.JurisdictionType,
                        JurisdictionName = breakdown.JurisdictionName,
                        Rate = breakdown.Rate,
                        Amount = breakdown.Amount,
                        IsCompound = breakdown.IsCompound
                    };
                }
            }
        }

        // Calculate shipping tax if applicable
        if (shippingAmount > 0)
        {
            var zones = await GetZonesForAddressAsync(address, ct);
            foreach (var zone in zones)
            {
                var rates = await GetRatesForZoneAsync(zone.Id, ct);
                foreach (var rate in rates.Where(r => r.IsActive && r.TaxShipping))
                {
                    summary.ShippingTax += rate.CalculateTax(shippingAmount);
                }
            }
            summary.TotalTax += summary.ShippingTax;
        }

        summary.JurisdictionBreakdown = jurisdictionTotals.Values.ToList();

        return summary;
    }

    public async Task<decimal> GetEffectiveTaxRateAsync(
        Address address,
        string? taxClass,
        CancellationToken ct = default)
    {
        var context = new TaxCalculationContext
        {
            Address = address,
            Amount = 100, // Use 100 as base for percentage
            TaxClass = taxClass
        };

        var result = await CalculateTaxAsync(context, ct);
        return result.EffectiveRate;
    }

    public async Task<bool> IsTaxableAddressAsync(Address address, CancellationToken ct = default)
    {
        var zones = await GetZonesForAddressAsync(address, ct);
        if (!zones.Any())
        {
            var defaultZone = await GetDefaultZoneAsync(ct);
            return defaultZone != null;
        }

        return zones.Any(z => z.Rates.Any(r => r.IsActive && r.Rate > 0));
    }

    #endregion

    #region Validation

    public Task<ValidationResult> ValidateCategoryAsync(TaxCategory category, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Category name is required." });
        }

        if (string.IsNullOrWhiteSpace(category.Code))
        {
            errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Category code is required." });
        }

        return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors));
    }

    public Task<ValidationResult> ValidateZoneAsync(TaxZone zone, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(zone.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Zone name is required." });
        }

        if (string.IsNullOrWhiteSpace(zone.Code))
        {
            errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Zone code is required." });
        }

        return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors));
    }

    public Task<ValidationResult> ValidateRateAsync(TaxRate rate, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(rate.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Rate name is required." });
        }

        if (rate.TaxZoneId == Guid.Empty)
        {
            errors.Add(new ValidationError { PropertyName = "TaxZoneId", ErrorMessage = "Tax zone is required." });
        }

        if (rate.TaxCategoryId == Guid.Empty)
        {
            errors.Add(new ValidationError { PropertyName = "TaxCategoryId", ErrorMessage = "Tax category is required." });
        }

        if (rate.Rate < 0)
        {
            errors.Add(new ValidationError { PropertyName = "Rate", ErrorMessage = "Rate cannot be negative." });
        }

        if (rate.Rate > 100 && rate.RateType == TaxRateType.Percentage)
        {
            errors.Add(new ValidationError { PropertyName = "Rate", ErrorMessage = "Percentage rate cannot exceed 100%." });
        }

        if (rate.EffectiveFrom.HasValue && rate.EffectiveTo.HasValue &&
            rate.EffectiveFrom > rate.EffectiveTo)
        {
            errors.Add(new ValidationError { PropertyName = "EffectiveFrom", ErrorMessage = "Effective from date must be before effective to date." });
        }

        return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors));
    }

    #endregion
}
