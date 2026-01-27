namespace UAlgora.Ecommerce.LicensePortal.Models;

/// <summary>
/// Configuration options for the License Portal.
/// </summary>
public class LicensePortalOptions
{
    public const string SectionName = "LicensePortal";

    public PricingOptions Pricing { get; set; } = new();
    public StripeOptions Stripe { get; set; } = new();
    public RazorpayOptions Razorpay { get; set; } = new();
    public EmailOptions Email { get; set; } = new();
    public string BaseUrl { get; set; } = "https://licenses.algoracommerce.com";
}

/// <summary>
/// Pricing configuration for license tiers.
/// </summary>
public class PricingOptions
{
    public LicenseTierPricing Standard { get; set; } = new()
    {
        AnnualPrice = 1500m,
        Currency = "USD",
        Features = ["1 Store", "Unlimited Products", "Unlimited Orders", "Advanced Reporting", "API Access", "Email Templates", "Returns Management"]
    };

    public LicenseTierPricing Enterprise { get; set; } = new()
    {
        AnnualPrice = 3000m,
        Currency = "USD",
        Features = ["Unlimited Stores", "All Standard Features", "Multi-currency", "B2B Features", "White Labeling", "Priority Support", "Custom Integrations"]
    };
}

/// <summary>
/// Pricing for a specific license tier.
/// </summary>
public class LicenseTierPricing
{
    public decimal AnnualPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? StripePriceId { get; set; }
    public string? RazorpayPlanId { get; set; }
    public List<string> Features { get; set; } = [];
}

/// <summary>
/// Stripe configuration.
/// </summary>
public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

/// <summary>
/// Razorpay configuration.
/// </summary>
public class RazorpayOptions
{
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

/// <summary>
/// Email configuration (SendGrid).
/// </summary>
public class EmailOptions
{
    public string SendGridApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@algoracommerce.com";
    public string FromName { get; set; } = "Algora Commerce";
}
