using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Banner document type definition.
/// Promotional banners for the home page and other pages.
/// </summary>
public sealed class BannerDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 11;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = BannerAlias,
            Name = "Algora Banner",
            Description = "A promotional banner with image, text, and call-to-action.",
            Icon = BannerIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateContentGroup(),
            CreateStyleGroup()
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
                    Description = "Banner heading",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "subtitle",
                    Name = "Subtitle",
                    Description = "Secondary text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "Additional text content",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "image",
                    Name = "Banner Image",
                    Description = "Background or product image",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "buttonText",
                    Name = "Button Text",
                    Description = "CTA button label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "buttonLink",
                    Name = "Button Link",
                    Description = "CTA button URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "discountText",
                    Name = "Discount Badge Text",
                    Description = "Optional discount badge (e.g., '50% OFF')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateStyleGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "style",
            Name = "Style",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "backgroundColor",
                    Name = "Background Color",
                    Description = "Background color (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "textColor",
                    Name = "Text Color",
                    Description = "Text color (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "bannerSize",
                    Name = "Banner Size",
                    Description = "small, medium, large, or full",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "sortOrder",
                    Name = "Sort Order",
                    Description = "Display order",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                }
            ]
        };
    }
}
