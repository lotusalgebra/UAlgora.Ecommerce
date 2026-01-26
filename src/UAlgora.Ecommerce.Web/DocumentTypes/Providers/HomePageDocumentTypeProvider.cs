using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Home Page document type definition.
/// The main landing page with configurable sections: hero, categories, deals, features, testimonials.
/// </summary>
public sealed class HomePageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 5; // After Site Settings

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = HomePageAlias,
            Name = "Algora Home Page",
            Description = "The main storefront home page with configurable hero, categories, deals, and more.",
            Icon = HomeIcon,
            IconColor = BrandColor,
            AllowedAsRoot = true,
            DefaultTemplate = "algoraHome",
            AllowedChildTypes = [HeroSlideAlias, BannerAlias, TestimonialAlias, FeatureAlias, LoginPageAlias, RegisterPageAlias, CheckoutPageAlias, CartPageAlias, ProductsPageAlias, ProductDetailPageAlias, AccountPageAlias, OrdersPageAlias, OrderDetailPageAlias, WishlistPageAlias, DealsPageAlias, OrderConfirmationPageAlias, AddressesPageAlias, SettingsPageAlias],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeroGroup(),
            CreateCategoriesGroup(),
            CreateDealsGroup(),
            CreateProductsGroup(),
            CreateBannersGroup(),
            CreateFeaturesGroup(),
            CreateTestimonialsGroup(),
            CreateSeoGroup()
        ];
    }

    private static PropertyGroupDefinition CreateHeroGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "hero",
            Name = "Hero Section",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "heroEnabled",
                    Name = "Enable Hero Carousel",
                    Description = "Show the hero carousel section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "heroAutoplay",
                    Name = "Autoplay",
                    Description = "Auto-rotate slides",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "heroInterval",
                    Name = "Slide Interval (ms)",
                    Description = "Time between slides in milliseconds (default: 5000)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateCategoriesGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "categories",
            Name = "Categories Section",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "categoriesEnabled",
                    Name = "Enable Categories Section",
                    Description = "Show the category cards section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "categoriesTitle",
                    Name = "Section Title",
                    Description = "Heading for the categories section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "categoriesSubtitle",
                    Name = "Section Subtitle",
                    Description = "Subheading text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "categoriesCount",
                    Name = "Number of Categories",
                    Description = "How many categories to display",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateDealsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "deals",
            Name = "Deal of the Day",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "dealsEnabled",
                    Name = "Enable Deals Section",
                    Description = "Show the deal of the day section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "dealsTitle",
                    Name = "Section Title",
                    Description = "Heading for deals section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "dealEndDate",
                    Name = "Deal End Date",
                    Description = "When the deal expires (for countdown timer)",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "dealProductsCount",
                    Name = "Number of Deal Products",
                    Description = "How many deal products to show",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "dealBackgroundColor",
                    Name = "Background Color",
                    Description = "Section background color (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateProductsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "products",
            Name = "Product Sections",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "featuredEnabled",
                    Name = "Enable Featured Products",
                    Description = "Show featured products section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "featuredTitle",
                    Name = "Featured Title",
                    Description = "Heading for featured products",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "featuredCount",
                    Name = "Featured Products Count",
                    Description = "Number of featured products to show",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "newArrivalsEnabled",
                    Name = "Enable New Arrivals",
                    Description = "Show new arrivals section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "newArrivalsTitle",
                    Name = "New Arrivals Title",
                    Description = "Heading for new arrivals",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "newArrivalsCount",
                    Name = "New Arrivals Count",
                    Description = "Number of new arrivals to show",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "bestSellersEnabled",
                    Name = "Enable Best Sellers",
                    Description = "Show best sellers section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "bestSellersTitle",
                    Name = "Best Sellers Title",
                    Description = "Heading for best sellers",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "bestSellersCount",
                    Name = "Best Sellers Count",
                    Description = "Number of best sellers to show",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateBannersGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "banners",
            Name = "Banners",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "bannersEnabled",
                    Name = "Enable Promotional Banners",
                    Description = "Show promotional banner section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateFeaturesGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "features",
            Name = "Features/USP",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "featuresEnabled",
                    Name = "Enable Features Section",
                    Description = "Show the USP/features section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "featuresTitle",
                    Name = "Section Title",
                    Description = "Optional heading for features section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateTestimonialsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "testimonials",
            Name = "Testimonials",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "testimonialsEnabled",
                    Name = "Enable Testimonials",
                    Description = "Show customer testimonials section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "testimonialsTitle",
                    Name = "Section Title",
                    Description = "Heading for testimonials",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "testimonialsSubtitle",
                    Name = "Section Subtitle",
                    Description = "Subheading text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
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
                },
                new PropertyDefinition
                {
                    Alias = "ogImage",
                    Name = "Social Share Image",
                    Description = "Image for social media sharing",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                }
            ]
        };
    }
}
