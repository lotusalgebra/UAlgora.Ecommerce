namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a warehouse or storage location for inventory.
/// </summary>
public class Warehouse : SoftDeleteEntity
{
    /// <summary>
    /// Store this warehouse belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Warehouse name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique warehouse code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of the warehouse.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this warehouse is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default warehouse.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Warehouse type.
    /// </summary>
    public WarehouseType Type { get; set; } = WarehouseType.Warehouse;

    /// <summary>
    /// Priority for stock allocation (lower = higher priority).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

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

    #region Contact

    /// <summary>
    /// Contact person name.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Contact email address.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? ContactPhone { get; set; }

    #endregion

    #region Settings

    /// <summary>
    /// Whether this warehouse can fulfill orders.
    /// </summary>
    public bool CanFulfillOrders { get; set; } = true;

    /// <summary>
    /// Whether this warehouse accepts returns.
    /// </summary>
    public bool AcceptsReturns { get; set; } = true;

    /// <summary>
    /// Whether to allow negative stock.
    /// </summary>
    public bool AllowNegativeStock { get; set; }

    /// <summary>
    /// Countries this warehouse can ship to (empty = all).
    /// </summary>
    public List<string> ShippingCountries { get; set; } = [];

    /// <summary>
    /// Operating hours (JSON).
    /// </summary>
    public Dictionary<string, string> OperatingHours { get; set; } = [];

    #endregion

    /// <summary>
    /// Stock levels in this warehouse.
    /// </summary>
    public List<WarehouseStock> StockLevels { get; set; } = [];

    /// <summary>
    /// Full address formatted.
    /// </summary>
    public string? FullAddress => string.Join(", ",
        new[] { AddressLine1, AddressLine2, City, State, PostalCode, Country }
        .Where(s => !string.IsNullOrEmpty(s)));
}

/// <summary>
/// Warehouse type.
/// </summary>
public enum WarehouseType
{
    /// <summary>
    /// Standard warehouse.
    /// </summary>
    Warehouse = 0,

    /// <summary>
    /// Retail store location.
    /// </summary>
    Store = 1,

    /// <summary>
    /// Distribution center.
    /// </summary>
    DistributionCenter = 2,

    /// <summary>
    /// Drop-ship location (supplier fulfills).
    /// </summary>
    DropShip = 3,

    /// <summary>
    /// Virtual warehouse (for aggregated view).
    /// </summary>
    Virtual = 4
}

/// <summary>
/// Stock level for a product/variant in a specific warehouse.
/// </summary>
public class WarehouseStock : BaseEntity
{
    /// <summary>
    /// Warehouse ID.
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant ID (null for main product).
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Available quantity for sale.
    /// </summary>
    public int QuantityOnHand { get; set; }

    /// <summary>
    /// Quantity reserved for orders.
    /// </summary>
    public int QuantityReserved { get; set; }

    /// <summary>
    /// Quantity on incoming orders/transfers.
    /// </summary>
    public int QuantityIncoming { get; set; }

    /// <summary>
    /// Low stock threshold for this location.
    /// </summary>
    public int? LowStockThreshold { get; set; }

    /// <summary>
    /// Reorder point quantity.
    /// </summary>
    public int? ReorderPoint { get; set; }

    /// <summary>
    /// Reorder quantity.
    /// </summary>
    public int? ReorderQuantity { get; set; }

    /// <summary>
    /// Bin/shelf location within warehouse.
    /// </summary>
    public string? BinLocation { get; set; }

    /// <summary>
    /// Last stock count date.
    /// </summary>
    public DateTime? LastCountedAt { get; set; }

    /// <summary>
    /// Last restock date.
    /// </summary>
    public DateTime? LastRestockedAt { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Warehouse? Warehouse { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Available quantity (on hand minus reserved).
    /// </summary>
    public int AvailableQuantity => QuantityOnHand - QuantityReserved;

    /// <summary>
    /// Whether stock is low.
    /// </summary>
    public bool IsLowStock => LowStockThreshold.HasValue && AvailableQuantity <= LowStockThreshold.Value;

    /// <summary>
    /// Whether stock needs reordering.
    /// </summary>
    public bool NeedsReorder => ReorderPoint.HasValue && AvailableQuantity <= ReorderPoint.Value;
}
