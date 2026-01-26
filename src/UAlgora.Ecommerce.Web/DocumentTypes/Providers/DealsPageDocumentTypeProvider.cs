using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Deals/Sale Page document type definition.
/// Displays products on sale with countdown timer, discount tiers, and newsletter signup.
/// </summary>
public sealed class DealsPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 16; // After Wishlist Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = DealsPageAlias,
            Name = "Algora Deals Page",
            Description = "Sale/deals page displaying products on sale with countdown timer, discount tiers, and promotional content.",
            Icon = DealsPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraDealsPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeroGroup(),
            CreateCountdownGroup(),
            CreateDiscountTiersGroup(),
            CreateCategoriesGroup(),
            CreateProductsGroup(),
            CreateEmptyStateGroup(),
            CreateNewsletterGroup(),
            CreateSeoGroup()
        ];
    }

    private static PropertyGroupDefinition CreateHeroGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "hero",
            Name = "Hero Banner",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "heroBadgeText",
                    Name = "Badge Text",
                    Description = "Text for the badge above title (default: 'Limited Time Offers')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "heroTitle",
                    Name = "Hero Title",
                    Description = "Main title text (default: 'SALE')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "heroSubtitle",
                    Name = "Hero Subtitle",
                    Description = "Subtitle text with discount info",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "maxDiscountPercent",
                    Name = "Max Discount Percentage",
                    Description = "Maximum discount percentage to display (default: 70)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateCountdownGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "countdown",
            Name = "Countdown Timer",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "showCountdown",
                    Name = "Show Countdown Timer",
                    Description = "Display the countdown timer section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "countdownLabel",
                    Name = "Countdown Label",
                    Description = "Label above countdown (default: 'Flash Sale Ends In')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "countdownSubtext",
                    Name = "Countdown Subtext",
                    Description = "Text below label (default: 'Don't Miss Out!')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "countdownEndDate",
                    Name = "Countdown End Date",
                    Description = "When the countdown ends. Leave empty for end of day.",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "hoursLabel",
                    Name = "Hours Label",
                    Description = "Label for hours (default: 'Hours')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "minutesLabel",
                    Name = "Minutes Label",
                    Description = "Label for minutes (default: 'Minutes')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "secondsLabel",
                    Name = "Seconds Label",
                    Description = "Label for seconds (default: 'Seconds')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "shopNowText",
                    Name = "Shop Now Button Text",
                    Description = "Text for shop now button (default: 'Shop Now')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateDiscountTiersGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "discountTiers",
            Name = "Discount Tiers",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "showDiscountTiers",
                    Name = "Show Discount Tiers",
                    Description = "Display the discount tier cards",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "tier1Percent",
                    Name = "Tier 1 Percentage",
                    Description = "First tier discount percentage (default: 20)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "tier2Percent",
                    Name = "Tier 2 Percentage",
                    Description = "Second tier discount percentage (default: 30)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "tier3Percent",
                    Name = "Tier 3 Percentage",
                    Description = "Third tier discount percentage (default: 50)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "tier4Percent",
                    Name = "Tier 4 Percentage",
                    Description = "Fourth tier discount percentage (default: 70)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "tierSuffix",
                    Name = "Tier Suffix Text",
                    Description = "Text after percentage (default: 'Off & More')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateCategoriesGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "categories",
            Name = "Category Filters",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "showCategoryPills",
                    Name = "Show Category Pills",
                    Description = "Display category filter pills",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "allDealsText",
                    Name = "All Deals Text",
                    Description = "Text for 'All Deals' button (default: 'All Deals')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "categoryList",
                    Name = "Category List",
                    Description = "Comma-separated list of categories to show (e.g., 'Dresses, Tops, Bottoms')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateProductsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "products",
            Name = "Products Section",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "sectionTitle",
                    Name = "Section Title",
                    Description = "Title for the products section (default: 'All Deals')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "productCountFormat",
                    Name = "Product Count Format",
                    Description = "Format for product count. Use {count} as placeholder (default: '{count} products on sale')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "sortByLabel",
                    Name = "Sort By Label",
                    Description = "Label for sort dropdown (default: 'Sort by: Featured')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "sortPriceLowText",
                    Name = "Sort Price Low Text",
                    Description = "Text for price low to high option (default: 'Price: Low to High')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "sortPriceHighText",
                    Name = "Sort Price High Text",
                    Description = "Text for price high to low option (default: 'Price: High to Low')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "sortDiscountText",
                    Name = "Sort Discount Text",
                    Description = "Text for biggest discount option (default: 'Biggest Discount')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "sortNewestText",
                    Name = "Sort Newest Text",
                    Description = "Text for newest first option (default: 'Newest First')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "pageSize",
                    Name = "Products Per Page",
                    Description = "Number of products to show per page (default: 24)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 7
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
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "emptyTitle",
                    Name = "Empty State Title",
                    Description = "Title when no deals available (default: 'No deals right now')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "emptyDescription",
                    Name = "Empty State Description",
                    Description = "Description when no deals available",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "browseAllText",
                    Name = "Browse All Button Text",
                    Description = "Text for browse all products button (default: 'Browse All Products')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "browseAllUrl",
                    Name = "Browse All Button URL",
                    Description = "URL for browse all products button (default: '/products')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateNewsletterGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "newsletter",
            Name = "Newsletter Section",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "showNewsletter",
                    Name = "Show Newsletter Section",
                    Description = "Display the newsletter signup section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "newsletterBadge",
                    Name = "Newsletter Badge",
                    Description = "Badge text above newsletter title (default: 'Never Miss a Deal')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "newsletterTitle",
                    Name = "Newsletter Title",
                    Description = "Title for newsletter section (default: 'Get Exclusive Offers')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "newsletterDescription",
                    Name = "Newsletter Description",
                    Description = "Description text for newsletter section",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "newsletterPlaceholder",
                    Name = "Email Placeholder",
                    Description = "Placeholder text for email input (default: 'Enter your email')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "newsletterButtonText",
                    Name = "Subscribe Button Text",
                    Description = "Text for subscribe button (default: 'Subscribe')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "newsletterDisclaimer",
                    Name = "Newsletter Disclaimer",
                    Description = "Small text below the form (default: 'Unsubscribe anytime. We respect your privacy.')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
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
