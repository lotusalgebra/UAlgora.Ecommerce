using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a configured payment method available for checkout.
/// </summary>
public class PaymentMethodConfig : SoftDeleteEntity
{
    /// <summary>
    /// Payment method name (e.g., "Credit Card", "PayPal").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique code for this method.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description shown to customers.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Instructions shown during checkout.
    /// </summary>
    public string? CheckoutInstructions { get; set; }

    /// <summary>
    /// Payment method type.
    /// </summary>
    public PaymentMethodType Type { get; set; } = PaymentMethodType.CreditCard;

    /// <summary>
    /// Associated payment gateway ID (if using gateway).
    /// </summary>
    public Guid? GatewayId { get; set; }

    /// <summary>
    /// Whether this method is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default payment method.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    #region Fee Settings

    /// <summary>
    /// Processing fee type.
    /// </summary>
    public PaymentFeeType FeeType { get; set; } = PaymentFeeType.None;

    /// <summary>
    /// Flat fee amount.
    /// </summary>
    public decimal? FlatFee { get; set; }

    /// <summary>
    /// Percentage fee (0-100).
    /// </summary>
    public decimal? PercentageFee { get; set; }

    /// <summary>
    /// Maximum fee cap.
    /// </summary>
    public decimal? MaxFee { get; set; }

    /// <summary>
    /// Whether fee is shown separately at checkout.
    /// </summary>
    public bool ShowFeeAtCheckout { get; set; } = true;

    #endregion

    #region Restrictions

    /// <summary>
    /// Minimum order amount required.
    /// </summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Maximum order amount allowed.
    /// </summary>
    public decimal? MaxOrderAmount { get; set; }

    /// <summary>
    /// Allowed countries (empty = all countries).
    /// </summary>
    public List<string> AllowedCountries { get; set; } = [];

    /// <summary>
    /// Excluded countries.
    /// </summary>
    public List<string> ExcludedCountries { get; set; } = [];

    /// <summary>
    /// Allowed currencies (empty = all currencies).
    /// </summary>
    public List<string> AllowedCurrencies { get; set; } = [];

    /// <summary>
    /// Allowed customer groups (empty = all groups).
    /// </summary>
    public List<string> AllowedCustomerGroups { get; set; } = [];

    #endregion

    #region Display Settings

    /// <summary>
    /// Icon name for display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Image URL (e.g., card logos).
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether to show card logos.
    /// </summary>
    public bool ShowCardLogos { get; set; } = true;

    /// <summary>
    /// Custom CSS class for styling.
    /// </summary>
    public string? CssClass { get; set; }

    #endregion

    #region Capture Settings

    /// <summary>
    /// Capture mode for payments.
    /// </summary>
    public PaymentCaptureMode CaptureMode { get; set; } = PaymentCaptureMode.Immediate;

    /// <summary>
    /// Days to auto-capture authorized payments.
    /// </summary>
    public int? AutoCaptureDays { get; set; }

    #endregion

    #region Security Settings

    /// <summary>
    /// Whether 3D Secure is required.
    /// </summary>
    public bool Require3DSecure { get; set; }

    /// <summary>
    /// Whether CVV is required.
    /// </summary>
    public bool RequireCvv { get; set; } = true;

    /// <summary>
    /// Whether billing address is required.
    /// </summary>
    public bool RequireBillingAddress { get; set; } = true;

    /// <summary>
    /// Whether to save payment methods for customers.
    /// </summary>
    public bool AllowSavePaymentMethod { get; set; } = true;

    #endregion

    #region Refund Settings

    /// <summary>
    /// Whether refunds are enabled.
    /// </summary>
    public bool AllowRefunds { get; set; } = true;

    /// <summary>
    /// Whether partial refunds are allowed.
    /// </summary>
    public bool AllowPartialRefunds { get; set; } = true;

    /// <summary>
    /// Refund time limit in days (0 = no limit).
    /// </summary>
    public int RefundTimeLimitDays { get; set; }

    #endregion

    /// <summary>
    /// Navigation property to gateway.
    /// </summary>
    public PaymentGateway? Gateway { get; set; }

    #region Computed Properties

    /// <summary>
    /// Whether this method requires a payment gateway.
    /// </summary>
    public bool RequiresGateway => Type is PaymentMethodType.CreditCard or PaymentMethodType.DebitCard
        or PaymentMethodType.PayPal or PaymentMethodType.Stripe;

    /// <summary>
    /// Whether this is an offline payment method.
    /// </summary>
    public bool IsOffline => Type is PaymentMethodType.BankTransfer or PaymentMethodType.CashOnDelivery;

    /// <summary>
    /// Calculates the processing fee for an amount.
    /// </summary>
    public decimal CalculateFee(decimal orderAmount)
    {
        if (FeeType == PaymentFeeType.None) return 0;

        var fee = FeeType switch
        {
            PaymentFeeType.FlatFee => FlatFee ?? 0,
            PaymentFeeType.Percentage => orderAmount * (PercentageFee ?? 0) / 100,
            PaymentFeeType.FlatPlusPercentage => (FlatFee ?? 0) + (orderAmount * (PercentageFee ?? 0) / 100),
            _ => 0
        };

        if (MaxFee.HasValue && fee > MaxFee.Value)
            fee = MaxFee.Value;

        return Math.Round(fee, 2);
    }

    /// <summary>
    /// Checks if this method is available for a given context.
    /// </summary>
    public bool IsAvailableFor(PaymentMethodCheckContext context)
    {
        if (!IsActive) return false;

        if (MinOrderAmount.HasValue && context.OrderAmount < MinOrderAmount.Value)
            return false;

        if (MaxOrderAmount.HasValue && context.OrderAmount > MaxOrderAmount.Value)
            return false;

        if (AllowedCountries.Count > 0 && !AllowedCountries.Contains(context.Country, StringComparer.OrdinalIgnoreCase))
            return false;

        if (ExcludedCountries.Contains(context.Country, StringComparer.OrdinalIgnoreCase))
            return false;

        if (AllowedCurrencies.Count > 0 && !AllowedCurrencies.Contains(context.CurrencyCode, StringComparer.OrdinalIgnoreCase))
            return false;

        if (AllowedCustomerGroups.Count > 0 && !string.IsNullOrEmpty(context.CustomerGroup) &&
            !AllowedCustomerGroups.Contains(context.CustomerGroup, StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }

    #endregion
}

/// <summary>
/// Context for checking payment method availability.
/// </summary>
public class PaymentMethodCheckContext
{
    public decimal OrderAmount { get; set; }
    public string Country { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "USD";
    public string? CustomerGroup { get; set; }
    public Guid? CustomerId { get; set; }
}

/// <summary>
/// Payment fee calculation type.
/// </summary>
public enum PaymentFeeType
{
    /// <summary>
    /// No processing fee.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flat fee amount.
    /// </summary>
    FlatFee = 1,

    /// <summary>
    /// Percentage of order total.
    /// </summary>
    Percentage = 2,

    /// <summary>
    /// Flat fee plus percentage.
    /// </summary>
    FlatPlusPercentage = 3
}

/// <summary>
/// Payment capture mode.
/// </summary>
public enum PaymentCaptureMode
{
    /// <summary>
    /// Capture immediately after authorization.
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// Manual capture required.
    /// </summary>
    Manual = 1,

    /// <summary>
    /// Auto-capture after a delay.
    /// </summary>
    Delayed = 2
}
