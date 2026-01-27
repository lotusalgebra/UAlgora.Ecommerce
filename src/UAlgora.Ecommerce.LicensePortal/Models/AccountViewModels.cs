using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.LicensePortal.Models;

/// <summary>
/// View model for the account dashboard.
/// </summary>
public class AccountDashboardViewModel
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public List<LicenseViewModel> Licenses { get; set; } = [];
    public List<SubscriptionViewModel> Subscriptions { get; set; } = [];
    public List<PaymentViewModel> RecentPayments { get; set; } = [];
    public AccountStats Stats { get; set; } = new();
}

/// <summary>
/// Account statistics.
/// </summary>
public class AccountStats
{
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal TotalSpent { get; set; }
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// View model for displaying a license.
/// </summary>
public class LicenseViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string MaskedKey { get; set; } = string.Empty;
    public LicenseType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public LicenseStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? LicensedDomains { get; set; }
    public string? Company { get; set; }
    public bool IsExpired { get; set; }
    public bool IsInGracePeriod { get; set; }
    public int? DaysUntilExpiration { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> EnabledFeatures { get; set; } = [];
}

/// <summary>
/// View model for displaying a subscription.
/// </summary>
public class SubscriptionViewModel
{
    public Guid Id { get; set; }
    public Guid LicenseId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public string LicenseTypeName { get; set; } = string.Empty;
    public string PaymentProvider { get; set; } = string.Empty;
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public LicenseSubscriptionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string FormattedAmount { get; set; } = string.Empty;
    public string BillingInterval { get; set; } = "year";
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public bool AutoRenew { get; set; }
    public bool CanCancel { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int DaysUntilRenewal { get; set; }
    public int PaymentCount { get; set; }
}

/// <summary>
/// View model for displaying a payment/invoice.
/// </summary>
public class PaymentViewModel
{
    public Guid Id { get; set; }
    public string PaymentProvider { get; set; } = string.Empty;
    public string ProviderPaymentId { get; set; } = string.Empty;
    public LicensePaymentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string FormattedAmount { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public string LicenseTypeName { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}

/// <summary>
/// View model for the licenses page.
/// </summary>
public class LicensesPageViewModel
{
    public string CustomerEmail { get; set; } = string.Empty;
    public List<LicenseViewModel> Licenses { get; set; } = [];
    public int TotalCount { get; set; }
}

/// <summary>
/// View model for the subscriptions page.
/// </summary>
public class SubscriptionsPageViewModel
{
    public string CustomerEmail { get; set; } = string.Empty;
    public List<SubscriptionViewModel> Subscriptions { get; set; } = [];
    public int TotalCount { get; set; }
}

/// <summary>
/// View model for the invoices page.
/// </summary>
public class InvoicesPageViewModel
{
    public string CustomerEmail { get; set; } = string.Empty;
    public List<PaymentViewModel> Payments { get; set; } = [];
    public int TotalCount { get; set; }
    public decimal TotalSpent { get; set; }
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Request model for canceling a subscription.
/// </summary>
public class CancelSubscriptionRequest
{
    public Guid SubscriptionId { get; set; }
    public bool CancelAtPeriodEnd { get; set; } = true;
    public string? Reason { get; set; }
}

/// <summary>
/// Login request model.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Login verification model (for magic link/OTP).
/// </summary>
public class LoginVerifyRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
