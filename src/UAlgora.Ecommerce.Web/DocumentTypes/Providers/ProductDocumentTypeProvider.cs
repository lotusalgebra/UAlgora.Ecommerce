using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Algora Product document type definition.
/// Products are leaf nodes in the content tree - they cannot contain children.
/// Follows Umbraco standard tab patterns: Content, Commerce, Media, Settings
/// </summary>
public sealed class ProductDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 20; // After Category

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = ProductAlias,
            Name = "Algora Product",
            Description = "Algora Commerce Product - A product page with pricing, inventory, and SEO support. Add under a Category.",
            Icon = "icon-barcode",
            IconColor = BrandColor,
            AllowedAsRoot = false, // Products must be children of Categories
            DefaultTemplate = "algoraProduct", // Links to Views/algoraProduct.cshtml
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateContentGroup(),
            CreateCommerceGroup(),
            CreateMediaGroup(),
            CreateSettingsGroup()
        ];
    }

    /// <summary>
    /// Content tab - Product information (Umbraco standard)
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
                    Alias = "productName",
                    Name = "Product Name",
                    Description = "The display name of the product",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "sku",
                    Name = "SKU",
                    Description = "Stock Keeping Unit - unique product identifier",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    IsMandatory = true,
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "shortDescription",
                    Name = "Short Description",
                    Description = "Brief summary for product listings",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "description",
                    Name = "Description",
                    Description = "Full product description",
                    DataType = WellKnown(WellKnownDataType.RichText, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "brand",
                    Name = "Brand",
                    Description = "Product brand or manufacturer",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "manufacturer",
                    Name = "Manufacturer",
                    Description = "Product manufacturer name",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "mpn",
                    Name = "MPN",
                    Description = "Manufacturer Part Number",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "gtin",
                    Name = "GTIN/Barcode",
                    Description = "Global Trade Item Number (UPC, EAN, ISBN)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                }
            ]
        };
    }

    /// <summary>
    /// Commerce tab - Pricing, inventory, and shipping (E-commerce specific)
    /// </summary>
    private static PropertyGroupDefinition CreateCommerceGroup()
    {
        var numericDataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring));

        return new PropertyGroupDefinition
        {
            Alias = "commerce",
            Name = "Commerce",
            SortOrder = 1,
            Properties =
            [
                // Pricing Section
                new PropertyDefinition
                {
                    Alias = "basePrice",
                    Name = "Base Price",
                    Description = "Regular retail price",
                    DataType = numericDataType,
                    IsMandatory = true,
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "salePrice",
                    Name = "Sale Price",
                    Description = "Discounted price (leave empty if not on sale)",
                    DataType = numericDataType,
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "costPrice",
                    Name = "Cost Price",
                    Description = "Your cost for margin calculation",
                    DataType = numericDataType,
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "compareAtPrice",
                    Name = "Compare At Price",
                    Description = "Original price for showing savings",
                    DataType = numericDataType,
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "currencyCode",
                    Name = "Currency",
                    Description = "Currency code (USD, EUR, GBP)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "taxIncluded",
                    Name = "Tax Included",
                    Description = "Prices include tax",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "taxClass",
                    Name = "Tax Class",
                    Description = "Tax classification",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                // Inventory Section
                new PropertyDefinition
                {
                    Alias = "trackInventory",
                    Name = "Track Inventory",
                    Description = "Enable stock tracking",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 10
                },
                new PropertyDefinition
                {
                    Alias = "stockQuantity",
                    Name = "Stock Quantity",
                    Description = "Available quantity",
                    DataType = numericDataType,
                    SortOrder = 11
                },
                new PropertyDefinition
                {
                    Alias = "lowStockThreshold",
                    Name = "Low Stock Alert",
                    Description = "Alert threshold",
                    DataType = numericDataType,
                    SortOrder = 12
                },
                new PropertyDefinition
                {
                    Alias = "allowBackorders",
                    Name = "Allow Backorders",
                    Description = "Allow orders when out of stock",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 13
                },
                // Shipping Section
                new PropertyDefinition
                {
                    Alias = "weight",
                    Name = "Weight (kg)",
                    Description = "Product weight",
                    DataType = numericDataType,
                    SortOrder = 20
                },
                new PropertyDefinition
                {
                    Alias = "length",
                    Name = "Length (cm)",
                    Description = "Package length",
                    DataType = numericDataType,
                    SortOrder = 21
                },
                new PropertyDefinition
                {
                    Alias = "width",
                    Name = "Width (cm)",
                    Description = "Package width",
                    DataType = numericDataType,
                    SortOrder = 22
                },
                new PropertyDefinition
                {
                    Alias = "height",
                    Name = "Height (cm)",
                    Description = "Package height",
                    DataType = numericDataType,
                    SortOrder = 23
                },
                new PropertyDefinition
                {
                    Alias = "shippingClass",
                    Name = "Shipping Class",
                    Description = "Shipping rate class",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 24
                },
                new PropertyDefinition
                {
                    Alias = "requiresShipping",
                    Name = "Requires Shipping",
                    Description = "Physical product needs shipping",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 25
                }
            ]
        };
    }

    /// <summary>
    /// Media tab - Product images (Umbraco standard)
    /// </summary>
    private static PropertyGroupDefinition CreateMediaGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "media",
            Name = "Media",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "primaryImage",
                    Name = "Primary Image",
                    Description = "Main product image",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "galleryImages",
                    Name = "Gallery Images",
                    Description = "Additional product images",
                    DataType = WellKnown(WellKnownDataType.MultipleMediaPicker, WellKnown(WellKnownDataType.Textarea)),
                    SortOrder = 1
                }
            ]
        };
    }

    /// <summary>
    /// Settings tab - SEO, visibility, navigation (Umbraco standard)
    /// </summary>
    private static PropertyGroupDefinition CreateSettingsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "settings",
            Name = "Settings",
            SortOrder = 3,
            Properties =
            [
                // Navigation/URL
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
                    Alias = "status",
                    Name = "Status",
                    Description = "Publication status",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "isVisible",
                    Name = "Visible in Store",
                    Description = "Show product in storefront",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "isFeatured",
                    Name = "Featured Product",
                    Description = "Feature on homepage",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "sortOrder",
                    Name = "Sort Order",
                    Description = "Display order in listings",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                },
                // SEO Section
                new PropertyDefinition
                {
                    Alias = "metaTitle",
                    Name = "Meta Title",
                    Description = "SEO page title (max 70 chars)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 10
                },
                new PropertyDefinition
                {
                    Alias = "metaDescription",
                    Name = "Meta Description",
                    Description = "SEO description (max 160 chars)",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 11
                },
                new PropertyDefinition
                {
                    Alias = "metaKeywords",
                    Name = "Meta Keywords",
                    Description = "Comma-separated keywords",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 12
                }
            ]
        };
    }
}
