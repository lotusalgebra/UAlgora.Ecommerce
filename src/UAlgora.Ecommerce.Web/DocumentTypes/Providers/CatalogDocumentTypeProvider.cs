using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Algora Catalog document type definition.
/// This is the root container for all categories and products - works like a CMS page.
/// </summary>
public sealed class CatalogDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 5; // First priority - must be created before Category

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = CatalogAlias,
            Name = "Algora Catalog",
            Description = "Algora Commerce Catalog - The root container for your product catalog. Add this to your content tree to start building your store.",
            Icon = "icon-store",
            IconColor = BrandColor,
            AllowedAsRoot = true,
            AllowedChildTypes = [CategoryAlias], // Only categories allowed as direct children
            DefaultTemplate = "algoraCatalog", // Links to Views/algoraCatalog.cshtml
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateContentGroup(),
            CreateSettingsGroup()
        ];
    }

    /// <summary>
    /// Content tab - Catalog page content
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
                new PropertyDefinition
                {
                    Alias = "title",
                    Name = "Catalog Title",
                    Description = "Main title for the catalog/shop page",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "subtitle",
                    Name = "Subtitle",
                    Description = "Optional subtitle or tagline",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "Catalog page description/introduction",
                    DataType = WellKnown(WellKnownDataType.RichText, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "heroImage",
                    Name = "Hero Image",
                    Description = "Banner/hero image for the catalog page",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                }
            ]
        };
    }

    /// <summary>
    /// Settings tab - SEO and page settings
    /// </summary>
    private static PropertyGroupDefinition CreateSettingsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "settings",
            Name = "Settings",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "productsPerPage",
                    Name = "Products Per Page",
                    Description = "Number of products to show per page",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "defaultSortOrder",
                    Name = "Default Sort Order",
                    Description = "Default product sorting (newest, price-asc, price-desc, name)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "showFilters",
                    Name = "Show Filters",
                    Description = "Display category and price filters",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "metaTitle",
                    Name = "Meta Title",
                    Description = "SEO page title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 10
                },
                new PropertyDefinition
                {
                    Alias = "metaDescription",
                    Name = "Meta Description",
                    Description = "SEO page description",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 11
                }
            ]
        };
    }
}
