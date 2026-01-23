using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for Webhook operations.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Gets a webhook by ID.
    /// </summary>
    Task<Webhook?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets webhooks by store.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default);

    /// <summary>
    /// Gets all active webhooks.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets webhooks subscribed to an event.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetByEventAsync(string eventType, Guid? storeId = null, CancellationToken ct = default);

    /// <summary>
    /// Creates a new webhook.
    /// </summary>
    Task<Webhook> CreateAsync(Webhook webhook, CancellationToken ct = default);

    /// <summary>
    /// Updates a webhook.
    /// </summary>
    Task<Webhook> UpdateAsync(Webhook webhook, CancellationToken ct = default);

    /// <summary>
    /// Deletes a webhook (soft delete).
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Triggers a webhook event.
    /// </summary>
    Task<WebhookTriggerResult> TriggerAsync(string eventType, object payload, Guid? storeId = null, CancellationToken ct = default);

    /// <summary>
    /// Retries a failed delivery.
    /// </summary>
    Task<WebhookDeliveryResult> RetryDeliveryAsync(Guid deliveryId, CancellationToken ct = default);

    /// <summary>
    /// Gets delivery history for a webhook.
    /// </summary>
    Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(Guid webhookId, int skip = 0, int take = 50, CancellationToken ct = default);

    /// <summary>
    /// Tests a webhook with sample data.
    /// </summary>
    Task<WebhookDeliveryResult> TestAsync(Guid webhookId, CancellationToken ct = default);

    /// <summary>
    /// Re-enables an auto-disabled webhook.
    /// </summary>
    Task<bool> ReEnableAsync(Guid webhookId, CancellationToken ct = default);

    /// <summary>
    /// Gets auto-disabled webhooks.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetAutoDisabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Processes pending retries.
    /// </summary>
    Task<int> ProcessPendingRetriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Generates a webhook secret.
    /// </summary>
    string GenerateSecret();

    /// <summary>
    /// Verifies a webhook signature.
    /// </summary>
    bool VerifySignature(string payload, string signature, string secret, WebhookAuthType authType = WebhookAuthType.HmacSha256);

    /// <summary>
    /// Cleans up old delivery records.
    /// </summary>
    Task<int> CleanupOldDeliveriesAsync(int daysToKeep = 30, CancellationToken ct = default);
}

/// <summary>
/// Result of triggering webhooks.
/// </summary>
public class WebhookTriggerResult
{
    public int WebhooksTriggered { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<WebhookDeliveryResult> Deliveries { get; set; } = [];
}

/// <summary>
/// Individual webhook delivery result.
/// </summary>
public class WebhookDeliveryResult
{
    public Guid WebhookId { get; set; }
    public Guid DeliveryId { get; set; }
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResponseBody { get; set; }
}

/// <summary>
/// Standard webhook event types.
/// </summary>
public static class WebhookEventTypes
{
    // Order events
    public const string OrderCreated = "order.created";
    public const string OrderPaid = "order.paid";
    public const string OrderShipped = "order.shipped";
    public const string OrderDelivered = "order.delivered";
    public const string OrderCancelled = "order.cancelled";
    public const string OrderRefunded = "order.refunded";

    // Product events
    public const string ProductCreated = "product.created";
    public const string ProductUpdated = "product.updated";
    public const string ProductDeleted = "product.deleted";
    public const string ProductOutOfStock = "product.out_of_stock";
    public const string ProductLowStock = "product.low_stock";

    // Customer events
    public const string CustomerRegistered = "customer.registered";
    public const string CustomerUpdated = "customer.updated";

    // Cart events
    public const string CartAbandoned = "cart.abandoned";
    public const string CheckoutStarted = "checkout.started";
    public const string CheckoutCompleted = "checkout.completed";

    // Return events
    public const string ReturnRequested = "return.requested";
    public const string ReturnApproved = "return.approved";
    public const string ReturnCompleted = "return.completed";

    // Gift card events
    public const string GiftCardIssued = "giftcard.issued";
    public const string GiftCardRedeemed = "giftcard.redeemed";
}
