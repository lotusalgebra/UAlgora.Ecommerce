using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Algora Checkout Step document type definition.
/// Represents a step in the multi-step checkout process.
/// </summary>
public sealed class CheckoutStepDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 40;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = CheckoutStepAlias,
            Name = "Algora Checkout Step",
            Description = "Algora Commerce Checkout Step - A step in the checkout process",
            Icon = "icon-directions-alt",
            IconColor = BrandColor,
            AllowedAsRoot = true,
            IsElement = false,
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
    /// Content tab - Step information
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
                    Alias = "stepName",
                    Name = "Step Name",
                    Description = "Display name for this checkout step",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "stepTitle",
                    Name = "Step Title",
                    Description = "Title shown to customer during checkout",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "stepDescription",
                    Name = "Step Description",
                    Description = "Brief description of this step",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "instructions",
                    Name = "Instructions",
                    Description = "Instructions or help text for the customer",
                    DataType = WellKnown(WellKnownDataType.RichText, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 3
                }
            ]
        };
    }

    /// <summary>
    /// Settings tab - Step configuration
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
                    Alias = "stepType",
                    Name = "Step Type",
                    Description = "Type of checkout step (e.g., Information, Shipping, Payment, Review)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "stepOrder",
                    Name = "Step Order",
                    Description = "Order of this step in the checkout flow (1, 2, 3...)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    IsMandatory = true,
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "isRequired",
                    Name = "Required Step",
                    Description = "Whether this step is required to complete checkout",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "isEnabled",
                    Name = "Enabled",
                    Description = "Enable or disable this checkout step",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "icon",
                    Name = "Step Icon",
                    Description = "Icon class for this step (e.g., icon-user, icon-truck, icon-credit-card)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "validationRules",
                    Name = "Validation Rules",
                    Description = "Custom validation rules (JSON format)",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 10
                }
            ]
        };
    }
}
