namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a shipping rate for a specific zone and method combination.
/// </summary>
public class ShippingRate : BaseEntity
{
    /// <summary>
    /// Shipping zone ID.
    /// </summary>
    public Guid ShippingZoneId { get; set; }

    /// <summary>
    /// Shipping method ID.
    /// </summary>
    public Guid ShippingMethodId { get; set; }

    /// <summary>
    /// Whether this rate is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    #region Rate Settings

    /// <summary>
    /// Base rate for this zone/method.
    /// </summary>
    public decimal BaseRate { get; set; }

    /// <summary>
    /// Rate per weight unit (overrides method setting).
    /// </summary>
    public decimal? PerWeightRate { get; set; }

    /// <summary>
    /// Rate per item (overrides method setting).
    /// </summary>
    public decimal? PerItemRate { get; set; }

    /// <summary>
    /// Percentage of order total (overrides method setting).
    /// </summary>
    public decimal? PercentageRate { get; set; }

    /// <summary>
    /// Handling fee for this zone.
    /// </summary>
    public decimal? HandlingFee { get; set; }

    #endregion

    #region Thresholds

    /// <summary>
    /// Minimum order weight for this rate.
    /// </summary>
    public decimal? MinWeight { get; set; }

    /// <summary>
    /// Maximum order weight for this rate.
    /// </summary>
    public decimal? MaxWeight { get; set; }

    /// <summary>
    /// Minimum order amount for this rate.
    /// </summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Maximum order amount for this rate.
    /// </summary>
    public decimal? MaxOrderAmount { get; set; }

    /// <summary>
    /// Order amount threshold for free shipping.
    /// </summary>
    public decimal? FreeShippingThreshold { get; set; }

    #endregion

    #region Delivery Estimate

    /// <summary>
    /// Minimum delivery days (overrides method setting).
    /// </summary>
    public int? EstimatedDaysMin { get; set; }

    /// <summary>
    /// Maximum delivery days (overrides method setting).
    /// </summary>
    public int? EstimatedDaysMax { get; set; }

    #endregion

    /// <summary>
    /// Navigation property to zone.
    /// </summary>
    public ShippingZone? Zone { get; set; }

    /// <summary>
    /// Navigation property to method.
    /// </summary>
    public ShippingMethod? Method { get; set; }

    #region Computed Properties

    /// <summary>
    /// Gets the effective delivery estimate text.
    /// </summary>
    public string? DeliveryEstimateText
    {
        get
        {
            var min = EstimatedDaysMin ?? Method?.EstimatedDaysMin;
            var max = EstimatedDaysMax ?? Method?.EstimatedDaysMax;

            if (min.HasValue && max.HasValue)
            {
                if (min == max)
                    return $"{min} business day{(min == 1 ? "" : "s")}";
                return $"{min}-{max} business days";
            }
            if (min.HasValue)
                return $"{min}+ business days";
            if (max.HasValue)
                return $"Up to {max} business days";

            return Method?.DeliveryEstimateText;
        }
    }

    /// <summary>
    /// Gets display name combining method and zone.
    /// </summary>
    public string DisplayName => $"{Method?.Name} - {Zone?.Name}";

    #endregion

    /// <summary>
    /// Calculates the shipping cost for given order details.
    /// </summary>
    public decimal CalculateCost(decimal orderTotal, decimal orderWeight, int itemCount)
    {
        if (Method == null) return BaseRate;

        // Check for free shipping threshold
        if (FreeShippingThreshold.HasValue && orderTotal >= FreeShippingThreshold.Value)
            return 0;

        var cost = BaseRate;

        switch (Method.CalculationType)
        {
            case ShippingCalculationType.FlatRate:
                cost = BaseRate;
                break;

            case ShippingCalculationType.FreeShipping:
                cost = 0;
                break;

            case ShippingCalculationType.WeightBased:
                var perWeight = PerWeightRate ?? Method.WeightPerUnitRate ?? 0;
                cost = BaseRate + (orderWeight * perWeight);
                break;

            case ShippingCalculationType.PriceBased:
                var percentage = PercentageRate ?? Method.PricePercentage ?? 0;
                cost = orderTotal * (percentage / 100);
                if (Method.MinimumCost.HasValue && cost < Method.MinimumCost.Value)
                    cost = Method.MinimumCost.Value;
                if (Method.MaximumCost.HasValue && cost > Method.MaximumCost.Value)
                    cost = Method.MaximumCost.Value;
                break;

            case ShippingCalculationType.PerItem:
                var perItem = PerItemRate ?? Method.PerItemRate ?? 0;
                cost = BaseRate + (itemCount * perItem);
                break;

            case ShippingCalculationType.CarrierCalculated:
                // Carrier-calculated rates come from external API
                cost = BaseRate;
                break;
        }

        // Add handling fee
        var handling = HandlingFee ?? Method.HandlingFee ?? 0;
        cost += handling;

        return Math.Max(0, cost);
    }

    /// <summary>
    /// Checks if order meets the requirements for this rate.
    /// </summary>
    public bool MeetsRequirements(decimal orderTotal, decimal orderWeight)
    {
        if (MinWeight.HasValue && orderWeight < MinWeight.Value)
            return false;
        if (MaxWeight.HasValue && orderWeight > MaxWeight.Value)
            return false;
        if (MinOrderAmount.HasValue && orderTotal < MinOrderAmount.Value)
            return false;
        if (MaxOrderAmount.HasValue && orderTotal > MaxOrderAmount.Value)
            return false;

        return true;
    }
}
