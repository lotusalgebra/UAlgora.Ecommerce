using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for shipping operations.
/// </summary>
public class ShippingService : IShippingService
{
    private readonly EcommerceDbContext _context;

    public ShippingService(EcommerceDbContext context)
    {
        _context = context;
    }

    #region Shipping Methods

    public async Task<ShippingMethod?> GetMethodByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ShippingMethods
            .Include(m => m.Rates)
            .ThenInclude(r => r.Zone)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<ShippingMethod?> GetMethodByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.ShippingMethods
            .Include(m => m.Rates)
            .FirstOrDefaultAsync(m => m.Code == code, ct);
    }

    public async Task<IReadOnlyList<ShippingMethod>> GetAllMethodsAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.ShippingMethods.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(m => m.IsActive);
        }

        return await query
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ShippingMethod>> GetActiveMethodsAsync(CancellationToken ct = default)
    {
        return await GetAllMethodsAsync(false, ct);
    }

    public async Task<IReadOnlyList<ShippingMethod>> GetMethodsForZoneAsync(Guid zoneId, CancellationToken ct = default)
    {
        return await _context.ShippingRates
            .Where(r => r.ShippingZoneId == zoneId && r.IsActive)
            .Include(r => r.Method)
            .Where(r => r.Method!.IsActive)
            .Select(r => r.Method!)
            .Distinct()
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<ShippingMethod> CreateMethodAsync(ShippingMethod method, CancellationToken ct = default)
    {
        _context.ShippingMethods.Add(method);
        await _context.SaveChangesAsync(ct);
        return method;
    }

    public async Task<ShippingMethod> UpdateMethodAsync(ShippingMethod method, CancellationToken ct = default)
    {
        _context.ShippingMethods.Update(method);
        await _context.SaveChangesAsync(ct);
        return method;
    }

    public async Task DeleteMethodAsync(Guid id, CancellationToken ct = default)
    {
        var method = await _context.ShippingMethods.FindAsync([id], ct);
        if (method != null)
        {
            method.IsDeleted = true;
            method.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateMethodSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in sortOrders)
        {
            var method = await _context.ShippingMethods.FindAsync([id], ct);
            if (method != null)
            {
                method.SortOrder = sortOrder;
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ShippingMethod> ToggleMethodStatusAsync(Guid id, CancellationToken ct = default)
    {
        var method = await _context.ShippingMethods.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Shipping method {id} not found.");

        method.IsActive = !method.IsActive;
        await _context.SaveChangesAsync(ct);
        return method;
    }

    #endregion

    #region Shipping Zones

    public async Task<ShippingZone?> GetZoneByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ShippingZones
            .Include(z => z.Rates)
            .ThenInclude(r => r.Method)
            .FirstOrDefaultAsync(z => z.Id == id, ct);
    }

    public async Task<ShippingZone?> GetZoneByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.ShippingZones
            .Include(z => z.Rates)
            .FirstOrDefaultAsync(z => z.Code == code, ct);
    }

    public async Task<IReadOnlyList<ShippingZone>> GetAllZonesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.ShippingZones.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(z => z.IsActive);
        }

        return await query
            .OrderByDescending(z => z.IsDefault)
            .ThenBy(z => z.SortOrder)
            .ThenBy(z => z.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ShippingZone>> GetActiveZonesAsync(CancellationToken ct = default)
    {
        return await GetAllZonesAsync(false, ct);
    }

    public async Task<ShippingZone?> GetDefaultZoneAsync(CancellationToken ct = default)
    {
        return await _context.ShippingZones
            .Where(z => z.IsDefault && z.IsActive)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ShippingZone?> GetZoneForAddressAsync(Address address, CancellationToken ct = default)
    {
        var zones = await _context.ShippingZones
            .Where(z => z.IsActive)
            .OrderByDescending(z => z.IsDefault)
            .ThenBy(z => z.SortOrder)
            .ToListAsync(ct);

        // Try to find a matching zone
        foreach (var zone in zones.Where(z => !z.IsDefault))
        {
            if (zone.MatchesAddress(address))
            {
                return zone;
            }
        }

        // Fall back to default zone
        return zones.FirstOrDefault(z => z.IsDefault);
    }

    public async Task<ShippingZone> CreateZoneAsync(ShippingZone zone, CancellationToken ct = default)
    {
        // If this is set as default, unset other defaults
        if (zone.IsDefault)
        {
            await UnsetDefaultZonesAsync(ct);
        }

        _context.ShippingZones.Add(zone);
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    public async Task<ShippingZone> UpdateZoneAsync(ShippingZone zone, CancellationToken ct = default)
    {
        // If this is set as default, unset other defaults
        if (zone.IsDefault)
        {
            await UnsetDefaultZonesAsync(zone.Id, ct);
        }

        _context.ShippingZones.Update(zone);
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    public async Task DeleteZoneAsync(Guid id, CancellationToken ct = default)
    {
        var zone = await _context.ShippingZones.FindAsync([id], ct);
        if (zone != null)
        {
            zone.IsDeleted = true;
            zone.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateZoneSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in sortOrders)
        {
            var zone = await _context.ShippingZones.FindAsync([id], ct);
            if (zone != null)
            {
                zone.SortOrder = sortOrder;
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetDefaultZoneAsync(Guid id, CancellationToken ct = default)
    {
        await UnsetDefaultZonesAsync(ct);

        var zone = await _context.ShippingZones.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Shipping zone {id} not found.");

        zone.IsDefault = true;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ShippingZone> ToggleZoneStatusAsync(Guid id, CancellationToken ct = default)
    {
        var zone = await _context.ShippingZones.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Shipping zone {id} not found.");

        zone.IsActive = !zone.IsActive;
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    private async Task UnsetDefaultZonesAsync(CancellationToken ct = default)
    {
        await UnsetDefaultZonesAsync(null, ct);
    }

    private async Task UnsetDefaultZonesAsync(Guid? excludeId, CancellationToken ct = default)
    {
        var defaultZones = await _context.ShippingZones
            .Where(z => z.IsDefault && (excludeId == null || z.Id != excludeId))
            .ToListAsync(ct);

        foreach (var zone in defaultZones)
        {
            zone.IsDefault = false;
        }
    }

    #endregion

    #region Shipping Rates

    public async Task<ShippingRate?> GetRateByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ShippingRates
            .Include(r => r.Zone)
            .Include(r => r.Method)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<ShippingRate>> GetRatesForZoneAsync(Guid zoneId, CancellationToken ct = default)
    {
        return await _context.ShippingRates
            .Include(r => r.Method)
            .Where(r => r.ShippingZoneId == zoneId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ShippingRate>> GetRatesForMethodAsync(Guid methodId, CancellationToken ct = default)
    {
        return await _context.ShippingRates
            .Include(r => r.Zone)
            .Where(r => r.ShippingMethodId == methodId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<ShippingRate?> GetRateAsync(Guid zoneId, Guid methodId, CancellationToken ct = default)
    {
        return await _context.ShippingRates
            .Include(r => r.Zone)
            .Include(r => r.Method)
            .FirstOrDefaultAsync(r => r.ShippingZoneId == zoneId && r.ShippingMethodId == methodId, ct);
    }

    public async Task<ShippingRate> CreateRateAsync(ShippingRate rate, CancellationToken ct = default)
    {
        _context.ShippingRates.Add(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task<ShippingRate> UpdateRateAsync(ShippingRate rate, CancellationToken ct = default)
    {
        _context.ShippingRates.Update(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task DeleteRateAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await _context.ShippingRates.FindAsync([id], ct);
        if (rate != null)
        {
            _context.ShippingRates.Remove(rate);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<ShippingRate>> CreateRatesForZoneAsync(Guid zoneId, decimal defaultRate, CancellationToken ct = default)
    {
        var methods = await GetActiveMethodsAsync(ct);
        var rates = new List<ShippingRate>();

        foreach (var method in methods)
        {
            // Check if rate already exists
            var existingRate = await GetRateAsync(zoneId, method.Id, ct);
            if (existingRate == null)
            {
                var rate = new ShippingRate
                {
                    ShippingZoneId = zoneId,
                    ShippingMethodId = method.Id,
                    BaseRate = defaultRate,
                    IsActive = true
                };
                _context.ShippingRates.Add(rate);
                rates.Add(rate);
            }
        }

        await _context.SaveChangesAsync(ct);
        return rates;
    }

    #endregion

    #region Rate Calculation

    public async Task<IReadOnlyList<AvailableShippingMethod>> GetShippingOptionsAsync(
        Address shippingAddress,
        decimal orderTotal,
        decimal orderWeight,
        int itemCount,
        CancellationToken ct = default)
    {
        var options = new List<AvailableShippingMethod>();

        // Get the zone for this address
        var zone = await GetZoneForAddressAsync(shippingAddress, ct);
        if (zone == null)
        {
            return options;
        }

        // Get rates for this zone
        var rates = await _context.ShippingRates
            .Include(r => r.Method)
            .Where(r => r.ShippingZoneId == zone.Id && r.IsActive && r.Method!.IsActive)
            .OrderBy(r => r.Method!.SortOrder)
            .ToListAsync(ct);

        foreach (var rate in rates)
        {
            if (rate.Method == null) continue;

            // Check if order meets requirements
            if (!rate.MeetsRequirements(orderTotal, orderWeight))
                continue;

            // Check method restrictions
            if (rate.Method.MinOrderAmount.HasValue && orderTotal < rate.Method.MinOrderAmount.Value)
                continue;
            if (rate.Method.MaxOrderAmount.HasValue && orderTotal > rate.Method.MaxOrderAmount.Value)
                continue;
            if (rate.Method.MinWeight.HasValue && orderWeight < rate.Method.MinWeight.Value)
                continue;
            if (rate.Method.MaxWeight.HasValue && orderWeight > rate.Method.MaxWeight.Value)
                continue;

            var cost = rate.CalculateCost(orderTotal, orderWeight, itemCount);

            options.Add(new AvailableShippingMethod
            {
                MethodId = rate.Method.Id,
                MethodCode = rate.Method.Code,
                Name = rate.Method.Name,
                Description = rate.Method.Description,
                Cost = cost,
                EstimatedDaysMin = rate.EstimatedDaysMin ?? rate.Method.EstimatedDaysMin,
                EstimatedDaysMax = rate.EstimatedDaysMax ?? rate.Method.EstimatedDaysMax,
                DeliveryEstimateText = rate.DeliveryEstimateText ?? rate.Method.DeliveryEstimateText,
                CarrierName = rate.Method.CarrierProviderId,
                IconName = rate.Method.IconName,
                SortOrder = rate.Method.SortOrder
            });
        }

        return options.OrderBy(o => o.SortOrder).ThenBy(o => o.Cost).ToList();
    }

    public async Task<ShippingCostResult> CalculateShippingCostAsync(
        Guid methodId,
        Address shippingAddress,
        decimal orderTotal,
        decimal orderWeight,
        int itemCount,
        CancellationToken ct = default)
    {
        var method = await GetMethodByIdAsync(methodId, ct);
        if (method == null)
        {
            return new ShippingCostResult
            {
                Success = false,
                ErrorMessage = "Shipping method not found."
            };
        }

        if (!method.IsActive)
        {
            return new ShippingCostResult
            {
                Success = false,
                ErrorMessage = "Shipping method is not available."
            };
        }

        var zone = await GetZoneForAddressAsync(shippingAddress, ct);
        if (zone == null)
        {
            return new ShippingCostResult
            {
                Success = false,
                ErrorMessage = "Shipping is not available to this address."
            };
        }

        var rate = await GetRateAsync(zone.Id, methodId, ct);
        if (rate == null || !rate.IsActive)
        {
            return new ShippingCostResult
            {
                Success = false,
                ErrorMessage = "Shipping rate not available for this zone."
            };
        }

        if (!rate.MeetsRequirements(orderTotal, orderWeight))
        {
            return new ShippingCostResult
            {
                Success = false,
                ErrorMessage = "Order does not meet shipping requirements."
            };
        }

        var cost = rate.CalculateCost(orderTotal, orderWeight, itemCount);
        var isFree = cost == 0;
        string? freeReason = null;

        if (isFree)
        {
            if (method.CalculationType == ShippingCalculationType.FreeShipping)
            {
                freeReason = "Free shipping method";
            }
            else if (rate.FreeShippingThreshold.HasValue && orderTotal >= rate.FreeShippingThreshold.Value)
            {
                freeReason = $"Order qualifies for free shipping (over {rate.FreeShippingThreshold:C})";
            }
            else if (method.FreeShippingThreshold.HasValue && orderTotal >= method.FreeShippingThreshold.Value)
            {
                freeReason = $"Order qualifies for free shipping (over {method.FreeShippingThreshold:C})";
            }
        }

        return new ShippingCostResult
        {
            Success = true,
            Cost = cost,
            IsFree = isFree,
            FreeShippingReason = freeReason,
            DeliveryEstimateText = rate.DeliveryEstimateText ?? method.DeliveryEstimateText
        };
    }

    public async Task<bool> CanShipToAsync(Address address, CancellationToken ct = default)
    {
        var zone = await GetZoneForAddressAsync(address, ct);
        if (zone == null)
        {
            return false;
        }

        // Check if there are any active methods with rates for this zone
        var hasRates = await _context.ShippingRates
            .AnyAsync(r => r.ShippingZoneId == zone.Id && r.IsActive && r.Method!.IsActive, ct);

        return hasRates;
    }

    #endregion

    #region Validation

    public Task<ValidationResult> ValidateMethodAsync(ShippingMethod method, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(method.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Method name is required." });
        }

        if (string.IsNullOrWhiteSpace(method.Code))
        {
            errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Method code is required." });
        }

        switch (method.CalculationType)
        {
            case ShippingCalculationType.FlatRate:
                if (!method.FlatRate.HasValue || method.FlatRate < 0)
                {
                    errors.Add(new ValidationError { PropertyName = "FlatRate", ErrorMessage = "Flat rate must be specified and non-negative." });
                }
                break;

            case ShippingCalculationType.WeightBased:
                if (!method.WeightPerUnitRate.HasValue || method.WeightPerUnitRate < 0)
                {
                    errors.Add(new ValidationError { PropertyName = "WeightPerUnitRate", ErrorMessage = "Weight rate must be specified and non-negative." });
                }
                break;

            case ShippingCalculationType.PriceBased:
                if (!method.PricePercentage.HasValue || method.PricePercentage < 0 || method.PricePercentage > 100)
                {
                    errors.Add(new ValidationError { PropertyName = "PricePercentage", ErrorMessage = "Price percentage must be between 0 and 100." });
                }
                break;

            case ShippingCalculationType.PerItem:
                if (!method.PerItemRate.HasValue || method.PerItemRate < 0)
                {
                    errors.Add(new ValidationError { PropertyName = "PerItemRate", ErrorMessage = "Per item rate must be specified and non-negative." });
                }
                break;
        }

        return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors));
    }

    public Task<ValidationResult> ValidateZoneAsync(ShippingZone zone, CancellationToken ct = default)
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

    public Task<ValidationResult> ValidateRateAsync(ShippingRate rate, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (rate.ShippingZoneId == Guid.Empty)
        {
            errors.Add(new ValidationError { PropertyName = "ShippingZoneId", ErrorMessage = "Zone is required." });
        }

        if (rate.ShippingMethodId == Guid.Empty)
        {
            errors.Add(new ValidationError { PropertyName = "ShippingMethodId", ErrorMessage = "Method is required." });
        }

        if (rate.BaseRate < 0)
        {
            errors.Add(new ValidationError { PropertyName = "BaseRate", ErrorMessage = "Base rate cannot be negative." });
        }

        return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors));
    }

    #endregion
}
