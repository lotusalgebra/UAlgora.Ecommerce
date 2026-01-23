using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for email template operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.EmailTemplates}")]
public class EmailTemplateManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailTemplateManagementApiController(IEmailTemplateService emailTemplateService)
    {
        _emailTemplateService = emailTemplateService;
    }

    /// <summary>
    /// Gets email templates by store.
    /// </summary>
    [HttpGet("by-store")]
    [ProducesResponseType<IReadOnlyList<EmailTemplate>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStore([FromQuery] Guid? storeId = null)
    {
        var templates = await _emailTemplateService.GetByStoreAsync(storeId);
        return Ok(templates);
    }

    /// <summary>
    /// Gets default templates (not store-specific).
    /// </summary>
    [HttpGet("defaults")]
    [ProducesResponseType<IReadOnlyList<EmailTemplate>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefaults()
    {
        var templates = await _emailTemplateService.GetDefaultTemplatesAsync();
        return Ok(templates);
    }

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmailTemplate>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var template = await _emailTemplateService.GetByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    /// <summary>
    /// Gets a template by code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType<EmailTemplate>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var template = await _emailTemplateService.GetByCodeAsync(code);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    /// <summary>
    /// Gets templates by event type.
    /// </summary>
    [HttpGet("by-event/{eventType}")]
    [ProducesResponseType<IReadOnlyList<EmailTemplate>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEventType(EmailTemplateEventType eventType)
    {
        var templates = await _emailTemplateService.GetByEventTypeAsync(eventType);
        return Ok(templates);
    }

    /// <summary>
    /// Gets the active template for an event type and language.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType<EmailTemplate>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveTemplate(
        [FromQuery] EmailTemplateEventType eventType,
        [FromQuery] string language = "en-US",
        [FromQuery] Guid? storeId = null)
    {
        var template = await _emailTemplateService.GetTemplateAsync(eventType, language, storeId);
        if (template == null)
        {
            return NotFound(new { error = $"No active template found for event type {eventType}" });
        }
        return Ok(template);
    }

    /// <summary>
    /// Creates a new email template.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<EmailTemplate>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEmailTemplateRequest request)
    {
        var template = new EmailTemplate
        {
            StoreId = request.StoreId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            EventType = request.EventType,
            Language = request.Language ?? "en-US",
            Subject = request.Subject,
            BodyHtml = request.BodyHtml,
            BodyText = request.BodyText,
            Preheader = request.Preheader,
            FromEmail = request.FromEmail,
            FromName = request.FromName,
            ReplyToEmail = request.ReplyToEmail,
            BccEmails = request.BccEmails,
            IsActive = request.IsActive ?? true,
            IsDefault = request.IsDefault ?? false,
            Priority = request.Priority ?? 0,
            DelayMinutes = request.DelayMinutes,
            LayoutTemplate = request.LayoutTemplate,
            CustomCss = request.CustomCss,
            HeaderImageUrl = request.HeaderImageUrl,
            FooterHtml = request.FooterHtml
        };

        var created = await _emailTemplateService.CreateAsync(template);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an email template.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<EmailTemplate>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmailTemplateRequest request)
    {
        var template = await _emailTemplateService.GetByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        // Update content
        if (!string.IsNullOrEmpty(request.Name))
            template.Name = request.Name;
        if (request.Description != null)
            template.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Subject))
            template.Subject = request.Subject;
        if (request.BodyHtml != null)
            template.BodyHtml = request.BodyHtml;
        if (request.BodyText != null)
            template.BodyText = request.BodyText;
        if (request.Preheader != null)
            template.Preheader = request.Preheader;

        // Update sender info
        if (request.FromEmail != null)
            template.FromEmail = request.FromEmail;
        if (request.FromName != null)
            template.FromName = request.FromName;
        if (request.ReplyToEmail != null)
            template.ReplyToEmail = request.ReplyToEmail;

        // Update settings
        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;
        if (request.IsDefault.HasValue)
            template.IsDefault = request.IsDefault.Value;
        if (request.Priority.HasValue)
            template.Priority = request.Priority.Value;

        // Update design
        if (request.CustomCss != null)
            template.CustomCss = request.CustomCss;
        if (request.HeaderImageUrl != null)
            template.HeaderImageUrl = request.HeaderImageUrl;
        if (request.FooterHtml != null)
            template.FooterHtml = request.FooterHtml;

        var updated = await _emailTemplateService.UpdateAsync(template);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an email template (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _emailTemplateService.DeleteAsync(id);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Renders a template with sample data.
    /// </summary>
    [HttpPost("{id:guid}/render")]
    [ProducesResponseType<RenderedEmail>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Render(Guid id, [FromBody] RenderTemplateRequest? request = null)
    {
        var data = request?.Data ?? new { };
        var rendered = await _emailTemplateService.RenderAsync(id, data);
        return Ok(rendered);
    }

    /// <summary>
    /// Renders a template by event type.
    /// </summary>
    [HttpPost("render-by-event")]
    [ProducesResponseType<RenderedEmail>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RenderByEventType([FromBody] RenderByEventRequest request)
    {
        var rendered = await _emailTemplateService.RenderAsync(
            request.EventType,
            request.Data ?? new { },
            request.Language ?? "en-US",
            request.StoreId);
        return Ok(rendered);
    }

    /// <summary>
    /// Sends a test email.
    /// </summary>
    [HttpPost("{id:guid}/send-test")]
    [ProducesResponseType<EmailSendResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendTest(Guid id, [FromBody] SendTestEmailRequest request)
    {
        var result = await _emailTemplateService.SendTestAsync(id, request.ToEmail);
        return Ok(result);
    }

    /// <summary>
    /// Sends an email using a template.
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType<EmailSendResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Send([FromBody] SendEmailRequest request)
    {
        var result = await _emailTemplateService.SendAsync(
            request.EventType,
            request.ToEmail,
            request.Data ?? new { },
            request.Language ?? "en-US",
            request.StoreId);
        return Ok(result);
    }

    /// <summary>
    /// Gets available variables for a template type.
    /// </summary>
    [HttpGet("variables/{eventType}")]
    [ProducesResponseType<IReadOnlyDictionary<string, string>>(StatusCodes.Status200OK)]
    public IActionResult GetAvailableVariables(EmailTemplateEventType eventType)
    {
        var variables = _emailTemplateService.GetAvailableVariables(eventType);
        return Ok(variables);
    }

    /// <summary>
    /// Duplicates a template.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType<EmailTemplate>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Duplicate(Guid id, [FromBody] DuplicateTemplateRequest request)
    {
        var template = await _emailTemplateService.DuplicateAsync(id, request.NewCode, request.StoreId);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
    }

    /// <summary>
    /// Gets all event types with their names.
    /// </summary>
    [HttpGet("event-types")]
    [ProducesResponseType<IReadOnlyList<EventTypeInfo>>(StatusCodes.Status200OK)]
    public IActionResult GetEventTypes()
    {
        var eventTypes = Enum.GetValues<EmailTemplateEventType>()
            .Select(e => new EventTypeInfo
            {
                Value = (int)e,
                Name = e.ToString(),
                Category = GetEventCategory(e)
            })
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Value)
            .ToList();

        return Ok(eventTypes);
    }

    private static string GetEventCategory(EmailTemplateEventType eventType)
    {
        var value = (int)eventType;
        return value switch
        {
            >= 100 and < 200 => "Order",
            >= 200 and < 300 => "Customer",
            >= 300 and < 400 => "Cart",
            >= 400 and < 500 => "Gift Card",
            >= 500 and < 600 => "Return",
            >= 600 and < 700 => "Inventory",
            >= 700 and < 800 => "Wishlist",
            >= 800 and < 900 => "Subscription",
            >= 900 and < 1000 => "Admin",
            _ => "Custom"
        };
    }
}

#region Request/Response Models

public class CreateEmailTemplateRequest
{
    public Guid? StoreId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public EmailTemplateEventType EventType { get; set; }
    public string? Language { get; set; }
    public required string Subject { get; set; }
    public string? BodyHtml { get; set; }
    public string? BodyText { get; set; }
    public string? Preheader { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public string? BccEmails { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
    public int? Priority { get; set; }
    public int? DelayMinutes { get; set; }
    public string? LayoutTemplate { get; set; }
    public string? CustomCss { get; set; }
    public string? HeaderImageUrl { get; set; }
    public string? FooterHtml { get; set; }
}

public class UpdateEmailTemplateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Subject { get; set; }
    public string? BodyHtml { get; set; }
    public string? BodyText { get; set; }
    public string? Preheader { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
    public int? Priority { get; set; }
    public string? CustomCss { get; set; }
    public string? HeaderImageUrl { get; set; }
    public string? FooterHtml { get; set; }
}

public class RenderTemplateRequest
{
    public object? Data { get; set; }
}

public class RenderByEventRequest
{
    public EmailTemplateEventType EventType { get; set; }
    public object? Data { get; set; }
    public string? Language { get; set; }
    public Guid? StoreId { get; set; }
}

public class SendTestEmailRequest
{
    public required string ToEmail { get; set; }
}

public class SendEmailRequest
{
    public EmailTemplateEventType EventType { get; set; }
    public required string ToEmail { get; set; }
    public object? Data { get; set; }
    public string? Language { get; set; }
    public Guid? StoreId { get; set; }
}

public class DuplicateTemplateRequest
{
    public required string NewCode { get; set; }
    public Guid? StoreId { get; set; }
}

public class EventTypeInfo
{
    public int Value { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
}

#endregion
