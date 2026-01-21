namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a tax rate linking a category and zone with specific rate configuration.
/// </summary>
public class TaxRate : SoftDeleteEntity
{
    /// <summary>
    /// Rate name for display purposes.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tax zone this rate applies to.
    /// </summary>
    public Guid TaxZoneId { get; set; }

    /// <summary>
    /// Tax category this rate applies to.
    /// </summary>
    public Guid TaxCategoryId { get; set; }

    /// <summary>
    /// Whether this rate is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Priority when multiple rates apply.
    /// </summary>
    public int Priority { get; set; }

    #region Rate Configuration

    /// <summary>
    /// Type of tax rate.
    /// </summary>
    public TaxRateType RateType { get; set; } = TaxRateType.Percentage;

    /// <summary>
    /// Tax rate as a percentage (e.g., 8.25 for 8.25%).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Fixed tax amount (for flat rate type).
    /// </summary>
    public decimal? FlatAmount { get; set; }

    /// <summary>
    /// Whether this is a compound rate (applied after other taxes).
    /// </summary>
    public bool IsCompound { get; set; }

    /// <summary>
    /// Whether to include shipping in the taxable amount.
    /// </summary>
    public bool TaxShipping { get; set; }

    #endregion

    #region Thresholds

    /// <summary>
    /// Minimum taxable amount (no tax below this).
    /// </summary>
    public decimal? MinimumAmount { get; set; }

    /// <summary>
    /// Maximum taxable amount (no additional tax above this).
    /// </summary>
    public decimal? MaximumAmount { get; set; }

    /// <summary>
    /// Maximum tax amount cap.
    /// </summary>
    public decimal? MaximumTax { get; set; }

    #endregion

    #region Jurisdiction

    /// <summary>
    /// Jurisdiction type (e.g., Federal, State, County, City).
    /// </summary>
    public string? JurisdictionType { get; set; }

    /// <summary>
    /// Jurisdiction name.
    /// </summary>
    public string? JurisdictionName { get; set; }

    /// <summary>
    /// Jurisdiction code for reporting.
    /// </summary>
    public string? JurisdictionCode { get; set; }

    #endregion

    #region Date Restrictions

    /// <summary>
    /// Start date when this rate becomes effective.
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// End date when this rate expires.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    #endregion

    /// <summary>
    /// Navigation property to tax zone.
    /// </summary>
    public TaxZone? TaxZone { get; set; }

    /// <summary>
    /// Navigation property to tax category.
    /// </summary>
    public TaxCategory? TaxCategory { get; set; }

    #region Computed Properties

    /// <summary>
    /// Whether this rate is currently effective.
    /// </summary>
    public bool IsCurrentlyEffective
    {
        get
        {
            var now = DateTime.UtcNow;
            if (EffectiveFrom.HasValue && now < EffectiveFrom.Value)
                return false;
            if (EffectiveTo.HasValue && now > EffectiveTo.Value)
                return false;
            return true;
        }
    }

    /// <summary>
    /// Display rate as percentage string.
    /// </summary>
    public string RateDisplay => RateType == TaxRateType.Percentage
        ? $"{Rate:0.##}%"
        : $"{FlatAmount:C}";

    #endregion

    /// <summary>
    /// Calculates the tax amount for a given taxable amount.
    /// </summary>
    /// <param name="taxableAmount">The amount to calculate tax on.</param>
    /// <param name="previousTax">Previous tax amount (for compound calculations).</param>
    /// <returns>The calculated tax amount.</returns>
    public decimal CalculateTax(decimal taxableAmount, decimal previousTax = 0)
    {
        if (!IsActive || !IsCurrentlyEffective)
            return 0;

        // Apply minimum threshold
        if (MinimumAmount.HasValue && taxableAmount < MinimumAmount.Value)
            return 0;

        // Adjust for maximum threshold
        var effectiveAmount = taxableAmount;
        if (MaximumAmount.HasValue && effectiveAmount > MaximumAmount.Value)
            effectiveAmount = MaximumAmount.Value;

        // For compound rates, include previous tax in the base
        if (IsCompound)
            effectiveAmount += previousTax;

        // Calculate tax based on type
        decimal tax;
        if (RateType == TaxRateType.Percentage)
        {
            tax = effectiveAmount * (Rate / 100);
        }
        else if (RateType == TaxRateType.FlatRate)
        {
            tax = FlatAmount ?? 0;
        }
        else // PerUnit - would need quantity parameter
        {
            tax = FlatAmount ?? 0;
        }

        // Apply maximum tax cap
        if (MaximumTax.HasValue && tax > MaximumTax.Value)
            tax = MaximumTax.Value;

        return Math.Round(tax, 2);
    }

    /// <summary>
    /// Checks if this rate applies to the given amount.
    /// </summary>
    public bool AppliesTo(decimal amount)
    {
        if (!IsActive || !IsCurrentlyEffective)
            return false;

        if (MinimumAmount.HasValue && amount < MinimumAmount.Value)
            return false;

        return true;
    }
}

/// <summary>
/// Tax rate calculation type.
/// </summary>
public enum TaxRateType
{
    /// <summary>
    /// Percentage of the taxable amount.
    /// </summary>
    Percentage = 0,

    /// <summary>
    /// Fixed flat rate per order.
    /// </summary>
    FlatRate = 1,

    /// <summary>
    /// Fixed amount per unit.
    /// </summary>
    PerUnit = 2
}
