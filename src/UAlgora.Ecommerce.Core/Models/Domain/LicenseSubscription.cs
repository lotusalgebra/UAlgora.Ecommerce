namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a recurring subscription for a license.
/// </summary>
public class LicenseSubscription : BaseEntity
{
    /// <summary>
    /// Associated license ID.
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Customer email for the subscription.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Payment provider (e.g., "Stripe", "Razorpay").
    /// </summary>
    public string PaymentProvider { get; set; } = string.Empty;

    /// <summary>
    /// Provider's subscription ID.
    /// </summary>
    public string ProviderSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// Provider's customer ID.
    /// </summary>
    public string? ProviderCustomerId { get; set; }

    /// <summary>
    /// Provider's price/plan ID.
    /// </summary>
    public string? ProviderPriceId { get; set; }

    /// <summary>
    /// Subscription status.
    /// </summary>
    public LicenseSubscriptionStatus Status { get; set; } = LicenseSubscriptionStatus.Active;

    /// <summary>
    /// Subscription amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Billing interval (e.g., "year", "month").
    /// </summary>
    public string BillingInterval { get; set; } = "year";

    /// <summary>
    /// Start of current billing period.
    /// </summary>
    public DateTime CurrentPeriodStart { get; set; }

    /// <summary>
    /// End of current billing period.
    /// </summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>
    /// When the subscription was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// When the subscription ends after cancellation.
    /// </summary>
    public DateTime? CancelAtPeriodEnd { get; set; }

    /// <summary>
    /// Whether auto-renewal is enabled.
    /// </summary>
    public bool AutoRenew { get; set; } = true;

    /// <summary>
    /// Number of successful payments.
    /// </summary>
    public int PaymentCount { get; set; }

    /// <summary>
    /// Last payment date.
    /// </summary>
    public DateTime? LastPaymentDate { get; set; }

    /// <summary>
    /// Next payment date.
    /// </summary>
    public DateTime? NextPaymentDate { get; set; }

    /// <summary>
    /// Licensed domain.
    /// </summary>
    public string? LicensedDomain { get; set; }

    /// <summary>
    /// License type/tier.
    /// </summary>
    public LicenseType LicenseType { get; set; }

    /// <summary>
    /// Failure count for payment retries.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Last failure reason.
    /// </summary>
    public string? LastFailureReason { get; set; }

    /// <summary>
    /// Metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to license.
    /// </summary>
    public License? License { get; set; }

    /// <summary>
    /// Payments for this subscription.
    /// </summary>
    public List<LicensePayment> Payments { get; set; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether the subscription is active.
    /// </summary>
    public bool IsActive => Status == LicenseSubscriptionStatus.Active &&
                           CurrentPeriodEnd >= DateTime.UtcNow;

    /// <summary>
    /// Whether the subscription is cancelled but still active.
    /// </summary>
    public bool IsCancelledButActive => CancelledAt.HasValue &&
                                        CurrentPeriodEnd >= DateTime.UtcNow;

    /// <summary>
    /// Days until renewal.
    /// </summary>
    public int DaysUntilRenewal => Math.Max(0, (CurrentPeriodEnd - DateTime.UtcNow).Days);

    /// <summary>
    /// Whether the subscription is past due.
    /// </summary>
    public bool IsPastDue => Status == LicenseSubscriptionStatus.PastDue;

    #endregion
}

/// <summary>
/// License subscription status.
/// </summary>
public enum LicenseSubscriptionStatus
{
    /// <summary>
    /// Subscription is active and paid.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Payment is past due.
    /// </summary>
    PastDue = 1,

    /// <summary>
    /// Subscription has been cancelled.
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Subscription has expired.
    /// </summary>
    Expired = 3,

    /// <summary>
    /// In trial period.
    /// </summary>
    Trialing = 4,

    /// <summary>
    /// Subscription is paused.
    /// </summary>
    Paused = 5,

    /// <summary>
    /// Incomplete - awaiting payment.
    /// </summary>
    Incomplete = 6,

    /// <summary>
    /// Incomplete and expired.
    /// </summary>
    IncompleteExpired = 7
}
