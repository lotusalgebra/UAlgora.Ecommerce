using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Cart Page document type definition.
/// Shopping cart with customizable labels, promo codes, and recommendations.
/// </summary>
public sealed class CartPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 9; // After Checkout Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = CartPageAlias,
            Name = "Algora Cart Page",
            Description = "Shopping cart page with customizable labels, promo codes, shipping thresholds, and product recommendations.",
            Icon = CartPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraCartPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeroGroup(),
            CreateEmptyCartGroup(),
            CreateCartItemsGroup(),
            CreatePromoCodeGroup(),
            CreateOrderSummaryGroup(),
            CreateShippingGroup(),
            CreateRecommendationsGroup(),
            CreateSeoGroup()
        ];
    }

    private static PropertyGroupDefinition CreateHeroGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "hero",
            Name = "Hero Section",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "pageTitle",
                    Name = "Page Title",
                    Description = "Main heading for the cart page (default: Your Bag)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbHome",
                    Name = "Breadcrumb Home Text",
                    Description = "Text for home link in breadcrumb",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "breadcrumbCurrent",
                    Name = "Breadcrumb Current Text",
                    Description = "Text for current page in breadcrumb",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateEmptyCartGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "emptyCart",
            Name = "Empty Cart State",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "emptyTitle",
                    Name = "Empty Cart Title",
                    Description = "Heading when cart is empty",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "emptyMessage",
                    Name = "Empty Cart Message",
                    Description = "Message shown when cart is empty",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "emptyButtonText",
                    Name = "Empty Cart Button Text",
                    Description = "Text for the start shopping button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "emptyButtonUrl",
                    Name = "Empty Cart Button URL",
                    Description = "URL for the start shopping button (default: /products)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateCartItemsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "cartItems",
            Name = "Cart Items Section",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "productColumnHeader",
                    Name = "Product Column Header",
                    Description = "Header text for product column",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "totalColumnHeader",
                    Name = "Total Column Header",
                    Description = "Header text for total column",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "colorLabel",
                    Name = "Color Label",
                    Description = "Label for color attribute",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "sizeLabel",
                    Name = "Size Label",
                    Description = "Label for size attribute",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "eachLabel",
                    Name = "Each Label",
                    Description = "Label for per-unit price (e.g., '$10.00 each')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "saveForLaterText",
                    Name = "Save for Later Text",
                    Description = "Text for move to wishlist button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "removeText",
                    Name = "Remove Text",
                    Description = "Text for remove item button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "continueShoppingText",
                    Name = "Continue Shopping Text",
                    Description = "Text for continue shopping link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "clearBagText",
                    Name = "Clear Bag Text",
                    Description = "Text for clear cart button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreatePromoCodeGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "promoCode",
            Name = "Promo Code Section",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "promoCodeEnabled",
                    Name = "Enable Promo Code",
                    Description = "Show promo code input field",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "promoCodeLabel",
                    Name = "Promo Code Label",
                    Description = "Label for promo code field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "promoCodePlaceholder",
                    Name = "Promo Code Placeholder",
                    Description = "Placeholder text for promo code input",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "promoCodeButtonText",
                    Name = "Apply Button Text",
                    Description = "Text for apply promo code button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateOrderSummaryGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "orderSummary",
            Name = "Order Summary",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "orderSummaryTitle",
                    Name = "Section Title",
                    Description = "Heading for order summary section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "subtotalLabel",
                    Name = "Subtotal Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "itemsLabel",
                    Name = "Items Label",
                    Description = "Text after item count (e.g., 'items')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "discountLabel",
                    Name = "Discount Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "shippingLabel",
                    Name = "Shipping Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "taxLabel",
                    Name = "Tax Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "totalLabel",
                    Name = "Total Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "checkoutButtonText",
                    Name = "Checkout Button Text",
                    Description = "Text for proceed to checkout button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "secureCheckoutText",
                    Name = "Secure Checkout Text",
                    Description = "Text for secure checkout badge",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateShippingGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "shipping",
            Name = "Free Shipping Promotion",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "freeShippingEnabled",
                    Name = "Enable Free Shipping Promotion",
                    Description = "Show free shipping progress bar",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "freeShippingThreshold",
                    Name = "Free Shipping Threshold",
                    Description = "Order amount for free shipping (default: 100)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "freeShippingMessage",
                    Name = "Free Shipping Message",
                    Description = "Message template (use {amount} for remaining amount)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "standardShippingCost",
                    Name = "Standard Shipping Cost",
                    Description = "Default shipping cost when under threshold",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "freeLabel",
                    Name = "Free Label",
                    Description = "Text shown when shipping is free",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateRecommendationsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "recommendations",
            Name = "Recommendations Section",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "recommendationsEnabled",
                    Name = "Enable Recommendations",
                    Description = "Show 'You might also like' section",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "recommendationsLabel",
                    Name = "Section Label",
                    Description = "Small label above title",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "recommendationsTitle",
                    Name = "Section Title",
                    Description = "Heading for recommendations section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "viewAllText",
                    Name = "View All Text",
                    Description = "Text for view all products link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "recommendationsCount",
                    Name = "Number of Products",
                    Description = "How many products to show (default: 4)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
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
            SortOrder = 7,
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
