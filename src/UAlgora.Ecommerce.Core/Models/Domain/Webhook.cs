namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a webhook subscription for external integrations.
/// </summary>
public class Webhook : SoftDeleteEntity
{
    /// <summary>
    /// Reference to the Umbraco content node ID.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Store this webhook belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Webhook display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Webhook description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Target URL to send webhook payloads to.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Secret key for HMAC signature verification.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Whether the webhook is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region Events

    /// <summary>
    /// Subscribed events as JSON array.
    /// </summary>
    public string EventsJson { get; set; } = "[]";

    /// <summary>
    /// Whether to subscribe to all events.
    /// </summary>
    public bool SubscribeToAll { get; set; }

    #endregion

    #region HTTP Settings

    /// <summary>
    /// HTTP method to use (POST, PUT).
    /// </summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// Content type for the request.
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Custom headers as JSON object.
    /// </summary>
    public string? HeadersJson { get; set; }

    /// <summary>
    /// Timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    #endregion

    #region Retry Settings

    /// <summary>
    /// Whether to retry failed deliveries.
    /// </summary>
    public bool RetryEnabled { get; set; } = true;

    /// <summary>
    /// Maximum retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Retry delay in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Whether to use exponential backoff.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    #endregion

    #region Filtering

    /// <summary>
    /// Filter conditions as JSON.
    /// Example: {"order.total": {"$gt": 100}}
    /// </summary>
    public string? FilterJson { get; set; }

    /// <summary>
    /// Only send for specific product IDs.
    /// </summary>
    public List<Guid> ProductIds { get; set; } = [];

    /// <summary>
    /// Only send for specific category IDs.
    /// </summary>
    public List<Guid> CategoryIds { get; set; } = [];

    #endregion

    #region Security

    /// <summary>
    /// API key/token for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Authentication type.
    /// </summary>
    public WebhookAuthType AuthType { get; set; } = WebhookAuthType.HmacSha256;

    /// <summary>
    /// Basic auth username.
    /// </summary>
    public string? BasicAuthUsername { get; set; }

    /// <summary>
    /// Basic auth password (encrypted).
    /// </summary>
    public string? BasicAuthPassword { get; set; }

    /// <summary>
    /// Bearer token (encrypted).
    /// </summary>
    public string? BearerToken { get; set; }

    /// <summary>
    /// SSL verification enabled.
    /// </summary>
    public bool VerifySsl { get; set; } = true;

    #endregion

    #region Delivery Statistics

    /// <summary>
    /// Total number of deliveries attempted.
    /// </summary>
    public int TotalDeliveries { get; set; }

    /// <summary>
    /// Number of successful deliveries.
    /// </summary>
    public int SuccessfulDeliveries { get; set; }

    /// <summary>
    /// Number of failed deliveries.
    /// </summary>
    public int FailedDeliveries { get; set; }

    /// <summary>
    /// Last delivery timestamp.
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// Last successful delivery timestamp.
    /// </summary>
    public DateTime? LastSuccessAt { get; set; }

    /// <summary>
    /// Last failure timestamp.
    /// </summary>
    public DateTime? LastFailureAt { get; set; }

    /// <summary>
    /// Last HTTP status code received.
    /// </summary>
    public int? LastStatusCode { get; set; }

    /// <summary>
    /// Last error message.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Average response time in milliseconds.
    /// </summary>
    public double? AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Consecutive failure count.
    /// </summary>
    public int ConsecutiveFailures { get; set; }

    #endregion

    #region Auto-Disable

    /// <summary>
    /// Maximum consecutive failures before auto-disable.
    /// </summary>
    public int MaxConsecutiveFailures { get; set; } = 10;

    /// <summary>
    /// Whether the webhook was auto-disabled.
    /// </summary>
    public bool IsAutoDisabled { get; set; }

    /// <summary>
    /// When the webhook was auto-disabled.
    /// </summary>
    public DateTime? AutoDisabledAt { get; set; }

    /// <summary>
    /// Reason for auto-disable.
    /// </summary>
    public string? AutoDisableReason { get; set; }

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

