using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for tax configuration and calculation operations.
/// </summary>
public interface ITaxService
{
    #region Tax Categories

    /// <summary>
    /// Gets a tax category by ID.
    /// </summary>
    Task<TaxCategory?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a tax category by code.
    /// </summary>
    Task<TaxCategory?> GetCategoryByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all tax categories.
    /// </summary>
    Task<IReadOnlyList<TaxCategory>> GetAllCategoriesAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets active tax categories.
    /// </summary>
    Task<IReadOnlyList<TaxCategory>> GetActiveCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the default tax category.
    /// </summary>
    Task<TaxCategory?> GetDefaultCategoryAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new tax category.
    /// </summary>
    Task<TaxCategory> CreateCategoryAsync(TaxCategory category, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing tax category.
    /// </summary>
    Task<TaxCategory> UpdateCategoryAsync(TaxCategory category, CancellationToken ct = default);

    /// <summary>
    /// Deletes a tax category.
    /// </summary>
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Sets a category as the default.
    /// </summary>
    Task SetDefaultCategoryAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates category sort orders.
    /// </summary>
    Task UpdateCategorySortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);

    /// <summary>
    /// Toggles category active status.
    /// </summary>
    Task<TaxCategory> ToggleCategoryStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Tax Zones

    /// <summary>
    /// Gets a tax zone by ID.
    /// </summary>
    Task<TaxZone?> GetZoneByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a tax zone by code.
    /// </summary>
    Task<TaxZone?> GetZoneByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all tax zones.
    /// </summary>
    Task<IReadOnlyList<TaxZone>> GetAllZonesAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets active tax zones.
    /// </summary>
    Task<IReadOnlyList<TaxZone>> GetActiveZonesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the default zone.
    /// </summary>
    Task<TaxZone?> GetDefaultZoneAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets zones matching an address (ordered by priority).
    /// </summary>
    Task<IReadOnlyList<TaxZone>> GetZonesForAddressAsync(Address address, CancellationToken ct = default);

    /// <summary>
    /// Creates a new tax zone.
    /// </summary>
    Task<TaxZone> CreateZoneAsync(TaxZone zone, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing tax zone.
    /// </summary>
    Task<TaxZone> UpdateZoneAsync(TaxZone zone, CancellationToken ct = default);

    /// <summary>
    /// Deletes a tax zone.
    /// </summary>
    Task DeleteZoneAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Sets a zone as the default.
    /// </summary>
    Task SetDefaultZoneAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates zone sort orders.
    /// </summary>
    Task UpdateZoneSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);

    /// <summary>
    /// Toggles zone active status.
    /// </summary>
    Task<TaxZone> ToggleZoneStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Tax Rates

    /// <summary>
    /// Gets a tax rate by ID.
    /// </summary>
    Task<TaxRate?> GetRateByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets rates for a zone.
    /// </summary>
    Task<IReadOnlyList<TaxRate>> GetRatesForZoneAsync(Guid zoneId, CancellationToken ct = default);

