namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents an audit log entry for tracking changes and actions.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Store this log entry belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Type of entity being audited.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity being audited.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Umbraco node ID (for content changes).
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Action performed.
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// Category of the action.
    /// </summary>
    public AuditCategory Category { get; set; }

    #region User Info

    /// <summary>
    /// User ID who performed the action.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username who performed the action.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// User email.
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// User role at time of action.
    /// </summary>
    public string? UserRole { get; set; }

    /// <summary>
    /// Whether this was a system/automated action.
    /// </summary>
    public bool IsSystemAction { get; set; }

    #endregion

    #region Request Info

    /// <summary>
    /// IP address of the request.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Request URL.
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// HTTP method.
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Request ID for correlation.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Session ID.
    /// </summary>
    public string? SessionId { get; set; }

    #endregion

    #region Change Data

    /// <summary>
    /// Description of the action.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Old values as JSON.
    /// </summary>
    public string? OldValuesJson { get; set; }

    /// <summary>
    /// New values as JSON.
    /// </summary>
    public string? NewValuesJson { get; set; }

    /// <summary>
    /// Changed properties as JSON array.
    /// </summary>
    public string? ChangedPropertiesJson { get; set; }

    /// <summary>
    /// Additional context data as JSON.
    /// </summary>
    public string? AdditionalDataJson { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Whether the action was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code if action failed.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Duration of the action in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    #endregion

    #region Timestamps

    /// <summary>
    /// When the action occurred (separate from CreatedAt for precision).
    /// </summary>
    public DateTime Timestamp { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Store navigation property.
    /// </summary>
    public Store? Store { get; set; }

    #endregion
}

/// <summary>
/// Audit action type.
/// </summary>
public enum AuditAction
{
    // CRUD Operations
    /// <summary>
    /// Entity created.
    /// </summary>
    Create = 0,

    /// <summary>
    /// Entity read/viewed.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Entity updated.
    /// </summary>
    Update = 2,

    /// <summary>
    /// Entity deleted.
    /// </summary>
    Delete = 3,

    /// <summary>
    /// Entity soft deleted.
    /// </summary>
    SoftDelete = 4,

    /// <summary>
    /// Entity restored from soft delete.
    /// </summary>
    Restore = 5,

    // Status Changes
    /// <summary>
    /// Status changed.
    /// </summary>
    StatusChange = 10,

    /// <summary>
    /// Published.
    /// </summary>
    Publish = 11,

    /// <summary>
    /// Unpublished.
    /// </summary>
    Unpublish = 12,

    /// <summary>
    /// Approved.
    /// </summary>
    Approve = 13,

    /// <summary>
    /// Rejected.
    /// </summary>
    Reject = 14,

    // Authentication
    /// <summary>
    /// User login.
    /// </summary>
    Login = 20,

    /// <summary>
    /// User logout.
    /// </summary>
    Logout = 21,

    /// <summary>
    /// Failed login attempt.
    /// </summary>
    LoginFailed = 22,

    /// <summary>
    /// Password changed.
    /// </summary>
    PasswordChange = 23,

    /// <summary>
    /// Password reset requested.
    /// </summary>
    PasswordReset = 24,

    /// <summary>
    /// Account locked.
    /// </summary>
    AccountLocked = 25,

    /// <summary>
    /// Account unlocked.
    /// </summary>
    AccountUnlocked = 26,

    // Order Operations
    /// <summary>
    /// Order placed.
    /// </summary>
    OrderPlaced = 30,

    /// <summary>
    /// Order paid.
    /// </summary>
    OrderPaid = 31,

    /// <summary>
    /// Order shipped.
    /// </summary>
    OrderShipped = 32,

    /// <summary>
    /// Order delivered.
    /// </summary>
    OrderDelivered = 33,

    /// <summary>
    /// Order cancelled.
    /// </summary>
    OrderCancelled = 34,

    /// <summary>
    /// Order refunded.
    /// </summary>
    OrderRefunded = 35,

    // Payment Operations
    /// <summary>
    /// Payment processed.
    /// </summary>
    PaymentProcessed = 40,

    /// <summary>
    /// Payment failed.
    /// </summary>
    PaymentFailed = 41,

    /// <summary>
    /// Refund processed.
    /// </summary>
    RefundProcessed = 42,

    // Inventory Operations
    /// <summary>
    /// Stock adjusted.
    /// </summary>
    StockAdjusted = 50,

    /// <summary>
    /// Stock transferred.
    /// </summary>
    StockTransferred = 51,

    /// <summary>
    /// Low stock alert.
    /// </summary>
    LowStockAlert = 52,

    // Configuration
    /// <summary>
    /// Configuration changed.
    /// </summary>
    ConfigurationChange = 60,

    /// <summary>
    /// Settings updated.
    /// </summary>
    SettingsUpdate = 61,

    // Import/Export
    /// <summary>
    /// Data imported.
    /// </summary>
    Import = 70,

    /// <summary>
    /// Data exported.
    /// </summary>
    Export = 71,

    // API Operations
    /// <summary>
    /// API key created.
    /// </summary>
    ApiKeyCreated = 80,

    /// <summary>
    /// API key revoked.
    /// </summary>
    ApiKeyRevoked = 81,

    /// <summary>
    /// Webhook triggered.
    /// </summary>
    WebhookTriggered = 82,

    // Email
    /// <summary>
    /// Email sent.
    /// </summary>
    EmailSent = 90,

    /// <summary>
    /// Email failed.
    /// </summary>
    EmailFailed = 91,

    // Other
    /// <summary>
    /// Custom action.
    /// </summary>
    Custom = 999
}

/// <summary>
/// Audit category.
/// </summary>
public enum AuditCategory
{
    /// <summary>
    /// Product-related actions.
    /// </summary>
    Product = 0,

    /// <summary>
    /// Category-related actions.
    /// </summary>
    Category = 1,

    /// <summary>
    /// Order-related actions.
    /// </summary>
    Order = 2,

    /// <summary>
    /// Customer-related actions.
    /// </summary>
    Customer = 3,

    /// <summary>
    /// Payment-related actions.
    /// </summary>
    Payment = 4,

    /// <summary>
    /// Shipping-related actions.
    /// </summary>
    Shipping = 5,

    /// <summary>
    /// Inventory-related actions.
    /// </summary>
    Inventory = 6,

    /// <summary>
    /// Discount-related actions.
    /// </summary>
    Discount = 7,

    /// <summary>
    /// Gift card-related actions.
    /// </summary>
    GiftCard = 8,

    /// <summary>
    /// Return-related actions.
    /// </summary>
    Return = 9,

    /// <summary>
    /// Authentication-related actions.
    /// </summary>
    Authentication = 10,

    /// <summary>
    /// Configuration-related actions.
    /// </summary>
    Configuration = 11,

    /// <summary>
    /// Store-related actions.
    /// </summary>
    Store = 12,

    /// <summary>
    /// Email-related actions.
    /// </summary>
    Email = 13,

    /// <summary>
    /// System-related actions.
    /// </summary>
    System = 14,

    /// <summary>
    /// API-related actions.
    /// </summary>
    Api = 15,

    /// <summary>
    /// Content-related actions.
    /// </summary>
    Content = 16,

    /// <summary>
    /// License-related actions.
    /// </summary>
    License = 17
}
