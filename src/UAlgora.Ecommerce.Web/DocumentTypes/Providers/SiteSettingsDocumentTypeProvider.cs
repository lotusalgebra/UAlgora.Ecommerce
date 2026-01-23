using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Site Settings document type definition.
/// Global settings for the entire storefront - branding, contact info, social links.
/// This should be created at the root level and is a singleton.
/// </summary>
public sealed class SiteSettingsDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 1; // First to be created

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = SiteSettingsAlias,
            Name = "Algora Site Settings",
            Description = "Global site settings - branding, contact information, social links, and footer content.",
            Icon = SettingsIcon,
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
            CreateBrandingGroup(),
            CreateContactGroup(),
            CreateSocialGroup(),
            CreateHeaderGroup(),
            CreateFooterGroup(),
            CreateSettingsGroup()
        ];
    }

    private static PropertyGroupDefinition CreateBrandingGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "branding",
            Name = "Branding",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "siteName",
                    Name = "Site Name",
                    Description = "The name of your store",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "tagline",
                    Name = "Tagline",
                    Description = "Short tagline or slogan",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "logo",
                    Name = "Logo",
                    Description = "Site logo image",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "logoDark",
                    Name = "Logo (Dark Mode)",
                    Description = "Logo for dark mode (optional)",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "favicon",
                    Name = "Favicon",
                    Description = "Browser favicon",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "primaryColor",
                    Name = "Primary Color",
                    Description = "Main brand color (hex code, e.g., #3B82F6)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "secondaryColor",
                    Name = "Secondary Color",
                    Description = "Secondary brand color (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateContactGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "contact",
            Name = "Contact",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "email",
                    Name = "Email",
                    Description = "Contact email address",
                    DataType = WellKnown(WellKnownDataType.EmailAddress, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "phone",
                    Name = "Phone",
                    Description = "Contact phone number",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "address",
                    Name = "Address",
                    Description = "Physical store address",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "businessHours",
                    Name = "Business Hours",
                    Description = "Store operating hours",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSocialGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "social",
            Name = "Social Media",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "facebook",
                    Name = "Facebook URL",
                    Description = "Facebook page URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "twitter",
                    Name = "Twitter/X URL",
                    Description = "Twitter/X profile URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "instagram",
                    Name = "Instagram URL",
                    Description = "Instagram profile URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "youtube",
                    Name = "YouTube URL",
                    Description = "YouTube channel URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "linkedin",
                    Name = "LinkedIn URL",
                    Description = "LinkedIn page URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "pinterest",
                    Name = "Pinterest URL",
                    Description = "Pinterest profile URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateHeaderGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "header",
            Name = "Header",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "topBarEnabled",
                    Name = "Show Top Bar",
                    Description = "Display the utility bar above header",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "topBarMessage",
                    Name = "Top Bar Message",
                    Description = "Promotional message in top bar (e.g., 'Free shipping on orders over $50!')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "showLanguageSelector",
                    Name = "Show Language Selector",
                    Description = "Display language dropdown",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "showCurrencySelector",
                    Name = "Show Currency Selector",
                    Description = "Display currency dropdown",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "enableDarkMode",
                    Name = "Enable Dark Mode Toggle",
                    Description = "Allow users to switch to dark mode",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateFooterGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "footer",
            Name = "Footer",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "footerAbout",
                    Name = "About Text",
                    Description = "Short about description for footer",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "copyrightText",
                    Name = "Copyright Text",
                    Description = "Copyright notice (use {year} for current year)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "newsletterEnabled",
                    Name = "Enable Newsletter",
                    Description = "Show newsletter signup in footer",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "newsletterTitle",
                    Name = "Newsletter Title",
                    Description = "Newsletter section heading",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "newsletterText",
                    Name = "Newsletter Text",
                    Description = "Newsletter description text",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "paymentIcons",
                    Name = "Payment Icons",
                    Description = "Payment method images for footer",
                    DataType = WellKnown(WellKnownDataType.MultipleMediaPicker, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSettingsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "settings",
            Name = "Settings",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "defaultCurrency",
                    Name = "Default Currency",
                    Description = "Default currency code (USD, EUR, GBP)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "defaultLanguage",
                    Name = "Default Language",
                    Description = "Default language code (en, es, fr)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "googleAnalyticsId",
                    Name = "Google Analytics ID",
                    Description = "GA tracking ID (e.g., G-XXXXXXXX)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "customHeadScripts",
                    Name = "Custom Head Scripts",
                    Description = "Custom scripts to add in <head>",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "customFooterScripts",
                    Name = "Custom Footer Scripts",
                    Description = "Custom scripts to add before </body>",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 4
                }
            ]
        };
    }
}
