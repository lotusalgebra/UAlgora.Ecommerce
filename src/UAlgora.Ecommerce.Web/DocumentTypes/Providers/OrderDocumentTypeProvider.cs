using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Algora Order document type definition.
/// Follows Umbraco standard tab patterns: Content, Commerce, Settings
/// </summary>
public sealed class OrderDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 30;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = OrderAlias,
            Name = "Algora Order",
            Description = "Algora Commerce Order - A customer order",
            Icon = "icon-invoice",
            IconColor = BrandColor,
            AllowedAsRoot = true,
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateContentGroup(),
            CreateCommerceGroup(),
            CreateSettingsGroup()
        ];
    }

    /// <summary>
    /// Content tab - Order and customer information (Umbraco standard)
    /// </summary>
    private static PropertyGroupDefinition CreateContentGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "content",
            Name = "Content",
            SortOrder = 0,
            Properties =
            [
                // Order Info
                new PropertyDefinition
                {
                    Alias = "orderNumber",
                    Name = "Order Number",
                    Description = "Unique order identifier",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "orderDate",
                    Name = "Order Date",
                    Description = "When the order was placed",
                    DataType = WellKnown(WellKnownDataType.DatePickerWithTime, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                // Customer Info
                new PropertyDefinition
                {
                    Alias = "customerName",
                    Name = "Customer Name",
                    Description = "Full name of the customer",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 10
                },
                new PropertyDefinition
                {
                    Alias = "customerEmail",
                    Name = "Customer Email",
                    Description = "Customer's email address",
                    DataType = WellKnown(WellKnownDataType.EmailAddress, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 11
                },
                new PropertyDefinition
                {
                    Alias = "customerPhone",
                    Name = "Customer Phone",
                    Description = "Customer's phone number",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 12
                },
                // Addresses
                new PropertyDefinition
                {
                    Alias = "shippingAddress",
                    Name = "Shipping Address",
                    Description = "Delivery address",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 20
                },
                new PropertyDefinition
                {
                    Alias = "billingAddress",
                    Name = "Billing Address",
                    Description = "Billing address",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 21
                },
                new PropertyDefinition
                {
                    Alias = "notes",
                    Name = "Order Notes",
                    Description = "Internal notes about the order",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 30
                }
            ]
        };
    }

    /// <summary>
    /// Commerce tab - Order totals and financial details
    /// </summary>
    private static PropertyGroupDefinition CreateCommerceGroup()
    {
        var numericDataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring));

        return new PropertyGroupDefinition
        {
            Alias = "commerce",
            Name = "Commerce",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "subtotal",
                    Name = "Subtotal",
                    Description = "Order subtotal before tax and shipping",
                    DataType = numericDataType,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "taxAmount",
                    Name = "Tax Amount",
                    Description = "Total tax amount",
                    DataType = numericDataType,
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "shippingAmount",
                    Name = "Shipping Amount",
                    Description = "Shipping cost",
                    DataType = numericDataType,
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "discountAmount",
                    Name = "Discount Amount",
                    Description = "Total discounts applied",
                    DataType = numericDataType,
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "grandTotal",
                    Name = "Grand Total",
                    Description = "Final order total",
                    DataType = numericDataType,
                    IsMandatory = true,
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "currencyCode",
                    Name = "Currency",
                    Description = "Currency code (USD, EUR, etc.)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    /// <summary>
    /// Settings tab - Order status and tracking (Umbraco standard)
    /// </summary>
    private static PropertyGroupDefinition CreateSettingsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "settings",
            Name = "Settings",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "orderStatus",
                    Name = "Order Status",
                    Description = "Current order status",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "paymentStatus",
                    Name = "Payment Status",
                    Description = "Payment processing status",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "shippingStatus",
                    Name = "Shipping Status",
                    Description = "Shipping/fulfillment status",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "trackingNumber",
                    Name = "Tracking Number",
                    Description = "Shipping tracking number",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }
}
