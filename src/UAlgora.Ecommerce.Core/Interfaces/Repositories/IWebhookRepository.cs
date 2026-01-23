using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Webhook operations.
/// </summary>
public interface IWebhookRepository : ISoftDeleteRepository<Webhook>
{
    /// <summary>
    /// Get webhooks by store.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default);

    /// <summary>
    /// Get active webhooks.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get webhooks subscribed to a specific event.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetByEventAsync(string eventType, Guid? storeId = null, CancellationToken ct = default);

    /// <summary>
    /// Get auto-disabled webhooks.
    /// </summary>
    Task<IReadOnlyList<Webhook>> GetAutoDisabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Get webhook deliveries.
    /// </summary>
    Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(Guid webhookId, int skip = 0, int take = 50, CancellationToken ct = default);

    /// <summary>
    /// Get recent failed deliveries for a webhook.
    /// </summary>
    Task<IReadOnlyList<WebhookDelivery>> GetFailedDeliveriesAsync(Guid webhookId, int take = 10, CancellationToken ct = default);

    /// <summary>
    /// Add a webhook delivery record.
    /// </summary>
    Task<WebhookDelivery> AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default);

    /// <summary>
    /// Update webhook statistics after a delivery.
    /// </summary>
    Task UpdateStatisticsAsync(Guid webhookId, bool isSuccess, int? statusCode, long durationMs, string? error = null, CancellationToken ct = default);

    /// <summary>
    /// Auto-disable a webhook after too many failures.
    /// </summary>
    Task<bool> AutoDisableAsync(Guid webhookId, string reason, CancellationToken ct = default);

    /// <summary>
    /// Re-enable a webhook.
    /// </summary>
    Task<bool> ReEnableAsync(Guid webhookId, CancellationToken ct = default);

    /// <summary>
    /// Get pending deliveries for retry.
    /// </summary>
    Task<IReadOnlyList<WebhookDelivery>> GetPendingRetriesAsync(int maxAttempts = 5, CancellationToken ct = default);

    /// <summary>
    /// Delete old delivery records.
    /// </summary>
    Task<int> DeleteOldDeliveriesAsync(DateTime cutoffDate, CancellationToken ct = default);
}
