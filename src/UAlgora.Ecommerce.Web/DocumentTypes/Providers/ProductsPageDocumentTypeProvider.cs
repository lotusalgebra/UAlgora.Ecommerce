using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Products Listing Page document type definition.
/// Configurable product catalog page with filters, sorting, and pagination.
/// </summary>
public sealed class ProductsPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 10; // After Cart Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = ProductsPageAlias,
            Name = "Algora Products Page",
            Description = "Product listing page with customizable hero, filters, sorting options, and pagination.",
            Icon = ProductsPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraProductsPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeroGroup(),
            CreateBreadcrumbGroup(),
            CreateFiltersGroup(),
            CreateSortingGroup(),
            CreateProductGridGroup(),
            CreateEmptyStateGroup(),
            CreateRecentlyViewedGroup(),
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
                    Alias = "defaultTitle",
                    Name = "Default Page Title",
                    Description = "Title when no category selected (default: All Products)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "saleTitle",
                    Name = "Sale Page Title",
                    Description = "Title when viewing sale items (default: Sale)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "productCountText",
                    Name = "Product Count Text",
                    Description = "Text after product count (default: pieces curated for you)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "searchResultsLabel",
                    Name = "Search Results Label",
                    Description = "Text before search term (default: Results for:)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateBreadcrumbGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "breadcrumb",
            Name = "Breadcrumb",
            SortOrder = 1,
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

    private static PropertyGroupDefinition CreateFiltersGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "filters",
            Name = "Filters Section",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "filtersButtonText",
                    Name = "Filters Button Text",
                    Description = "Text for the filters toggle button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "allCategoriesText",
                    Name = "All Categories Text",
                    Description = "Text for 'All' category pill",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "categoryLabel",
                    Name = "Category Label",
                    Description = "Label for category filter",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "priceRangeLabel",
                    Name = "Price Range Label",
                    Description = "Label for price range filter",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "minPricePlaceholder",
                    Name = "Min Price Placeholder",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "maxPricePlaceholder",
                    Name = "Max Price Placeholder",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "sizeLabel",
                    Name = "Size Label",
                    Description = "Label for size filter",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "colorLabel",
                    Name = "Color Label",
                    Description = "Label for color filter",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "optionsLabel",
                    Name = "Options Label",
                    Description = "Label for filter options section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                },
                new PropertyDefinition
                {
                    Alias = "inStockLabel",
                    Name = "In Stock Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 9
                },
                new PropertyDefinition
                {
                    Alias = "onSaleLabel",
                    Name = "On Sale Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 10
                },
                new PropertyDefinition
                {
                    Alias = "applyButtonText",
                    Name = "Apply Button Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 11
                },
                new PropertyDefinition
                {
                    Alias = "clearButtonText",
                    Name = "Clear Button Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 12
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSortingGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "sorting",
            Name = "Sorting Options",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "sortLabel",
                    Name = "Sort Label",
                    Description = "Label before sort dropdown",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "sortFeatured",
                    Name = "Featured Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "sortNewest",
                    Name = "Newest Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "sortPriceLowHigh",
                    Name = "Price Low to High Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "sortPriceHighLow",
                    Name = "Price High to Low Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "sortNameAZ",
                    Name = "Name A-Z Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "sortTopRated",
                    Name = "Top Rated Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateProductGridGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "productGrid",
            Name = "Product Grid",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "productsPerPage",
                    Name = "Products Per Page",
                    Description = "Number of products to show per page (default: 12)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "showingText",
                    Name = "Showing Text Template",
                    Description = "Template for pagination info (use {start}, {end}, {total})",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
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
                    Description = "Title when no products found",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "emptyMessage",
                    Name = "Empty State Message",
                    Description = "Message when no products found",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "emptyButtonText",
                    Name = "Empty State Button Text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
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
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "recentlyViewedEnabled",
                    Name = "Enable Recently Viewed",
                    Description = "Show recently viewed products section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "recentlyViewedLabel",
                    Name = "Section Label",
                    Description = "Small label above title",
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
                    Description = "Text when no recently viewed items",
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
