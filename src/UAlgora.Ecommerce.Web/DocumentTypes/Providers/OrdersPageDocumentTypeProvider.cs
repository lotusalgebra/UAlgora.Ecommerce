using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Orders Page document type definition.
/// Customer order history page with configurable labels and filters.
/// </summary>
public sealed class OrdersPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 13; // After Account Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = OrdersPageAlias,
            Name = "Algora Orders Page",
            Description = "Customer order history page with configurable labels, filters, and empty state.",
            Icon = OrdersPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraOrdersPage",
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
            CreateFiltersGroup(),
            CreateOrderListGroup(),
            CreateEmptyStateGroup(),
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
                    Description = "Text for orders breadcrumb (default: Orders)",
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
                    Alias = "pageTitle",
                    Name = "Page Title",
                    Description = "Main heading for the orders page (default: My Orders)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "ordersCountFormat",
                    Name = "Orders Count Format",
                    Description = "Format for orders count (use {count} as placeholder, default: {count} order(s))",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateFiltersGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "filters",
            Name = "Filter Labels",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "filterByLabel",
                    Name = "Filter By Label",
                    Description = "Label before filter buttons (default: Filter by:)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "filterAll",
                    Name = "All Orders Filter",
                    Description = "Label for all orders filter (default: All Orders)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "filterPending",
                    Name = "Pending Filter",
                    Description = "Label for pending filter (default: Pending)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "filterProcessing",
                    Name = "Processing Filter",
                    Description = "Label for processing filter (default: Processing)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "filterShipped",
                    Name = "Shipped Filter",
                    Description = "Label for shipped filter (default: Shipped)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "filterDelivered",
                    Name = "Delivered Filter",
                    Description = "Label for delivered filter (default: Delivered)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "filterCancelled",
                    Name = "Cancelled Filter",
                    Description = "Label for cancelled filter (default: Cancelled)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateOrderListGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "orderList",
            Name = "Order List Labels",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "orderNumberLabel",
                    Name = "Order Number Label",
                    Description = "Label for order number (default: Order Number)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "placedOnLabel",
                    Name = "Placed On Label",
                    Description = "Label for order date (default: Placed On)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "totalLabel",
                    Name = "Total Label",
                    Description = "Label for order total (default: Total)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "viewDetailsText",
                    Name = "View Details Text",
                    Description = "Text for view details link (default: View Details)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "moreItemsFormat",
                    Name = "More Items Format",
                    Description = "Format for showing more items (use {count} as placeholder, default: +{count})",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "itemsToShow",
                    Name = "Product Images to Show",
                    Description = "Number of product images to show per order (default: 4)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateEmptyStateGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "emptyState",
            Name = "Empty State",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "noOrdersTitle",
                    Name = "No Orders Title",
                    Description = "Title when no orders exist (default: No orders yet)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "noOrdersText",
                    Name = "No Orders Text",
                    Description = "Message when no orders exist",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "startShoppingText",
                    Name = "Start Shopping Button Text",
                    Description = "Text for start shopping button (default: Start Shopping)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
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
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "trackPackageText",
                    Name = "Track Package Text",
                    Description = "Text for track package button (default: Track Package)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "buyAgainText",
                    Name = "Buy Again Text",
                    Description = "Text for buy again button (default: Buy Again)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "returnItemText",
                    Name = "Return Item Text",
                    Description = "Text for return item button (default: Return Item)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "writeReviewText",
                    Name = "Write Review Text",
                    Description = "Text for write review button (default: Write Review)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
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
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "metaTitle",
                    Name = "Meta Title",
                    Description = "Page title for search engines (default: My Orders)",
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
