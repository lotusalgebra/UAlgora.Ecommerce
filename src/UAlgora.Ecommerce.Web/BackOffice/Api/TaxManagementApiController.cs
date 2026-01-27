using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for tax management in the backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/tax")]
public class TaxManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ITaxService _taxService;

    public TaxManagementApiController(ITaxService taxService)
    {
        _taxService = taxService;
    }

    #region Tax Categories

    /// <summary>
    /// Gets all tax categories.
    /// </summary>
    [HttpGet("category")]
    public async Task<IActionResult> GetCategories([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var categories = await _taxService.GetAllCategoriesAsync(includeInactive, ct);
        return Ok(new TaxCategoryListResponse { Items = categories.ToList() });
    }

    /// <summary>
    /// Gets a tax category by ID.
    /// </summary>
    [HttpGet("category/{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    /// <summary>
    /// Creates a new tax category.
    /// </summary>
    [HttpPost("category")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateTaxCategoryRequest request, CancellationToken ct = default)
    {
        var category = new TaxCategory
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            IsTaxExempt = request.IsTaxExempt,
            ExternalTaxCode = request.ExternalTaxCode,
            SortOrder = request.SortOrder,
            // GST fields
            IsGst = request.IsGst,
            GstType = request.GstType,
            CgstRate = request.CgstRate,
            SgstRate = request.SgstRate,
            IgstRate = request.IgstRate,
            HsnCode = request.HsnCode,
            SacCode = request.SacCode
        };

        var validation = await _taxService.ValidateCategoryAsync(category, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var created = await _taxService.CreateCategoryAsync(category, ct);
        return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a tax category.
    /// </summary>
    [HttpPut("category/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateTaxCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = request.Name;
        category.Code = request.Code;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.IsDefault = request.IsDefault;
        category.IsTaxExempt = request.IsTaxExempt;
        category.ExternalTaxCode = request.ExternalTaxCode;
        category.SortOrder = request.SortOrder;
        // GST fields
        category.IsGst = request.IsGst;
        category.GstType = request.GstType;
        category.CgstRate = request.CgstRate;
        category.SgstRate = request.SgstRate;
        category.IgstRate = request.IgstRate;
        category.HsnCode = request.HsnCode;
        category.SacCode = request.SacCode;

        var validation = await _taxService.ValidateCategoryAsync(category, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var updated = await _taxService.UpdateCategoryAsync(category, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a tax category.
    /// </summary>
    [HttpDelete("category/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }

        await _taxService.DeleteCategoryAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Gets rates for a category.
    /// </summary>
    [HttpGet("category/{id:guid}/rates")]
    public async Task<IActionResult> GetCategoryRates(Guid id, CancellationToken ct = default)
    {
        var rates = await _taxService.GetRatesForCategoryAsync(id, ct);
        return Ok(new TaxRateListResponse { Items = rates.ToList() });
    }

    /// <summary>
    /// Sets a category as default.
    /// </summary>
    [HttpPost("category/{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultCategory(Guid id, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }

        await _taxService.SetDefaultCategoryAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Duplicates a tax category.
    /// </summary>
    [HttpPost("category/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateCategory(Guid id, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }

        var duplicate = new TaxCategory
        {
            Name = $"{category.Name} (Copy)",
            Code = $"{category.Code}-copy-{DateTime.UtcNow.Ticks}",
            Description = category.Description,
            IsActive = false, // Start inactive
            IsDefault = false, // Don't copy default status
            IsTaxExempt = category.IsTaxExempt,
            ExternalTaxCode = category.ExternalTaxCode,
            SortOrder = category.SortOrder + 1,
            // Copy GST fields
            IsGst = category.IsGst,
            GstType = category.GstType,
            CgstRate = category.CgstRate,
            SgstRate = category.SgstRate,
            IgstRate = category.IgstRate,
            HsnCode = category.HsnCode,
            SacCode = category.SacCode
        };

        var created = await _taxService.CreateCategoryAsync(duplicate, ct);
        return Ok(created);
    }

    /// <summary>
    /// Toggles the active status of a tax category.
    /// </summary>
    [HttpPost("category/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleCategoryStatus(Guid id, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }

        category.IsActive = !category.IsActive;
        var updated = await _taxService.UpdateCategoryAsync(category, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the sort order of a tax category.
    /// </summary>
    [HttpPost("category/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateCategorySort(Guid id, [FromBody] TaxSortOrderRequest request, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }

        category.SortOrder = request.SortOrder;
        var updated = await _taxService.UpdateCategoryAsync(category, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Toggles the tax exempt status of a category.
    /// </summary>
    [HttpPost("category/{id:guid}/toggle-exempt")]
    public async Task<IActionResult> ToggleCategoryExempt(Guid id, CancellationToken ct = default)
    {
        var category = await _taxService.GetCategoryByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound();
        }

        category.IsTaxExempt = !category.IsTaxExempt;
        var updated = await _taxService.UpdateCategoryAsync(category, ct);
        return Ok(updated);
    }

    #endregion

    #region Tax Zones

    /// <summary>
    /// Gets all tax zones.
    /// </summary>
    [HttpGet("zone")]
    public async Task<IActionResult> GetZones([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var zones = await _taxService.GetAllZonesAsync(includeInactive, ct);
        return Ok(new TaxZoneListResponse { Items = zones.ToList() });
    }

    /// <summary>
    /// Gets a tax zone by ID.
    /// </summary>
    [HttpGet("zone/{id:guid}")]
    public async Task<IActionResult> GetZone(Guid id, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }
        return Ok(zone);
    }

    /// <summary>
    /// Creates a new tax zone.
    /// </summary>
    [HttpPost("zone")]
    public async Task<IActionResult> CreateZone([FromBody] CreateTaxZoneRequest request, CancellationToken ct = default)
    {
        var zone = new TaxZone
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            Priority = request.Priority,
            SortOrder = request.SortOrder,
            Countries = request.Countries ?? [],
            States = request.States ?? [],
            PostalCodePatterns = request.PostalCodePatterns ?? [],
            Cities = request.Cities ?? [],
            ExcludedCountries = request.ExcludedCountries ?? [],
            ExcludedStates = request.ExcludedStates ?? [],
            ExcludedPostalCodes = request.ExcludedPostalCodes ?? []
        };

        var validation = await _taxService.ValidateZoneAsync(zone, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var created = await _taxService.CreateZoneAsync(zone, ct);
        return CreatedAtAction(nameof(GetZone), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a tax zone.
    /// </summary>
    [HttpPut("zone/{id:guid}")]
    public async Task<IActionResult> UpdateZone(Guid id, [FromBody] UpdateTaxZoneRequest request, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.Name = request.Name;
        zone.Code = request.Code;
        zone.Description = request.Description;
        zone.IsActive = request.IsActive;
        zone.IsDefault = request.IsDefault;
        zone.Priority = request.Priority;
        zone.SortOrder = request.SortOrder;
        zone.Countries = request.Countries ?? [];
        zone.States = request.States ?? [];
        zone.PostalCodePatterns = request.PostalCodePatterns ?? [];
        zone.Cities = request.Cities ?? [];
        zone.ExcludedCountries = request.ExcludedCountries ?? [];
        zone.ExcludedStates = request.ExcludedStates ?? [];
        zone.ExcludedPostalCodes = request.ExcludedPostalCodes ?? [];

        var validation = await _taxService.ValidateZoneAsync(zone, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var updated = await _taxService.UpdateZoneAsync(zone, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a tax zone.
    /// </summary>
    [HttpDelete("zone/{id:guid}")]
    public async Task<IActionResult> DeleteZone(Guid id, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        await _taxService.DeleteZoneAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Gets rates for a zone.
    /// </summary>
    [HttpGet("zone/{id:guid}/rates")]
    public async Task<IActionResult> GetZoneRates(Guid id, CancellationToken ct = default)
    {
        var rates = await _taxService.GetRatesForZoneAsync(id, ct);
        return Ok(new TaxRateListResponse { Items = rates.ToList() });
    }

    /// <summary>
    /// Sets a zone as default.
    /// </summary>
    [HttpPost("zone/{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultZone(Guid id, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        await _taxService.SetDefaultZoneAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Duplicates a tax zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateZone(Guid id, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        var duplicate = new TaxZone
        {
            Name = $"{zone.Name} (Copy)",
            Code = $"{zone.Code}-copy-{DateTime.UtcNow.Ticks}",
            Description = zone.Description,
            IsActive = false, // Start inactive
            IsDefault = false, // Don't copy default status
            Priority = zone.Priority,
            SortOrder = zone.SortOrder + 1,
            Countries = zone.Countries.ToList(),
            States = zone.States.ToList(),
            PostalCodePatterns = zone.PostalCodePatterns.ToList(),
            Cities = zone.Cities.ToList(),
            ExcludedCountries = zone.ExcludedCountries.ToList(),
            ExcludedStates = zone.ExcludedStates.ToList(),
            ExcludedPostalCodes = zone.ExcludedPostalCodes.ToList()
        };

        var created = await _taxService.CreateZoneAsync(duplicate, ct);
        return Ok(created);
    }

    /// <summary>
    /// Toggles the active status of a tax zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleZoneStatus(Guid id, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.IsActive = !zone.IsActive;
        var updated = await _taxService.UpdateZoneAsync(zone, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the sort order of a tax zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateZoneSort(Guid id, [FromBody] TaxSortOrderRequest request, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.SortOrder = request.SortOrder;
        var updated = await _taxService.UpdateZoneAsync(zone, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the priority of a tax zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/update-priority")]
    public async Task<IActionResult> UpdateZonePriority(Guid id, [FromBody] TaxPriorityRequest request, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
        if (zone == null)
        {
            return NotFound();
        }

        zone.Priority = request.Priority;
        var updated = await _taxService.UpdateZoneAsync(zone, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Clears all geographic regions from a tax zone.
    /// </summary>
    [HttpPost("zone/{id:guid}/clear-regions")]
    public async Task<IActionResult> ClearZoneRegions(Guid id, CancellationToken ct = default)
    {
        var zone = await _taxService.GetZoneByIdAsync(id, ct);
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

        var updated = await _taxService.UpdateZoneAsync(zone, ct);
        return Ok(updated);
    }

    #endregion

    #region Tax Rates

    /// <summary>
    /// Gets all tax rates.
    /// </summary>
    [HttpGet("rate")]
    public async Task<IActionResult> GetRates([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var rates = await _taxService.GetAllRatesAsync(includeInactive, ct);
        return Ok(new TaxRateListResponse { Items = rates.ToList() });
    }

    /// <summary>
    /// Gets a tax rate by ID.
    /// </summary>
    [HttpGet("rate/{id:guid}")]
    public async Task<IActionResult> GetRate(Guid id, CancellationToken ct = default)
    {
        var rate = await _taxService.GetRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }
        return Ok(rate);
    }

    /// <summary>
    /// Creates a new tax rate.
    /// </summary>
    [HttpPost("rate")]
    public async Task<IActionResult> CreateRate([FromBody] CreateTaxRateRequest request, CancellationToken ct = default)
    {
        var rate = new TaxRate
        {
            Name = request.Name,
            TaxZoneId = request.TaxZoneId,
            TaxCategoryId = request.TaxCategoryId,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            Priority = request.Priority,
            RateType = Enum.TryParse<TaxRateType>(request.RateType, out var rateType) ? rateType : TaxRateType.Percentage,
            Rate = request.Rate,
            FlatAmount = request.FlatAmount,
            IsCompound = request.IsCompound,
            TaxShipping = request.TaxShipping,
            MinimumAmount = request.MinimumAmount,
            MaximumAmount = request.MaximumAmount,
            MaximumTax = request.MaximumTax,
            JurisdictionType = request.JurisdictionType,
            JurisdictionName = request.JurisdictionName,
            JurisdictionCode = request.JurisdictionCode,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        var validation = await _taxService.ValidateRateAsync(rate, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var created = await _taxService.CreateRateAsync(rate, ct);
        return CreatedAtAction(nameof(GetRate), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a tax rate.
    /// </summary>
    [HttpPut("rate/{id:guid}")]
    public async Task<IActionResult> UpdateRate(Guid id, [FromBody] UpdateTaxRateRequest request, CancellationToken ct = default)
    {
        var rate = await _taxService.GetRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        rate.Name = request.Name;
        rate.TaxZoneId = request.TaxZoneId;
        rate.TaxCategoryId = request.TaxCategoryId;
        rate.IsActive = request.IsActive;
        rate.SortOrder = request.SortOrder;
        rate.Priority = request.Priority;
        rate.RateType = Enum.TryParse<TaxRateType>(request.RateType, out var rateType) ? rateType : TaxRateType.Percentage;
        rate.Rate = request.Rate;
        rate.FlatAmount = request.FlatAmount;
        rate.IsCompound = request.IsCompound;
        rate.TaxShipping = request.TaxShipping;
        rate.MinimumAmount = request.MinimumAmount;
        rate.MaximumAmount = request.MaximumAmount;
        rate.MaximumTax = request.MaximumTax;
        rate.JurisdictionType = request.JurisdictionType;
        rate.JurisdictionName = request.JurisdictionName;
        rate.JurisdictionCode = request.JurisdictionCode;
        rate.EffectiveFrom = request.EffectiveFrom;
        rate.EffectiveTo = request.EffectiveTo;

        var validation = await _taxService.ValidateRateAsync(rate, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var updated = await _taxService.UpdateRateAsync(rate, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a tax rate.
    /// </summary>
    [HttpDelete("rate/{id:guid}")]
    public async Task<IActionResult> DeleteRate(Guid id, CancellationToken ct = default)
    {
        var rate = await _taxService.GetRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        await _taxService.DeleteRateAsync(id, ct);
        return NoContent();
    }

    #endregion
}

#region Request/Response Models

public class TaxCategoryListResponse
{
    public List<TaxCategory> Items { get; set; } = [];
}

public class TaxZoneListResponse
{
    public List<TaxZone> Items { get; set; } = [];
}

public class TaxRateListResponse
{
    public List<TaxRate> Items { get; set; } = [];
}

public class CreateTaxCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool IsTaxExempt { get; set; }
    public string? ExternalTaxCode { get; set; }
    public int SortOrder { get; set; }

    // GST fields
    public bool IsGst { get; set; }
    public string? GstType { get; set; }
    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }
    public string? HsnCode { get; set; }
    public string? SacCode { get; set; }
}

public class UpdateTaxCategoryRequest : CreateTaxCategoryRequest { }

public class CreateTaxZoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public int SortOrder { get; set; }
    public List<string>? Countries { get; set; }
    public List<string>? States { get; set; }
    public List<string>? PostalCodePatterns { get; set; }
    public List<string>? Cities { get; set; }
    public List<string>? ExcludedCountries { get; set; }
    public List<string>? ExcludedStates { get; set; }
    public List<string>? ExcludedPostalCodes { get; set; }
}

public class UpdateTaxZoneRequest : CreateTaxZoneRequest { }

public class CreateTaxRateRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid TaxZoneId { get; set; }
    public Guid TaxCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int Priority { get; set; }
    public string RateType { get; set; } = "Percentage";
    public decimal Rate { get; set; }
    public decimal? FlatAmount { get; set; }
    public bool IsCompound { get; set; }
    public bool TaxShipping { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public decimal? MaximumTax { get; set; }
    public string? JurisdictionType { get; set; }
    public string? JurisdictionName { get; set; }
    public string? JurisdictionCode { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class UpdateTaxRateRequest : CreateTaxRateRequest { }

public class TaxSortOrderRequest
{
    public int SortOrder { get; set; }
}

public class TaxPriorityRequest
{
    public int Priority { get; set; }
}

#endregion
