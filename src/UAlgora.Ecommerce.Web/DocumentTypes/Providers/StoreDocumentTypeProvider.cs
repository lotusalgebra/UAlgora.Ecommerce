using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Store document type definition for multi-store support.
/// Each store can have its own branding, settings, currencies, and configuration.
/// </summary>
public sealed class StoreDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 0; // First - root container for multi-store

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = StoreAlias,
            Name = "Algora Store",
            Description = "A store in a multi-store e-commerce setup. Configure branding, currencies, and store-specific settings.",
            Icon = StoreIcon,
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
            CreateBasicGroup(),
            CreateBrandingGroup(),
            CreateContactGroup(),
            CreateLocalizationGroup(),
            CreateCheckoutGroup(),
            CreateSocialGroup(),
            CreateLicenseGroup()
        ];
    }

    private static PropertyGroupDefinition CreateBasicGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "basic",
            Name = "Basic Information",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "storeCode",
                    Name = "Store Code",
                    Description = "Unique identifier code for this store (e.g., 'US', 'UK', 'EU')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "storeName",
                    Name = "Store Name",
                    Description = "Display name for the store",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "Store description for SEO and internal reference",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "domain",
                    Name = "Domain",
                    Description = "Primary domain for this store (e.g., 'store.example.com')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "urlSlug",
                    Name = "URL Slug",
                    Description = "URL path if running multiple stores on same domain (e.g., '/us', '/uk')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "isActive",
                    Name = "Active",
                    Description = "Whether this store is active",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateBrandingGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "branding",
            Name = "Branding",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "logo",
                    Name = "Logo",
                    Description = "Store logo",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "logoDark",
                    Name = "Logo (Dark Mode)",
                    Description = "Logo for dark mode",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "favicon",
                    Name = "Favicon",
                    Description = "Browser favicon",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "primaryColor",
                    Name = "Primary Color",
                    Description = "Main brand color (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "secondaryColor",
                    Name = "Secondary Color",
                    Description = "Secondary brand color (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "accentColor",
                    Name = "Accent Color",
                    Description = "Accent color for buttons and CTAs (hex code)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateContactGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "contact",
            Name = "Contact Information",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "contactEmail",
                    Name = "Contact Email",
                    Description = "Primary contact email",
                    DataType = WellKnown(WellKnownDataType.EmailAddress, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "supportEmail",
                    Name = "Support Email",
                    Description = "Customer support email",
                    DataType = WellKnown(WellKnownDataType.EmailAddress, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "phone",
                    Name = "Phone",
                    Description = "Contact phone number",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "addressLine1",
                    Name = "Address Line 1",
                    Description = "Street address",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "addressLine2",
                    Name = "Address Line 2",
                    Description = "Suite, apartment, etc.",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "city",
                    Name = "City",
                    Description = "City",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "state",
                    Name = "State/Province",
                    Description = "State or province",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "postalCode",
                    Name = "Postal Code",
                    Description = "ZIP or postal code",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "countryCode",
                    Name = "Country Code",
                    Description = "ISO country code (e.g., US, UK, DE)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateLocalizationGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "localization",
            Name = "Localization",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "defaultCurrency",
                    Name = "Default Currency",
                    Description = "Default currency code (USD, EUR, GBP)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "supportedCurrencies",
                    Name = "Supported Currencies",
                    Description = "Comma-separated currency codes (e.g., USD,EUR,GBP)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "defaultLanguage",
                    Name = "Default Language",
                    Description = "Default language code (en-US, de-DE)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "supportedLanguages",
                    Name = "Supported Languages",
                    Description = "Comma-separated language codes",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "timezone",
                    Name = "Timezone",
                    Description = "Store timezone (e.g., America/New_York)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "taxIncludedInPrices",
                    Name = "Prices Include Tax",
                    Description = "Are displayed prices tax-inclusive?",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateCheckoutGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "checkout",
            Name = "Checkout Settings",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "allowGuestCheckout",
                    Name = "Allow Guest Checkout",
                    Description = "Allow checkout without account",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "minimumOrderAmount",
                    Name = "Minimum Order Amount",
                    Description = "Minimum order value for checkout",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "freeShippingThreshold",
                    Name = "Free Shipping Threshold",
                    Description = "Order amount for free shipping",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "orderNumberPrefix",
                    Name = "Order Number Prefix",
                    Description = "Prefix for order numbers (e.g., ORD, INV)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "maxCartItems",
                    Name = "Max Cart Items",
                    Description = "Maximum items allowed in cart",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
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
            SortOrder = 5,
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
                    Alias = "tiktok",
                    Name = "TikTok URL",
                    Description = "TikTok profile URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateLicenseGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "license",
            Name = "License & Status",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "licenseKey",
                    Name = "License Key",
                    Description = "Algora Commerce license key for this store",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "licenseType",
                    Name = "License Type",
                    Description = "Trial, Standard, or Enterprise",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "storeStatus",
                    Name = "Store Status",
                    Description = "Active, Maintenance, Suspended, or Closed",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }
}
