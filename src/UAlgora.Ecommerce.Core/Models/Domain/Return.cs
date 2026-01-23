namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a product return/refund request.
/// </summary>
public class Return : SoftDeleteEntity
{
    /// <summary>
    /// Store this return belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Order ID this return is for.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Customer ID requesting the return.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Unique return number (RMA number).
    /// </summary>
    public string ReturnNumber { get; set; } = string.Empty;

    /// <summary>
    /// Return status.
    /// </summary>
    public ReturnStatus Status { get; set; } = ReturnStatus.Requested;

    /// <summary>
    /// Return type.
    /// </summary>
    public ReturnType Type { get; set; } = ReturnType.Refund;

    #region Reason & Notes

    /// <summary>
    /// Reason category for the return.
    /// </summary>
    public ReturnReason Reason { get; set; }

    /// <summary>
    /// Detailed reason description from customer.
    /// </summary>
    public string? ReasonDetails { get; set; }

    /// <summary>
    /// Customer's additional comments.
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Admin/internal notes.
    /// </summary>
    public string? AdminNotes { get; set; }

    #endregion

    #region Financial

    /// <summary>
    /// Total refund amount requested.
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Actual refund amount approved.
    /// </summary>
    public decimal? ApprovedRefundAmount { get; set; }

    /// <summary>
    /// Restocking fee charged.
    /// </summary>
    public decimal RestockingFee { get; set; }

