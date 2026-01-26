using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Order Confirmation Page document type definition.
/// Displays order confirmation with status, delivery info, and next steps.
/// </summary>
public sealed class OrderConfirmationPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 17; // After Deals Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = OrderConfirmationPageAlias,
            Name = "Algora Order Confirmation Page",
            Description = "Order confirmation page displayed after successful purchase with order status and next steps.",
            Icon = OrderConfirmationPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraOrderConfirmationPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeroGroup(),
            CreateOrderInfoGroup(),
            CreateEmailGroup(),
            CreateStatusGroup(),
            CreateDeliveryGroup(),
            CreateActionsGroup(),
            CreateWhatsNextGroup(),
            CreateHelpGroup(),
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
                    Alias = "heroTitle",
                    Name = "Hero Title",
                    Description = "Main success title (default: 'Order Confirmed')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "heroSubtitle",
                    Name = "Hero Subtitle",
                    Description = "Subtitle text (default: 'Thank you for your purchase, you're going to love it.')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "orderNumberPrefix",
                    Name = "Order Number Prefix",
                    Description = "Text before order number (default: 'Order #')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "showConfetti",
                    Name = "Show Confetti Animation",
                    Description = "Display confetti animation on page load",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateOrderInfoGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "orderInfo",
            Name = "Order Info Section",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "orderNumberLabel",
                    Name = "Order Number Label",
                    Description = "Label for order number (default: 'Order Number')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "orderDateLabel",
                    Name = "Order Date Label",
                    Description = "Label for order date (default: 'Order Date')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "paymentLabel",
                    Name = "Payment Label",
                    Description = "Label for payment status (default: 'Payment')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "paidText",
                    Name = "Paid Status Text",
                    Description = "Text for paid status (default: 'Paid')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateEmailGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "email",
            Name = "Email Confirmation",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "emailTitle",
                    Name = "Email Confirmation Title",
                    Description = "Title for email notice (default: 'Confirmation Email Sent')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "emailDescription",
                    Name = "Email Description",
                    Description = "Description text. Use {email} as placeholder (default: 'We've sent order details and tracking info to {email}')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateStatusGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "status",
            Name = "Order Status",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "statusTitle",
                    Name = "Status Section Title",
                    Description = "Title for status section (default: 'Order Status')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "confirmedLabel",
                    Name = "Confirmed Step Label",
                    Description = "Label for confirmed step (default: 'Confirmed')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "processingLabel",
                    Name = "Processing Step Label",
                    Description = "Label for processing step (default: 'Processing')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "shippedLabel",
                    Name = "Shipped Step Label",
                    Description = "Label for shipped step (default: 'Shipped')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "deliveredLabel",
                    Name = "Delivered Step Label",
                    Description = "Label for delivered step (default: 'Delivered')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateDeliveryGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "delivery",
            Name = "Delivery Info",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "deliveryLabel",
                    Name = "Delivery Label",
                    Description = "Label for estimated delivery (default: 'Estimated Delivery')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "deliveryDaysMin",
                    Name = "Minimum Delivery Days",
                    Description = "Minimum days for delivery estimate (default: 5)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "deliveryDaysMax",
                    Name = "Maximum Delivery Days",
                    Description = "Maximum days for delivery estimate (default: 7)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateActionsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "actions",
            Name = "Action Buttons",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "viewOrderText",
                    Name = "View Order Button Text",
                    Description = "Text for view order button (default: 'View Order Details')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "continueShoppingText",
                    Name = "Continue Shopping Button Text",
                    Description = "Text for continue shopping button (default: 'Continue Shopping')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "continueShoppingUrl",
                    Name = "Continue Shopping URL",
                    Description = "URL for continue shopping button (default: '/products')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateWhatsNextGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "whatsNext",
            Name = "What's Next Section",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "showWhatsNext",
                    Name = "Show What's Next Section",
                    Description = "Display the what's next cards",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "trackTitle",
                    Name = "Track Updates Title",
                    Description = "Title for tracking card (default: 'Track Updates')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "trackDescription",
                    Name = "Track Updates Description",
                    Description = "Description for tracking card",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "returnsTitle",
                    Name = "Easy Returns Title",
                    Description = "Title for returns card (default: 'Easy Returns')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "returnsDescription",
                    Name = "Easy Returns Description",
                    Description = "Description for returns card",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "returnDays",
                    Name = "Return Policy Days",
                    Description = "Number of days for return policy (default: 30)",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "pointsTitle",
                    Name = "Earn Points Title",
                    Description = "Title for loyalty points card (default: 'Earn Points')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "pointsDescription",
                    Name = "Earn Points Description",
                    Description = "Description for points card. Use {points} as placeholder",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "showLoyaltyPoints",
                    Name = "Show Loyalty Points Card",
                    Description = "Display the loyalty points card",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateHelpGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "help",
            Name = "Help Section",
            SortOrder = 7,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "helpText",
                    Name = "Help Text",
                    Description = "Text for help section (default: 'Questions about your order?')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "contactLinkText",
                    Name = "Contact Link Text",
                    Description = "Text for contact link (default: 'Contact our support team')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "contactUrl",
                    Name = "Contact URL",
                    Description = "URL for contact link (default: '/contact')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
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
            SortOrder = 8,
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
