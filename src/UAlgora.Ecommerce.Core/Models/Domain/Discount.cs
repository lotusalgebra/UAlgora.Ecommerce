using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a discount or promotion.
/// </summary>
public class Discount : SoftDeleteEntity
{
    /// <summary>
    /// Umbraco content node ID for CMS sync.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Store this discount belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Discount name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Discount description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Coupon code (if coupon-based discount).
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Discount type.
    /// </summary>
    public DiscountType Type { get; set; } = DiscountType.Percentage;

    /// <summary>
    /// Discount scope.
    /// </summary>
    public DiscountScope Scope { get; set; } = DiscountScope.Order;

    /// <summary>
    /// Discount value (percentage or fixed amount).
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Maximum discount amount (for percentage discounts).
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    #region Conditions

    /// <summary>
    /// Minimum order amount to qualify.
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Minimum quantity to qualify.
    /// </summary>
    public int? MinimumQuantity { get; set; }

    /// <summary>
    /// Maximum quantity the discount applies to.
    /// </summary>
    public int? MaximumQuantity { get; set; }

    /// <summary>
    /// Product IDs this discount applies to.
    /// </summary>
    public List<Guid> ApplicableProductIds { get; set; } = [];

    /// <summary>
    /// Category IDs this discount applies to.
    /// </summary>
    public List<Guid> ApplicableCategoryIds { get; set; } = [];

    /// <summary>
    /// Customer IDs eligible for this discount.
    /// </summary>
    public List<Guid> EligibleCustomerIds { get; set; } = [];

    /// <summary>
    /// Customer tiers eligible for this discount.
    /// </summary>
    public List<string> EligibleCustomerTiers { get; set; } = [];

    /// <summary>
    /// Whether this is for first-time customers only.
    /// </summary>
    public bool FirstTimeCustomerOnly { get; set; }

    /// <summary>
    /// Product IDs excluded from this discount.
    /// </summary>
    public List<Guid> ExcludedProductIds { get; set; } = [];

    /// <summary>
    /// Category IDs excluded from this discount.
    /// </summary>
    public List<Guid> ExcludedCategoryIds { get; set; } = [];

    /// <summary>
    /// Whether sale items are excluded.
    /// </summary>
    public bool ExcludeSaleItems { get; set; }

    #endregion

    #region Buy X Get Y

    /// <summary>
    /// Buy quantity for Buy X Get Y.
    /// </summary>
    public int? BuyQuantity { get; set; }

    /// <summary>
    /// Get quantity for Buy X Get Y.
    /// </summary>
    public int? GetQuantity { get; set; }

    /// <summary>
    /// Get product IDs for Buy X Get Y (if different from buy products).
    /// </summary>
    public List<Guid> GetProductIds { get; set; } = [];

    #endregion

    #region Bundle

    /// <summary>
    /// Product IDs included in the bundle.
    /// </summary>
    public List<Guid> BundleProductIds { get; set; } = [];

    /// <summary>
    /// Bundle discount value (percentage or fixed off the bundle total).
    /// </summary>
    public decimal? BundleDiscountValue { get; set; }

    #endregion

    #region Bulk / Volume

    /// <summary>
    /// Volume tier thresholds (JSON: [{quantity, discountPercent}]).
    /// </summary>
    public List<VolumeTier> VolumeTiers { get; set; } = [];

    #endregion

    #region Seasonal

    /// <summary>
    /// Season label (e.g., "Summer Sale", "Post-Holiday Clearance").
    /// </summary>
    public string? SeasonLabel { get; set; }

    #endregion

    #region Early Payment

    /// <summary>
    /// Number of days within which early payment qualifies for the discount.
    /// </summary>
    public int? EarlyPaymentDays { get; set; }

    /// <summary>
    /// Standard payment term in days (e.g., 30).
    /// </summary>
    public int? StandardPaymentDays { get; set; }

    #endregion

    #region Referral

    /// <summary>
    /// Discount value for the referred (new) customer.
    /// </summary>
    public decimal? ReferralNewCustomerValue { get; set; }

    /// <summary>
    /// Whether the referral gives both referrer and referee a discount.
    /// </summary>
    public bool ReferralTwoWay { get; set; }

    #endregion

    #region Loyalty Program

    /// <summary>
    /// Points threshold required to unlock this discount.
    /// </summary>
    public int? LoyaltyPointsThreshold { get; set; }

    /// <summary>
    /// Loyalty tier required (e.g., "Gold", "Platinum").
    /// </summary>
    public string? LoyaltyTierRequired { get; set; }

    #endregion

    #region Email Subscription

    /// <summary>
    /// Whether this discount requires a new email subscription sign-up.
    /// </summary>
    public bool RequiresEmailSubscription { get; set; }

    /// <summary>
    /// Whether this discount applies to cart abandonment recovery.
    /// </summary>
    public bool IsCartAbandonmentRecovery { get; set; }

    #endregion

    #region Overstock

