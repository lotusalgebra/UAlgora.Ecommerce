using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.LicensePortal.Models;

/// <summary>
/// View model for the checkout page.
/// </summary>
public class CheckoutViewModel
{
    public LicenseType SelectedTier { get; set; }
    public string TierName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public List<string> Features { get; set; } = [];

    // Customer info
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CompanyName { get; set; }
    public string? Domain { get; set; }

    // Payment provider keys
    public string StripePublishableKey { get; set; } = string.Empty;
    public string RazorpayKeyId { get; set; } = string.Empty;

    // For Razorpay
    public string? RazorpayOrderId { get; set; }
}

/// <summary>
/// View model for the success page.
/// </summary>
public class SuccessViewModel
{
    public string LicenseKey { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public List<string> NextSteps { get; set; } = [];
}

/// <summary>
/// View model for the pricing page.
/// </summary>
public class PricingViewModel
{
    public LicenseTierViewModel Trial { get; set; } = new();
    public LicenseTierViewModel Standard { get; set; } = new();
    public LicenseTierViewModel Enterprise { get; set; } = new();
}

/// <summary>
/// View model for a single license tier.
/// </summary>
public class LicenseTierViewModel
{
    public LicenseType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceDisplay { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = "year";
    public List<string> Features { get; set; } = [];
    public bool IsPopular { get; set; }
    public string? CtaText { get; set; }
    public string? CtaUrl { get; set; }
}

/// <summary>
/// Request model for creating a Stripe checkout session.
/// </summary>
public class StripeCheckoutRequest
{
    public LicenseType Tier { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Domain { get; set; }
}

/// <summary>
/// Request model for creating a Razorpay order.
/// </summary>
public class RazorpayOrderRequest
{
    public LicenseType Tier { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Domain { get; set; }
}

/// <summary>
/// Response model for Razorpay order creation.
/// </summary>
public class RazorpayOrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string KeyId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for verifying Razorpay payment.
/// </summary>
public class RazorpayVerifyRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public LicenseType Tier { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Domain { get; set; }
}
