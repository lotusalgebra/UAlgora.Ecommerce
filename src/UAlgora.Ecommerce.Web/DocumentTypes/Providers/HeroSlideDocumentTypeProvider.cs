using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Hero Slide document type definition.
/// Individual slides for the hero carousel on the home page.
/// </summary>
public sealed class HeroSlideDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 10;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = HeroSlideAlias,
            Name = "Algora Hero Slide",
            Description = "A slide for the hero carousel - includes image, title, text, and CTA button.",
            Icon = SliderIcon,
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
                    Description = "Main heading text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "subtitle",
                    Name = "Subtitle",
                    Description = "Secondary heading or tagline",
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
                    Alias = "backgroundImage",
                    Name = "Background Image",
                    Description = "Full-width background image",
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
                    Alias = "secondaryButtonText",
                    Name = "Secondary Button Text",
                    Description = "Optional second button label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "secondaryButtonLink",
                    Name = "Secondary Button Link",
                    Description = "Optional second button URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
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
                    Description = "Gradient or solid background color (hex or Tailwind class)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "textColor",
                    Name = "Text Color",
                    Description = "Text color (light/dark)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "textAlignment",
                    Name = "Text Alignment",
                    Description = "left, center, or right",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "overlayOpacity",
                    Name = "Overlay Opacity",
                    Description = "Dark overlay opacity (0-100)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                }
            ]
        };
    }
}
