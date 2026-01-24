namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a shareable payment link for collecting payments.
/// </summary>
public class PaymentLink : SoftDeleteEntity
{
    /// <summary>
    /// Reference to the Umbraco content node ID.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Store this payment link belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    #region Link Identity

    /// <summary>
    /// Unique code/slug for the payment link URL.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the payment link.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description shown to customer.
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Amount Configuration

    /// <summary>
    /// Payment link type.
    /// </summary>
    public PaymentLinkType Type { get; set; } = PaymentLinkType.FixedAmount;

    /// <summary>
    /// Fixed amount for payment (required for FixedAmount type).
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Currency code (ISO 4217).
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Minimum amount for CustomerChoice type.
    /// </summary>
    public decimal? MinimumAmount { get; set; }

    /// <summary>
    /// Maximum amount for CustomerChoice type.
    /// </summary>
    public decimal? MaximumAmount { get; set; }

    /// <summary>
    /// Suggested amounts for CustomerChoice type (JSON array).
    /// </summary>
    public string? SuggestedAmountsJson { get; set; }

    /// <summary>
    /// Whether to allow customers to add a tip.
    /// </summary>
    public bool AllowTip { get; set; }

    /// <summary>
    /// Suggested tip percentages (JSON array, e.g., [10, 15, 20]).
    /// </summary>
    public string? TipPercentagesJson { get; set; }

    #endregion

    #region Product Configuration

    /// <summary>
    /// Product ID for product-specific links.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Product variant ID for product-specific links.
    /// </summary>
    public Guid? ProductVariantId { get; set; }

    /// <summary>
    /// Whether to allow quantity selection.
    /// </summary>
    public bool AllowQuantity { get; set; }

    /// <summary>
    /// Maximum quantity allowed (null = unlimited).
    /// </summary>
    public int? MaxQuantity { get; set; }

    #endregion

    #region Validity & Limits

    /// <summary>
    /// Current status of the payment link.
    /// </summary>
    public PaymentLinkStatus Status { get; set; } = PaymentLinkStatus.Active;

    /// <summary>
    /// Whether the link is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date from which the link becomes active.
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Date when the link expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Maximum number of times this link can be used (null = unlimited).
    /// </summary>
    public int? MaxUses { get; set; }

    /// <summary>
    /// Number of times the link has been used.
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Total amount collected through this link.
    /// </summary>
    public decimal TotalCollected { get; set; }

    #endregion

    #region Customer Configuration

    /// <summary>
    /// Whether to require customer email.
    /// </summary>
    public bool RequireEmail { get; set; } = true;

    /// <summary>
    /// Whether to require customer phone.
    /// </summary>
    public bool RequirePhone { get; set; }

    /// <summary>
    /// Whether to require billing address.
    /// </summary>
    public bool RequireBillingAddress { get; set; }

    /// <summary>
    /// Whether to require shipping address.
    /// </summary>
    public bool RequireShippingAddress { get; set; }

    /// <summary>
    /// Custom fields to collect (JSON array).
    /// </summary>
    public string? CustomFieldsJson { get; set; }

    /// <summary>
    /// Pre-filled customer email (for personalized links).
    /// </summary>
    public string? PrefilledEmail { get; set; }

    /// <summary>
    /// Pre-filled customer name (for personalized links).
    /// </summary>
    public string? PrefilledName { get; set; }

    #endregion

    #region Checkout Experience

    /// <summary>
    /// Custom success message after payment.
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Custom redirect URL after successful payment.
    /// </summary>
    public string? SuccessRedirectUrl { get; set; }

    /// <summary>
    /// Custom cancel/back URL.
    /// </summary>
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Allowed payment method IDs (JSON array, null = all).
    /// </summary>
    public string? AllowedPaymentMethodsJson { get; set; }

    /// <summary>
    /// Custom branding color.
    /// </summary>
    public string? BrandColor { get; set; }

    /// <summary>
    /// Custom logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Terms and conditions URL.
    /// </summary>
    public string? TermsUrl { get; set; }

    #endregion

    #region Notifications

    /// <summary>
    /// Email to notify on successful payment.
    /// </summary>
    public string? NotificationEmail { get; set; }

    /// <summary>
    /// Whether to send email receipt to customer.
    /// </summary>
    public bool SendCustomerReceipt { get; set; } = true;

