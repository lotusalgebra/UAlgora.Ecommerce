using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a customer order.
/// </summary>
public class Order : SoftDeleteEntity
{
    /// <summary>
    /// Human-readable order number (e.g., "ORD-2024-0001").
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID (null for guest orders).
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Cart ID this order was created from.
    /// </summary>
    public Guid? CartId { get; set; }

    #region Status

    /// <summary>
    /// Order lifecycle status.
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Payment status.
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Fulfillment status.
    /// </summary>
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.Unfulfilled;

    #endregion

    #region Customer Information

    /// <summary>
    /// Customer email.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number.
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Customer full name.
    /// </summary>
    public string? CustomerName { get; set; }

    #endregion

    #region Addresses

    /// <summary>
    /// Shipping address ID.
    /// </summary>
    public Guid? ShippingAddressId { get; set; }

    /// <summary>
    /// Shipping address.
    /// </summary>
    public Address? ShippingAddress { get; set; }

    /// <summary>
    /// Billing address ID.
    /// </summary>
    public Guid? BillingAddressId { get; set; }

    /// <summary>
    /// Billing address.
    /// </summary>
    public Address? BillingAddress { get; set; }

    /// <summary>
    /// Whether billing address is same as shipping.
    /// </summary>
    public bool BillingSameAsShipping { get; set; } = true;

    #endregion

    #region Line Items

    /// <summary>
    /// Order line items.
    /// </summary>
    public List<OrderLine> Lines { get; set; } = [];

    #endregion

    #region Pricing

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Subtotal before discounts and shipping.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Total discount amount.
    /// </summary>
    public decimal DiscountTotal { get; set; }

    /// <summary>
    /// Shipping cost.
    /// </summary>
    public decimal ShippingTotal { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal TaxTotal { get; set; }

    /// <summary>
    /// Grand total.
    /// </summary>
    public decimal GrandTotal { get; set; }

    /// <summary>
    /// Amount paid so far.
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Amount refunded.
    /// </summary>
    public decimal RefundedAmount { get; set; }

    #endregion

    #region Discounts

    /// <summary>
    /// Applied discounts.
    /// </summary>
    public List<AppliedDiscount> AppliedDiscounts { get; set; } = [];

    /// <summary>
    /// Applied coupon code.
    /// </summary>
    public string? CouponCode { get; set; }

    #endregion

    #region Payment

    /// <summary>
    /// Payment method used.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Payment provider (e.g., "stripe", "paypal").
    /// </summary>
    public string? PaymentProvider { get; set; }

    /// <summary>
    /// Payment intent/transaction ID from provider.
    /// </summary>
    public string? PaymentIntentId { get; set; }

    /// <summary>
    /// Payment records.
    /// </summary>
    public List<Payment> Payments { get; set; } = [];

    #endregion

    #region Shipping

    /// <summary>
    /// Shipping method ID.
    /// </summary>
    public string? ShippingMethod { get; set; }

    /// <summary>
    /// Shipping method name.
    /// </summary>
    public string? ShippingMethodName { get; set; }

    /// <summary>
    /// Primary tracking number.
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Carrier name.
    /// </summary>
    public string? Carrier { get; set; }

    /// <summary>
    /// Shipment records.
    /// </summary>
    public List<Shipment> Shipments { get; set; } = [];

    /// <summary>
    /// Estimated delivery date.
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    #endregion

    #region Notes

    /// <summary>
    /// Customer-provided notes.
    /// </summary>
    public string? CustomerNote { get; set; }

    /// <summary>
    /// Internal staff notes.
    /// </summary>
    public string? InternalNote { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// IP address of the order placement.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the order placement.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Source/channel of the order.
    /// </summary>
    public string? Source { get; set; }

    #endregion

    #region Timestamps

    /// <summary>
    /// When the order was placed.
    /// </summary>
    public DateTime? PlacedAt { get; set; }

    /// <summary>
    /// When the order was confirmed.
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// When the order was paid.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// When the order was shipped.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// When the order was delivered.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// When the order was completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// When the order was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Reason for cancellation.
    /// </summary>
    public string? CancellationReason { get; set; }

    #endregion

    /// <summary>
    /// Navigation property to customer.
    /// </summary>
    public Customer? Customer { get; set; }

    #region Computed Properties

    /// <summary>
    /// Total number of items ordered.
    /// </summary>
    public int TotalItems => Lines.Sum(l => l.Quantity);

    /// <summary>
    /// Number of items fulfilled.
    /// </summary>
    public int FulfilledItems => Lines.Sum(l => l.FulfilledQuantity);

    /// <summary>
    /// Outstanding balance.
    /// </summary>
    public decimal BalanceDue => GrandTotal - PaidAmount;

    /// <summary>
    /// Whether the order is fully paid.
    /// </summary>
    public bool IsFullyPaid => PaidAmount >= GrandTotal;

    /// <summary>
    /// Whether the order is fully fulfilled.
    /// </summary>
    public bool IsFullyFulfilled => FulfillmentStatus == FulfillmentStatus.Fulfilled;

    /// <summary>
    /// Whether the order can be cancelled.
    /// </summary>
    public bool CanCancel => Status is OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Processing
                             && FulfillmentStatus == FulfillmentStatus.Unfulfilled;

    /// <summary>
    /// Whether the order can be refunded.
    /// </summary>
    public bool CanRefund => PaidAmount > RefundedAmount
                             && PaymentStatus is PaymentStatus.Captured or PaymentStatus.PartiallyRefunded;

    #endregion
}

/// <summary>
/// Represents a line item in an order.
/// </summary>
public class OrderLine : BaseEntity
{
    /// <summary>
    /// Parent order ID.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant ID if applicable.
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Product name (snapshot at time of order).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product/variant SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Variant name if applicable.
    /// </summary>
    public string? VariantName { get; set; }

    /// <summary>
    /// Variant options (e.g., {"Size": "Large", "Color": "Blue"}).
    /// </summary>
    public Dictionary<string, string>? VariantOptions { get; set; }

    /// <summary>
    /// Product/variant image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Quantity fulfilled/shipped.
    /// </summary>
    public int FulfilledQuantity { get; set; }

    /// <summary>
    /// Quantity returned.
    /// </summary>
    public int ReturnedQuantity { get; set; }

    /// <summary>
    /// Unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Original price before discounts.
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Line total (quantity * unit price).
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Discount amount for this line.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Tax amount for this line.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Weight per unit.
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Navigation property to order.
    /// </summary>
    public Order? Order { get; set; }

    #region Computed Properties

    /// <summary>
    /// Remaining quantity to fulfill.
    /// </summary>
    public int RemainingQuantity => Quantity - FulfilledQuantity;

    /// <summary>
    /// Whether this line is fully fulfilled.
    /// </summary>
    public bool IsFullyFulfilled => FulfilledQuantity >= Quantity;

    /// <summary>
    /// Final line total after discounts.
    /// </summary>
    public decimal FinalLineTotal => LineTotal - DiscountAmount;

    /// <summary>
    /// Total weight for this line.
    /// </summary>
    public decimal? TotalWeight => Weight.HasValue ? Weight.Value * Quantity : null;

    #endregion
}
