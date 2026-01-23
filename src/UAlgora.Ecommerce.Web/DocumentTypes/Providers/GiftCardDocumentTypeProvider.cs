using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Gift Card document type definition.
/// Gift cards can be purchased, sent as gifts, and redeemed at checkout.
/// </summary>
public sealed class GiftCardDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 20;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = GiftCardAlias,
            Name = "Algora Gift Card",
            Description = "A gift card with code, balance, and redemption settings.",
            Icon = GiftCardIcon,
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
            CreateValueGroup(),
            CreateRecipientGroup(),
            CreateValidityGroup(),
            CreateRestrictionsGroup()
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
                    Alias = "code",
                    Name = "Gift Card Code",
                    Description = "Unique redemption code",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "name",
                    Name = "Name",
                    Description = "Display name for the gift card",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "giftCardType",
                    Name = "Type",
                    Description = "Physical, Digital, or Promotional",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "status",
                    Name = "Status",
                    Description = "Active, Redeemed, Expired, Disabled, Pending",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "isActive",
                    Name = "Active",
                    Description = "Is this gift card active?",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateValueGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "value",
            Name = "Value & Balance",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "initialValue",
                    Name = "Initial Value",
                    Description = "Original gift card value",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "balance",
                    Name = "Current Balance",
                    Description = "Remaining balance",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "currencyCode",
                    Name = "Currency",
                    Description = "Currency code (USD, EUR, GBP)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateRecipientGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "recipient",
            Name = "Recipient Information",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "recipientName",
                    Name = "Recipient Name",
                    Description = "Name of the gift card recipient",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "recipientEmail",
                    Name = "Recipient Email",
                    Description = "Email address for delivery",
                    DataType = WellKnown(WellKnownDataType.EmailAddress, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "senderName",
                    Name = "Sender Name",
                    Description = "Name of the person who sent the gift",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "message",
                    Name = "Personal Message",
                    Description = "Gift message from sender",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateValidityGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "validity",
            Name = "Validity",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "issuedAt",
                    Name = "Issued Date",
                    Description = "When the gift card was issued",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "validFrom",
                    Name = "Valid From",
                    Description = "Date from which the card can be used",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "expiresAt",
                    Name = "Expiration Date",
                    Description = "When the gift card expires (leave empty for no expiration)",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "usageCount",
                    Name = "Usage Count",
                    Description = "Number of times used",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "lastUsedAt",
                    Name = "Last Used",
                    Description = "When the gift card was last used",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateRestrictionsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "restrictions",
            Name = "Restrictions",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "minimumOrderAmount",
                    Name = "Minimum Order Amount",
                    Description = "Minimum order value to use this gift card",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "maxRedemptionPerOrder",
                    Name = "Max Redemption Per Order",
                    Description = "Maximum amount that can be redeemed in a single order",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "canCombineWithDiscounts",
                    Name = "Can Combine with Discounts",
                    Description = "Allow use with other discount codes",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "notes",
                    Name = "Admin Notes",
                    Description = "Internal notes about this gift card",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                }
            ]
        };
    }
}