    /// <summary>
    /// Whether this is an overstock/clearance discount.
    /// </summary>
    public bool IsOverstockClearance { get; set; }

    #endregion

    #region Trade-In Credit

    /// <summary>
    /// Trade-in credit amount per item traded in.
    /// </summary>
    public decimal? TradeInCreditPerItem { get; set; }

    /// <summary>
    /// Product IDs eligible for trade-in.
    /// </summary>
    public List<Guid> TradeInProductIds { get; set; } = [];

    /// <summary>
    /// Product IDs that can be purchased with the trade-in credit.
    /// </summary>
    public List<Guid> TradeInTargetProductIds { get; set; } = [];

    #endregion

    #region Usage Limits

    /// <summary>
    /// Total usage limit across all customers.
    /// </summary>
    public int? TotalUsageLimit { get; set; }

    /// <summary>
    /// Usage limit per customer.
    /// </summary>
    public int? PerCustomerLimit { get; set; }

    /// <summary>
    /// Current usage count.
    /// </summary>
    public int UsageCount { get; set; }

    #endregion

    #region Validity

    /// <summary>
    /// Whether the discount is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Start date of validity.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date of validity.
    /// </summary>
    public DateTime? EndDate { get; set; }

    #endregion

    #region Stacking

    /// <summary>
    /// Whether this discount can be combined with other discounts.
    /// </summary>
    public bool CanCombine { get; set; }

    /// <summary>
    /// Priority for discount application (higher = applied first).
    /// </summary>
    public int Priority { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether this is a coupon code discount.
    /// </summary>
    public bool IsCoupon => !string.IsNullOrWhiteSpace(Code);

    /// <summary>
    /// Whether the discount is currently valid.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (!IsActive) return false;

            var now = DateTime.UtcNow;

            if (StartDate.HasValue && now < StartDate.Value) return false;
            if (EndDate.HasValue && now > EndDate.Value) return false;
            if (TotalUsageLimit.HasValue && UsageCount >= TotalUsageLimit.Value) return false;

            return true;
        }
    }

    /// <summary>
    /// Whether the usage limit has been reached.
    /// </summary>
    public bool IsUsageLimitReached =>
        TotalUsageLimit.HasValue && UsageCount >= TotalUsageLimit.Value;

    /// <summary>
    /// Remaining uses (null if unlimited).
    /// </summary>
    public int? RemainingUses =>
        TotalUsageLimit.HasValue ? TotalUsageLimit.Value - UsageCount : null;

    /// <summary>
    /// Display value (e.g., "20%" or "$10.00").
    /// </summary>
    public string DisplayValue => Type switch
    {
        DiscountType.Percentage => $"{Value}%",
        DiscountType.FixedAmount => $"${Value:F2}",
        DiscountType.FreeShipping => "Free Shipping",
        DiscountType.BuyXGetY => $"Buy {BuyQuantity} Get {GetQuantity}",
        DiscountType.EarlyPayment => $"{Value}% (within {EarlyPaymentDays}d)",
        DiscountType.Overstock => $"{Value}% Clearance",
        DiscountType.Bundle => BundleDiscountValue.HasValue ? $"{BundleDiscountValue}% Bundle" : $"${Value:F2} Bundle",
        DiscountType.BulkVolume => VolumeTiers.Count > 0 ? $"Up to {VolumeTiers.Max(t => t.DiscountPercent)}%" : $"{Value}%",
        DiscountType.Seasonal => $"{Value}% {SeasonLabel ?? "Seasonal"}",
        DiscountType.Referral => ReferralTwoWay ? $"{Value}% Two-Way Referral" : $"{Value}% Referral",
        DiscountType.LoyaltyProgram => $"{Value}% Loyalty",
        DiscountType.EmailSubscription => $"{Value}% Email Signup",
        DiscountType.TradeInCredit => TradeInCreditPerItem.HasValue ? $"${TradeInCreditPerItem:F2} Trade-In" : $"${Value:F2} Trade-In",
        _ => Value.ToString("F2")
    };

    #endregion
}

/// <summary>
/// Represents a volume/bulk pricing tier.
/// </summary>
public class VolumeTier
{
    /// <summary>
    /// Minimum quantity to qualify for this tier.
    /// </summary>
    public int MinQuantity { get; set; }

    /// <summary>
    /// Discount percentage for this tier.
    /// </summary>
    public decimal DiscountPercent { get; set; }
}

/// <summary>
/// Tracks discount usage by customer.
/// </summary>
public class DiscountUsage : BaseEntity
{
    /// <summary>
    /// Discount ID.
    /// </summary>
    public Guid DiscountId { get; set; }

    /// <summary>
    /// Customer ID.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Order ID where discount was used.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Discount amount applied.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Navigation property to discount.
    /// </summary>
    public Discount? Discount { get; set; }

    /// <summary>
    /// Navigation property to customer.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Navigation property to order.
    /// </summary>
    public Order? Order { get; set; }
}
