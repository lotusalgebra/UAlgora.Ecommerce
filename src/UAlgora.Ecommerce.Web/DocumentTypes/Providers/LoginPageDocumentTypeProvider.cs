using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using static UAlgora.Ecommerce.Web.DocumentTypes.Models.DataTypeReference;
using static UAlgora.Ecommerce.Web.DocumentTypes.Providers.AlgoraDocumentTypeConstants;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Provides the Login Page document type definition.
/// A CMS-managed login page with customizable content, hero section, and testimonials.
/// </summary>
public sealed class LoginPageDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public int Priority => 6; // After Home Page

    public DocumentTypeDefinition GetDefinition()
    {
        return new DocumentTypeDefinition
        {
            Alias = LoginPageAlias,
            Name = "Algora Login Page",
            Description = "Customer login page with customizable hero image, testimonial, and social login options.",
            Icon = LoginIcon,
            IconColor = BrandColor,
            AllowedAsRoot = false,
            DefaultTemplate = "algoraLoginPage",
            AllowedChildTypes = [],
            PropertyGroups = GetPropertyGroups()
        };
    }

    private static IReadOnlyList<PropertyGroupDefinition> GetPropertyGroups()
    {
        return
        [
            CreateHeroGroup(),
            CreateFormGroup(),
            CreateTestimonialGroup(),
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
                    Description = "Tailwind color class for gradient start (e.g., primary-500)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "heroGradientTo",
                    Name = "Gradient End Color",
                    Description = "Tailwind color class for gradient end (e.g., accent-500)",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
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
            SortOrder = 1,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "formTitle",
                    Name = "Form Title",
                    Description = "Heading above the login form (e.g., 'Sign In.')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "formSubtitle",
                    Name = "Form Subtitle",
                    Description = "Subheading text (e.g., 'Welcome back! Please enter your details.')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "emailLabel",
                    Name = "Email Field Label",
                    Description = "Label for the email input field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "passwordLabel",
                    Name = "Password Field Label",
                    Description = "Label for the password input field",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "rememberMeLabel",
                    Name = "Remember Me Label",
                    Description = "Label for the remember me checkbox",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "submitButtonText",
                    Name = "Submit Button Text",
                    Description = "Text for the sign in button",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
                },
                new PropertyDefinition
                {
                    Alias = "loadingText",
                    Name = "Loading Text",
                    Description = "Text shown while signing in",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 6
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateTestimonialGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "testimonial",
            Name = "Testimonial",
            SortOrder = 2,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "testimonialEnabled",
                    Name = "Show Testimonial",
                    Description = "Display a customer testimonial on the login page",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "testimonialText",
                    Name = "Testimonial Quote",
                    Description = "The customer testimonial text",
                    DataType = WellKnown(WellKnownDataType.Textarea),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "testimonialAuthor",
                    Name = "Author Name",
                    Description = "Name of the testimonial author",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "testimonialRole",
                    Name = "Author Role",
                    Description = "Role or title (e.g., 'Verified Customer')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "testimonialRating",
                    Name = "Rating (1-5)",
                    Description = "Star rating to display",
                    DataType = WellKnown(WellKnownDataType.Numeric, WellKnown(WellKnownDataType.Textstring)),
                    SortOrder = 4
                }
            ]
        };
    }

    private static PropertyGroupDefinition CreateSocialLoginGroup()
    {
        return new PropertyGroupDefinition
        {
            Alias = "socialLogin",
            Name = "Social Login",
            SortOrder = 3,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "enableGoogleLogin",
                    Name = "Enable Google Login",
                    Description = "Show Google sign-in button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "enableAppleLogin",
                    Name = "Enable Apple Login",
                    Description = "Show Apple sign-in button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "enableFacebookLogin",
                    Name = "Enable Facebook Login",
                    Description = "Show Facebook sign-in button",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "socialDividerText",
                    Name = "Divider Text",
                    Description = "Text between social buttons and form (e.g., 'Or continue with email')",
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
            Name = "Links & Navigation",
            SortOrder = 4,
            Properties =
            [
                new PropertyDefinition
                {
                    Alias = "forgotPasswordText",
                    Name = "Forgot Password Text",
                    Description = "Text for forgot password link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 0
                },
                new PropertyDefinition
                {
                    Alias = "forgotPasswordUrl",
                    Name = "Forgot Password URL",
                    Description = "Link to forgot password page",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 1
                },
                new PropertyDefinition
                {
                    Alias = "registerPrompt",
                    Name = "Register Prompt Text",
                    Description = "Text before register link (e.g., 'Don't have an account?')",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 2
                },
                new PropertyDefinition
                {
                    Alias = "registerLinkText",
                    Name = "Register Link Text",
                    Description = "Text for the register link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 3
                },
                new PropertyDefinition
                {
                    Alias = "guestCheckoutEnabled",
                    Name = "Enable Guest Checkout Link",
                    Description = "Show continue as guest link",
                    DataType = WellKnown(WellKnownDataType.TrueFalse),
                    SortOrder = 4
                },
                new PropertyDefinition
                {
                    Alias = "guestCheckoutText",
                    Name = "Guest Checkout Text",
                    Description = "Text for guest checkout link",
                    DataType = WellKnown(WellKnownDataType.Textstring),
                    SortOrder = 5
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
