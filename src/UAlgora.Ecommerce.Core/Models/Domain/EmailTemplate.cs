namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents an email template that can be managed in the CMS.
/// </summary>
public class EmailTemplate : SoftDeleteEntity
{
    /// <summary>
    /// Reference to the Umbraco content node ID.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Store this template belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Unique template code/identifier.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the template.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Event type that triggers this email.
    /// </summary>
    public EmailTemplateEventType EventType { get; set; }

    /// <summary>
    /// Language/culture code for this template.
    /// </summary>
    public string Language { get; set; } = "en-US";

    #region Email Content

    /// <summary>
    /// Email subject line (supports placeholders).
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body content (supports placeholders).
    /// </summary>
    public string? BodyHtml { get; set; }

    /// <summary>
    /// Plain text body content (supports placeholders).
    /// </summary>
    public string? BodyText { get; set; }

    /// <summary>
    /// Preheader text (preview text in email clients).
    /// </summary>
    public string? Preheader { get; set; }

    #endregion

    #region Sender Info

    /// <summary>
    /// From email address (overrides default).
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// From name (overrides default).
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Reply-to email address.
    /// </summary>
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// BCC email addresses (comma-separated).
    /// </summary>
    public string? BccEmails { get; set; }

    #endregion

    #region Settings

    /// <summary>
    /// Whether this template is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this template is the default for its event type.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Priority for sending (higher = more important).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Delay in minutes before sending (for abandoned cart, etc.).
    /// </summary>
    public int? DelayMinutes { get; set; }

    #endregion

    #region Design

    /// <summary>
    /// Email template layout to use.
    /// </summary>
    public string? LayoutTemplate { get; set; }

    /// <summary>
    /// Custom CSS styles.
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Header image URL.
    /// </summary>
    public string? HeaderImageUrl { get; set; }

    /// <summary>
    /// Footer HTML content.
    /// </summary>
    public string? FooterHtml { get; set; }

    #endregion

    #region Variables

    /// <summary>
    /// Available placeholder variables as JSON.
    /// Example: [{"name": "{{customer.name}}", "description": "Customer full name"}]
    /// </summary>
    public string? AvailableVariablesJson { get; set; }

    /// <summary>
    /// Sample data for preview as JSON.
    /// </summary>
    public string? SampleDataJson { get; set; }

    #endregion

    #region Tracking

    /// <summary>
    /// Number of times this template has been sent.
    /// </summary>
    public int SendCount { get; set; }

    /// <summary>
    /// Last time this template was sent.
    /// </summary>
    public DateTime? LastSentAt { get; set; }

    /// <summary>
    /// Last time this template was tested.
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    #endregion

    #region Metadata

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

    #endregion
}

/// <summary>
/// Email template event types.
/// </summary>
public enum EmailTemplateEventType
{
    // Order Events
    /// <summary>
    /// Order confirmation email.
    /// </summary>
    OrderConfirmation = 100,

    /// <summary>
    /// Order is being processed.
    /// </summary>
    OrderProcessing = 101,

    /// <summary>
    /// Order has shipped.
    /// </summary>
    OrderShipped = 102,

    /// <summary>
    /// Order has been delivered.
    /// </summary>
    OrderDelivered = 103,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    OrderCancelled = 104,

    /// <summary>
    /// Order has been refunded.
    /// </summary>
    OrderRefunded = 105,

    /// <summary>
    /// Order partially shipped.
    /// </summary>
    OrderPartiallyShipped = 106,

    /// <summary>
    /// Order requires action.
    /// </summary>
    OrderRequiresAction = 107,

    // Customer Events
    /// <summary>
    /// Welcome email for new customers.
    /// </summary>
    CustomerWelcome = 200,

    /// <summary>
    /// Password reset email.
    /// </summary>
    CustomerPasswordReset = 201,

    /// <summary>
    /// Email verification.
    /// </summary>
    CustomerEmailVerification = 202,

    /// <summary>
    /// Account updated notification.
    /// </summary>
    CustomerAccountUpdated = 203,

    /// <summary>
    /// Customer birthday email.
    /// </summary>
    CustomerBirthday = 204,

    /// <summary>
    /// Request for product review.
    /// </summary>
    CustomerReviewRequest = 205,

    // Cart Events
    /// <summary>
    /// Abandoned cart reminder (1 hour).
    /// </summary>
    CartAbandoned1Hour = 300,

    /// <summary>
    /// Abandoned cart reminder (24 hours).
    /// </summary>
    CartAbandoned24Hours = 301,

    /// <summary>
    /// Abandoned cart reminder (72 hours).
    /// </summary>
    CartAbandoned72Hours = 302,

    /// <summary>
    /// Abandoned cart final reminder.
    /// </summary>
    CartAbandonedFinal = 303,

    // Gift Card Events
    /// <summary>
    /// Gift card issued/delivered.
    /// </summary>
    GiftCardIssued = 400,

    /// <summary>
    /// Gift card redeemed notification.
    /// </summary>
    GiftCardRedeemed = 401,

    /// <summary>
    /// Gift card balance low.
    /// </summary>
    GiftCardBalanceLow = 402,

    /// <summary>
    /// Gift card expiring soon.
    /// </summary>
    GiftCardExpiringSoon = 403,

    // Return Events
    /// <summary>
    /// Return request received.
    /// </summary>
    ReturnRequested = 500,

    /// <summary>
    /// Return approved.
    /// </summary>
    ReturnApproved = 501,

    /// <summary>
    /// Return rejected.
    /// </summary>
    ReturnRejected = 502,

    /// <summary>
    /// Return items received.
    /// </summary>
    ReturnItemsReceived = 503,

    /// <summary>
    /// Return refund processed.
    /// </summary>
    ReturnRefundProcessed = 504,

    // Inventory Events
    /// <summary>
    /// Low stock alert (admin).
    /// </summary>
    InventoryLowStock = 600,

    /// <summary>
    /// Out of stock alert (admin).
    /// </summary>
    InventoryOutOfStock = 601,

    /// <summary>
    /// Back in stock notification (customer).
    /// </summary>
    InventoryBackInStock = 602,

    // Wishlist Events
    /// <summary>
    /// Wishlist item on sale.
    /// </summary>
    WishlistItemOnSale = 700,

    /// <summary>
    /// Wishlist item back in stock.
    /// </summary>
    WishlistItemBackInStock = 701,

    /// <summary>
    /// Wishlist item low stock.
    /// </summary>
    WishlistItemLowStock = 702,

    // Subscription Events
    /// <summary>
    /// Subscription renewal reminder.
    /// </summary>
    SubscriptionRenewalReminder = 800,

    /// <summary>
    /// Subscription renewed.
    /// </summary>
    SubscriptionRenewed = 801,

    /// <summary>
    /// Subscription cancelled.
    /// </summary>
    SubscriptionCancelled = 802,

    /// <summary>
    /// Subscription payment failed.
    /// </summary>
    SubscriptionPaymentFailed = 803,

    // Admin Events
    /// <summary>
    /// New order notification (admin).
    /// </summary>
    AdminNewOrder = 900,

    /// <summary>
    /// New customer notification (admin).
    /// </summary>
    AdminNewCustomer = 901,

    /// <summary>
    /// New return request (admin).
    /// </summary>
    AdminNewReturn = 902,

    /// <summary>
    /// Daily sales summary (admin).
    /// </summary>
    AdminDailySummary = 903,

    /// <summary>
    /// Weekly sales report (admin).
    /// </summary>
    AdminWeeklyReport = 904,

    // Custom
    /// <summary>
    /// Custom/marketing email.
    /// </summary>
    Custom = 999
}
