namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a stock transfer between warehouses.
/// </summary>
public class StockTransfer : BaseEntity
{
    /// <summary>
    /// Transfer reference number.
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Source warehouse ID.
    /// </summary>
    public Guid SourceWarehouseId { get; set; }

    /// <summary>
    /// Destination warehouse ID.
    /// </summary>
    public Guid DestinationWarehouseId { get; set; }

    /// <summary>
    /// Transfer status.
    /// </summary>
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;

    /// <summary>
    /// Transfer notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User who created the transfer.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who approved the transfer.
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Date when approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Date when shipped from source.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Date when received at destination.
    /// </summary>
    public DateTime? ReceivedAt { get; set; }

    /// <summary>
    /// Expected arrival date.
    /// </summary>
    public DateTime? ExpectedArrivalDate { get; set; }

    /// <summary>
    /// Tracking number for shipment.
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Carrier/shipping method.
    /// </summary>
    public string? Carrier { get; set; }

    /// <summary>
    /// Transfer line items.
    /// </summary>
    public List<StockTransferItem> Items { get; set; } = [];

    /// <summary>
    /// Source warehouse navigation property.
    /// </summary>
    public Warehouse? SourceWarehouse { get; set; }

    /// <summary>
    /// Destination warehouse navigation property.
    /// </summary>
    public Warehouse? DestinationWarehouse { get; set; }

    /// <summary>
    /// Total quantity being transferred.
    /// </summary>
    public int TotalQuantity => Items.Sum(i => i.QuantityRequested);

    /// <summary>
    /// Total quantity received.
    /// </summary>
    public int TotalReceived => Items.Sum(i => i.QuantityReceived);

    /// <summary>
    /// Whether transfer is complete (all items received).
    /// </summary>
    public bool IsComplete => Status == StockTransferStatus.Completed &&
        Items.All(i => i.QuantityReceived == i.QuantityRequested);
}

/// <summary>
/// Stock transfer line item.
/// </summary>
public class StockTransferItem : BaseEntity
{
    /// <summary>
    /// Parent transfer ID.
    /// </summary>
    public Guid StockTransferId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant ID (null for main product).
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity requested to transfer.
    /// </summary>
    public int QuantityRequested { get; set; }

    /// <summary>
    /// Quantity shipped from source.
    /// </summary>
    public int QuantityShipped { get; set; }

    /// <summary>
    /// Quantity received at destination.
    /// </summary>
    public int QuantityReceived { get; set; }

    /// <summary>
    /// Quantity damaged during transfer.
    /// </summary>
    public int QuantityDamaged { get; set; }

    /// <summary>
    /// Source bin location.
    /// </summary>
    public string? SourceBinLocation { get; set; }

    /// <summary>
    /// Destination bin location.
    /// </summary>
    public string? DestinationBinLocation { get; set; }

    /// <summary>
    /// Item notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public StockTransfer? StockTransfer { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Quantity in transit (shipped but not received).
    /// </summary>
    public int QuantityInTransit => QuantityShipped - QuantityReceived - QuantityDamaged;

    /// <summary>
    /// Quantity variance (received vs requested).
    /// </summary>
    public int Variance => QuantityReceived - QuantityRequested;
}

/// <summary>
/// Stock transfer status.
/// </summary>
public enum StockTransferStatus
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
    /// Approved - ready to ship.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// In transit.
    /// </summary>
    InTransit = 3,

    /// <summary>
    /// Partially received.
    /// </summary>
    PartiallyReceived = 4,

    /// <summary>
    /// Completed - fully received.
    /// </summary>
    Completed = 5,

    /// <summary>
    /// Cancelled.
    /// </summary>
    Cancelled = 6
}
