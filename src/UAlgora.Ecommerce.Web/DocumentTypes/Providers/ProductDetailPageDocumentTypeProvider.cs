using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Product Detail Page document type definition.
/// Template page for product details with configurable labels, sections, and trust badges.
/// </summary>
public sealed class ProductDetailPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 11; // After Products Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = ProductDetailPageAlias,
            Name = "Algora Product Detail Page",
            Description = "Product detail page template with customizable labels, accordion sections, trust badges, and related products.",
            Icon = ProductIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraProductDetailPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateBreadcrumbGroup(),
            CreateLabelsGroup(),
            CreateButtonsGroup(),
            CreateTrustBadgesGroup(),
            CreateAccordionGroup(),
            CreateRelatedProductsGroup(),
            CreateReviewsGroup(),
            CreateRecentlyViewedGroup(),
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
                    Description = "Text for home link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbShop",
                    Name = "Shop Text",
                    Description = "Text for shop link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateLabelsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "labels",
            Name = "Product Labels",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "newBadgeText",
                    Name = "New Badge Text",
                    Description = "Text for new product badge",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "newBadgeDays",
                    Name = "New Badge Days",
                    Description = "Number of days product is considered 'new' (default: 14)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "reviewsLabel",
                    Name = "Reviews Label",
                    Description = "Label after review count (e.g., 'reviews')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "saveLabel",
                    Name = "Save Label",
                    Description = "Label before discount amount (e.g., 'Save')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "colorLabel",
                    Name = "Color Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "sizeLabel",
                    Name = "Size Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "sizeGuideText",
                    Name = "Size Guide Text",
                    Description = "Text for size guide link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "selectLabel",
                    Name = "Select Label",
                    Description = "Default text when no size selected",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "zoomText",
                    Name = "Zoom Text",
                    Description = "Text for image zoom indicator",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateButtonsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "buttons",
            Name = "Buttons & Actions",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "addToBagText",
                    Name = "Add to Bag Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "soldOutText",
                    Name = "Sold Out Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "notifyMeText",
                    Name = "Notify Me Text",
                    Description = "Text for back-in-stock notification button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "selectSizeAlert",
                    Name = "Select Size Alert",
                    Description = "Alert message when size not selected",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "addedToWishlistText",
                    Name = "Added to Wishlist Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "removedFromWishlistText",
                    Name = "Removed from Wishlist Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateTrustBadgesGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "trustBadges",
            Name = "Trust Badges",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "freeShippingText",
                    Name = "Free Shipping Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "easyReturnsText",
                    Name = "Easy Returns Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "secureCheckoutText",
                    Name = "Secure Checkout Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "shippingTimeText",
                    Name = "Shipping Time Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateAccordionGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "accordion",
            Name = "Accordion Sections",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "descriptionTitle",
                    Name = "Description Title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "noDescriptionText",
                    Name = "No Description Text",
                    Description = "Text when product has no description",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "detailsCareTitle",
                    Name = "Details & Care Title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "careInstructions",
                    Name = "Care Instructions",
                    Description = "Default care instructions (one per line)",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "shippingReturnsTitle",
                    Name = "Shipping & Returns Title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "shippingReturnsContent",
                    Name = "Shipping & Returns Content",
                    Description = "Default shipping and returns policy",
                    DataType = WellKnown(WellKnownDataType.RichText, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateRelatedProductsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "relatedProducts",
            Name = "Related Products Section",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "relatedEnabled",
                    Name = "Enable Related Products",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "relatedLabel",
                    Name = "Section Label",
                    Description = "Small label above title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "relatedTitle",
                    Name = "Section Title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "relatedViewAllText",
                    Name = "View All Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "relatedCount",
                    Name = "Number of Products",
                    Description = "How many related products to show (default: 4)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateReviewsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "reviews",
            Name = "Reviews Section",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "reviewsEnabled",
                    Name = "Enable Reviews Section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "reviewsSectionLabel",
                    Name = "Section Label",
                    Description = "Small label above title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "reviewsSectionTitle",
                    Name = "Section Title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "noReviewsText",
                    Name = "No Reviews Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "writeReviewText",
                    Name = "Write Review Button Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "reviewsCount",
                    Name = "Number of Reviews to Show",
                    Description = "How many reviews to display (default: 6)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateRecentlyViewedGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "recentlyViewed",
            Name = "Recently Viewed Section",
            SortOrder = 7,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "recentlyViewedEnabled",
                    Name = "Enable Recently Viewed",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "recentlyViewedLabel",
                    Name = "Section Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "recentlyViewedTitle",
                    Name = "Section Title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "recentlyViewedEmptyText",
                    Name = "Empty Text",
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
            SortOrder = 8,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "metaTitleTemplate",
                    Name = "Meta Title Template",
                    Description = "Use {productName}, {brand}, {category} as placeholders",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "metaDescriptionTemplate",
                    Name = "Meta Description Template",
                    Description = "Use {productName}, {brand}, {shortDescription} as placeholders",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                }
            ]
        };
    }
}
