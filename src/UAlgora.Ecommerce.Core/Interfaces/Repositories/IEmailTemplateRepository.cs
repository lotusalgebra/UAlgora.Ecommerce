using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Email Template operations.
/// </summary>
public interface IEmailTemplateRepository : ISoftDeleteRepository<EmailTemplate>
{
    /// <summary>
    /// Get an email template by code.
    /// </summary>
    Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Get email templates by store.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default);

    /// <summary>
    /// Get email templates by event type.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetByEventTypeAsync(EmailTemplateEventType eventType, CancellationToken ct = default);

    /// <summary>
    /// Get the active email template for a specific event type and language.
    /// </summary>
    Task<EmailTemplate?> GetActiveTemplateAsync(EmailTemplateEventType eventType, string language, Guid? storeId = null, CancellationToken ct = default);

    /// <summary>
    /// Get all active email templates.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get email templates by language.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetByLanguageAsync(string language, CancellationToken ct = default);

    /// <summary>
    /// Check if a template code already exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Increment the send count for a template.
    /// </summary>
    Task IncrementSendCountAsync(Guid templateId, CancellationToken ct = default);

    /// <summary>
    /// Get default templates (not store-specific).
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetDefaultTemplatesAsync(CancellationToken ct = default);
}
