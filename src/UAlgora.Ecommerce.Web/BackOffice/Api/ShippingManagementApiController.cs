using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for shipping management in the backoffice.
/// </summary>
[ApiController]
[BackOfficeRoute("ecommerce/shipping")]
[MapToApi("ecommerce-management-api")]
public class ShippingManagementApiController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingManagementApiController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    #region Shipping Methods

    /// <summary>
    /// Gets all shipping methods.
    /// </summary>
    [HttpGet("method")]
    public async Task<IActionResult> GetAllMethods(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var methods = await _shippingService.GetAllMethodsAsync(includeInactive, ct);
        return Ok(new { items = methods.Select(MapMethodToResponse) });
    }

    /// <summary>
    /// Gets a shipping method by ID.
    /// </summary>
    [HttpGet("method/{id:guid}")]
    public async Task<IActionResult> GetMethod(Guid id, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        return Ok(MapMethodToResponse(method));
    }

    /// <summary>
    /// Creates a new shipping method.
    /// </summary>
    [HttpPost("method")]
    public async Task<IActionResult> CreateMethod([FromBody] CreateShippingMethodRequest request, CancellationToken ct = default)
    {
        var method = new ShippingMethod
        {
            Name = request.Name,
            Description = request.Description,
            Code = request.Code,
            CalculationType = Enum.Parse<ShippingCalculationType>(request.CalculationType ?? "FlatRate"),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            FlatRate = request.FlatRate,
            WeightBaseRate = request.WeightBaseRate,
            WeightPerUnitRate = request.WeightPerUnitRate,
            WeightUnit = request.WeightUnit ?? "kg",
            PricePercentage = request.PricePercentage,
            MinimumCost = request.MinimumCost,
            MaximumCost = request.MaximumCost,
            PerItemRate = request.PerItemRate,
            HandlingFee = request.HandlingFee,
            FreeShippingThreshold = request.FreeShippingThreshold,
            FreeShippingRequiresCoupon = request.FreeShippingRequiresCoupon,
            MinWeight = request.MinWeight,
            MaxWeight = request.MaxWeight,
            MinOrderAmount = request.MinOrderAmount,
            MaxOrderAmount = request.MaxOrderAmount,
            EstimatedDaysMin = request.EstimatedDaysMin,
            EstimatedDaysMax = request.EstimatedDaysMax,
            DeliveryEstimateText = request.DeliveryEstimateText,
            CarrierProviderId = request.CarrierProviderId,
            CarrierServiceCode = request.CarrierServiceCode,
            UseCarrierRates = request.UseCarrierRates,
            IconName = request.IconName,
            IsTaxable = request.IsTaxable
        };

        var created = await _shippingService.CreateMethodAsync(method, ct);
        return Ok(MapMethodToResponse(created));
    }

    /// <summary>
    /// Updates a shipping method.
    /// </summary>
    [HttpPut("method/{id:guid}")]
    public async Task<IActionResult> UpdateMethod(Guid id, [FromBody] UpdateShippingMethodRequest request, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.Name = request.Name;
        method.Description = request.Description;
        method.Code = request.Code;
        method.CalculationType = Enum.Parse<ShippingCalculationType>(request.CalculationType ?? "FlatRate");
        method.IsActive = request.IsActive;
        method.SortOrder = request.SortOrder;
        method.FlatRate = request.FlatRate;
        method.WeightBaseRate = request.WeightBaseRate;
        method.WeightPerUnitRate = request.WeightPerUnitRate;
        method.WeightUnit = request.WeightUnit ?? "kg";
        method.PricePercentage = request.PricePercentage;
        method.MinimumCost = request.MinimumCost;
        method.MaximumCost = request.MaximumCost;
        method.PerItemRate = request.PerItemRate;
        method.HandlingFee = request.HandlingFee;
        method.FreeShippingThreshold = request.FreeShippingThreshold;
        method.FreeShippingRequiresCoupon = request.FreeShippingRequiresCoupon;
        method.MinWeight = request.MinWeight;
        method.MaxWeight = request.MaxWeight;
        method.MinOrderAmount = request.MinOrderAmount;
        method.MaxOrderAmount = request.MaxOrderAmount;
        method.EstimatedDaysMin = request.EstimatedDaysMin;
        method.EstimatedDaysMax = request.EstimatedDaysMax;
        method.DeliveryEstimateText = request.DeliveryEstimateText;
        method.CarrierProviderId = request.CarrierProviderId;
        method.CarrierServiceCode = request.CarrierServiceCode;
        method.UseCarrierRates = request.UseCarrierRates;
        method.IconName = request.IconName;
        method.IsTaxable = request.IsTaxable;

        var updated = await _shippingService.UpdateMethodAsync(method, ct);
        return Ok(MapMethodToResponse(updated));
    }

    /// <summary>
    /// Deletes a shipping method.
    /// </summary>
    [HttpDelete("method/{id:guid}")]
    public async Task<IActionResult> DeleteMethod(Guid id, CancellationToken ct = default)
    {
        await _shippingService.DeleteMethodAsync(id, ct);
        return Ok();
    }

    /// <summary>
    /// Gets rates for a method.
    /// </summary>
    [HttpGet("method/{id:guid}/rates")]
    public async Task<IActionResult> GetMethodRates(Guid id, CancellationToken ct = default)
    {
        var rates = await _shippingService.GetRatesForMethodAsync(id, ct);
        return Ok(new { items = rates.Select(MapRateToResponse) });
    }

    /// <summary>
    /// Duplicates a shipping method.
    /// </summary>
    [HttpPost("method/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateMethod(Guid id, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        var duplicate = new ShippingMethod
        {
            Name = $"{method.Name} (Copy)",
            Description = method.Description,
            Code = $"{method.Code}-copy-{DateTime.UtcNow.Ticks}",
            CalculationType = method.CalculationType,
            IsActive = false, // Start inactive
            SortOrder = method.SortOrder + 1,
            FlatRate = method.FlatRate,
            WeightBaseRate = method.WeightBaseRate,
            WeightPerUnitRate = method.WeightPerUnitRate,
            WeightUnit = method.WeightUnit,
            PricePercentage = method.PricePercentage,
            MinimumCost = method.MinimumCost,
            MaximumCost = method.MaximumCost,
            PerItemRate = method.PerItemRate,
            HandlingFee = method.HandlingFee,
            FreeShippingThreshold = method.FreeShippingThreshold,
            FreeShippingRequiresCoupon = method.FreeShippingRequiresCoupon,
            MinWeight = method.MinWeight,
            MaxWeight = method.MaxWeight,
            MinOrderAmount = method.MinOrderAmount,
            MaxOrderAmount = method.MaxOrderAmount,
            EstimatedDaysMin = method.EstimatedDaysMin,
            EstimatedDaysMax = method.EstimatedDaysMax,
            DeliveryEstimateText = method.DeliveryEstimateText,
            CarrierProviderId = method.CarrierProviderId,
            CarrierServiceCode = method.CarrierServiceCode,
            UseCarrierRates = method.UseCarrierRates,
            IconName = method.IconName,
            IsTaxable = method.IsTaxable
        };

        var created = await _shippingService.CreateMethodAsync(duplicate, ct);
        return Ok(MapMethodToResponse(created));
    }

    /// <summary>
    /// Toggles the active status of a shipping method.
    /// </summary>
    [HttpPost("method/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleMethodStatus(Guid id, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.IsActive = !method.IsActive;
        var updated = await _shippingService.UpdateMethodAsync(method, ct);
        return Ok(MapMethodToResponse(updated));
    }

    /// <summary>
    /// Updates the sort order of a shipping method.
    /// </summary>
    [HttpPost("method/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateMethodSort(Guid id, [FromBody] ShippingSortOrderRequest request, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.SortOrder = request.SortOrder;
        var updated = await _shippingService.UpdateMethodAsync(method, ct);
        return Ok(MapMethodToResponse(updated));
    }

    /// <summary>
    /// Toggles the taxable status of a shipping method.
    /// </summary>
    [HttpPost("method/{id:guid}/toggle-taxable")]
    public async Task<IActionResult> ToggleMethodTaxable(Guid id, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.IsTaxable = !method.IsTaxable;
        var updated = await _shippingService.UpdateMethodAsync(method, ct);
        return Ok(MapMethodToResponse(updated));
    }

    /// <summary>
    /// Updates the delivery estimate of a shipping method.
    /// </summary>
    [HttpPost("method/{id:guid}/update-delivery")]
    public async Task<IActionResult> UpdateMethodDelivery(Guid id, [FromBody] ShippingDeliveryEstimateRequest request, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.EstimatedDaysMin = request.EstimatedDaysMin;
        method.EstimatedDaysMax = request.EstimatedDaysMax;
        method.DeliveryEstimateText = request.DeliveryEstimateText;
        var updated = await _shippingService.UpdateMethodAsync(method, ct);
        return Ok(MapMethodToResponse(updated));
    }

    /// <summary>
    /// Updates the free shipping threshold of a shipping method.
    /// </summary>
    [HttpPost("method/{id:guid}/update-free-threshold")]
    public async Task<IActionResult> UpdateMethodFreeThreshold(Guid id, [FromBody] ShippingFreeThresholdRequest request, CancellationToken ct = default)
    {
        var method = await _shippingService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.FreeShippingThreshold = request.Threshold;
        var updated = await _shippingService.UpdateMethodAsync(method, ct);
        return Ok(MapMethodToResponse(updated));
    }

    #endregion

    #region Shipping Zones

    /// <summary>
    /// Gets all shipping zones.
    /// </summary>
    [HttpGet("zone")]
    public async Task<IActionResult> GetAllZones(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var zones = await _shippingService.GetAllZonesAsync(includeInactive, ct);
        return Ok(new { items = zones.Select(MapZoneToResponse) });
    }

    /// <summary>
    /// Gets a shipping zone by ID.
    /// </summary>
    [HttpGet("zone/{id:guid}")]
    public async Task<IActionResult> GetZone(Guid id, CancellationToken ct = default)
    {
        var zone = await _shippingService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        return Ok(MapZoneToResponse(zone));
    }

    /// <summary>
    /// Creates a new shipping zone.
    /// </summary>
    [HttpPost("zone")]
    public async Task<IActionResult> CreateZone([FromBody] CreateShippingZoneRequest request, CancellationToken ct = default)
    {
        var zone = new ShippingZone
        {
            Name = request.Name,
            Description = request.Description,
            Code = request.Code,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder,
            Countries = request.Countries ?? [],
            States = request.States ?? [],
            PostalCodePatterns = request.PostalCodePatterns ?? [],
            Cities = request.Cities ?? [],
            ExcludedCountries = request.ExcludedCountries ?? [],
            ExcludedStates = request.ExcludedStates ?? [],
            ExcludedPostalCodes = request.ExcludedPostalCodes ?? []
        };

        var created = await _shippingService.CreateZoneAsync(zone, ct);
        return Ok(MapZoneToResponse(created));
    }

    /// <summary>
    /// Updates a shipping zone.
    /// </summary>
    [HttpPut("zone/{id:guid}")]
    public async Task<IActionResult> UpdateZone(Guid id, [FromBody] UpdateShippingZoneRequest request, CancellationToken ct = default)
    {
        var zone = await _shippingService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.Name = request.Name;
        zone.Description = request.Description;
        zone.Code = request.Code;
        zone.IsActive = request.IsActive;
        zone.IsDefault = request.IsDefault;
        zone.SortOrder = request.SortOrder;
        zone.Countries = request.Countries ?? [];
        zone.States = request.States ?? [];
        zone.PostalCodePatterns = request.PostalCodePatterns ?? [];
        zone.Cities = request.Cities ?? [];
        zone.ExcludedCountries = request.ExcludedCountries ?? [];
        zone.ExcludedStates = request.ExcludedStates ?? [];
        zone.ExcludedPostalCodes = request.ExcludedPostalCodes ?? [];

        var updated = await _shippingService.UpdateZoneAsync(zone, ct);
        return Ok(MapZoneToResponse(updated));
    }

    /// <summary>
    /// Deletes a shipping zone.
    /// </summary>
    [HttpDelete("zone/{id:guid}")]
    public async Task<IActionResult> DeleteZone(Guid id, CancellationToken ct = default)
    {
        await _shippingService.DeleteZoneAsync(id, ct);
        return Ok();
    }

    /// <summary>
    /// Gets rates for a zone.
    /// </summary>
    [HttpGet("zone/{id:guid}/rates")]
    public async Task<IActionResult> GetZoneRates(Guid id, CancellationToken ct = default)
    {
        var rates = await _shippingService.GetRatesForZoneAsync(id, ct);
        return Ok(new { items = rates.Select(MapRateToResponse) });
    }

    /// <summary>
    /// Sets a zone as default.
    /// </summary>
    [HttpPost("zone/{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultZone(Guid id, CancellationToken ct = default)
    {
        await _shippingService.SetDefaultZoneAsync(id, ct);
        return Ok();
    }

    /// <summary>
    /// Duplicates a shipping zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateZone(Guid id, CancellationToken ct = default)
    {
        var zone = await _shippingService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        var duplicate = new ShippingZone
        {
            Name = $"{zone.Name} (Copy)",
            Description = zone.Description,
            Code = $"{zone.Code}-copy-{DateTime.UtcNow.Ticks}",
            IsActive = false, // Start inactive
            IsDefault = false, // Don't copy default status
            SortOrder = zone.SortOrder + 1,
            Countries = zone.Countries.ToList(),
            States = zone.States.ToList(),
            PostalCodePatterns = zone.PostalCodePatterns.ToList(),
            Cities = zone.Cities.ToList(),
            ExcludedCountries = zone.ExcludedCountries.ToList(),
            ExcludedStates = zone.ExcludedStates.ToList(),
            ExcludedPostalCodes = zone.ExcludedPostalCodes.ToList()
        };

        var created = await _shippingService.CreateZoneAsync(duplicate, ct);
        return Ok(MapZoneToResponse(created));
    }

    /// <summary>
    /// Toggles the active status of a shipping zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleZoneStatus(Guid id, CancellationToken ct = default)
    {
        var zone = await _shippingService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.IsActive = !zone.IsActive;
        var updated = await _shippingService.UpdateZoneAsync(zone, ct);
        return Ok(MapZoneToResponse(updated));
    }

    /// <summary>
    /// Updates the sort order of a shipping zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateZoneSort(Guid id, [FromBody] ShippingSortOrderRequest request, CancellationToken ct = default)
    {
        var zone = await _shippingService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.SortOrder = request.SortOrder;
        var updated = await _shippingService.UpdateZoneAsync(zone, ct);
        return Ok(MapZoneToResponse(updated));
    }

    /// <summary>
    /// Clears all geographic regions from a shipping zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/clear-regions")]
    public async Task<IActionResult> ClearZoneRegions(Guid id, CancellationToken ct = default)
    {
        var zone = await _shippingService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.Countries = [];
        zone.States = [];
        zone.PostalCodePatterns = [];
        zone.Cities = [];
        zone.ExcludedCountries = [];
        zone.ExcludedStates = [];
        zone.ExcludedPostalCodes = [];

        var updated = await _shippingService.UpdateZoneAsync(zone, ct);
        return Ok(MapZoneToResponse(updated));
    }

    #endregion

    #region Shipping Rates

    /// <summary>
    /// Gets a shipping rate by ID.
    /// </summary>
    [HttpGet("rate/{id:guid}")]
    public async Task<IActionResult> GetRate(Guid id, CancellationToken ct = default)
    {
        var rate = await _shippingService.GetRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        return Ok(MapRateToResponse(rate));
    }

    /// <summary>
    /// Creates a new shipping rate.
    /// </summary>
    [HttpPost("rate")]
    public async Task<IActionResult> CreateRate([FromBody] CreateShippingRateRequest request, CancellationToken ct = default)
    {
        var rate = new ShippingRate
        {
            ShippingZoneId = request.ShippingZoneId,
            ShippingMethodId = request.ShippingMethodId,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            BaseRate = request.BaseRate,
            PerWeightRate = request.PerWeightRate,
            PerItemRate = request.PerItemRate,
            PercentageRate = request.PercentageRate,
            HandlingFee = request.HandlingFee,
            MinWeight = request.MinWeight,
            MaxWeight = request.MaxWeight,
            MinOrderAmount = request.MinOrderAmount,
            MaxOrderAmount = request.MaxOrderAmount,
            FreeShippingThreshold = request.FreeShippingThreshold,
            EstimatedDaysMin = request.EstimatedDaysMin,
            EstimatedDaysMax = request.EstimatedDaysMax
        };

        var created = await _shippingService.CreateRateAsync(rate, ct);
        return Ok(MapRateToResponse(created));
    }

    /// <summary>
    /// Updates a shipping rate.
    /// </summary>
    [HttpPut("rate/{id:guid}")]
    public async Task<IActionResult> UpdateRate(Guid id, [FromBody] UpdateShippingRateRequest request, CancellationToken ct = default)
    {
        var rate = await _shippingService.GetRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        rate.IsActive = request.IsActive;
        rate.SortOrder = request.SortOrder;
        rate.BaseRate = request.BaseRate;
        rate.PerWeightRate = request.PerWeightRate;
        rate.PerItemRate = request.PerItemRate;
        rate.PercentageRate = request.PercentageRate;
        rate.HandlingFee = request.HandlingFee;
        rate.MinWeight = request.MinWeight;
        rate.MaxWeight = request.MaxWeight;
        rate.MinOrderAmount = request.MinOrderAmount;
        rate.MaxOrderAmount = request.MaxOrderAmount;
        rate.FreeShippingThreshold = request.FreeShippingThreshold;
        rate.EstimatedDaysMin = request.EstimatedDaysMin;
        rate.EstimatedDaysMax = request.EstimatedDaysMax;

        var updated = await _shippingService.UpdateRateAsync(rate, ct);
        return Ok(MapRateToResponse(updated));
    }

    /// <summary>
    /// Patches a shipping rate (partial update).
    /// </summary>
    [HttpPatch("rate/{id:guid}")]
    public async Task<IActionResult> PatchRate(Guid id, [FromBody] Dictionary<string, object> updates, CancellationToken ct = default)
    {
        var rate = await _shippingService.GetRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        foreach (var (key, value) in updates)
        {
            switch (key.ToLower())
            {
                case "isactive":
                    rate.IsActive = Convert.ToBoolean(value);
                    break;
                case "baserate":
                    rate.BaseRate = Convert.ToDecimal(value);
                    break;
                case "handlingfee":
                    rate.HandlingFee = value == null ? null : Convert.ToDecimal(value);
                    break;
                case "freeshippingthreshold":
                    rate.FreeShippingThreshold = value == null ? null : Convert.ToDecimal(value);
                    break;
            }
        }

        var updated = await _shippingService.UpdateRateAsync(rate, ct);
        return Ok(MapRateToResponse(updated));
    }

    /// <summary>
    /// Deletes a shipping rate.
    /// </summary>
    [HttpDelete("rate/{id:guid}")]
    public async Task<IActionResult> DeleteRate(Guid id, CancellationToken ct = default)
    {
        await _shippingService.DeleteRateAsync(id, ct);
        return Ok();
    }

    #endregion

    #region Mapping Helpers

    private static object MapMethodToResponse(ShippingMethod method) => new
    {
        method.Id,
        method.Name,
        method.Description,
        method.Code,
        CalculationType = method.CalculationType.ToString(),
        method.IsActive,
        method.SortOrder,
        method.FlatRate,
        method.WeightBaseRate,
        method.WeightPerUnitRate,
        method.WeightUnit,
        method.PricePercentage,
        method.MinimumCost,
        method.MaximumCost,
        method.PerItemRate,
        method.HandlingFee,
        method.FreeShippingThreshold,
        method.FreeShippingRequiresCoupon,
        method.MinWeight,
        method.MaxWeight,
        method.MinOrderAmount,
        method.MaxOrderAmount,
        method.EstimatedDaysMin,
        method.EstimatedDaysMax,
        method.DeliveryEstimateText,
        method.CarrierProviderId,
        method.CarrierServiceCode,
        method.UseCarrierRates,
        method.IconName,
        method.IsTaxable,
        method.CreatedAt,
        method.UpdatedAt
    };

    private static object MapZoneToResponse(ShippingZone zone) => new
    {
        zone.Id,
        zone.Name,
        zone.Description,
        zone.Code,
        zone.IsActive,
        zone.IsDefault,
        zone.SortOrder,
        zone.Countries,
        zone.States,
        zone.PostalCodePatterns,
        zone.Cities,
        zone.ExcludedCountries,
        zone.ExcludedStates,
        zone.ExcludedPostalCodes,
        zone.RegionSummary,
        zone.CreatedAt,
        zone.UpdatedAt
    };

    private static object MapRateToResponse(ShippingRate rate) => new
    {
        rate.Id,
        rate.ShippingZoneId,
        rate.ShippingMethodId,
        rate.IsActive,
        rate.SortOrder,
        rate.BaseRate,
        rate.PerWeightRate,
        rate.PerItemRate,
        rate.PercentageRate,
        rate.HandlingFee,
        rate.MinWeight,
        rate.MaxWeight,
        rate.MinOrderAmount,
        rate.MaxOrderAmount,
        rate.FreeShippingThreshold,
        rate.EstimatedDaysMin,
        rate.EstimatedDaysMax,
        ZoneName = rate.Zone?.Name,
        MethodName = rate.Method?.Name,
        rate.CreatedAt,
        rate.UpdatedAt
    };

    #endregion
}

#region Request Models

public class CreateShippingMethodRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? CalculationType { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public decimal? FlatRate { get; set; }
    public decimal? WeightBaseRate { get; set; }
    public decimal? WeightPerUnitRate { get; set; }
    public string? WeightUnit { get; set; }
    public decimal? PricePercentage { get; set; }
    public decimal? MinimumCost { get; set; }
    public decimal? MaximumCost { get; set; }
    public decimal? PerItemRate { get; set; }
    public decimal? HandlingFee { get; set; }
    public decimal? FreeShippingThreshold { get; set; }
    public bool FreeShippingRequiresCoupon { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public string? DeliveryEstimateText { get; set; }
    public string? CarrierProviderId { get; set; }
    public string? CarrierServiceCode { get; set; }
    public bool UseCarrierRates { get; set; }
    public string? IconName { get; set; }
    public bool IsTaxable { get; set; } = true;
}

public class UpdateShippingMethodRequest : CreateShippingMethodRequest { }

public class CreateShippingZoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public List<string>? Countries { get; set; }
    public List<string>? States { get; set; }
    public List<string>? PostalCodePatterns { get; set; }
    public List<string>? Cities { get; set; }
    public List<string>? ExcludedCountries { get; set; }
    public List<string>? ExcludedStates { get; set; }
    public List<string>? ExcludedPostalCodes { get; set; }
}

public class UpdateShippingZoneRequest : CreateShippingZoneRequest { }

public class CreateShippingRateRequest
{
    public Guid ShippingZoneId { get; set; }
    public Guid ShippingMethodId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public decimal BaseRate { get; set; }
    public decimal? PerWeightRate { get; set; }
    public decimal? PerItemRate { get; set; }
    public decimal? PercentageRate { get; set; }
    public decimal? HandlingFee { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }
    public decimal? FreeShippingThreshold { get; set; }
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
}

public class UpdateShippingRateRequest : CreateShippingRateRequest { }

public class ShippingSortOrderRequest
{
    public int SortOrder { get; set; }
}

public class ShippingDeliveryEstimateRequest
{
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public string? DeliveryEstimateText { get; set; }
}

public class ShippingFreeThresholdRequest
{
    public decimal? Threshold { get; set; }
}

#endregion
