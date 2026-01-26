using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Register Page document type definition.
/// A CMS-managed registration page with customizable benefits, form fields, and social signup options.
/// </summary>
public sealed class RegisterPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 7; // After Login Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = RegisterPageAlias,
            Name = "Algora Register Page",
            Description = "Customer registration page with customizable benefits, form content, and social signup options.",
            Icon = RegisterIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraRegisterPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeroGroup(),
            CreateBenefitsGroup(),
            CreateFormGroup(),
            CreateSocialLoginGroup(),
            CreateLinksGroup(),
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
                    Alias = "heroImage",
                    Name = "Hero Background Image",
                    Description = "Large background image for the left panel (recommended: 1200x1600px)",
                    DataType = WellKnown(WellKnownDataType.MediaPicker, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "heroTitle",
                    Name = "Hero Title",
                    Description = "Main heading text (supports line breaks with <br>)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "heroSubtitle",
                    Name = "Hero Subtitle",
                    Description = "Supporting text below the title",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "heroGradientFrom",
                    Name = "Gradient Start Color",
                    Description = "Tailwind color class for gradient start (e.g., accent-500)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "heroGradientTo",
                    Name = "Gradient End Color",
                    Description = "Tailwind color class for gradient end (e.g., primary-500)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateBenefitsGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "benefits",
            Name = "Membership Benefits",
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "benefitsEnabled",
                    Name = "Show Benefits Section",
                    Description = "Display membership benefits on the registration page",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "benefit1Title",
                    Name = "Benefit 1 Title",
                    Description = "First benefit heading (e.g., '15% Off First Order')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "benefit1Description",
                    Name = "Benefit 1 Description",
                    Description = "First benefit description",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "benefit2Title",
                    Name = "Benefit 2 Title",
                    Description = "Second benefit heading (e.g., 'Free Shipping')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "benefit2Description",
                    Name = "Benefit 2 Description",
                    Description = "Second benefit description",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "benefit3Title",
                    Name = "Benefit 3 Title",
                    Description = "Third benefit heading (e.g., 'Early Access')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "benefit3Description",
                    Name = "Benefit 3 Description",
                    Description = "Third benefit description",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "benefit4Title",
                    Name = "Benefit 4 Title",
                    Description = "Fourth benefit heading (e.g., 'Wishlist & Saves')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "benefit4Description",
                    Name = "Benefit 4 Description",
                    Description = "Fourth benefit description",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateFormGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "form",
            Name = "Form Content",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "formTitle",
                    Name = "Form Title",
                    Description = "Heading above the registration form (e.g., 'Create Account.')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "formSubtitle",
                    Name = "Form Subtitle",
                    Description = "Subheading text (e.g., 'Join us and start your fashion journey today.')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "firstNameLabel",
                    Name = "First Name Label",
                    Description = "Label for the first name field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "lastNameLabel",
                    Name = "Last Name Label",
                    Description = "Label for the last name field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "emailLabel",
                    Name = "Email Field Label",
                    Description = "Label for the email input field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "passwordLabel",
                    Name = "Password Field Label",
                    Description = "Label for the password input field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "confirmPasswordLabel",
                    Name = "Confirm Password Label",
                    Description = "Label for the confirm password field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "submitButtonText",
                    Name = "Submit Button Text",
                    Description = "Text for the create account button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
                },
                new PropertyDefinition
                {
                    Alias = "loadingText",
                    Name = "Loading Text",
                    Description = "Text shown while creating account",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 8
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSocialLoginGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "socialLogin",
            Name = "Social Signup",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "enableGoogleSignup",
                    Name = "Enable Google Signup",
                    Description = "Show Google sign-up button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "enableAppleSignup",
                    Name = "Enable Apple Signup",
                    Description = "Show Apple sign-up button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "enableFacebookSignup",
                    Name = "Enable Facebook Signup",
                    Description = "Show Facebook sign-up button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "socialDividerText",
                    Name = "Divider Text",
                    Description = "Text between social buttons and form (e.g., 'Or register with email')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateLinksGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "links",
            Name = "Links & Legal",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "termsText",
                    Name = "Terms Text",
                    Description = "Terms agreement text (e.g., 'I agree to the')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "termsLinkText",
                    Name = "Terms Link Text",
                    Description = "Text for Terms of Service link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "termsUrl",
                    Name = "Terms URL",
                    Description = "Link to Terms of Service page",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "privacyLinkText",
                    Name = "Privacy Link Text",
                    Description = "Text for Privacy Policy link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "privacyUrl",
                    Name = "Privacy URL",
                    Description = "Link to Privacy Policy page",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "marketingOptInText",
                    Name = "Marketing Opt-in Text",
                    Description = "Text for marketing emails checkbox",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "loginPrompt",
                    Name = "Login Prompt Text",
                    Description = "Text before login link (e.g., 'Already have an account?')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                },
                new PropertyDefinition
                {
                    Alias = "loginLinkText",
                    Name = "Login Link Text",
                    Description = "Text for the login link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 7
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
