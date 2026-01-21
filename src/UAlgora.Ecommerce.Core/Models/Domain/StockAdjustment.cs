namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a stock adjustment record.
/// </summary>
public class StockAdjustment : BaseEntity
{
    /// <summary>
    /// Adjustment reference number.
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse ID.
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Adjustment type/reason.
    /// </summary>
    public StockAdjustmentType Type { get; set; }

    /// <summary>
    /// Adjustment status.
    /// </summary>
    public StockAdjustmentStatus Status { get; set; } = StockAdjustmentStatus.Draft;

    /// <summary>
    /// Detailed reason/notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Reference to external document (e.g., return order).
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// User who created the adjustment.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who approved the adjustment.
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Date when approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Date when completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Adjustment line items.
    /// </summary>
    public List<StockAdjustmentItem> Items { get; set; } = [];

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Warehouse? Warehouse { get; set; }

    /// <summary>
    /// Total quantity adjusted (sum of all items).
    /// </summary>
    public int TotalQuantityAdjusted => Items.Sum(i => i.QuantityAdjusted);

    /// <summary>
    /// Total value impact.
    /// </summary>
    public decimal TotalValueImpact => Items.Sum(i => i.ValueImpact);
}

/// <summary>
/// Stock adjustment line item.
/// </summary>
public class StockAdjustmentItem : BaseEntity
{
    /// <summary>
    /// Parent adjustment ID.
    /// </summary>
    public Guid StockAdjustmentId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant ID (null for main product).
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Product SKU at time of adjustment.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product name at time of adjustment.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity before adjustment.
    /// </summary>
    public int QuantityBefore { get; set; }

    /// <summary>
    /// Quantity after adjustment.
    /// </summary>
    public int QuantityAfter { get; set; }

    /// <summary>
    /// Adjustment amount (positive or negative).
    /// </summary>
    public int QuantityAdjusted { get; set; }

    /// <summary>
    /// Unit cost at time of adjustment.
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Item-specific reason/notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Bin/shelf location.
    /// </summary>
    public string? BinLocation { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public StockAdjustment? StockAdjustment { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Value impact of this adjustment.
    /// </summary>
    public decimal ValueImpact => (UnitCost ?? 0) * QuantityAdjusted;
}

/// <summary>
/// Stock adjustment type.
/// </summary>
public enum StockAdjustmentType
{
    /// <summary>
    /// Inventory count correction.
    /// </summary>
    InventoryCount = 0,

    /// <summary>
    /// Damaged goods.
    /// </summary>
    Damage = 1,

    /// <summary>
    /// Theft or loss.
    /// </summary>
    Theft = 2,

    /// <summary>
    /// Expired products.
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Return to vendor.
    /// </summary>
    ReturnToVendor = 4,

    /// <summary>
    /// Customer return.
    /// </summary>
    CustomerReturn = 5,

    /// <summary>
    /// Internal use/samples.
    /// </summary>
    InternalUse = 6,

    /// <summary>
    /// Found stock.
    /// </summary>
    Found = 7,

    /// <summary>
    /// Initial stock entry.
    /// </summary>
    InitialStock = 8,

    /// <summary>
    /// Manual correction.
    /// </summary>
    ManualCorrection = 9,

    /// <summary>
    /// Other adjustment.
    /// </summary>
    Other = 99
}

/// <summary>
/// Stock adjustment status.
/// </summary>
public enum StockAdjustmentStatus
{
    /// <summary>
    /// Draft - not yet submitted.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Pending approval.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Approved - ready to apply.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Completed - applied to inventory.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Rejected.
    /// </summary>
    Rejected = 5
}
