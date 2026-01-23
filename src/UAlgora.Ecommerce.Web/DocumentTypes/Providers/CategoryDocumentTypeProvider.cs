using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Algora Category document type definition.
/// Categories work like CMS pages - they can have URLs, templates, and nested content.
/// Follows Umbraco standard tab patterns: Content, Media, Settings
/// </summary>
public sealed class CategoryDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 15; // After Catalog, before Product

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = CategoryAlias,
            Name = "Algora Category",
            Description = "Algora Commerce Category - A product category page for organizing products. Add under Catalog or another Category.",
            Icon = "icon-categories",
            IconColor = BrandColor,
            AllowedAsRoot = false, // Categories must be children of Catalog or other Categories
            AllowedChildTypes = [CategoryAlias, ProductAlias], // Can contain sub-categories and products
            DefaultTemplate = "algoraCategory", // Links to Views/algoraCategory.cshtml
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateContentGroup(),
            CreateMediaGroup(),
            CreateSettingsGroup()
        ];
    }

    /// <summary>
    /// Content tab - Category information (Umbraco standard)
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
                    Alias = "categoryName",
                    Name = "Category Name",
                    Description = "Display name for the category",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "Category description",
                    DataType = WellKnown(WellKnownDataType.RichText, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 1
                }
            ]
        };
    }

    /// <summary>
    /// Media tab - Category images (Umbraco standard)
    /// </summary>
    private static PropertyGroupDefinition CreateMediaGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "media",
            Name = "Media",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "image",
                    Name = "Category Image",
                    Description = "Image representing the category",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "bannerImage",
                    Name = "Banner Image",
                    Description = "Banner image for category pages",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                }
            ]
        };
    }

    /// <summary>
    /// Settings tab - Navigation, visibility, SEO (Umbraco standard)
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
                    Alias = "slug",
                    Name = "URL Slug",
                    Description = "URL-friendly identifier",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "isVisible",
                    Name = "Visible",
                    Description = "Show category in navigation",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "sortOrder",
                    Name = "Sort Order",
                    Description = "Order in navigation (lower = first)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
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