    /// <summary>
    /// Delivery logs for this webhook.
    /// </summary>
    public List<WebhookDelivery> Deliveries { get; set; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Success rate percentage.
    /// </summary>
    public decimal SuccessRate => TotalDeliveries > 0
        ? Math.Round((decimal)SuccessfulDeliveries / TotalDeliveries * 100, 2)
        : 0;

    /// <summary>
    /// Whether the webhook is healthy (recent successes, no consecutive failures).
    /// </summary>
    public bool IsHealthy => IsActive && !IsAutoDisabled && ConsecutiveFailures < 3;

    #endregion
}

/// <summary>
/// Webhook delivery log.
/// </summary>
public class WebhookDelivery : BaseEntity
{
    /// <summary>
    /// Webhook ID.
    /// </summary>
    public Guid WebhookId { get; set; }

    /// <summary>
    /// Event type that triggered this delivery.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Request payload as JSON.
    /// </summary>
    public string? RequestPayload { get; set; }

    /// <summary>
    /// Request headers as JSON.
    /// </summary>
    public string? RequestHeaders { get; set; }

    /// <summary>
    /// Response status code.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Response body (truncated).
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Response headers as JSON.
    /// </summary>
    public string? ResponseHeaders { get; set; }

    /// <summary>
    /// Whether the delivery was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error type/code.
    /// </summary>
    public string? ErrorType { get; set; }

    /// <summary>
    /// Delivery duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Retry attempt number (0 = first attempt).
    /// </summary>
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Entity ID that triggered this webhook (order ID, product ID, etc.).
    /// </summary>
    public Guid? TriggerEntityId { get; set; }

    /// <summary>
    /// Entity type that triggered this webhook.
    /// </summary>
    public string? TriggerEntityType { get; set; }

    /// <summary>
    /// IP address of the webhook server.
    /// </summary>
    public string? ServerIpAddress { get; set; }

    /// <summary>
    /// When the delivery was scheduled.
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// When the delivery was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When the delivery completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Webhook navigation property.
    /// </summary>
    public Webhook? Webhook { get; set; }

    #endregion
}

/// <summary>
/// Webhook authentication type.
/// </summary>
public enum WebhookAuthType
{
    /// <summary>
    /// No authentication.
    /// </summary>
    None = 0,

    /// <summary>
    /// HMAC SHA-256 signature in header.
    /// </summary>
    HmacSha256 = 1,

    /// <summary>
    /// HMAC SHA-512 signature in header.
    /// </summary>
    HmacSha512 = 2,

    /// <summary>
    /// API key in header.
    /// </summary>
    ApiKey = 3,

    /// <summary>
    /// HTTP Basic authentication.
    /// </summary>
    BasicAuth = 4,

    /// <summary>
    /// Bearer token authentication.
    /// </summary>
    BearerToken = 5
}

/// <summary>
/// Webhook event types.
/// </summary>
public static class WebhookEvents
{
    // Order Events
    public const string OrderCreated = "order.created";
    public const string OrderUpdated = "order.updated";
    public const string OrderPaid = "order.paid";
    public const string OrderShipped = "order.shipped";
    public const string OrderDelivered = "order.delivered";
    public const string OrderCancelled = "order.cancelled";
    public const string OrderRefunded = "order.refunded";

    // Product Events
    public const string ProductCreated = "product.created";
    public const string ProductUpdated = "product.updated";
    public const string ProductDeleted = "product.deleted";
    public const string ProductPublished = "product.published";
    public const string ProductUnpublished = "product.unpublished";

    // Inventory Events
    public const string StockUpdated = "stock.updated";
    public const string StockLow = "stock.low";
    public const string StockOut = "stock.out";

    // Customer Events
    public const string CustomerCreated = "customer.created";
    public const string CustomerUpdated = "customer.updated";
    public const string CustomerDeleted = "customer.deleted";

    // Cart Events
    public const string CartCreated = "cart.created";
    public const string CartUpdated = "cart.updated";
    public const string CartAbandoned = "cart.abandoned";

    // Payment Events
    public const string PaymentReceived = "payment.received";
    public const string PaymentFailed = "payment.failed";
    public const string RefundIssued = "refund.issued";

    // Gift Card Events
    public const string GiftCardIssued = "giftcard.issued";
    public const string GiftCardRedeemed = "giftcard.redeemed";

    // Return Events
    public const string ReturnRequested = "return.requested";
    public const string ReturnApproved = "return.approved";
    public const string ReturnRejected = "return.rejected";
    public const string ReturnCompleted = "return.completed";

    // Review Events
    public const string ReviewCreated = "review.created";
    public const string ReviewApproved = "review.approved";

    // Discount Events
    public const string DiscountCreated = "discount.created";
    public const string DiscountUsed = "discount.used";
    public const string DiscountExpired = "discount.expired";
}
