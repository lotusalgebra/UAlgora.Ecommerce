using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Testimonial document type definition.
/// Customer testimonials/reviews for social proof.
/// </summary>
public sealed class TestimonialDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 12;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = TestimonialAlias,
            Name = "Algora Testimonial",
            Description = "A customer testimonial with name, photo, rating, and review text.",
            Icon = TestimonialIcon,
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
                    Alias = "customerName",
                    Name = "Customer Name",
                    Description = "Name of the customer",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "customerTitle",
                    Name = "Customer Title",
                    Description = "Job title or location (e.g., 'Marketing Manager' or 'New York, NY')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "customerPhoto",
                    Name = "Customer Photo",
                    Description = "Profile photo",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "rating",
                    Name = "Rating",
                    Description = "Star rating (1-5)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "reviewText",
                    Name = "Review Text",
                    Description = "The testimonial content",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    IsMandatory = true,
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "reviewDate",
                    Name = "Review Date",
                    Description = "When the review was written",
                    DataType = WellKnown(WellKnownDataType.DatePicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "isVerified",
                    Name = "Verified Purchase",
                    Description = "Is this a verified purchase?",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "sortOrder",
                    Name = "Sort Order",
                    Description = "Display order",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 7
                }
            ]
        };
    }
}
