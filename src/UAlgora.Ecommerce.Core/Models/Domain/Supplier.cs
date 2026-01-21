namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a supplier/vendor for products.
/// </summary>
public class Supplier : SoftDeleteEntity
{
    /// <summary>
    /// Supplier name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique supplier code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the supplier.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this supplier is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Supplier type.
    /// </summary>
    public SupplierType Type { get; set; } = SupplierType.Manufacturer;

    #region Contact Information

    /// <summary>
    /// Primary contact name.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Contact email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Contact phone.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Website URL.
    /// </summary>
    public string? Website { get; set; }

    #endregion

    #region Address

    /// <summary>
    /// Street address line 1.
    /// </summary>
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Street address line 2.
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province/Region.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Postal/ZIP code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string? Country { get; set; }

    #endregion

    #region Business Information

    /// <summary>
    /// Tax ID / VAT number.
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Payment terms (e.g., Net 30).
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Default lead time in days.
    /// </summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// Minimum order value.
    /// </summary>
    public decimal? MinOrderValue { get; set; }

    /// <summary>
    /// Currency code for transactions.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Supplier rating (1-5).
    /// </summary>
    public decimal? Rating { get; set; }

    #endregion

    /// <summary>
    /// Additional notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Products from this supplier.
    /// </summary>
    public List<SupplierProduct> Products { get; set; } = [];

    /// <summary>
    /// Full address formatted.
    /// </summary>
    public string? FullAddress => string.Join(", ",
        new[] { AddressLine1, AddressLine2, City, State, PostalCode, Country }
        .Where(s => !string.IsNullOrEmpty(s)));
}

/// <summary>
/// Supplier type.
/// </summary>
public enum SupplierType
{
    /// <summary>
    /// Manufacturer.
    /// </summary>
    Manufacturer = 0,

    /// <summary>
    /// Distributor.
    /// </summary>
    Distributor = 1,

    /// <summary>
    /// Wholesaler.
    /// </summary>
    Wholesaler = 2,

    /// <summary>
    /// Drop shipper.
    /// </summary>
    DropShipper = 3,

    /// <summary>
    /// Importer.
    /// </summary>
    Importer = 4,

    /// <summary>
    /// Other.
    /// </summary>
    Other = 99
}

/// <summary>
/// Links a supplier to a product with supplier-specific details.
/// </summary>
public class SupplierProduct : BaseEntity
{
    /// <summary>
    /// Supplier ID.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant ID (null for main product).
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Supplier's SKU for this product.
    /// </summary>
    public string? SupplierSku { get; set; }

    /// <summary>
    /// Cost price from this supplier.
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Minimum order quantity.
    /// </summary>
    public int MinOrderQuantity { get; set; } = 1;

    /// <summary>
    /// Order quantity increment.
    /// </summary>
    public int OrderIncrement { get; set; } = 1;

    /// <summary>
    /// Lead time in days for this product.
    /// </summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// Whether this is the primary supplier.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Whether this supplier can currently supply.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last order date from this supplier.
    /// </summary>
    public DateTime? LastOrderDate { get; set; }

    /// <summary>
    /// Notes specific to this supplier-product relationship.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Supplier? Supplier { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Product? Product { get; set; }
}

/// <summary>
/// Purchase order for restocking from a supplier.
/// </summary>
public class PurchaseOrder : BaseEntity
{
    /// <summary>
    /// Purchase order number.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Supplier ID.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Destination warehouse ID.
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Order status.
    /// </summary>
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    /// <summary>
    /// Order date.
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expected delivery date.
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// Actual delivery date.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Subtotal before tax and shipping.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Shipping cost.
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Discount amount.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Total order amount.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Supplier's reference/confirmation number.
    /// </summary>
    public string? SupplierReference { get; set; }

    /// <summary>
    /// Payment terms.
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Shipping method.
    /// </summary>
    public string? ShippingMethod { get; set; }

    /// <summary>
    /// Tracking number.
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Order notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Internal notes (not sent to supplier).
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// User who created the order.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who approved the order.
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Date when approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Date when sent to supplier.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Order line items.
    /// </summary>
    public List<PurchaseOrderItem> Items { get; set; } = [];

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Supplier? Supplier { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Warehouse? Warehouse { get; set; }

    /// <summary>
    /// Total quantity ordered.
    /// </summary>
    public int TotalQuantityOrdered => Items.Sum(i => i.QuantityOrdered);

    /// <summary>
    /// Total quantity received.
    /// </summary>
    public int TotalQuantityReceived => Items.Sum(i => i.QuantityReceived);

    /// <summary>
    /// Whether fully received.
    /// </summary>
    public bool IsFullyReceived => Items.All(i => i.QuantityReceived >= i.QuantityOrdered);
}

/// <summary>
/// Purchase order line item.
/// </summary>
public class PurchaseOrderItem : BaseEntity
{
    /// <summary>
    /// Parent order ID.
    /// </summary>
    public Guid PurchaseOrderId { get; set; }

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
    /// Supplier's SKU.
    /// </summary>
    public string? SupplierSku { get; set; }

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int QuantityOrdered { get; set; }

    /// <summary>
    /// Quantity received.
    /// </summary>
    public int QuantityReceived { get; set; }

    /// <summary>
    /// Quantity rejected/damaged.
    /// </summary>
    public int QuantityRejected { get; set; }

    /// <summary>
    /// Unit cost.
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Discount percentage.
    /// </summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>
    /// Tax rate percentage.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Line total.
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Destination bin location.
    /// </summary>
    public string? BinLocation { get; set; }

    /// <summary>
    /// Item notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public PurchaseOrder? PurchaseOrder { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Quantity pending (ordered minus received minus rejected).
    /// </summary>
    public int QuantityPending => QuantityOrdered - QuantityReceived - QuantityRejected;
}

/// <summary>
/// Purchase order status.
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// Draft - not yet submitted.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Pending approval.
    /// </summary>
    PendingApproval = 1,

    /// <summary>
    /// Approved - ready to send.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Sent to supplier.
    /// </summary>
    Sent = 3,

    /// <summary>
    /// Confirmed by supplier.
    /// </summary>
    Confirmed = 4,

    /// <summary>
    /// Partially received.
    /// </summary>
    PartiallyReceived = 5,

    /// <summary>
    /// Fully received.
    /// </summary>
    Received = 6,

    /// <summary>
    /// Completed and closed.
    /// </summary>
    Completed = 7,

    /// <summary>
    /// Cancelled.
    /// </summary>
    Cancelled = 8
}
