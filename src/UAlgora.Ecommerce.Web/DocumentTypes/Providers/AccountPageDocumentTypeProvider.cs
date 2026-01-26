using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Account Dashboard Page document type definition.
/// Customer account dashboard with configurable labels and sections.
/// </summary>
public sealed class AccountPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 12; // After Product Detail Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = AccountPageAlias,
            Name = "Algora Account Page",
            Description = "Customer account dashboard page with configurable sections for orders, wishlist, addresses, and stats.",
            Icon = AccountPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraAccountPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateBreadcrumbGroup(),
            CreateHeroGroup(),
            CreateSidebarGroup(),
            CreateStatsGroup(),
            CreateOrdersGroup(),
            CreateWishlistGroup(),
            CreateAddressesGroup(),
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
                    Description = "Text for account breadcrumb (default: My Account)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateHeroGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "hero",
            Name = "Hero Section",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "welcomeText",
                    Name = "Welcome Text",
                    Description = "Welcome message (use {firstName} as placeholder, default: Welcome back, {firstName})",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "memberSinceText",
                    Name = "Member Since Text",
                    Description = "Text before member date (default: Member since)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "goldMemberLabel",
                    Name = "Gold Member Label",
                    Description = "Label for gold members (default: Gold Member)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "goldMemberThreshold",
                    Name = "Gold Member Points Threshold",
                    Description = "Loyalty points needed for gold status (default: 1000)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "memberLabel",
                    Name = "Regular Member Label",
                    Description = "Label for regular members (default: Member)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "editProfileText",
                    Name = "Edit Profile Link Text",
                    Description = "Text for edit profile link (default: Edit Profile)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSidebarGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "sidebar",
            Name = "Sidebar Navigation",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "navDashboard",
                    Name = "Dashboard Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "navOrders",
                    Name = "My Orders Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "navWishlist",
                    Name = "Wishlist Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "navAddresses",
                    Name = "Addresses Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "navSettings",
                    Name = "Settings Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "navSignOut",
                    Name = "Sign Out Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
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
            Name = "Stats Cards",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "statsOrdersLabel",
                    Name = "Orders Card Label",
                    Description = "Upper label for orders card (default: Orders)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "statsOrdersSubtext",
                    Name = "Orders Card Subtext",
                    Description = "Subtext below orders count (default: Total orders placed)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "statsEmailLabel",
                    Name = "Email Card Label",
                    Description = "Upper label for email card (default: Email)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "statsEmailSubtext",
                    Name = "Email Card Subtext",
                    Description = "Subtext below email (default: Account email)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "statsPointsLabel",
                    Name = "Points Card Label",
                    Description = "Upper label for points card (default: Points)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "statsPointsSubtext",
                    Name = "Points Card Subtext",
                    Description = "Subtext below points (default: Reward points)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "statsCreditLabel",
                    Name = "Credit Card Label",
                    Description = "Upper label for store credit card (default: Credit)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "statsCreditSubtext",
                    Name = "Credit Card Subtext",
                    Description = "Subtext below credit (default: Store credit)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateOrdersGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "orders",
            Name = "Recent Orders Section",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "ordersTitle",
                    Name = "Section Title",
                    Description = "Title for recent orders section (default: Recent Orders)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "ordersViewAll",
                    Name = "View All Text",
                    Description = "Text for view all link (default: View All)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "ordersCount",
                    Name = "Number of Orders to Show",
                    Description = "How many recent orders to display (default: 5)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "noOrdersTitle",
                    Name = "No Orders Title",
                    Description = "Title when no orders exist (default: No orders yet)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "noOrdersText",
                    Name = "No Orders Text",
                    Description = "Message when no orders exist",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "startShoppingText",
                    Name = "Start Shopping Button Text",
                    Description = "Text for start shopping button (default: Start Shopping)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateWishlistGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "wishlist",
            Name = "Wishlist Section",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "wishlistEnabled",
                    Name = "Enable Wishlist Section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "wishlistTitle",
                    Name = "Section Title",
                    Description = "Title for wishlist section (default: Your Wishlist)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "wishlistViewAll",
                    Name = "View All Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "emptyWishlistTitle",
                    Name = "Empty Wishlist Title",
                    Description = "Title when wishlist is empty (default: Your wishlist is empty)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "emptyWishlistText",
                    Name = "Empty Wishlist Text",
                    Description = "Message when wishlist is empty",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "browseProductsText",
                    Name = "Browse Products Button Text",
                    Description = "Text for browse products button (default: Browse Products)",
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
            Name = "Addresses Section",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "addressesEnabled",
                    Name = "Enable Addresses Section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "addressesTitle",
                    Name = "Section Title",
                    Description = "Title for addresses section (default: Saved Addresses)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "addNewText",
                    Name = "Add New Button Text",
                    Description = "Text for add new address button (default: Add New)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "defaultLabel",
                    Name = "Default Label",
                    Description = "Label for default address (default: Default)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "shippingLabel",
                    Name = "Shipping Address Label",
                    Description = "Label for shipping address type (default: Shipping Address)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "billingLabel",
                    Name = "Billing Address Label",
                    Description = "Label for billing address type (default: Billing Address)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "bothLabel",
                    Name = "Both Address Label",
                    Description = "Label when address is both types (default: Shipping & Billing)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "editText",
                    Name = "Edit Button Text",
                    Description = "Text for edit button (default: Edit)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "noAddressTitle",
                    Name = "No Address Title",
                    Description = "Title when no address saved (default: No address saved)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                },
                new PropertyDefinition
                {
                    Alias = "noAddressText",
                    Name = "No Address Text",
                    Description = "Message when no address saved",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 9
                },
                new PropertyDefinition
                {
                    Alias = "addAddressText",
                    Name = "Add Address Button Text",
                    Description = "Text for add address link (default: Add Address)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 10
                },
                new PropertyDefinition
                {
                    Alias = "addAnotherText",
                    Name = "Add Another Text",
                    Description = "Text for add another address (default: Add another address)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 11
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
                    Alias = "metaTitle",
                    Name = "Meta Title",
                    Description = "Page title for search engines (default: My Account)",
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
