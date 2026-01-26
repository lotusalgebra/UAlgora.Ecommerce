using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Checkout Page document type definition.
/// Multi-step checkout flow with configurable steps, labels, and options.
/// </summary>
public sealed class CheckoutPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 8; // After Register Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = CheckoutPageAlias,
            Name = "Algora Checkout Page",
            Description = "Multi-step checkout page with configurable steps, express checkout, and payment options.",
            Icon = CheckoutPageIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraCheckoutPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeaderGroup(),
            CreateStepsGroup(),
            CreateExpressCheckoutGroup(),
            CreateContactFormGroup(),
            CreateShippingFormGroup(),
            CreatePaymentFormGroup(),
            CreateReviewGroup(),
            CreateTrustBadgesGroup(),
            CreateSeoGroup()
        ];
    }

    private static PropertyGroupDefinition CreateHeaderGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "header",
            Name = "Header",
            SortOrder = 0,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "logoText",
                    Name = "Logo Text",
                    Description = "Text displayed next to the logo (default: ALGORA)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "secureBadgeText",
                    Name = "Secure Badge Text",
                    Description = "Text shown in the secure checkout badge",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "backToCartText",
                    Name = "Back to Cart Text",
                    Description = "Link text to return to cart",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateStepsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "steps",
            Name = "Checkout Steps",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "step1Name",
                    Name = "Step 1 Name",
                    Description = "Name for Information step (default: Information)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "step2Name",
                    Name = "Step 2 Name",
                    Description = "Name for Shipping step (default: Shipping)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "step3Name",
                    Name = "Step 3 Name",
                    Description = "Name for Payment step (default: Payment)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "step4Name",
                    Name = "Step 4 Name",
                    Description = "Name for Review step (default: Review)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateExpressCheckoutGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "expressCheckout",
            Name = "Express Checkout",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "expressCheckoutEnabled",
                    Name = "Enable Express Checkout",
                    Description = "Show express checkout options (Apple Pay, Google Pay, PayPal)",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "expressCheckoutTitle",
                    Name = "Express Checkout Title",
                    Description = "Text above express checkout buttons",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "applePayEnabled",
                    Name = "Enable Apple Pay",
                    Description = "Show Apple Pay button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "googlePayEnabled",
                    Name = "Enable Google Pay",
                    Description = "Show Google Pay button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "paypalExpressEnabled",
                    Name = "Enable PayPal Express",
                    Description = "Show PayPal express button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "expressDividerText",
                    Name = "Divider Text",
                    Description = "Text between express checkout and form (default: Or continue below)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateContactFormGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "contactForm",
            Name = "Contact Information Form",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "contactSectionTitle",
                    Name = "Section Title",
                    Description = "Heading for contact information section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "contactSectionSubtitle",
                    Name = "Section Subtitle",
                    Description = "Subheading text",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "emailLabel",
                    Name = "Email Label",
                    Description = "Label for email field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "firstNameLabel",
                    Name = "First Name Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "lastNameLabel",
                    Name = "Last Name Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "phoneLabel",
                    Name = "Phone Label",
                    Description = "Label for phone field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "continueToShippingText",
                    Name = "Continue Button Text",
                    Description = "Text for the continue to shipping button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateShippingFormGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "shippingForm",
            Name = "Shipping Address Form",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "shippingAddressTitle",
                    Name = "Section Title",
                    Description = "Heading for shipping address section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "addressLabel",
                    Name = "Address Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "apartmentLabel",
                    Name = "Apartment Label",
                    Description = "Label for apartment/suite field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "cityLabel",
                    Name = "City Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "stateLabel",
                    Name = "State Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "postalCodeLabel",
                    Name = "Postal Code Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "countryLabel",
                    Name = "Country Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "saveAddressLabel",
                    Name = "Save Address Checkbox Label",
                    Description = "Label for save address checkbox",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "shippingMethodTitle",
                    Name = "Shipping Method Title",
                    Description = "Heading for shipping method selection",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                },
                new PropertyDefinition
                {
                    Alias = "shippingNote",
                    Name = "Shipping Note",
                    Description = "Info message about delivery times",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 9
                },
                new PropertyDefinition
                {
                    Alias = "continueToPaymentText",
                    Name = "Continue Button Text",
                    Description = "Text for the continue to payment button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 10
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreatePaymentFormGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "paymentForm",
            Name = "Payment Form",
            SortOrder = 5,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "paymentSectionTitle",
                    Name = "Section Title",
                    Description = "Heading for payment method section",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "creditCardEnabled",
                    Name = "Enable Credit Card",
                    Description = "Allow credit/debit card payments",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "creditCardLabel",
                    Name = "Credit Card Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "paypalEnabled",
                    Name = "Enable PayPal",
                    Description = "Allow PayPal payments",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "cardNumberLabel",
                    Name = "Card Number Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "cardNameLabel",
                    Name = "Name on Card Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "expiryLabel",
                    Name = "Expiry Date Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "cvvLabel",
                    Name = "CVV Label",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "billingAddressLabel",
                    Name = "Billing Address Label",
                    Description = "Label for billing same as shipping checkbox",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                },
                new PropertyDefinition
                {
                    Alias = "securityNotice",
                    Name = "Security Notice",
                    Description = "Text about payment security",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 9
                },
                new PropertyDefinition
                {
                    Alias = "continueToReviewText",
                    Name = "Continue Button Text",
                    Description = "Text for the review order button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 10
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateReviewGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "review",
            Name = "Order Review",
            SortOrder = 6,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "reviewTitle",
                    Name = "Review Section Title",
                    Description = "Heading for order review step",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "reviewSubtitle",
                    Name = "Review Section Subtitle",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "termsText",
                    Name = "Terms Agreement Text",
                    Description = "Text for terms checkbox (use {terms} and {privacy} for links)",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "termsUrl",
                    Name = "Terms & Conditions URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "privacyUrl",
                    Name = "Privacy Policy URL",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "placeOrderText",
                    Name = "Place Order Button Text",
                    Description = "Text for the final submit button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "processingText",
                    Name = "Processing Text",
                    Description = "Text shown while processing order",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateTrustBadgesGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "trustBadges",
            Name = "Trust Badges",
            SortOrder = 7,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "trustBadgesEnabled",
                    Name = "Enable Trust Badges",
                    Description = "Show trust badges in order summary",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "sslBadgeText",
                    Name = "SSL Badge Text",
                    Description = "Text for SSL secure badge",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "returnsBadgeText",
                    Name = "Returns Badge Text",
                    Description = "Text for easy returns badge",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "shippingBadgeText",
                    Name = "Shipping Badge Text",
                    Description = "Text for fast shipping badge",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
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
