using System.Text.Json;
using System.Text.RegularExpressions;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for Email Template operations.
/// </summary>
public partial class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;

    public EmailTemplateService(IEmailTemplateRepository emailTemplateRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _emailTemplateRepository.GetByIdAsync(id, ct);
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _emailTemplateRepository.GetByCodeAsync(code, ct: ct);
    }

    public async Task<EmailTemplate?> GetTemplateAsync(EmailTemplateEventType eventType, string language = "en-US", Guid? storeId = null, CancellationToken ct = default)
    {
        return await _emailTemplateRepository.GetActiveTemplateAsync(eventType, language, storeId, ct);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default)
    {
        return await _emailTemplateRepository.GetByStoreAsync(storeId, ct);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByEventTypeAsync(EmailTemplateEventType eventType, CancellationToken ct = default)
    {
        return await _emailTemplateRepository.GetByEventTypeAsync(eventType, ct);
    }

    public async Task<EmailTemplate> CreateAsync(EmailTemplate template, CancellationToken ct = default)
    {
        // Validate unique code
        if (await _emailTemplateRepository.CodeExistsAsync(template.Code, ct: ct))
        {
            throw new InvalidOperationException($"Email template with code '{template.Code}' already exists.");
        }

        return await _emailTemplateRepository.AddAsync(template, ct);
    }

    public async Task<EmailTemplate> UpdateAsync(EmailTemplate template, CancellationToken ct = default)
    {
        // Validate unique code
        if (await _emailTemplateRepository.CodeExistsAsync(template.Code, template.Id, ct))
        {
            throw new InvalidOperationException($"Email template with code '{template.Code}' already exists.");
        }

        return await _emailTemplateRepository.UpdateAsync(template, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _emailTemplateRepository.SoftDeleteAsync(id, ct);
    }

    public async Task<RenderedEmail> RenderAsync(EmailTemplateEventType eventType, object data, string language = "en-US", Guid? storeId = null, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync(eventType, language, storeId, ct);
        if (template == null)
        {
            throw new InvalidOperationException($"No template found for event type '{eventType}' and language '{language}'.");
        }

        return RenderTemplate(template, data);
    }

    public async Task<RenderedEmail> RenderAsync(Guid templateId, object data, CancellationToken ct = default)
    {
        var template = await _emailTemplateRepository.GetByIdAsync(templateId, ct);
        if (template == null)
        {
            throw new InvalidOperationException($"Template with ID '{templateId}' not found.");
        }

        return RenderTemplate(template, data);
    }

    public async Task<EmailSendResult> SendAsync(EmailTemplateEventType eventType, string toEmail, object data, string language = "en-US", Guid? storeId = null, CancellationToken ct = default)
    {
        try
        {
            var template = await GetTemplateAsync(eventType, language, storeId, ct);
            if (template == null)
            {
                return EmailSendResult.Failed($"No template found for event type '{eventType}'.");
            }

            var rendered = RenderTemplate(template, data);

            // In a real implementation, this would use an email provider (SendGrid, SMTP, etc.)
            // For now, we'll simulate success and update the template stats
            template.SendCount++;
            template.LastSentAt = DateTime.UtcNow;
            await _emailTemplateRepository.UpdateAsync(template, ct);

            return EmailSendResult.Successful($"MSG-{Guid.NewGuid():N}");
        }
        catch (Exception ex)
        {
            return EmailSendResult.Failed(ex.Message);
        }
    }

    public async Task<EmailSendResult> SendTestAsync(Guid templateId, string toEmail, CancellationToken ct = default)
    {
        try
        {
            var template = await _emailTemplateRepository.GetByIdAsync(templateId, ct);
            if (template == null)
            {
                return EmailSendResult.Failed("Template not found.");
            }

            // Use sample data if available
            object sampleData = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(template.SampleDataJson))
            {
                try
                {
                    sampleData = JsonSerializer.Deserialize<Dictionary<string, object>>(template.SampleDataJson)
                        ?? new Dictionary<string, object>();
                }
                catch
                {
                    // Use empty data if parsing fails
                }
            }

            var rendered = RenderTemplate(template, sampleData);

            // Update last tested timestamp
            template.LastTestedAt = DateTime.UtcNow;
            await _emailTemplateRepository.UpdateAsync(template, ct);

            return EmailSendResult.Successful($"TEST-{Guid.NewGuid():N}");
        }
        catch (Exception ex)
        {
            return EmailSendResult.Failed(ex.Message);
        }
    }

    public IReadOnlyDictionary<string, string> GetAvailableVariables(EmailTemplateEventType eventType)
    {
        // Return common variables plus event-specific ones
        var variables = new Dictionary<string, string>
        {
            ["{{store.name}}"] = "Store name",
            ["{{store.url}}"] = "Store URL",
            ["{{store.logo}}"] = "Store logo URL",
            ["{{store.email}}"] = "Store contact email",
            ["{{store.phone}}"] = "Store phone number",
            ["{{current.date}}"] = "Current date",
            ["{{current.year}}"] = "Current year"
        };

        // Add event-specific variables
        switch (eventType)
        {
            case EmailTemplateEventType.OrderConfirmation:
            case EmailTemplateEventType.OrderProcessing:
            case EmailTemplateEventType.OrderShipped:
            case EmailTemplateEventType.OrderDelivered:
            case EmailTemplateEventType.OrderCancelled:
            case EmailTemplateEventType.OrderRefunded:
                variables["{{order.number}}"] = "Order number";
                variables["{{order.date}}"] = "Order date";
                variables["{{order.total}}"] = "Order total";
                variables["{{order.subtotal}}"] = "Order subtotal";
                variables["{{order.tax}}"] = "Tax amount";
                variables["{{order.shipping}}"] = "Shipping cost";
                variables["{{order.items}}"] = "Order items HTML";
                variables["{{customer.name}}"] = "Customer full name";
                variables["{{customer.email}}"] = "Customer email";
                variables["{{shipping.address}}"] = "Shipping address";
                variables["{{billing.address}}"] = "Billing address";
                break;

            case EmailTemplateEventType.CustomerWelcome:
            case EmailTemplateEventType.CustomerPasswordReset:
            case EmailTemplateEventType.CustomerEmailVerification:
                variables["{{customer.name}}"] = "Customer full name";
                variables["{{customer.firstName}}"] = "Customer first name";
                variables["{{customer.email}}"] = "Customer email";
                variables["{{reset.url}}"] = "Password reset URL";
                variables["{{verify.url}}"] = "Verification URL";
                break;

            case EmailTemplateEventType.GiftCardIssued:
            case EmailTemplateEventType.GiftCardRedeemed:
                variables["{{giftcard.code}}"] = "Gift card code";
                variables["{{giftcard.balance}}"] = "Gift card balance";
                variables["{{giftcard.initialValue}}"] = "Initial value";
                variables["{{giftcard.expiry}}"] = "Expiration date";
                variables["{{recipient.name}}"] = "Recipient name";
                variables["{{sender.name}}"] = "Sender name";
                variables["{{sender.message}}"] = "Personal message";
                break;

            case EmailTemplateEventType.ReturnRequested:
            case EmailTemplateEventType.ReturnApproved:
            case EmailTemplateEventType.ReturnRejected:
            case EmailTemplateEventType.ReturnRefundProcessed:
                variables["{{return.number}}"] = "Return number";
                variables["{{return.status}}"] = "Return status";
                variables["{{return.amount}}"] = "Refund amount";
                variables["{{return.items}}"] = "Return items HTML";
                variables["{{order.number}}"] = "Original order number";
                break;

            case EmailTemplateEventType.CartAbandoned1Hour:
            case EmailTemplateEventType.CartAbandoned24Hours:
            case EmailTemplateEventType.CartAbandoned72Hours:
            case EmailTemplateEventType.CartAbandonedFinal:
                variables["{{cart.items}}"] = "Cart items HTML";
                variables["{{cart.total}}"] = "Cart total";
                variables["{{cart.url}}"] = "Cart recovery URL";
                variables["{{customer.name}}"] = "Customer name";
                break;
        }

        return variables;
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetDefaultTemplatesAsync(CancellationToken ct = default)
    {
        return await _emailTemplateRepository.GetDefaultTemplatesAsync(ct);
    }

    public async Task<EmailTemplate> DuplicateAsync(Guid templateId, string newCode, Guid? storeId = null, CancellationToken ct = default)
    {
        var original = await _emailTemplateRepository.GetByIdAsync(templateId, ct);
        if (original == null)
        {
            throw new InvalidOperationException($"Template with ID '{templateId}' not found.");
        }

        // Validate unique code
        if (await _emailTemplateRepository.CodeExistsAsync(newCode, ct: ct))
        {
            throw new InvalidOperationException($"Email template with code '{newCode}' already exists.");
        }

        var duplicate = new EmailTemplate
        {
            StoreId = storeId ?? original.StoreId,
            Code = newCode,
            Name = $"{original.Name} (Copy)",
            Description = original.Description,
            EventType = original.EventType,
            Language = original.Language,
            Subject = original.Subject,
            BodyHtml = original.BodyHtml,
            BodyText = original.BodyText,
            Preheader = original.Preheader,
            FromEmail = original.FromEmail,
            FromName = original.FromName,
            ReplyToEmail = original.ReplyToEmail,
            BccEmails = original.BccEmails,
            IsActive = false, // Start as inactive
            IsDefault = false,
            Priority = original.Priority,
            DelayMinutes = original.DelayMinutes,
            LayoutTemplate = original.LayoutTemplate,
            CustomCss = original.CustomCss,
            HeaderImageUrl = original.HeaderImageUrl,
            FooterHtml = original.FooterHtml,
            AvailableVariablesJson = original.AvailableVariablesJson,
            SampleDataJson = original.SampleDataJson
        };

        return await _emailTemplateRepository.AddAsync(duplicate, ct);
    }

    private RenderedEmail RenderTemplate(EmailTemplate template, object data)
    {
        var dataDict = ConvertToFlatDictionary(data);

        return new RenderedEmail
        {
            Subject = ReplaceVariables(template.Subject, dataDict),
            BodyHtml = ReplaceVariables(template.BodyHtml ?? "", dataDict),
            BodyText = template.BodyText != null ? ReplaceVariables(template.BodyText, dataDict) : null,
            Preheader = template.Preheader != null ? ReplaceVariables(template.Preheader, dataDict) : null,
            FromEmail = template.FromEmail,
            FromName = template.FromName,
            ReplyToEmail = template.ReplyToEmail,
            BccEmails = !string.IsNullOrEmpty(template.BccEmails)
                ? template.BccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : null
        };
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> data)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        // Replace {{variable}} patterns
        return VariableRegex().Replace(template, match =>
        {
            var key = match.Groups[1].Value.Trim();
            return data.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    private static Dictionary<string, string> ConvertToFlatDictionary(object data)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (data is IDictionary<string, object> dict)
        {
            FlattenDictionary(dict, "", result);
        }
        else
        {
            // Convert object to dictionary using reflection
            var properties = data.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(data);
                if (value != null)
                {
                    result[prop.Name.ToLowerInvariant()] = value.ToString() ?? "";
                }
            }
        }

        // Add common variables
        result["current.date"] = DateTime.UtcNow.ToString("MMMM d, yyyy");
        result["current.year"] = DateTime.UtcNow.Year.ToString();

        return result;
    }

    private static void FlattenDictionary(IDictionary<string, object> dict, string prefix, Dictionary<string, string> result)
    {
        foreach (var kvp in dict)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            if (kvp.Value is IDictionary<string, object> nested)
            {
                FlattenDictionary(nested, key, result);
            }
            else
            {
                result[key.ToLowerInvariant()] = kvp.Value?.ToString() ?? "";
            }
        }
    }

    [GeneratedRegex(@"\{\{([^}]+)\}\}")]
    private static partial Regex VariableRegex();
}
