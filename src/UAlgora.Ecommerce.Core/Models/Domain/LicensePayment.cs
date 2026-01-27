namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a payment transaction for a license subscription.
/// </summary>
public class LicensePayment : BaseEntity
{
    /// <summary>
    /// Associated subscription ID.
    /// </summary>
    public Guid? SubscriptionId { get; set; }

    /// <summary>
    /// Associated license ID (for one-time purchases).
    /// </summary>
    public Guid? LicenseId { get; set; }

    /// <summary>
    /// Payment provider (e.g., "Stripe", "Razorpay").
    /// </summary>
    public string PaymentProvider { get; set; } = string.Empty;

    /// <summary>
    /// Provider's payment/transaction ID.
    /// </summary>
    public string ProviderPaymentId { get; set; } = string.Empty;

    /// <summary>
    /// Provider's customer ID.
    /// </summary>
    public string? ProviderCustomerId { get; set; }

    /// <summary>
    /// Provider's invoice ID.
    /// </summary>
    public string? ProviderInvoiceId { get; set; }

    /// <summary>
    /// Provider's charge ID.
    /// </summary>
    public string? ProviderChargeId { get; set; }

    /// <summary>
    /// Payment status.
    /// </summary>
    public LicensePaymentStatus Status { get; set; } = LicensePaymentStatus.Pending;

    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Customer email.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer name.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Receipt URL from provider.
    /// </summary>
    public string? ReceiptUrl { get; set; }

    /// <summary>
    /// Invoice URL from provider.
    /// </summary>
    public string? InvoiceUrl { get; set; }

    /// <summary>
    /// Failure reason if payment failed.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Failure code from provider.
    /// </summary>
    public string? FailureCode { get; set; }

    /// <summary>
    /// When the payment was completed.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// When the payment was refunded.
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Refund amount (partial refunds supported).
    /// </summary>
    public decimal? RefundedAmount { get; set; }

    /// <summary>
    /// Refund reason.
    /// </summary>
    public string? RefundReason { get; set; }

    /// <summary>
    /// Payment type (e.g., "subscription", "one_time").
    /// </summary>
    public string PaymentType { get; set; } = "subscription";

    /// <summary>
    /// Billing period start for this payment.
    /// </summary>
    public DateTime? PeriodStart { get; set; }

    /// <summary>
    /// Billing period end for this payment.
    /// </summary>
    public DateTime? PeriodEnd { get; set; }

    /// <summary>
    /// License type this payment is for.
    /// </summary>
    public LicenseType LicenseType { get; set; }

    #region Card Details

    /// <summary>
    /// Card brand (Visa, Mastercard, etc.).
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Card last 4 digits.
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Card country.
    /// </summary>
    public string? CardCountry { get; set; }

    #endregion

    /// <summary>
    /// Raw response from provider (JSON).
    /// </summary>
    public string? RawResponseJson { get; set; }

    /// <summary>
    /// Metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to subscription.
    /// </summary>
    public LicenseSubscription? Subscription { get; set; }

    /// <summary>
    /// Navigation property to license.
    /// </summary>
    public License? License { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether the payment was successful.
    /// </summary>
    public bool IsSuccessful => Status == LicensePaymentStatus.Succeeded;

    /// <summary>
    /// Whether the payment can be refunded.
    /// </summary>
    public bool CanRefund => Status == LicensePaymentStatus.Succeeded &&
                             RefundedAt == null &&
                             PaidAt.HasValue &&
                             PaidAt.Value.AddDays(180) >= DateTime.UtcNow; // 180 day refund window

    /// <summary>
    /// Net amount after refund.
    /// </summary>
    public decimal NetAmount => Amount - (RefundedAmount ?? 0);

    #endregion
}

/// <summary>
/// License payment status.
/// </summary>
public enum LicensePaymentStatus
{
    /// <summary>
    /// Payment is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment succeeded.
    /// </summary>
    Succeeded = 1,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Payment was refunded.
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Payment was partially refunded.
    /// </summary>
    PartiallyRefunded = 4,

    /// <summary>
    /// Payment requires action (3D Secure, etc.).
    /// </summary>
    RequiresAction = 5,

    /// <summary>
    /// Payment was cancelled.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Processing by payment provider.
    /// </summary>
    Processing = 7
}
