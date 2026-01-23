using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Email Template document type definition.
/// Allows CMS-driven email templates for all transactional emails.
/// </summary>
public sealed class EmailTemplateDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 21;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = EmailTemplateAlias,
            Name = "Algora Email Template",
            Description = "An email template for transactional emails (order confirmation, shipping, etc.).",
            Icon = EmailIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            IsElement = false,
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateBasicGroup(),
            CreateContentGroup(),
            CreateSenderGroup(),
            CreateSettingsGroup(),
            CreateDesignGroup()
        ];
    }

    private static PropertyGroupDefinition CreateBasicGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "basic",
            Name = "Basic Information",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "templateCode",
                    Name = "Template Code",
                    Description = "Unique identifier (e.g., 'order_confirmation', 'order_shipped')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "templateName",
                    Name = "Template Name",
                    Description = "Display name for this template",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "What this template is used for",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "eventType",
                    Name = "Event Type",
                    Description = "When this email is triggered (e.g., OrderConfirmation, OrderShipped)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "language",
                    Name = "Language",
                    Description = "Language code (e.g., en-US, de-DE)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "isActive",
                    Name = "Active",
                    Description = "Is this template active?",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "isDefault",
                    Name = "Default Template",
                    Description = "Is this the default template for this event type?",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateContentGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "content",
            Name = "Email Content",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "subject",
                    Name = "Subject Line",
                    Description = "Email subject (supports placeholders like {{order.number}})",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "preheader",
                    Name = "Preheader Text",
                    Description = "Preview text shown in email clients",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "bodyHtml",
                    Name = "HTML Body",
                    Description = "HTML content of the email (supports placeholders)",
                    DataType = WellKnown(WellKnownDataType.RichText, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "bodyText",
                    Name = "Plain Text Body",
                    Description = "Plain text version for email clients that don't support HTML",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSenderGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "sender",
            Name = "Sender Settings",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "fromEmail",
                    Name = "From Email",
                    Description = "Sender email address (leave empty for default)",
                    DataType = WellKnown(WellKnownDataType.EmailAddress, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "fromName",
                    Name = "From Name",
                    Description = "Sender display name (leave empty for default)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "replyToEmail",
                    Name = "Reply-To Email",
                    Description = "Reply-to address (leave empty for from email)",
                    DataType = WellKnown(WellKnownDataType.EmailAddress, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "bccEmails",
                    Name = "BCC Emails",
                    Description = "Comma-separated BCC addresses",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSettingsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "settings",
            Name = "Settings",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "priority",
                    Name = "Priority",
                    Description = "Sending priority (higher = more important)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "delayMinutes",
                    Name = "Delay (Minutes)",
                    Description = "Delay before sending (e.g., for abandoned cart emails)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "availableVariables",
                    Name = "Available Variables",
                    Description = "JSON array of available placeholder variables",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "sampleData",
                    Name = "Sample Data",
                    Description = "JSON sample data for template preview",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateDesignGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "design",
            Name = "Design",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "layoutTemplate",
                    Name = "Layout Template",
                    Description = "Base layout template name",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "headerImage",
                    Name = "Header Image",
                    Description = "Image displayed in email header",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "footerHtml",
                    Name = "Footer HTML",
                    Description = "Custom footer content",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "customCss",
                    Name = "Custom CSS",
                    Description = "Additional CSS styles",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                }
            ]
        };
    }
}
