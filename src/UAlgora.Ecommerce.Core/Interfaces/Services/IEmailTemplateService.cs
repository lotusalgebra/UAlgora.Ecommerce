using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for Email Template operations.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a template by code.
    /// </summary>
    Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets the active template for an event type and language.
    /// </summary>
    Task<EmailTemplate?> GetTemplateAsync(EmailTemplateEventType eventType, string language = "en-US", Guid? storeId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets all templates for a store.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default);

    /// <summary>
    /// Gets all templates by event type.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetByEventTypeAsync(EmailTemplateEventType eventType, CancellationToken ct = default);

    /// <summary>
    /// Creates a new email template.
    /// </summary>
    Task<EmailTemplate> CreateAsync(EmailTemplate template, CancellationToken ct = default);

    /// <summary>
    /// Updates an email template.
    /// </summary>
    Task<EmailTemplate> UpdateAsync(EmailTemplate template, CancellationToken ct = default);

    /// <summary>
    /// Deletes a template (soft delete).
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Renders a template with data.
    /// </summary>
    Task<RenderedEmail> RenderAsync(EmailTemplateEventType eventType, object data, string language = "en-US", Guid? storeId = null, CancellationToken ct = default);

    /// <summary>
    /// Renders a specific template with data.
    /// </summary>
    Task<RenderedEmail> RenderAsync(Guid templateId, object data, CancellationToken ct = default);

    /// <summary>
    /// Sends an email using a template.
    /// </summary>
    Task<EmailSendResult> SendAsync(EmailTemplateEventType eventType, string toEmail, object data, string language = "en-US", Guid? storeId = null, CancellationToken ct = default);

    /// <summary>
    /// Sends a test email.
    /// </summary>
    Task<EmailSendResult> SendTestAsync(Guid templateId, string toEmail, CancellationToken ct = default);

    /// <summary>
    /// Gets available variables for a template type.
    /// </summary>
    IReadOnlyDictionary<string, string> GetAvailableVariables(EmailTemplateEventType eventType);

    /// <summary>
    /// Gets default templates (not store-specific).
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetDefaultTemplatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Duplicates a template.
    /// </summary>
    Task<EmailTemplate> DuplicateAsync(Guid templateId, string newCode, Guid? storeId = null, CancellationToken ct = default);
}

/// <summary>
/// Rendered email content.
/// </summary>
public class RenderedEmail
{
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? Preheader { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public List<string>? BccEmails { get; set; }
}

/// <summary>
/// Email send result.
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }

    public static EmailSendResult Successful(string? messageId = null) => new()
    {
        Success = true,
        MessageId = messageId
    };

    public static EmailSendResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