    /// <summary>
    /// Return shipping cost (who pays).
    /// </summary>
    public decimal ReturnShippingCost { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Refund method.
    /// </summary>
    public RefundMethod RefundMethod { get; set; } = RefundMethod.OriginalPayment;

    /// <summary>
    /// Store credit amount (if RefundMethod is StoreCredit).
    /// </summary>
    public decimal? StoreCreditAmount { get; set; }

    /// <summary>
    /// Gift card ID if refund was to gift card.
    /// </summary>
    public Guid? RefundGiftCardId { get; set; }

    #endregion

    #region Timestamps

    /// <summary>
    /// When the return was requested.
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// When the return was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// When the return was rejected.
    /// </summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>
    /// When items were received back.
    /// </summary>
    public DateTime? ReceivedAt { get; set; }

    /// <summary>
    /// When the refund was processed.
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// When the return was completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    #endregion

    #region Processing

    /// <summary>
    /// Admin user who approved/rejected the return.
    /// </summary>
    public string? ProcessedBy { get; set; }

    /// <summary>
    /// Rejection reason (if rejected).
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Warehouse ID items should be returned to.
    /// </summary>
    public Guid? ReturnWarehouseId { get; set; }

    #endregion

    #region Shipping

    /// <summary>
    /// Return shipping label URL.
    /// </summary>
    public string? ReturnLabelUrl { get; set; }

    /// <summary>
    /// Return tracking number.
    /// </summary>
    public string? ReturnTrackingNumber { get; set; }

    /// <summary>
    /// Return shipping carrier.
    /// </summary>
    public string? ReturnCarrier { get; set; }

    /// <summary>
    /// Whether return shipping is prepaid.
    /// </summary>
    public bool IsReturnShippingPrepaid { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Custom metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Store navigation property.
    /// </summary>
    public Store? Store { get; set; }

    /// <summary>
    /// Order navigation property.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// Customer navigation property.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Return line items.
    /// </summary>
    public List<ReturnItem> Items { get; set; } = [];

    /// <summary>
    /// Refund gift card navigation property.
    /// </summary>
    public GiftCard? RefundGiftCard { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Total quantity of items being returned.
    /// </summary>
    public int TotalItemsQuantity => Items.Sum(i => i.Quantity);

    /// <summary>
    /// Whether the return can be approved.
    /// </summary>
    public bool CanApprove => Status == ReturnStatus.Requested || Status == ReturnStatus.UnderReview;

    /// <summary>
    /// Whether the return can be rejected.
    /// </summary>
    public bool CanReject => Status == ReturnStatus.Requested || Status == ReturnStatus.UnderReview;

    /// <summary>
    /// Whether items have been received.
    /// </summary>
    public bool ItemsReceived => ReceivedAt.HasValue;

    /// <summary>
    /// Whether the refund has been processed.
    /// </summary>
    public bool IsRefunded => RefundedAt.HasValue;

    /// <summary>
    /// Net refund amount after fees.
    /// </summary>
    public decimal NetRefundAmount => (ApprovedRefundAmount ?? RefundAmount) - RestockingFee - ReturnShippingCost;

    #endregion
}

/// <summary>
/// Individual item in a return.
/// </summary>
public class ReturnItem : BaseEntity
{
    /// <summary>
    /// Return ID this item belongs to.
    /// </summary>
    public Guid ReturnId { get; set; }

    /// <summary>
    /// Order line ID being returned.
    /// </summary>
    public Guid OrderLineId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product variant ID (if applicable).
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Quantity being returned.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Refund amount for this item.
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Reason for returning this specific item.
    /// </summary>
    public ReturnReason Reason { get; set; }

    /// <summary>
    /// Detailed reason for this item.
    /// </summary>
    public string? ReasonDetails { get; set; }

    /// <summary>
    /// Condition of the returned item.
    /// </summary>
    public ReturnItemCondition Condition { get; set; } = ReturnItemCondition.Unknown;

    /// <summary>
    /// Condition notes after inspection.
    /// </summary>
    public string? ConditionNotes { get; set; }

    /// <summary>
    /// Whether this item should be restocked.
    /// </summary>
    public bool ShouldRestock { get; set; } = true;

    /// <summary>
    /// Quantity actually restocked.
    /// </summary>
    public int RestockedQuantity { get; set; }

    /// <summary>
    /// Whether the item has been inspected.
    /// </summary>
    public bool IsInspected { get; set; }

    /// <summary>
    /// When the item was inspected.
    /// </summary>
    public DateTime? InspectedAt { get; set; }

    /// <summary>
    /// Who inspected the item.
    /// </summary>
    public string? InspectedBy { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Return navigation property.
    /// </summary>
    public Return? Return { get; set; }

    /// <summary>
    /// Order line navigation property.
    /// </summary>
    public OrderLine? OrderLine { get; set; }

    /// <summary>
    /// Product navigation property.
    /// </summary>
    public Product? Product { get; set; }

    #endregion
}

/// <summary>
/// Return request status.
/// </summary>
public enum ReturnStatus
{
    /// <summary>
    /// Return has been requested by customer.
    /// </summary>
    Requested = 0,

    /// <summary>
    /// Return is under review by admin.
    /// </summary>
    UnderReview = 1,

    /// <summary>
    /// Return has been approved.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Return has been rejected.
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Waiting for items to be returned.
    /// </summary>
    AwaitingReturn = 4,

    /// <summary>
    /// Items have been received.
    /// </summary>
    ItemsReceived = 5,

    /// <summary>
    /// Items are being inspected.
    /// </summary>
    InspectionInProgress = 6,

    /// <summary>
    /// Refund is being processed.
    /// </summary>
    RefundProcessing = 7,

    /// <summary>
    /// Refund has been issued.
    /// </summary>
    Refunded = 8,

    /// <summary>
    /// Return is complete.
    /// </summary>
    Completed = 9,

    /// <summary>
    /// Return was cancelled.
    /// </summary>
    Cancelled = 10
}

/// <summary>
/// Type of return.
/// </summary>
public enum ReturnType
{
    /// <summary>
    /// Full refund to original payment method.
    /// </summary>
    Refund = 0,

    /// <summary>
    /// Exchange for different product.
    /// </summary>
    Exchange = 1,

    /// <summary>
    /// Store credit/gift card.
    /// </summary>
    StoreCredit = 2,

    /// <summary>
    /// Replacement of same product.
    /// </summary>
    Replacement = 3
}

/// <summary>
/// Return reason category.
/// </summary>
public enum ReturnReason
{
    /// <summary>
    /// Item doesn't fit.
    /// </summary>
    DoesNotFit = 0,

    /// <summary>
    /// Item defective/damaged on arrival.
    /// </summary>
    Defective = 1,

    /// <summary>
    /// Wrong item received.
    /// </summary>
    WrongItem = 2,

    /// <summary>
    /// Item not as described.
    /// </summary>
    NotAsDescribed = 3,

    /// <summary>
    /// Changed mind.
    /// </summary>
    ChangedMind = 4,

    /// <summary>
    /// Item arrived too late.
    /// </summary>
    ArrivedTooLate = 5,

    /// <summary>
    /// Better price found elsewhere.
    /// </summary>
    BetterPriceFound = 6,

    /// <summary>
    /// Quality not as expected.
    /// </summary>
    QualityIssue = 7,

    /// <summary>
    /// Ordered by mistake.
    /// </summary>
    OrderedByMistake = 8,

    /// <summary>
    /// Item damaged during shipping.
    /// </summary>
    DamagedInShipping = 9,

    /// <summary>
    /// Other reason.
    /// </summary>
    Other = 99
}

/// <summary>
/// Refund method.
/// </summary>
public enum RefundMethod
{
    /// <summary>
    /// Refund to original payment method.
    /// </summary>
    OriginalPayment = 0,

    /// <summary>
    /// Refund as store credit/gift card.
    /// </summary>
    StoreCredit = 1,

    /// <summary>
    /// Bank transfer.
    /// </summary>
    BankTransfer = 2,

    /// <summary>
    /// Check/cheque.
    /// </summary>
    Check = 3,

    /// <summary>
    /// Exchange for different product.
    /// </summary>
    Exchange = 4
}

/// <summary>
/// Condition of returned item.
/// </summary>
public enum ReturnItemCondition
{
    /// <summary>
    /// Condition not yet assessed.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Item is unopened/sealed.
    /// </summary>
    Unopened = 1,

    /// <summary>
    /// Item like new, can be resold.
    /// </summary>
    LikeNew = 2,

    /// <summary>
    /// Item used but good condition.
    /// </summary>
    Used = 3,

    /// <summary>
    /// Item is damaged.
    /// </summary>
    Damaged = 4,

    /// <summary>
    /// Item is defective.
    /// </summary>
    Defective = 5,

    /// <summary>
    /// Item cannot be resold.
    /// </summary>
    Unsellable = 6
}
