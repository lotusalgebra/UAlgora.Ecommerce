using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Wishlist Page document type definition.
/// Displays customer's saved products with add to cart functionality.
/// </summary>
public sealed class WishlistPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 15; // After Order Detail Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = WishlistPageAlias,
            Name = "Algora Wishlist Page",
            Description = "Customer wishlist page displaying saved products with add to cart and share functionality.",
            Icon = WishlistPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraWishlistPage",
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
            CreateEmptyStateGroup(),
            CreateProductCardGroup(),
            CreateActionsGroup(),
            CreateNotificationsGroup(),
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
                    Alias = "breadcrumbHomeText",
                    Name = "Home Text",
                    Description = "Text for home link in breadcrumb (default: 'Home')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbWishlistText",
                    Name = "Wishlist Text",
                    Description = "Text for wishlist in breadcrumb (default: 'My Wishlist')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
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
                    Description = "Main heading for the wishlist page (default: 'My Wishlist')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "itemsCountFormat",
                    Name = "Items Count Format",
                    Description = "Format for items count. Use {count} as placeholder (default: '{count} items saved')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "shareButtonText",
                    Name = "Share Button Text",
                    Description = "Text for share list button (default: 'Share List')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "showShareButton",
                    Name = "Show Share Button",
                    Description = "Display the share list button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
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
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "emptyTitle",
                    Name = "Empty State Title",
                    Description = "Title when wishlist is empty (default: 'Your wishlist is empty')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "emptyDescription",
                    Name = "Empty State Description",
                    Description = "Description when wishlist is empty",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "exploreButtonText",
                    Name = "Explore Button Text",
                    Description = "Text for explore products button (default: 'Explore Products')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "exploreButtonUrl",
                    Name = "Explore Button URL",
                    Description = "URL for explore products button (default: '/products')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateProductCardGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "productCard",
            Name = "Product Card",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "soldOutBadgeText",
                    Name = "Sold Out Badge Text",
                    Description = "Text for sold out badge (default: 'SOLD OUT')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "defaultBrandText",
                    Name = "Default Brand Text",
                    Description = "Default brand when product has no brand (default: 'Algora')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "addedDateFormat",
                    Name = "Added Date Format",
                    Description = "Format for added date. Use {date} as placeholder (default: 'Added {date}')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "placeholderImage",
                    Name = "Placeholder Image",
                    Description = "Default image when product has no image",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateActionsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "actions",
            Name = "Actions",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "addToBagText",
                    Name = "Add to Bag Text",
                    Description = "Text for add to bag button (default: 'Add to Bag')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "outOfStockText",
                    Name = "Out of Stock Text",
                    Description = "Text for out of stock button (default: 'Out of Stock')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "clearWishlistText",
                    Name = "Clear Wishlist Text",
                    Description = "Text for clear wishlist button (default: 'Clear Wishlist')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "addAllToBagText",
                    Name = "Add All to Bag Text",
                    Description = "Text for add all to bag button (default: 'Add All to Bag')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "clearConfirmText",
                    Name = "Clear Confirmation Text",
                    Description = "Confirmation message when clearing wishlist",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateNotificationsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "notifications",
            Name = "Notifications",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "addedToBagMessage",
                    Name = "Added to Bag Message",
                    Description = "Notification when item added to bag (default: 'Added to bag!')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "addedAllToBagFormat",
                    Name = "Added All to Bag Format",
                    Description = "Notification format. Use {count} as placeholder (default: 'Added {count} items to bag!')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "noItemsInStockMessage",
                    Name = "No Items in Stock Message",
                    Description = "Message when no items are in stock (default: 'No items in stock to add')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "itemRemovedMessage",
                    Name = "Item Removed Message",
                    Description = "Message when item removed (default: 'Item removed from wishlist')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "wishlistClearedMessage",
                    Name = "Wishlist Cleared Message",
                    Description = "Message when wishlist cleared (default: 'Wishlist cleared')",
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
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "metaTitle",
                    Name = "Meta Title",
                    Description = "Page title for search engines",
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
