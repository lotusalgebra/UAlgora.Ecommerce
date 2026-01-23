using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Notification handler that syncs Umbraco Email Template content to the database.
/// </summary>
public sealed class ContentToEmailTemplateSyncHandler :
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly ILogger<ContentToEmailTemplateSyncHandler> _logger;

    public ContentToEmailTemplateSyncHandler(
        IEmailTemplateRepository emailTemplateRepository,
        ILogger<ContentToEmailTemplateSyncHandler> logger)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _logger = logger;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.PublishedEntities)
        {
            if (!IsAlgoraEmailTemplate(content))
                continue;

            await SyncEmailTemplateToDatabaseAsync(content, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.UnpublishedEntities)
        {
            if (!IsAlgoraEmailTemplate(content))
                continue;

            await DeactivateEmailTemplateAsync(content.Id, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.DeletedEntities)
        {
            if (!IsAlgoraEmailTemplate(content))
                continue;

            await DeleteEmailTemplateAsync(content.Id, cancellationToken);
        }
    }

    private bool IsAlgoraEmailTemplate(IContent content)
    {
        return content.ContentType.Alias == AlgoraDocumentTypeConstants.EmailTemplateAlias;
    }

    private async Task SyncEmailTemplateToDatabaseAsync(IContent content, CancellationToken ct)
    {
        try
        {
            var code = content.GetValue<string>("templateCode");
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogWarning("Cannot sync email template without code. Content ID: {ContentId}", content.Id);
                return;
            }

            var existingTemplate = await _emailTemplateRepository.GetByCodeAsync(code, ct: ct);

            if (existingTemplate != null)
            {
                MapContentToEmailTemplate(content, existingTemplate);
                await _emailTemplateRepository.UpdateAsync(existingTemplate, ct);
                _logger.LogInformation("Updated email template in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
            else
            {
                var newTemplate = new EmailTemplate();
                MapContentToEmailTemplate(content, newTemplate);
                await _emailTemplateRepository.AddAsync(newTemplate, ct);
                _logger.LogInformation("Created email template in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing email template to database. Content ID: {ContentId}", content.Id);
        }
    }

    private async Task DeactivateEmailTemplateAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var templates = await _emailTemplateRepository.GetActiveAsync(ct);
            var template = templates.FirstOrDefault(t => t.UmbracoNodeId == contentId);
            if (template != null)
            {
                template.IsActive = false;
                await _emailTemplateRepository.UpdateAsync(template, ct);
                _logger.LogInformation("Deactivated email template: {Code}", template.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating email template. Content ID: {ContentId}", contentId);
        }
    }

    private async Task DeleteEmailTemplateAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var templates = await _emailTemplateRepository.GetActiveAsync(ct);
            var template = templates.FirstOrDefault(t => t.UmbracoNodeId == contentId);
            if (template != null)
            {
                await _emailTemplateRepository.SoftDeleteAsync(template.Id, ct);
                _logger.LogInformation("Deleted email template from database: {Code}", template.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email template. Content ID: {ContentId}", contentId);
        }
    }

    private void MapContentToEmailTemplate(IContent content, EmailTemplate template)
    {
        template.UmbracoNodeId = content.Id;
        template.Code = content.GetValue<string>("templateCode") ?? "";
        template.Name = content.GetValue<string>("templateName") ?? "";
        template.Description = content.GetValue<string>("description");

        var eventTypeStr = content.GetValue<string>("eventType");
        if (!string.IsNullOrEmpty(eventTypeStr) && Enum.TryParse<EmailTemplateEventType>(eventTypeStr, true, out var eventType))
        {
            template.EventType = eventType;
        }

        template.Language = content.GetValue<string>("language") ?? "en-US";
        template.IsActive = content.GetValue<bool>("isActive");
        template.IsDefault = content.GetValue<bool>("isDefault");

        // Content
        template.Subject = content.GetValue<string>("subject") ?? "";
        template.Preheader = content.GetValue<string>("preheader");
        template.BodyHtml = content.GetValue<string>("bodyHtml");
        template.BodyText = content.GetValue<string>("bodyText");

        // Sender
        template.FromEmail = content.GetValue<string>("fromEmail");
        template.FromName = content.GetValue<string>("fromName");
        template.ReplyToEmail = content.GetValue<string>("replyToEmail");
        template.BccEmails = content.GetValue<string>("bccEmails");

        // Settings
        template.Priority = content.GetValue<int>("priority");
        template.DelayMinutes = GetNullableInt(content, "delayMinutes");
        template.AvailableVariablesJson = content.GetValue<string>("availableVariables");
        template.SampleDataJson = content.GetValue<string>("sampleData");

        // Design
        template.LayoutTemplate = content.GetValue<string>("layoutTemplate");
        template.HeaderImageUrl = content.GetValue<string>("headerImage");
        template.FooterHtml = content.GetValue<string>("footerHtml");
        template.CustomCss = content.GetValue<string>("customCss");
    }

    private static int? GetNullableInt(IContent content, string alias)
    {
        var value = content.GetValue<int>(alias);
        return value == 0 ? null : value;
    }
}