    /// <summary>
    /// Gets rates for a category.
    /// </summary>
    Task<IReadOnlyList<TaxRate>> GetRatesForCategoryAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Gets rate for a specific zone and category.
    /// </summary>
    Task<TaxRate?> GetRateAsync(Guid zoneId, Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Gets all tax rates.
    /// </summary>
    Task<IReadOnlyList<TaxRate>> GetAllRatesAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Creates a new tax rate.
    /// </summary>
    Task<TaxRate> CreateRateAsync(TaxRate rate, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing tax rate.
    /// </summary>
    Task<TaxRate> UpdateRateAsync(TaxRate rate, CancellationToken ct = default);

    /// <summary>
    /// Deletes a tax rate.
    /// </summary>
    Task DeleteRateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Bulk creates rates for a zone across all categories.
    /// </summary>
    Task<IReadOnlyList<TaxRate>> CreateRatesForZoneAsync(Guid zoneId, decimal defaultRate, CancellationToken ct = default);

    /// <summary>
    /// Toggles rate active status.
    /// </summary>
    Task<TaxRate> ToggleRateStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Tax Calculation

    /// <summary>
    /// Calculates tax for an order line.
    /// </summary>
    Task<TaxCalculationResult> CalculateTaxAsync(
        TaxCalculationContext context,
        CancellationToken ct = default);

    /// <summary>
    /// Calculates tax for multiple items.
    /// </summary>
    Task<TaxSummary> CalculateOrderTaxAsync(
        Address address,
        IEnumerable<TaxableItem> items,
        decimal shippingAmount = 0,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the effective tax rate for an address and category.
    /// </summary>
    Task<decimal> GetEffectiveTaxRateAsync(
        Address address,
        string? taxClass,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if an address is taxable.
    /// </summary>
    Task<bool> IsTaxableAddressAsync(Address address, CancellationToken ct = default);

    #endregion

    #region Validation

    /// <summary>
    /// Validates a tax category.
    /// </summary>
    Task<ValidationResult> ValidateCategoryAsync(TaxCategory category, CancellationToken ct = default);

    /// <summary>
    /// Validates a tax zone.
    /// </summary>
    Task<ValidationResult> ValidateZoneAsync(TaxZone zone, CancellationToken ct = default);

    /// <summary>
    /// Validates a tax rate.
    /// </summary>
    Task<ValidationResult> ValidateRateAsync(TaxRate rate, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Context for tax calculation.
/// </summary>
public class TaxCalculationContext
{
    /// <summary>
    /// Address for tax jurisdiction determination.
    /// </summary>
    public Address Address { get; set; } = new();

    /// <summary>
    /// Taxable amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tax class/category code.
    /// </summary>
    public string? TaxClass { get; set; }

    /// <summary>
    /// Whether shipping is included.
    /// </summary>
    public bool IncludesShipping { get; set; }

    /// <summary>
    /// Shipping amount if applicable.
    /// </summary>
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Customer tax exemption number.
    /// </summary>
    public string? ExemptionNumber { get; set; }

    /// <summary>
    /// Whether customer is tax exempt.
    /// </summary>
    public bool IsTaxExempt { get; set; }
}

/// <summary>
/// Result of tax calculation.
/// </summary>
public class TaxCalculationResult
{
    /// <summary>
    /// Whether calculation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Total tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Total taxable amount.
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Exempt amount.
    /// </summary>
    public decimal ExemptAmount { get; set; }

    /// <summary>
    /// Effective combined tax rate.
    /// </summary>
    public decimal EffectiveRate { get; set; }

    /// <summary>
    /// Breakdown by jurisdiction.
    /// </summary>
    public List<TaxBreakdown> Breakdown { get; set; } = [];

    /// <summary>
    /// Whether product is tax exempt.
    /// </summary>
    public bool IsExempt { get; set; }

    /// <summary>
    /// Reason for exemption if applicable.
    /// </summary>
    public string? ExemptionReason { get; set; }

    /// <summary>
    /// Error message if calculation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tax breakdown by jurisdiction or rate.
/// </summary>
public class TaxBreakdown
{
    /// <summary>
    /// Jurisdiction type.
    /// </summary>
    public string JurisdictionType { get; set; } = string.Empty;

    /// <summary>
    /// Jurisdiction name.
    /// </summary>
    public string JurisdictionName { get; set; } = string.Empty;

    /// <summary>
    /// Tax rate applied.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Whether this is a compound rate.
    /// </summary>
    public bool IsCompound { get; set; }
}

/// <summary>
/// Represents a taxable item.
/// </summary>
public class TaxableItem
{
    /// <summary>
    /// Item identifier.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Item amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Quantity.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Tax class/category.
    /// </summary>
    public string? TaxClass { get; set; }

    /// <summary>
    /// Whether item is tax exempt.
    /// </summary>
    public bool IsTaxExempt { get; set; }
}

/// <summary>
/// Summary of tax calculation for an order.
/// </summary>
public class TaxSummary
{
    /// <summary>
    /// Total tax amount.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Total taxable amount.
    /// </summary>
    public decimal TotalTaxable { get; set; }

    /// <summary>
    /// Total exempt amount.
    /// </summary>
    public decimal TotalExempt { get; set; }

    /// <summary>
    /// Tax on shipping.
    /// </summary>
    public decimal ShippingTax { get; set; }

    /// <summary>
    /// Breakdown by item.
    /// </summary>
    public List<TaxItemResult> ItemResults { get; set; } = [];

    /// <summary>
    /// Breakdown by jurisdiction.
    /// </summary>
    public List<TaxBreakdown> JurisdictionBreakdown { get; set; } = [];
}

/// <summary>
/// Tax calculation result per item.
/// </summary>
public class TaxItemResult
{
    /// <summary>
    /// Item identifier.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Taxable amount.
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Effective rate.
    /// </summary>
    public decimal EffectiveRate { get; set; }

    /// <summary>
    /// Whether item is exempt.
    /// </summary>
    public bool IsExempt { get; set; }
}
