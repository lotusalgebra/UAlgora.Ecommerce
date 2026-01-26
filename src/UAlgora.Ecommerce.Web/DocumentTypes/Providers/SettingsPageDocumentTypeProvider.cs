using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Settings Page document type definition.
/// Customer account settings page with profile, password, and notification management.
/// </summary>
public sealed class SettingsPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 19;

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = SettingsPageAlias,
            Name = "Algora Settings Page",
            Description = "Customer account settings page for profile, password, and notifications.",
            Icon = SettingsPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraSettingsPage",
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateBreadcrumbGroup(),
            CreateProfileGroup(),
            CreatePasswordGroup(),
            CreateNotificationsGroup(),
            CreateDangerZoneGroup(),
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
                    Alias = "breadcrumbSettingsText",
                    Name = "Settings Text",
                    Description = "Text for current page in breadcrumb",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateProfileGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "profile",
            Name = "Profile Section",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "profileTitle",
                    Name = "Profile Section Title",
                    Description = "Heading for profile information section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "profileSubtitle",
                    Name = "Profile Section Subtitle",
                    Description = "Subheading for profile section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "emailDisabledNote",
                    Name = "Email Disabled Note",
                    Description = "Note explaining why email cannot be changed",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "saveProfileButtonText",
                    Name = "Save Profile Button",
                    Description = "Text for save profile button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreatePasswordGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "password",
            Name = "Password Section",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "passwordTitle",
                    Name = "Password Section Title",
                    Description = "Heading for password change section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "passwordSubtitle",
                    Name = "Password Section Subtitle",
                    Description = "Subheading for password section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "currentPasswordLabel",
                    Name = "Current Password Label",
                    Description = "Label for current password field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "newPasswordLabel",
                    Name = "New Password Label",
                    Description = "Label for new password field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "confirmPasswordLabel",
                    Name = "Confirm Password Label",
                    Description = "Label for confirm password field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "passwordRequirement",
                    Name = "Password Requirement Text",
                    Description = "Text showing password requirements",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "updatePasswordButtonText",
                    Name = "Update Password Button",
                    Description = "Text for update password button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateNotificationsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "notifications",
            Name = "Notifications Section",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "notificationsTitle",
                    Name = "Notifications Section Title",
                    Description = "Heading for notification preferences",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "notificationsSubtitle",
                    Name = "Notifications Section Subtitle",
                    Description = "Subheading for notifications section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "marketingEmailsLabel",
                    Name = "Marketing Emails Label",
                    Description = "Label for marketing emails toggle",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "marketingEmailsDescription",
                    Name = "Marketing Emails Description",
                    Description = "Description for marketing emails option",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "smsNotificationsLabel",
                    Name = "SMS Notifications Label",
                    Description = "Label for SMS notifications toggle",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "smsNotificationsDescription",
                    Name = "SMS Notifications Description",
                    Description = "Description for SMS notifications option",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "orderUpdatesLabel",
                    Name = "Order Updates Label",
                    Description = "Label for order updates toggle",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "orderUpdatesDescription",
                    Name = "Order Updates Description",
                    Description = "Description for order updates option",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateDangerZoneGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "dangerZone",
            Name = "Danger Zone",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "dangerZoneTitle",
                    Name = "Danger Zone Title",
                    Description = "Heading for danger zone section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "dangerZoneSubtitle",
                    Name = "Danger Zone Subtitle",
                    Description = "Subheading for danger zone",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "deleteAccountLabel",
                    Name = "Delete Account Label",
                    Description = "Label for delete account action",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "deleteAccountDescription",
                    Name = "Delete Account Description",
                    Description = "Description for delete account action",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "deleteButtonText",
                    Name = "Delete Button Text",
                    Description = "Text for delete account button",
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
