using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for webhook operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Webhooks}")]
public class WebhookManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhookManagementApiController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    /// <summary>
    /// Gets all webhooks.
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType<IReadOnlyList<Webhook>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeId = null)
    {
        if (storeId.HasValue)
        {
            var webhooks = await _webhookService.GetByStoreAsync(storeId.Value);
            return Ok(webhooks);
        }
        // Return active webhooks by default
        var activeWebhooks = await _webhookService.GetActiveAsync();
        return Ok(activeWebhooks);
    }

    /// <summary>
    /// Gets all active webhooks.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType<IReadOnlyList<Webhook>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var webhooks = await _webhookService.GetActiveAsync();
        return Ok(webhooks);
    }

    /// <summary>
    /// Gets a webhook by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Webhook>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var webhook = await _webhookService.GetByIdAsync(id);
        if (webhook == null)
        {
            return NotFound();
        }
        return Ok(webhook);
    }

    /// <summary>
    /// Gets webhooks by store.
    /// </summary>
    [HttpGet("by-store/{storeId:guid}")]
    [ProducesResponseType<IReadOnlyList<Webhook>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var webhooks = await _webhookService.GetByStoreAsync(storeId);
        return Ok(webhooks);
    }

    /// <summary>
    /// Gets webhooks subscribed to an event.
    /// </summary>
    [HttpGet("by-event/{eventType}")]
    [ProducesResponseType<IReadOnlyList<Webhook>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEvent(string eventType, [FromQuery] Guid? storeId = null)
    {
        var webhooks = await _webhookService.GetByEventAsync(eventType, storeId);
        return Ok(webhooks);
    }

    /// <summary>
    /// Creates a new webhook.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<Webhook>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateWebhookRequest request)
    {
        var webhook = new Webhook
        {
            Name = request.Name,
            Description = request.Description,
            Url = request.Url,
            StoreId = request.StoreId,
            IsActive = request.IsActive ?? true,
            EventsJson = System.Text.Json.JsonSerializer.Serialize(request.Events ?? []),
            SubscribeToAll = request.SubscribeToAll ?? false,
            HttpMethod = request.HttpMethod ?? "POST",
            ContentType = request.ContentType ?? "application/json",
            TimeoutSeconds = request.TimeoutSeconds ?? 30,
            RetryEnabled = request.RetryEnabled ?? true,
            MaxRetries = request.MaxRetries ?? 3,
            AuthType = request.AuthType ?? WebhookAuthType.HmacSha256
        };

        var created = await _webhookService.CreateAsync(webhook);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a webhook.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<Webhook>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWebhookRequest request)
    {
        var webhook = await _webhookService.GetByIdAsync(id);
        if (webhook == null)
        {
            return NotFound();
        }

        webhook.Name = request.Name ?? webhook.Name;
        webhook.Description = request.Description ?? webhook.Description;
        webhook.Url = request.Url ?? webhook.Url;
        webhook.IsActive = request.IsActive ?? webhook.IsActive;

        if (request.Events != null)
        {
            webhook.EventsJson = System.Text.Json.JsonSerializer.Serialize(request.Events);
        }

        var updated = await _webhookService.UpdateAsync(webhook);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a webhook.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _webhookService.DeleteAsync(id);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Tests a webhook.
    /// </summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType<WebhookDeliveryResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Test(Guid id)
    {
        var result = await _webhookService.TestAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Triggers a webhook event.
    /// </summary>
    [HttpPost("trigger")]
    [ProducesResponseType<WebhookTriggerResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Trigger([FromBody] TriggerWebhookRequest request)
    {
        var result = await _webhookService.TriggerAsync(request.EventType, request.Payload, request.StoreId);
        return Ok(result);
    }

    /// <summary>
    /// Gets webhook deliveries.
    /// </summary>
    [HttpGet("{id:guid}/deliveries")]
    [ProducesResponseType<IReadOnlyList<WebhookDelivery>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveries(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var deliveries = await _webhookService.GetDeliveriesAsync(id, skip, take);
        return Ok(deliveries);
    }

    /// <summary>
    /// Retries a failed delivery.
    /// </summary>
    [HttpPost("deliveries/{deliveryId:guid}/retry")]
    [ProducesResponseType<WebhookDeliveryResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RetryDelivery(Guid deliveryId)
    {
        var result = await _webhookService.RetryDeliveryAsync(deliveryId);
        return Ok(result);
    }

    /// <summary>
    /// Gets auto-disabled webhooks.
    /// </summary>
    [HttpGet("auto-disabled")]
    [ProducesResponseType<IReadOnlyList<Webhook>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAutoDisabled()
    {
        var webhooks = await _webhookService.GetAutoDisabledAsync();
        return Ok(webhooks);
    }

    /// <summary>
    /// Re-enables an auto-disabled webhook.
    /// </summary>
    [HttpPost("{id:guid}/re-enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReEnable(Guid id)
    {
        var result = await _webhookService.ReEnableAsync(id);
        if (!result)
        {
            return BadRequest(new { error = "Failed to re-enable webhook" });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// Generates a new webhook secret.
    /// </summary>
    [HttpGet("generate-secret")]
    [ProducesResponseType<WebhookSecretResult>(StatusCodes.Status200OK)]
    public IActionResult GenerateSecret()
    {
        var secret = _webhookService.GenerateSecret();
        return Ok(new WebhookSecretResult { Secret = secret });
    }

    /// <summary>
    /// Verifies a webhook signature.
    /// </summary>
    [HttpPost("verify-signature")]
    [ProducesResponseType<SignatureVerificationResult>(StatusCodes.Status200OK)]
    public IActionResult VerifySignature([FromBody] VerifySignatureRequest request)
    {
        var isValid = _webhookService.VerifySignature(
            request.Payload,
            request.Signature,
            request.Secret,
            request.AuthType ?? WebhookAuthType.HmacSha256);

        return Ok(new SignatureVerificationResult { IsValid = isValid });
    }

    /// <summary>
    /// Processes pending retries.
    /// </summary>
    [HttpPost("process-retries")]
    [ProducesResponseType<ProcessRetriesResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessRetries()
    {
        var count = await _webhookService.ProcessPendingRetriesAsync();
        return Ok(new ProcessRetriesResult { ProcessedCount = count });
    }

    /// <summary>
    /// Cleans up old deliveries.
    /// </summary>
    [HttpPost("cleanup")]
    [ProducesResponseType<CleanupResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Cleanup([FromQuery] int daysToKeep = 30)
    {
        var count = await _webhookService.CleanupOldDeliveriesAsync(daysToKeep);
        return Ok(new CleanupResult { DeletedCount = count });
    }
}

#region Request/Response Models

public class CreateWebhookRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Url { get; set; }
    public Guid? StoreId { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Events { get; set; }
    public bool? SubscribeToAll { get; set; }
    public string? HttpMethod { get; set; }
    public string? ContentType { get; set; }
    public int? TimeoutSeconds { get; set; }
    public bool? RetryEnabled { get; set; }
    public int? MaxRetries { get; set; }
    public WebhookAuthType? AuthType { get; set; }
}

public class UpdateWebhookRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Events { get; set; }
}

public class TriggerWebhookRequest
{
    public required string EventType { get; set; }
    public object Payload { get; set; } = new { };
    public Guid? StoreId { get; set; }
}

public class VerifySignatureRequest
{
    public required string Payload { get; set; }
    public required string Signature { get; set; }
    public required string Secret { get; set; }
    public WebhookAuthType? AuthType { get; set; }
}

public class WebhookSecretResult
{
    public required string Secret { get; set; }
}

public class SignatureVerificationResult
{
    public bool IsValid { get; set; }
}

public class ProcessRetriesResult
{
    public int ProcessedCount { get; set; }
}

public class CleanupResult
{
    public int DeletedCount { get; set; }
}

#endregion
