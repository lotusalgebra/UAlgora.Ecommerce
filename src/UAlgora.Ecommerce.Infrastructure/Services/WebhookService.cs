using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for Webhook operations.
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public WebhookService(
        IWebhookRepository webhookRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookService> logger)
    {
        _webhookRepository = webhookRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Webhook?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _webhookRepository.GetByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<Webhook>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default)
    {
        return await _webhookRepository.GetByStoreAsync(storeId, ct);
    }

    public async Task<IReadOnlyList<Webhook>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _webhookRepository.GetActiveAsync(ct);
    }

    public async Task<IReadOnlyList<Webhook>> GetByEventAsync(string eventType, Guid? storeId = null, CancellationToken ct = default)
    {
        return await _webhookRepository.GetByEventAsync(eventType, storeId, ct);
    }

    public async Task<Webhook> CreateAsync(Webhook webhook, CancellationToken ct = default)
    {
        // Generate secret if not provided
        if (string.IsNullOrEmpty(webhook.Secret))
        {
            webhook.Secret = GenerateSecret();
        }

        return await _webhookRepository.AddAsync(webhook, ct);
    }

    public async Task<Webhook> UpdateAsync(Webhook webhook, CancellationToken ct = default)
    {
        return await _webhookRepository.UpdateAsync(webhook, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _webhookRepository.SoftDeleteAsync(id, ct);
    }

    public async Task<WebhookTriggerResult> TriggerAsync(string eventType, object payload, Guid? storeId = null, CancellationToken ct = default)
    {
        var result = new WebhookTriggerResult();

        // Get all webhooks subscribed to this event
        var webhooks = await _webhookRepository.GetByEventAsync(eventType, storeId, ct);

        result.WebhooksTriggered = webhooks.Count;

        foreach (var webhook in webhooks)
        {
            if (!webhook.IsActive || webhook.IsAutoDisabled)
            {
                continue;
            }

            var deliveryResult = await DeliverAsync(webhook, eventType, payload, ct);
            result.Deliveries.Add(deliveryResult);

            if (deliveryResult.Success)
            {
                result.SuccessCount++;
            }
            else
            {
                result.FailedCount++;
            }
        }

        return result;
    }

    private async Task<WebhookDeliveryResult> DeliverAsync(Webhook webhook, string eventType, object payload, CancellationToken ct)
    {
        var delivery = new WebhookDelivery
        {
            WebhookId = webhook.Id,
            EventType = eventType,
            StartedAt = DateTime.UtcNow,
            AttemptNumber = 0
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
            delivery.RequestPayload = payloadJson;

            using var client = _httpClientFactory.CreateClient("WebhookClient");
            client.Timeout = TimeSpan.FromSeconds(webhook.TimeoutSeconds);

            var request = new HttpRequestMessage(
                new HttpMethod(webhook.HttpMethod),
                webhook.Url);

            request.Content = new StringContent(payloadJson, Encoding.UTF8, webhook.ContentType);

            // Add authentication headers
            AddAuthenticationHeaders(request, webhook, payloadJson);

            // Add custom headers
            AddCustomHeaders(request, webhook);

            // Add standard webhook headers
            request.Headers.Add("X-Webhook-Event", eventType);
            request.Headers.Add("X-Webhook-Id", webhook.Id.ToString());
            request.Headers.Add("X-Webhook-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            delivery.RequestHeaders = SerializeHeaders(request.Headers);

            var response = await client.SendAsync(request, ct);
            stopwatch.Stop();

            delivery.StatusCode = (int)response.StatusCode;
            delivery.DurationMs = stopwatch.ElapsedMilliseconds;
            delivery.ResponseHeaders = SerializeHeaders(response.Headers);

            // Read response body (truncate if too long)
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            delivery.ResponseBody = responseBody.Length > 4000 ? responseBody[..4000] : responseBody;

            delivery.IsSuccess = response.IsSuccessStatusCode;
            delivery.CompletedAt = DateTime.UtcNow;

            if (!delivery.IsSuccess)
            {
                delivery.ErrorMessage = $"HTTP {delivery.StatusCode}: {response.ReasonPhrase}";
                delivery.ErrorType = "HttpError";
            }

            // Save delivery record
            await _webhookRepository.AddDeliveryAsync(delivery, ct);

            // Update webhook statistics
            await _webhookRepository.UpdateStatisticsAsync(
                webhook.Id,
                delivery.IsSuccess,
                delivery.StatusCode,
                delivery.DurationMs,
                delivery.ErrorMessage,
                ct);

            // Check for auto-disable
            if (!delivery.IsSuccess)
            {
                var updatedWebhook = await _webhookRepository.GetByIdAsync(webhook.Id, ct);
                if (updatedWebhook != null && updatedWebhook.ConsecutiveFailures >= updatedWebhook.MaxConsecutiveFailures)
                {
                    await _webhookRepository.AutoDisableAsync(
                        webhook.Id,
                        $"Auto-disabled after {updatedWebhook.ConsecutiveFailures} consecutive failures",
                        ct);

                    _logger.LogWarning("Webhook {WebhookId} auto-disabled after {Failures} consecutive failures",
                        webhook.Id, updatedWebhook.ConsecutiveFailures);
                }
            }

            return new WebhookDeliveryResult
            {
                WebhookId = webhook.Id,
                DeliveryId = delivery.Id,
                Success = delivery.IsSuccess,
                StatusCode = delivery.StatusCode,
                DurationMs = delivery.DurationMs,
                ErrorMessage = delivery.ErrorMessage,
                ResponseBody = delivery.ResponseBody
            };
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            delivery.DurationMs = stopwatch.ElapsedMilliseconds;
            delivery.IsSuccess = false;
            delivery.ErrorMessage = "Request timed out";
            delivery.ErrorType = "Timeout";
            delivery.CompletedAt = DateTime.UtcNow;

            await _webhookRepository.AddDeliveryAsync(delivery, ct);
            await _webhookRepository.UpdateStatisticsAsync(webhook.Id, false, null, delivery.DurationMs, delivery.ErrorMessage, ct);

            return new WebhookDeliveryResult
            {
                WebhookId = webhook.Id,
                DeliveryId = delivery.Id,
                Success = false,
                DurationMs = delivery.DurationMs,
                ErrorMessage = delivery.ErrorMessage
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            delivery.DurationMs = stopwatch.ElapsedMilliseconds;
            delivery.IsSuccess = false;
            delivery.ErrorMessage = ex.Message;
            delivery.ErrorType = "HttpRequestError";
            delivery.CompletedAt = DateTime.UtcNow;

            await _webhookRepository.AddDeliveryAsync(delivery, ct);
            await _webhookRepository.UpdateStatisticsAsync(webhook.Id, false, null, delivery.DurationMs, delivery.ErrorMessage, ct);

            _logger.LogError(ex, "Webhook delivery failed for {WebhookId}", webhook.Id);

            return new WebhookDeliveryResult
            {
                WebhookId = webhook.Id,
                DeliveryId = delivery.Id,
                Success = false,
                DurationMs = delivery.DurationMs,
                ErrorMessage = delivery.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            delivery.DurationMs = stopwatch.ElapsedMilliseconds;
            delivery.IsSuccess = false;
            delivery.ErrorMessage = ex.Message;
            delivery.ErrorType = ex.GetType().Name;
            delivery.CompletedAt = DateTime.UtcNow;

            await _webhookRepository.AddDeliveryAsync(delivery, ct);
            await _webhookRepository.UpdateStatisticsAsync(webhook.Id, false, null, delivery.DurationMs, delivery.ErrorMessage, ct);

            _logger.LogError(ex, "Unexpected error during webhook delivery for {WebhookId}", webhook.Id);

            return new WebhookDeliveryResult
            {
                WebhookId = webhook.Id,
                DeliveryId = delivery.Id,
                Success = false,
                DurationMs = delivery.DurationMs,
                ErrorMessage = delivery.ErrorMessage
            };
        }
    }

    private void AddAuthenticationHeaders(HttpRequestMessage request, Webhook webhook, string payload)
    {
        switch (webhook.AuthType)
        {
            case WebhookAuthType.HmacSha256:
                if (!string.IsNullOrEmpty(webhook.Secret))
                {
                    var signature = ComputeHmacSha256(payload, webhook.Secret);
                    request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");
                }
                break;

            case WebhookAuthType.HmacSha512:
                if (!string.IsNullOrEmpty(webhook.Secret))
                {
                    var signature = ComputeHmacSha512(payload, webhook.Secret);
                    request.Headers.Add("X-Webhook-Signature", $"sha512={signature}");
                }
                break;

            case WebhookAuthType.ApiKey:
                if (!string.IsNullOrEmpty(webhook.ApiKey))
                {
                    request.Headers.Add("X-API-Key", webhook.ApiKey);
                }
                break;

            case WebhookAuthType.BasicAuth:
                if (!string.IsNullOrEmpty(webhook.BasicAuthUsername) && !string.IsNullOrEmpty(webhook.BasicAuthPassword))
                {
                    var credentials = Convert.ToBase64String(
                        Encoding.ASCII.GetBytes($"{webhook.BasicAuthUsername}:{webhook.BasicAuthPassword}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case WebhookAuthType.BearerToken:
                if (!string.IsNullOrEmpty(webhook.BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", webhook.BearerToken);
                }
                break;
        }
    }

    private void AddCustomHeaders(HttpRequestMessage request, Webhook webhook)
    {
        if (string.IsNullOrEmpty(webhook.HeadersJson))
        {
            return;
        }

        try
        {
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(webhook.HeadersJson);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse custom headers for webhook {WebhookId}", webhook.Id);
        }
    }

    private static string SerializeHeaders(HttpHeaders headers)
    {
        var dict = headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
        return JsonSerializer.Serialize(dict, JsonOptions);
    }

    public async Task<WebhookDeliveryResult> RetryDeliveryAsync(Guid deliveryId, CancellationToken ct = default)
    {
        var deliveries = await _webhookRepository.GetDeliveriesAsync(Guid.Empty, 0, int.MaxValue, ct);
        var originalDelivery = deliveries.FirstOrDefault(d => d.Id == deliveryId);

        if (originalDelivery == null)
        {
            return new WebhookDeliveryResult
            {
                Success = false,
                ErrorMessage = "Delivery not found"
            };
        }

        var webhook = await _webhookRepository.GetByIdAsync(originalDelivery.WebhookId, ct);
        if (webhook == null)
        {
            return new WebhookDeliveryResult
            {
                Success = false,
                ErrorMessage = "Webhook not found"
            };
        }

        // Parse original payload
        object? payload = null;
        if (!string.IsNullOrEmpty(originalDelivery.RequestPayload))
        {
            payload = JsonSerializer.Deserialize<object>(originalDelivery.RequestPayload, JsonOptions);
        }

        return await DeliverAsync(webhook, originalDelivery.EventType, payload ?? new { }, ct);
    }

    public async Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(Guid webhookId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _webhookRepository.GetDeliveriesAsync(webhookId, skip, take, ct);
    }

    public async Task<WebhookDeliveryResult> TestAsync(Guid webhookId, CancellationToken ct = default)
    {
        var webhook = await _webhookRepository.GetByIdAsync(webhookId, ct);
        if (webhook == null)
        {
            return new WebhookDeliveryResult
            {
                Success = false,
                ErrorMessage = "Webhook not found"
            };
        }

        var testPayload = new
        {
            eventType = "test.ping",
            timestamp = DateTime.UtcNow,
            webhookId = webhookId,
            message = "This is a test webhook delivery from Algora Commerce",
            data = new
            {
                testId = Guid.NewGuid(),
                sampleOrder = new
                {
                    id = Guid.NewGuid(),
                    orderNumber = "TEST-001",
                    total = 99.99m,
                    currency = "USD"
                }
            }
        };

        return await DeliverAsync(webhook, "test.ping", testPayload, ct);
    }

    public async Task<bool> ReEnableAsync(Guid webhookId, CancellationToken ct = default)
    {
        return await _webhookRepository.ReEnableAsync(webhookId, ct);
    }

    public async Task<IReadOnlyList<Webhook>> GetAutoDisabledAsync(CancellationToken ct = default)
    {
        return await _webhookRepository.GetAutoDisabledAsync(ct);
    }

    public async Task<int> ProcessPendingRetriesAsync(CancellationToken ct = default)
    {
        var pendingDeliveries = await _webhookRepository.GetPendingRetriesAsync(ct: ct);
        var processed = 0;

        foreach (var delivery in pendingDeliveries)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var webhook = await _webhookRepository.GetByIdAsync(delivery.WebhookId, ct);
            if (webhook == null || !webhook.IsActive || webhook.IsAutoDisabled || !webhook.RetryEnabled)
            {
                continue;
            }

            // Check if we've exceeded max retries
            if (delivery.AttemptNumber >= webhook.MaxRetries)
            {
                continue;
            }

            // Calculate delay with optional exponential backoff
            var delay = webhook.RetryDelaySeconds;
            if (webhook.UseExponentialBackoff)
            {
                delay = delay * (int)Math.Pow(2, delivery.AttemptNumber);
            }

            // Check if enough time has passed since last attempt
            var nextRetryTime = delivery.CompletedAt?.AddSeconds(delay) ?? DateTime.UtcNow;
            if (DateTime.UtcNow < nextRetryTime)
            {
                continue;
            }

            // Retry the delivery
            object? payload = null;
            if (!string.IsNullOrEmpty(delivery.RequestPayload))
            {
                try
                {
                    payload = JsonSerializer.Deserialize<object>(delivery.RequestPayload, JsonOptions);
                }
                catch
                {
                    continue;
                }
            }

            await DeliverAsync(webhook, delivery.EventType, payload ?? new { }, ct);
            processed++;
        }

        return processed;
    }

    public string GenerateSecret()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public bool VerifySignature(string payload, string signature, string secret, WebhookAuthType authType = WebhookAuthType.HmacSha256)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        string computedSignature;
        string expectedPrefix;

        switch (authType)
        {
            case WebhookAuthType.HmacSha256:
                computedSignature = ComputeHmacSha256(payload, secret);
                expectedPrefix = "sha256=";
                break;

            case WebhookAuthType.HmacSha512:
                computedSignature = ComputeHmacSha512(payload, secret);
                expectedPrefix = "sha512=";
                break;

            default:
                return false;
        }

        // Remove prefix if present
        var providedSignature = signature.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
            ? signature[expectedPrefix.Length..]
            : signature;

        // Use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(providedSignature));
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeHmacSha512(string data, string secret)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<int> CleanupOldDeliveriesAsync(int daysToKeep = 30, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        return await _webhookRepository.DeleteOldDeliveriesAsync(cutoffDate, ct);
    }
}
