using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Webhook document type definition.
/// Allows configuring webhooks for external integrations.
/// </summary>
public sealed class WebhookDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 23;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = WebhookAlias,
            Name = "Algora Webhook",
            Description = "A webhook subscription for sending event notifications to external systems.",
            Icon = WebhookIcon,
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
            CreateEventsGroup(),
            CreateSecurityGroup(),
            CreateRetryGroup(),
            CreateStatsGroup()
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
                    Alias = "webhookName",
                    Name = "Name",
                    Description = "Display name for this webhook",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "What this webhook is used for",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "url",
                    Name = "Webhook URL",
                    Description = "Target URL to send events to",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "httpMethod",
                    Name = "HTTP Method",
                    Description = "POST or PUT",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "contentType",
                    Name = "Content Type",
                    Description = "Request content type (application/json)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "isActive",
                    Name = "Active",
                    Description = "Is this webhook active?",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateEventsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "events",
            Name = "Events",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "subscribeToAll",
                    Name = "Subscribe to All Events",
                    Description = "Send notifications for all event types",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "subscribedEvents",
                    Name = "Subscribed Events",
                    Description = "Comma-separated event types (e.g., order.created, order.paid, product.updated)",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "filterJson",
                    Name = "Event Filter",
                    Description = "JSON filter conditions (e.g., {\"order.total\": {\"$gt\": 100}})",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSecurityGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "security",
            Name = "Security",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "authType",
                    Name = "Authentication Type",
                    Description = "None, HmacSha256, HmacSha512, ApiKey, BasicAuth, BearerToken",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "secret",
                    Name = "Secret Key",
                    Description = "HMAC signing secret or API key",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "bearerToken",
                    Name = "Bearer Token",
                    Description = "Bearer token for authentication",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "basicAuthUsername",
                    Name = "Basic Auth Username",
                    Description = "Username for basic authentication",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "basicAuthPassword",
                    Name = "Basic Auth Password",
                    Description = "Password for basic authentication",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "customHeaders",
                    Name = "Custom Headers",
                    Description = "JSON object of custom headers to send",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "verifySsl",
                    Name = "Verify SSL",
                    Description = "Validate SSL certificate",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateRetryGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "retry",
            Name = "Retry Settings",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "retryEnabled",
                    Name = "Enable Retries",
                    Description = "Retry failed deliveries",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "maxRetries",
                    Name = "Max Retries",
                    Description = "Maximum retry attempts",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "retryDelaySeconds",
                    Name = "Retry Delay (seconds)",
                    Description = "Seconds between retries",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "useExponentialBackoff",
                    Name = "Exponential Backoff",
                    Description = "Increase delay between retries",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "timeoutSeconds",
                    Name = "Timeout (seconds)",
                    Description = "Request timeout",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "maxConsecutiveFailures",
                    Name = "Max Consecutive Failures",
                    Description = "Auto-disable after this many failures",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateStatsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "stats",
            Name = "Statistics",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "totalDeliveries",
                    Name = "Total Deliveries",
                    Description = "Total delivery attempts (read-only)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "successfulDeliveries",
                    Name = "Successful Deliveries",
                    Description = "Successful deliveries (read-only)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "failedDeliveries",
                    Name = "Failed Deliveries",
                    Description = "Failed deliveries (read-only)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "lastTriggeredAt",
                    Name = "Last Triggered",
                    Description = "Last delivery attempt (read-only)",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "lastStatusCode",
                    Name = "Last Status Code",
                    Description = "HTTP status from last attempt (read-only)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "lastError",
                    Name = "Last Error",
                    Description = "Error from last failed attempt (read-only)",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 5
                }
            ]
        };
    }
}
