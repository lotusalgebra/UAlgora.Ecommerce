namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a gift card that can be purchased and redeemed.
/// </summary>
public class GiftCard : SoftDeleteEntity
{
    /// <summary>
    /// Reference to the Umbraco content node ID.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Store this gift card belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Unique gift card code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the gift card.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Initial value when issued.
    /// </summary>
    public decimal InitialValue { get; set; }

    /// <summary>
    /// Current remaining balance.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Currency code (ISO 4217).
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Gift card status.
    /// </summary>
    public GiftCardStatus Status { get; set; } = GiftCardStatus.Active;

    /// <summary>
    /// Gift card type.
    /// </summary>
    public GiftCardType Type { get; set; } = GiftCardType.Physical;

    #region Issuance

    /// <summary>
    /// When the gift card was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// Customer ID who purchased this gift card.
    /// </summary>
    public Guid? PurchasedByCustomerId { get; set; }

    /// <summary>
    /// Order ID that generated this gift card.
    /// </summary>
    public Guid? IssuedByOrderId { get; set; }

    /// <summary>
    /// Customer ID this gift card was issued to.
    /// </summary>
    public Guid? IssuedToCustomerId { get; set; }

    /// <summary>
    /// Recipient name.
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// Recipient email for delivery.
    /// </summary>
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Sender name for gift message.
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// Personal message from sender.
    /// </summary>
    public string? Message { get; set; }

    #endregion

    #region Expiration & Validity

    /// <summary>
    /// When the gift card expires (null = never expires).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Date from which the gift card becomes active.
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Whether the gift card can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Usage Tracking

    /// <summary>
    /// Number of times the gift card has been used.
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Last time the gift card was used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Customer ID who last used this gift card.
    /// </summary>
    public Guid? LastUsedByCustomerId { get; set; }

    #endregion

    #region Restrictions

    /// <summary>
    /// Minimum order amount required to use this gift card.
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Maximum amount that can be redeemed per order.
    /// </summary>
    public decimal? MaxRedemptionPerOrder { get; set; }

    /// <summary>
    /// Category IDs this gift card is restricted to.
    /// </summary>
    public List<Guid> RestrictedToCategoryIds { get; set; } = [];

    /// <summary>
    /// Product IDs this gift card is restricted to.
    /// </summary>
    public List<Guid> RestrictedToProductIds { get; set; } = [];

    /// <summary>
    /// Whether gift card can be combined with other discounts.
    /// </summary>
    public bool CanCombineWithDiscounts { get; set; } = true;

    #endregion

    #region Metadata

    /// <summary>
    /// Admin notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Custom metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Store navigation property.
    /// </summary>
    public Store? Store { get; set; }

    /// <summary>
    /// Customer who purchased this gift card.
    /// </summary>
    public Customer? PurchasedByCustomer { get; set; }

    /// <summary>
    /// Customer this gift card was issued to.
    /// </summary>
    public Customer? IssuedToCustomer { get; set; }

    /// <summary>
    /// Transactions on this gift card.
    /// </summary>
    public List<GiftCardTransaction> Transactions { get; set; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether the gift card is expired.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Whether the gift card is valid for use.
    /// </summary>
    public bool IsValid => IsActive && !IsExpired && Status == GiftCardStatus.Active && Balance > 0 &&
                           (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow);

    /// <summary>
    /// Total amount redeemed from this gift card.
    /// </summary>
    public decimal AmountRedeemed => InitialValue - Balance;

    /// <summary>
    /// Percentage of value remaining.
    /// </summary>
    public decimal BalancePercentage => InitialValue > 0 ? Math.Round((Balance / InitialValue) * 100, 2) : 0;

    #endregion
}

/// <summary>
/// Gift card transaction record.
/// </summary>
public class GiftCardTransaction : BaseEntity
{
    /// <summary>
    /// Gift card ID.
    /// </summary>
    public Guid GiftCardId { get; set; }

    /// <summary>
    /// Order ID (if redemption).
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Transaction type.
    /// </summary>
    public GiftCardTransactionType Type { get; set; }

    /// <summary>
    /// Transaction amount (positive for additions, negative for redemptions).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Balance before this transaction.
    /// </summary>
    public decimal BalanceBefore { get; set; }

    /// <summary>
    /// Balance after this transaction.
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// User/admin who performed this transaction.
    /// </summary>
    public string? PerformedBy { get; set; }

    /// <summary>
    /// Customer ID who used this gift card.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Transaction notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Reference number for external systems.
    /// </summary>
    public string? ReferenceNumber { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Gift card navigation property.
    /// </summary>
    public GiftCard? GiftCard { get; set; }

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
/// Gift card status.
/// </summary>
public enum GiftCardStatus
{
    /// <summary>
    /// Gift card is active and can be used.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Gift card has been fully redeemed.
    /// </summary>
    Redeemed = 1,

    /// <summary>
    /// Gift card has expired.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Gift card has been disabled by admin.
    /// </summary>
    Disabled = 3,

    /// <summary>
    /// Gift card is pending activation (e.g., not yet delivered).
    /// </summary>
    Pending = 4
}

/// <summary>
/// Gift card type.
/// </summary>
public enum GiftCardType
{
    /// <summary>
    /// Physical gift card.
    /// </summary>
    Physical = 0,

    /// <summary>
    /// Digital/virtual gift card.
    /// </summary>
    Digital = 1,

    /// <summary>
    /// Promotional/bonus gift card.
    /// </summary>
    Promotional = 2
}

/// <summary>
/// Gift card transaction type.
/// </summary>
public enum GiftCardTransactionType
{
    /// <summary>
    /// Initial issuance.
    /// </summary>
    Issue = 0,

    /// <summary>
    /// Redemption at checkout.
    /// </summary>
    Redeem = 1,

    /// <summary>
    /// Refund to gift card.
    /// </summary>
    Refund = 2,

    /// <summary>
    /// Manual balance adjustment.
    /// </summary>
    Adjustment = 3,

    /// <summary>
    /// Balance transfer from another gift card.
    /// </summary>
    Transfer = 4,

    /// <summary>
    /// Expiration (balance zeroed out).
    /// </summary>
    Expiration = 5,

    /// <summary>
    /// Top-up/reload.
    /// </summary>
    Reload = 6
}
