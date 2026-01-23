using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Discount document type definition.
/// Comprehensive discount/coupon management from the CMS.
/// </summary>
public sealed class DiscountDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 22;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = DiscountAlias,
            Name = "Algora Discount",
            Description = "A discount or coupon code with rules, conditions, and usage limits.",
            Icon = DiscountIcon,
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
            CreateConditionsGroup(),
            CreateUsageLimitsGroup(),
            CreateValidityGroup()
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
                    Alias = "discountName",
                    Name = "Name",
                    Description = "Internal name for this discount",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "Description of the discount",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "code",
                    Name = "Coupon Code",
                    Description = "Code customers enter at checkout (leave empty for automatic discounts)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "isActive",
                    Name = "Active",
                    Description = "Is this discount active?",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateValueGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "value",
            Name = "Discount Value",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "discountType",
                    Name = "Discount Type",
                    Description = "Percentage, FixedAmount, FreeShipping, or BuyXGetY",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "discountScope",
                    Name = "Applies To",
                    Description = "Order, Products, Shipping, or Category",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "discountValue",
                    Name = "Value",
                    Description = "Discount amount (percentage or fixed)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    IsMandatory = true,
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "maxDiscountAmount",
                    Name = "Maximum Discount",
                    Description = "Cap on discount amount (for percentage discounts)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateConditionsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "conditions",
            Name = "Conditions",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "minimumOrderAmount",
                    Name = "Minimum Order Amount",
                    Description = "Minimum order value to qualify",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "minimumQuantity",
                    Name = "Minimum Quantity",
                    Description = "Minimum items in cart to qualify",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "firstTimeCustomerOnly",
                    Name = "First-Time Customers Only",
                    Description = "Only for customers without previous orders",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "excludeSaleItems",
                    Name = "Exclude Sale Items",
                    Description = "Don't apply to items already on sale",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "eligibleCustomerTiers",
                    Name = "Eligible Customer Tiers",
                    Description = "Comma-separated tier names (leave empty for all)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateUsageLimitsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "limits",
            Name = "Usage Limits",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "totalUsageLimit",
                    Name = "Total Usage Limit",
                    Description = "Maximum times this code can be used (leave empty for unlimited)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "perCustomerLimit",
                    Name = "Per Customer Limit",
                    Description = "Maximum times per customer",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "usageCount",
                    Name = "Current Usage",
                    Description = "Number of times used (read-only)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "canCombine",
                    Name = "Can Combine",
                    Description = "Can be used with other discounts",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "priority",
                    Name = "Priority",
                    Description = "Order of application (higher = first)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateValidityGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "validity",
            Name = "Validity Period",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "startDate",
                    Name = "Start Date",
                    Description = "When the discount becomes active",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "endDate",
                    Name = "End Date",
                    Description = "When the discount expires",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                }
            ]
        };
    }
}
