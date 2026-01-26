using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Order Detail Page document type definition.
/// Individual order detail page with configurable labels and sections.
/// </summary>
public sealed class OrderDetailPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 14; // After Orders Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = OrderDetailPageAlias,
            Name = "Algora Order Detail Page",
            Description = "Individual order detail page with status timeline, items, addresses, and summary.",
            Icon = OrderDetailPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraOrderDetailPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateBreadcrumbGroup(),
            CreateHeaderGroup(),
            CreateTimelineGroup(),
            CreateOrderItemsGroup(),
            CreateAddressesGroup(),
            CreateSummaryGroup(),
            CreateActionsGroup(),
            CreateSeoGroup()
        ];
    }

    private static PropertyGroupDefinition CreateBreadcrumbGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "breadcrumb",
            Name = "Breadcrumb",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "breadcrumbHome",
                    Name = "Home Text",
                    Description = "Text for home link (default: Home)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbAccount",
                    Name = "Account Text",
                    Description = "Text for account link (default: Account)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbOrders",
                    Name = "Orders Text",
                    Description = "Text for orders link (default: Orders)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateHeaderGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "header",
            Name = "Page Header",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "orderLabel",
                    Name = "Order Label",
                    Description = "Label before order number (default: Order)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "placedOnLabel",
                    Name = "Placed On Label",
                    Description = "Label before order date (default: Placed on)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateTimelineGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "timeline",
            Name = "Order Status Timeline",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "statusTitle",
                    Name = "Section Title",
                    Description = "Title for status section (default: Order Status)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "confirmedTitle",
                    Name = "Order Confirmed Title",
                    Description = "Title for confirmed step (default: Order Confirmed)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "confirmedText",
                    Name = "Order Confirmed Text",
                    Description = "Description for confirmed step",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "processingTitle",
                    Name = "Processing Title",
                    Description = "Title for processing step (default: Processing)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "processingText",
                    Name = "Processing Text",
                    Description = "Description for processing step",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "shippedTitle",
                    Name = "Shipped Title",
                    Description = "Title for shipped step (default: Shipped)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "shippedText",
                    Name = "Shipped Text",
                    Description = "Description for shipped step",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "trackingLabel",
                    Name = "Tracking Label",
                    Description = "Label before tracking number (default: Tracking:)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "deliveredTitle",
                    Name = "Delivered Title",
                    Description = "Title for delivered step (default: Delivered)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                },
                new PropertyDefinition
                {
                    Alias = "deliveredText",
                    Name = "Delivered Text",
                    Description = "Description for delivered step",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 9
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateOrderItemsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "orderItems",
            Name = "Order Items Section",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "itemsTitle",
                    Name = "Section Title",
                    Description = "Title for order items section (default: Order Items)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "itemsCountFormat",
                    Name = "Items Count Format",
                    Description = "Format for items count (use {count}, default: {count} item(s))",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "skuLabel",
                    Name = "SKU Label",
                    Description = "Label before SKU (default: SKU:)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "sizeLabel",
                    Name = "Size Label",
                    Description = "Label before size/variant (default: Size:)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "qtyLabel",
                    Name = "Quantity Label",
                    Description = "Label before quantity (default: Qty:)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "eachLabel",
                    Name = "Each Label",
                    Description = "Text after unit price (default: each)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateAddressesGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "addresses",
            Name = "Address Sections",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "shippingAddressTitle",
                    Name = "Shipping Address Title",
                    Description = "Title for shipping address card (default: Shipping Address)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "billingAddressTitle",
                    Name = "Billing Address Title",
                    Description = "Title for billing address card (default: Billing Address)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSummaryGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "summary",
            Name = "Order Summary",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "summaryTitle",
                    Name = "Section Title",
                    Description = "Title for order summary (default: Order Summary)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "subtotalLabel",
                    Name = "Subtotal Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "shippingLabel",
                    Name = "Shipping Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "taxLabel",
                    Name = "Tax Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "discountLabel",
                    Name = "Discount Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "totalLabel",
                    Name = "Total Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "paymentMethodTitle",
                    Name = "Payment Method Title",
                    Description = "Title for payment method section (default: Payment Method)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "paidOnLabel",
                    Name = "Paid On Label",
                    Description = "Label before payment date (default: Paid on)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateActionsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "actions",
            Name = "Action Buttons",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "trackPackageText",
                    Name = "Track Package Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "buyAgainText",
                    Name = "Buy Again Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "needHelpText",
                    Name = "Need Help Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "needHelpUrl",
                    Name = "Need Help URL",
                    Description = "URL for need help button (default: /contact)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "backToOrdersText",
                    Name = "Back to Orders Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSeoGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "seo",
            Name = "SEO",
            SortOrder = 7,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "metaTitleTemplate",
                    Name = "Meta Title Template",
                    Description = "Use {orderNumber} as placeholder (default: Order {orderNumber})",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "metaDescription",
                    Name = "Meta Description",
                    Description = "Page description for search engines",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                }
            ]
        };
    }
}