    /// <summary>
    /// Email template ID for customer receipt.
    /// </summary>
    public Guid? ReceiptEmailTemplateId { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Internal reference number.
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Internal notes (not shown to customer).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Tags for organization (JSON array).
    /// </summary>
    public string? TagsJson { get; set; }

    /// <summary>
    /// Custom metadata (JSON object).
    /// </summary>
    public string? MetadataJson { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Store navigation property.
    /// </summary>
    public Store? Store { get; set; }

    /// <summary>
    /// Product navigation property.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Payments made through this link.
    /// </summary>
    public List<PaymentLinkPayment> Payments { get; set; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether the link has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Whether the link is valid for use.
    /// </summary>
    public bool IsValid => IsActive && !IsExpired && Status == PaymentLinkStatus.Active &&
                           (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow) &&
                           (!MaxUses.HasValue || UsageCount < MaxUses);

    /// <summary>
    /// Remaining uses (null if unlimited).
    /// </summary>
    public int? RemainingUses => MaxUses.HasValue ? MaxUses.Value - UsageCount : null;

    /// <summary>
    /// Full URL for the payment link.
    /// </summary>
    public string GetFullUrl(string baseUrl) => $"{baseUrl.TrimEnd('/')}/pay/{Code}";

    #endregion
}

/// <summary>
/// Represents a payment made through a payment link.
/// </summary>
public class PaymentLinkPayment : BaseEntity
{
    /// <summary>
    /// Payment link ID.
    /// </summary>
    public Guid PaymentLinkId { get; set; }

    /// <summary>
    /// Order ID created from this payment.
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Payment ID.
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// Customer ID (if registered).
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Amount paid.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tip amount (if applicable).
    /// </summary>
    public decimal TipAmount { get; set; }

    /// <summary>
    /// Total amount (amount + tip).
    /// </summary>
    public decimal TotalAmount => Amount + TipAmount;

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Payment status.
    /// </summary>
    public PaymentLinkPaymentStatus Status { get; set; }

    /// <summary>
    /// Customer email.
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Customer name.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Customer phone.
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Billing address (JSON).
    /// </summary>
    public string? BillingAddressJson { get; set; }

    /// <summary>
    /// Shipping address (JSON).
    /// </summary>
    public string? ShippingAddressJson { get; set; }

    /// <summary>
    /// Custom field responses (JSON).
    /// </summary>
    public string? CustomFieldsJson { get; set; }

    /// <summary>
    /// Payment gateway reference.
    /// </summary>
    public string? GatewayReference { get; set; }

    /// <summary>
    /// IP address of the payer.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the payer.
    /// </summary>
    public string? UserAgent { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Payment link navigation property.
    /// </summary>
    public PaymentLink? PaymentLink { get; set; }

    /// <summary>
    /// Order navigation property.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// Customer navigation property.
    /// </summary>
    public Customer? Customer { get; set; }

    #endregion
}

/// <summary>
/// Payment link type.
/// </summary>
public enum PaymentLinkType
{
    /// <summary>
    /// Fixed amount that customer must pay.
    /// </summary>
    FixedAmount = 0,

    /// <summary>
    /// Customer can enter any amount (with optional min/max).
    /// </summary>
    CustomerChoice = 1,

    /// <summary>
    /// Link to a specific product.
    /// </summary>
    Product = 2,

    /// <summary>
    /// Recurring subscription payment.
    /// </summary>
    Subscription = 3,

    /// <summary>
    /// Invoice payment.
    /// </summary>
    Invoice = 4,

    /// <summary>
    /// Donation with optional suggested amounts.
    /// </summary>
    Donation = 5
}

/// <summary>
/// Payment link status.
/// </summary>
public enum PaymentLinkStatus
{
    /// <summary>
    /// Link is active and can accept payments.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Link is paused (temporarily disabled).
    /// </summary>
    Paused = 1,

    /// <summary>
    /// Link has expired.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Link reached maximum uses.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Link is archived (soft deleted).
    /// </summary>
    Archived = 4
}

/// <summary>
/// Payment link payment status.
/// </summary>
public enum PaymentLinkPaymentStatus
{
    /// <summary>
    /// Payment is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is processing.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment was refunded.
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// Payment was cancelled.
    /// </summary>
    Cancelled = 5
}
