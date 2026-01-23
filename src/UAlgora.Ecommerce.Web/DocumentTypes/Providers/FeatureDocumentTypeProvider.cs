using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Feature/USP document type definition.
/// Features like Free Shipping, Easy Returns, Secure Payment, 24/7 Support.
/// </summary>
public sealed class FeatureDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 13;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = FeatureAlias,
            Name = "Algora Feature",
            Description = "A store feature/USP with icon, title, and description (e.g., Free Shipping, Easy Returns).",
            Icon = FeatureIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateContentGroup()
        ];
    }

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
                    Name = "Title",
                    Description = "Feature title (e.g., 'Free Shipping')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "Short description text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "icon",
                    Name = "Icon",
                    Description = "Icon name (e.g., 'truck', 'refresh', 'shield', 'headset') or custom SVG",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "iconImage",
                    Name = "Icon Image",
                    Description = "Custom icon image (optional, overrides icon name)",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "iconColor",
                    Name = "Icon Color",
                    Description = "Icon color (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "link",
                    Name = "Link",
                    Description = "Optional link URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "sortOrder",
                    Name = "Sort Order",
                    Description = "Display order",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 6
                }
            ]
        };
    }
}
