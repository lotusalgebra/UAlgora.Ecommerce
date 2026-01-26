using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Addresses Page document type definition.
/// Customer address management page with add/edit/delete functionality.
/// </summary>
public sealed class AddressesPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 18;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = AddressesPageAlias,
            Name = "Algora Addresses Page",
            Description = "Customer address management page for shipping and billing addresses.",
            Icon = AddressesPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraAddressesPage",
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateBreadcrumbGroup(),
            CreateHeaderGroup(),
            CreateEmptyStateGroup(),
            CreateAddressCardGroup(),
            CreateModalGroup(),
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
                    Alias = "breadcrumbHomeText",
                    Name = "Home Text",
                    Description = "Text for home link in breadcrumb",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbAccountText",
                    Name = "Account Text",
                    Description = "Text for account link in breadcrumb",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbAddressesText",
                    Name = "Addresses Text",
                    Description = "Text for current page in breadcrumb",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateHeaderGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "header",
            Name = "Page Header",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "pageTitle",
                    Name = "Page Title",
                    Description = "Main heading for the addresses page",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "addButtonText",
                    Name = "Add Button Text",
                    Description = "Text for the add new address button",
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
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "emptyTitle",
                    Name = "Empty State Title",
                    Description = "Title when no addresses are saved",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "emptyDescription",
                    Name = "Empty State Description",
                    Description = "Description text when no addresses",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "emptyButtonText",
                    Name = "Empty State Button",
                    Description = "Button text to add first address",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateAddressCardGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "addressCard",
            Name = "Address Card",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "defaultShippingBadge",
                    Name = "Default Shipping Badge",
                    Description = "Badge text for default shipping address",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "defaultBillingBadge",
                    Name = "Default Billing Badge",
                    Description = "Badge text for default billing address",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "editButtonText",
                    Name = "Edit Button Text",
                    Description = "Text for edit address button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "setShippingDefaultText",
                    Name = "Set Shipping Default Text",
                    Description = "Text for set as shipping default button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "setBillingDefaultText",
                    Name = "Set Billing Default Text",
                    Description = "Text for set as billing default button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "deleteButtonText",
                    Name = "Delete Button Text",
                    Description = "Text for delete address button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateModalGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "modal",
            Name = "Add/Edit Modal",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "addModalTitle",
                    Name = "Add Modal Title",
                    Description = "Title when adding new address",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "editModalTitle",
                    Name = "Edit Modal Title",
                    Description = "Title when editing address",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "cancelButtonText",
                    Name = "Cancel Button Text",
                    Description = "Text for cancel button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "saveButtonText",
                    Name = "Save Button Text",
                    Description = "Text for save button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "updateButtonText",
                    Name = "Update Button Text",
                    Description = "Text for update button when editing",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
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
            SortOrder = 5,
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
