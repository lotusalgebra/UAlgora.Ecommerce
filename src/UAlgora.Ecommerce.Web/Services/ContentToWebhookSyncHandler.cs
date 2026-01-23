using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using AlgoraWebhook = UAlgora.Ecommerce.Core.Models.Domain.Webhook;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Notification handler that syncs Umbraco Webhook content to the database.
/// </summary>
public sealed class ContentToWebhookSyncHandler :
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly ILogger<ContentToWebhookSyncHandler> _logger;

    public ContentToWebhookSyncHandler(
        IWebhookRepository webhookRepository,
        ILogger<ContentToWebhookSyncHandler> logger)
    {
        _webhookRepository = webhookRepository;
        _logger = logger;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.PublishedEntities)
        {
            if (!IsAlgoraWebhook(content))
                continue;

            await SyncWebhookToDatabaseAsync(content, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.UnpublishedEntities)
        {
            if (!IsAlgoraWebhook(content))
                continue;

            await DeactivateWebhookAsync(content.Id, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.DeletedEntities)
        {
            if (!IsAlgoraWebhook(content))
                continue;

            await DeleteWebhookAsync(content.Id, cancellationToken);
        }
    }

    private bool IsAlgoraWebhook(IContent content)
    {
        return content.ContentType.Alias == AlgoraDocumentTypeConstants.WebhookAlias;
    }

    private async Task SyncWebhookToDatabaseAsync(IContent content, CancellationToken ct)
    {
        try
        {
            var name = content.GetValue<string>("webhookName");
            var url = content.GetValue<string>("url");

            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Cannot sync webhook without URL. Content ID: {ContentId}", content.Id);
                return;
            }

            // Find by URL since webhooks don't have a unique code
            var existingWebhooks = await _webhookRepository.GetActiveAsync(ct);
            var existingWebhook = existingWebhooks.FirstOrDefault(w => w.UmbracoNodeId == content.Id);

            if (existingWebhook != null)
            {
                MapContentToWebhook(content, existingWebhook);
                await _webhookRepository.UpdateAsync(existingWebhook, ct);
                _logger.LogInformation("Updated webhook in database: {Name} (Umbraco Node: {NodeId})", name, content.Id);
            }
            else
            {
                var newWebhook = new AlgoraWebhook();
                MapContentToWebhook(content, newWebhook);
                await _webhookRepository.AddAsync(newWebhook, ct);
                _logger.LogInformation("Created webhook in database: {Name} (Umbraco Node: {NodeId})", name, content.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing webhook to database. Content ID: {ContentId}", content.Id);
        }
    }

    private async Task DeactivateWebhookAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var webhooks = await _webhookRepository.GetActiveAsync(ct);
            var webhook = webhooks.FirstOrDefault(w => w.UmbracoNodeId == contentId);
            if (webhook != null)
            {
                webhook.IsActive = false;
                await _webhookRepository.UpdateAsync(webhook, ct);
                _logger.LogInformation("Deactivated webhook: {Name}", webhook.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating webhook. Content ID: {ContentId}", contentId);
        }
    }

    private async Task DeleteWebhookAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var webhooks = await _webhookRepository.GetActiveAsync(ct);
            var webhook = webhooks.FirstOrDefault(w => w.UmbracoNodeId == contentId);
            if (webhook != null)
            {
                await _webhookRepository.SoftDeleteAsync(webhook.Id, ct);
                _logger.LogInformation("Deleted webhook from database: {Name}", webhook.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook. Content ID: {ContentId}", contentId);
        }
    }

    private void MapContentToWebhook(IContent content, AlgoraWebhook webhook)
    {
        webhook.UmbracoNodeId = content.Id;
        webhook.Name = content.GetValue<string>("webhookName") ?? "";
        webhook.Description = content.GetValue<string>("description");
        webhook.Url = content.GetValue<string>("url") ?? "";
        webhook.HttpMethod = content.GetValue<string>("httpMethod") ?? "POST";
        webhook.ContentType = content.GetValue<string>("contentType") ?? "application/json";
        webhook.IsActive = content.GetValue<bool>("isActive");

        // Events
        webhook.SubscribeToAll = content.GetValue<bool>("subscribeToAll");
        var eventsStr = content.GetValue<string>("subscribedEvents");
        if (!string.IsNullOrEmpty(eventsStr))
        {
            // Store as JSON array
            var eventsList = eventsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            webhook.EventsJson = System.Text.Json.JsonSerializer.Serialize(eventsList);
        }
        webhook.FilterJson = content.GetValue<string>("filterJson");

        // Security
        var authTypeStr = content.GetValue<string>("authType");
        if (!string.IsNullOrEmpty(authTypeStr) && Enum.TryParse<WebhookAuthType>(authTypeStr, true, out var authType))
        {
            webhook.AuthType = authType;
        }
        webhook.Secret = content.GetValue<string>("secret");
        webhook.BearerToken = content.GetValue<string>("bearerToken");
        webhook.BasicAuthUsername = content.GetValue<string>("basicAuthUsername");
        webhook.BasicAuthPassword = content.GetValue<string>("basicAuthPassword");
        webhook.HeadersJson = content.GetValue<string>("customHeaders");
        webhook.VerifySsl = content.GetValue<bool>("verifySsl");

        // Retry
        webhook.RetryEnabled = content.GetValue<bool>("retryEnabled");
        webhook.MaxRetries = GetNullableInt(content, "maxRetries") ?? 3;
        webhook.RetryDelaySeconds = GetNullableInt(content, "retryDelaySeconds") ?? 60;
        webhook.UseExponentialBackoff = content.GetValue<bool>("useExponentialBackoff");
        webhook.TimeoutSeconds = GetNullableInt(content, "timeoutSeconds") ?? 30;
        webhook.MaxConsecutiveFailures = GetNullableInt(content, "maxConsecutiveFailures") ?? 10;

        // Stats (read-only, but sync them back)
        webhook.TotalDeliveries = content.GetValue<int>("totalDeliveries");
        webhook.SuccessfulDeliveries = content.GetValue<int>("successfulDeliveries");
        webhook.FailedDeliveries = content.GetValue<int>("failedDeliveries");
        webhook.LastTriggeredAt = GetNullableDateTime(content, "lastTriggeredAt");
        webhook.LastStatusCode = GetNullableInt(content, "lastStatusCode");
        webhook.LastError = content.GetValue<string>("lastError");
    }

    private static int? GetNullableInt(IContent content, string alias)
    {
        var value = content.GetValue<int>(alias);
        return value == 0 ? null : value;
    }

    private static DateTime? GetNullableDateTime(IContent content, string alias)
    {
        var value = content.GetValue<DateTime>(alias);
        return value == default ? null : value;
    }
}
