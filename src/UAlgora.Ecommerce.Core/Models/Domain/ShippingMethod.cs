namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a shipping method available for checkout.
/// </summary>
public class ShippingMethod : SoftDeleteEntity
{
    /// <summary>
    /// Shipping method name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Method description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unique code for this method.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Calculation type for shipping cost.
    /// </summary>
    public ShippingCalculationType CalculationType { get; set; } = ShippingCalculationType.FlatRate;

    /// <summary>
    /// Whether this method is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    #region Flat Rate Settings

    /// <summary>
    /// Flat rate cost (when CalculationType is FlatRate).
    /// </summary>
    public decimal? FlatRate { get; set; }

    #endregion

    #region Weight-Based Settings

    /// <summary>
    /// Base rate for weight-based calculation.
    /// </summary>
    public decimal? WeightBaseRate { get; set; }

    /// <summary>
    /// Rate per weight unit.
    /// </summary>
    public decimal? WeightPerUnitRate { get; set; }

    /// <summary>
    /// Weight unit (kg, lb).
    /// </summary>
    public string WeightUnit { get; set; } = "kg";

    #endregion

    #region Price-Based Settings

    /// <summary>
    /// Percentage of order total (when CalculationType is PriceBased).
    /// </summary>
    public decimal? PricePercentage { get; set; }

    /// <summary>
    /// Minimum shipping cost for percentage-based.
    /// </summary>
    public decimal? MinimumCost { get; set; }

    /// <summary>
    /// Maximum shipping cost for percentage-based.
    /// </summary>
    public decimal? MaximumCost { get; set; }

    #endregion

    #region Per Item Settings

    /// <summary>
    /// Cost per item (when CalculationType is PerItem).
    /// </summary>
    public decimal? PerItemRate { get; set; }

    /// <summary>
    /// Base handling fee added to per-item cost.
    /// </summary>
    public decimal? HandlingFee { get; set; }

    #endregion

    #region Free Shipping Settings

    /// <summary>
    /// Minimum order amount for free shipping.
    /// </summary>
    public decimal? FreeShippingThreshold { get; set; }

    /// <summary>
    /// Whether free shipping requires a coupon.
    /// </summary>
    public bool FreeShippingRequiresCoupon { get; set; }

    #endregion

    #region Restrictions

    /// <summary>
    /// Minimum order weight allowed.
    /// </summary>
    public decimal? MinWeight { get; set; }

    /// <summary>
    /// Maximum order weight allowed.
    /// </summary>
    public decimal? MaxWeight { get; set; }

    /// <summary>
    /// Minimum order amount allowed.
    /// </summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Maximum order amount allowed.
    /// </summary>
    public decimal? MaxOrderAmount { get; set; }

    #endregion

    #region Delivery Estimate

    /// <summary>
    /// Minimum delivery days.
    /// </summary>
    public int? EstimatedDaysMin { get; set; }

    /// <summary>
    /// Maximum delivery days.
    /// </summary>
    public int? EstimatedDaysMax { get; set; }

    /// <summary>
    /// Delivery description (e.g., "2-5 business days").
    /// </summary>
    public string? DeliveryEstimateText { get; set; }

    #endregion

    #region Carrier Integration

    /// <summary>
    /// Carrier provider ID for real-time rates.
    /// </summary>
    public string? CarrierProviderId { get; set; }

    /// <summary>
    /// Carrier service code (e.g., "ground", "express").
    /// </summary>
    public string? CarrierServiceCode { get; set; }

    /// <summary>
    /// Whether to fetch rates from carrier API.
    /// </summary>
    public bool UseCarrierRates { get; set; }

    #endregion

    #region Display Settings

    /// <summary>
    /// Icon name for display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Image URL for display.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Tax class for shipping.
    /// </summary>
    public string? TaxClass { get; set; }

    /// <summary>
    /// Whether shipping is taxable.
    /// </summary>
    public bool IsTaxable { get; set; } = true;

    #endregion

    /// <summary>
    /// Zones where this method is available.
    /// </summary>
    public List<ShippingZone> Zones { get; set; } = [];

    /// <summary>
    /// Rates for this method across zones.
    /// </summary>
    public List<ShippingRate> Rates { get; set; } = [];

    #region Computed Properties

    /// <summary>
    /// Gets the display name with delivery estimate.
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(DeliveryEstimateText)
        ? $"{Name} ({DeliveryEstimateText})"
        : Name;

    /// <summary>
    /// Whether this method uses carrier integration.
    /// </summary>
    public bool HasCarrierIntegration => UseCarrierRates && !string.IsNullOrEmpty(CarrierProviderId);

    #endregion
}

/// <summary>
/// Shipping calculation type.
/// </summary>
public enum ShippingCalculationType
{
    /// <summary>
    /// Fixed flat rate.
    /// </summary>
    FlatRate = 0,

    /// <summary>
    /// Free shipping.
    /// </summary>
    FreeShipping = 1,

    /// <summary>
    /// Based on order weight.
    /// </summary>
    WeightBased = 2,

    /// <summary>
    /// Based on order price.
    /// </summary>
    PriceBased = 3,

    /// <summary>
    /// Per item rate.
    /// </summary>
    PerItem = 4,

    /// <summary>
    /// Real-time carrier rates.
    /// </summary>
    CarrierCalculated = 5
}
