using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a payment transaction.
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// Order ID this payment is for.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Payment status.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Payment method type.
    /// </summary>
    public PaymentMethodType MethodType { get; set; }

    /// <summary>
    /// Payment provider (e.g., "stripe", "paypal").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Payment method display name (e.g., "Visa ending in 4242").
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Provider transaction ID.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Payment intent ID (for Stripe, etc.).
    /// </summary>
    public string? PaymentIntentId { get; set; }

    /// <summary>
    /// Charge ID from provider.
    /// </summary>
    public string? ChargeId { get; set; }

    /// <summary>
    /// Refund ID if this is a refund.
    /// </summary>
    public string? RefundId { get; set; }

    /// <summary>
    /// Whether this is a refund transaction.
    /// </summary>
    public bool IsRefund { get; set; }

    /// <summary>
    /// Parent payment ID (for refunds).
    /// </summary>
    public Guid? ParentPaymentId { get; set; }

    #region Card Details (if applicable)

    /// <summary>
    /// Card brand (Visa, Mastercard, etc.).
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Card last 4 digits.
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Card expiry month.
    /// </summary>
    public int? CardExpiryMonth { get; set; }

    /// <summary>
    /// Card expiry year.
    /// </summary>
    public int? CardExpiryYear { get; set; }

    #endregion

    #region Risk/Fraud

    /// <summary>
    /// Risk level from provider.
    /// </summary>
    public string? RiskLevel { get; set; }

    /// <summary>
    /// Risk score from provider.
    /// </summary>
    public int? RiskScore { get; set; }

    /// <summary>
    /// AVS check result.
    /// </summary>
    public string? AvsResult { get; set; }

    /// <summary>
    /// CVV check result.
    /// </summary>
    public string? CvvResult { get; set; }

    #endregion

    /// <summary>
    /// Error message if payment failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code from provider.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Raw response from provider (JSON).
    /// </summary>
    public string? RawResponse { get; set; }

    /// <summary>
    /// When the payment was captured.
    /// </summary>
    public DateTime? CapturedAt { get; set; }

    /// <summary>
    /// When the payment was refunded.
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Navigation property to order.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// Navigation property to parent payment (for refunds).
    /// </summary>
    public Payment? ParentPayment { get; set; }

    /// <summary>
    /// Child refund payments.
    /// </summary>
    public List<Payment> Refunds { get; set; } = [];

    #region Computed Properties

    /// <summary>
    /// Whether the payment was successful.
    /// </summary>
    public bool IsSuccessful => Status == PaymentStatus.Captured;

    /// <summary>
    /// Whether the payment can be refunded.
    /// </summary>
    public bool CanRefund => Status == PaymentStatus.Captured && !IsRefund;

    /// <summary>
    /// Total refunded amount.
    /// </summary>
    public decimal TotalRefunded => Refunds.Where(r => r.Status == PaymentStatus.Refunded).Sum(r => r.Amount);

    /// <summary>
    /// Remaining refundable amount.
    /// </summary>
    public decimal RefundableAmount => Amount - TotalRefunded;

    #endregion
}

/// <summary>
/// Represents a stored payment method for a customer.
/// </summary>
public class StoredPaymentMethod : BaseEntity
{
    /// <summary>
    /// Customer ID.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Payment method type.
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Payment provider.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Provider payment method ID (e.g., Stripe payment method ID).
    /// </summary>
    public string ProviderMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Display name (e.g., "Visa ending in 4242").
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Card brand (if card).
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Card last 4 digits (if card).
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Card expiry month (if card).
    /// </summary>
    public int? CardExpiryMonth { get; set; }

    /// <summary>
    /// Card expiry year (if card).
    /// </summary>
    public int? CardExpiryYear { get; set; }

    /// <summary>
    /// Whether this is the default payment method.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Billing address associated with this payment method.
    /// </summary>
    public Guid? BillingAddressId { get; set; }

    /// <summary>
    /// Navigation property to customer.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Navigation property to billing address.
    /// </summary>
    public Address? BillingAddress { get; set; }

    #region Computed Properties

    /// <summary>
    /// Whether the card is expired.
    /// </summary>
    public bool IsExpired
    {
        get
        {
            if (!CardExpiryMonth.HasValue || !CardExpiryYear.HasValue)
                return false;

            var now = DateTime.UtcNow;
            return CardExpiryYear.Value < now.Year ||
                   (CardExpiryYear.Value == now.Year && CardExpiryMonth.Value < now.Month);
        }
    }

    #endregion
}
