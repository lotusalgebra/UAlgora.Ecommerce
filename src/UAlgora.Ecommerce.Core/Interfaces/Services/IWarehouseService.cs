using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for warehouse and inventory management operations.
/// </summary>
public interface IWarehouseService
{
    #region Warehouses

    /// <summary>
    /// Gets a warehouse by ID.
    /// </summary>
    Task<Warehouse?> GetWarehouseByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a warehouse by code.
    /// </summary>
    Task<Warehouse?> GetWarehouseByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all warehouses.
    /// </summary>
    Task<IReadOnlyList<Warehouse>> GetAllWarehousesAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets the default warehouse.
    /// </summary>
    Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new warehouse.
    /// </summary>
    Task<Warehouse> CreateWarehouseAsync(Warehouse warehouse, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing warehouse.
    /// </summary>
    Task<Warehouse> UpdateWarehouseAsync(Warehouse warehouse, CancellationToken ct = default);

    /// <summary>
    /// Deletes a warehouse.
    /// </summary>
    Task DeleteWarehouseAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Sets a warehouse as the default.
    /// </summary>
    Task SetDefaultWarehouseAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Validates a warehouse.
    /// </summary>
    Task<ValidationResult> ValidateWarehouseAsync(Warehouse warehouse, CancellationToken ct = default);

    #endregion

    #region Warehouse Stock

    /// <summary>
    /// Gets stock levels for a warehouse.
    /// </summary>
    Task<IReadOnlyList<WarehouseStock>> GetWarehouseStockAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>
    /// Gets stock level for a specific product in a warehouse.
    /// </summary>
    Task<WarehouseStock?> GetStockAsync(Guid warehouseId, Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets stock levels for a product across all warehouses.
    /// </summary>
    Task<IReadOnlyList<WarehouseStock>> GetProductStockAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Updates stock level.
    /// </summary>
    Task<WarehouseStock> UpdateStockAsync(WarehouseStock stock, CancellationToken ct = default);

    /// <summary>
    /// Gets low stock items.
    /// </summary>
    Task<IReadOnlyList<WarehouseStock>> GetLowStockItemsAsync(Guid? warehouseId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets out of stock items.
    /// </summary>
    Task<IReadOnlyList<WarehouseStock>> GetOutOfStockItemsAsync(Guid? warehouseId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets items needing reorder.
    /// </summary>
    Task<IReadOnlyList<WarehouseStock>> GetReorderItemsAsync(Guid? warehouseId = null, CancellationToken ct = default);

    #endregion

    #region Stock Adjustments

    /// <summary>
    /// Gets a stock adjustment by ID.
    /// </summary>
    Task<StockAdjustment?> GetAdjustmentByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets stock adjustments for a warehouse.
    /// </summary>
    Task<IReadOnlyList<StockAdjustment>> GetWarehouseAdjustmentsAsync(Guid warehouseId, int days = 30, CancellationToken ct = default);

    /// <summary>
    /// Creates a stock adjustment.
    /// </summary>
    Task<StockAdjustment> CreateAdjustmentAsync(StockAdjustment adjustment, CancellationToken ct = default);

    /// <summary>
    /// Updates a stock adjustment.
    /// </summary>
    Task<StockAdjustment> UpdateAdjustmentAsync(StockAdjustment adjustment, CancellationToken ct = default);

    /// <summary>
    /// Approves a stock adjustment.
    /// </summary>
    Task<StockAdjustment> ApproveAdjustmentAsync(Guid id, string approvedBy, CancellationToken ct = default);

    /// <summary>
    /// Completes a stock adjustment (applies to inventory).
    /// </summary>
    Task<StockAdjustment> CompleteAdjustmentAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Cancels a stock adjustment.
    /// </summary>
    Task<StockAdjustment> CancelAdjustmentAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Generates next adjustment reference number.
    /// </summary>
    Task<string> GenerateAdjustmentReferenceAsync(CancellationToken ct = default);

    #endregion

    #region Stock Transfers

    /// <summary>
    /// Gets a stock transfer by ID.
    /// </summary>
    Task<StockTransfer?> GetTransferByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets stock transfers.
    /// </summary>
    Task<IReadOnlyList<StockTransfer>> GetTransfersAsync(Guid? warehouseId = null, int days = 30, CancellationToken ct = default);

    /// <summary>
    /// Creates a stock transfer.
    /// </summary>
    Task<StockTransfer> CreateTransferAsync(StockTransfer transfer, CancellationToken ct = default);

    /// <summary>
    /// Updates a stock transfer.
    /// </summary>
    Task<StockTransfer> UpdateTransferAsync(StockTransfer transfer, CancellationToken ct = default);

    /// <summary>
    /// Approves a stock transfer.
    /// </summary>
    Task<StockTransfer> ApproveTransferAsync(Guid id, string approvedBy, CancellationToken ct = default);

    /// <summary>
    /// Ships a stock transfer.
    /// </summary>
    Task<StockTransfer> ShipTransferAsync(Guid id, string? trackingNumber = null, CancellationToken ct = default);

    /// <summary>
    /// Receives a stock transfer.
    /// </summary>
    Task<StockTransfer> ReceiveTransferAsync(Guid id, List<TransferReceiveItem> receivedItems, CancellationToken ct = default);

    /// <summary>
    /// Cancels a stock transfer.
    /// </summary>
    Task<StockTransfer> CancelTransferAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Generates next transfer reference number.
    /// </summary>
    Task<string> GenerateTransferReferenceAsync(CancellationToken ct = default);

    #endregion

    #region Suppliers

    /// <summary>
    /// Gets a supplier by ID.
    /// </summary>
    Task<Supplier?> GetSupplierByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a supplier by code.
    /// </summary>
    Task<Supplier?> GetSupplierByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all suppliers.
    /// </summary>
    Task<IReadOnlyList<Supplier>> GetAllSuppliersAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets suppliers for a product.
    /// </summary>
    Task<IReadOnlyList<SupplierProduct>> GetProductSuppliersAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    Task<Supplier> CreateSupplierAsync(Supplier supplier, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    Task<Supplier> UpdateSupplierAsync(Supplier supplier, CancellationToken ct = default);

    /// <summary>
    /// Deletes a supplier.
    /// </summary>
    Task DeleteSupplierAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Validates a supplier.
    /// </summary>
    Task<ValidationResult> ValidateSupplierAsync(Supplier supplier, CancellationToken ct = default);

    /// <summary>
    /// Links a product to a supplier.
    /// </summary>
    Task<SupplierProduct> LinkProductToSupplierAsync(SupplierProduct supplierProduct, CancellationToken ct = default);

    /// <summary>
    /// Unlinks a product from a supplier.
    /// </summary>
    Task UnlinkProductFromSupplierAsync(Guid supplierId, Guid productId, Guid? variantId = null, CancellationToken ct = default);

    #endregion

    #region Purchase Orders

    /// <summary>
    /// Gets a purchase order by ID.
    /// </summary>
    Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets purchase orders.
    /// </summary>
    Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(Guid? supplierId = null, int days = 30, CancellationToken ct = default);

    /// <summary>
    /// Creates a purchase order.
    /// </summary>
    Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order, CancellationToken ct = default);

    /// <summary>
    /// Updates a purchase order.
    /// </summary>
    Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder order, CancellationToken ct = default);

    /// <summary>
    /// Approves a purchase order.
    /// </summary>
    Task<PurchaseOrder> ApprovePurchaseOrderAsync(Guid id, string approvedBy, CancellationToken ct = default);

    /// <summary>
    /// Sends a purchase order to supplier.
    /// </summary>
    Task<PurchaseOrder> SendPurchaseOrderAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Receives items on a purchase order.
    /// </summary>
    Task<PurchaseOrder> ReceivePurchaseOrderAsync(Guid id, List<PurchaseOrderReceiveItem> receivedItems, CancellationToken ct = default);

    /// <summary>
    /// Completes a purchase order.
    /// </summary>
    Task<PurchaseOrder> CompletePurchaseOrderAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Cancels a purchase order.
    /// </summary>
    Task<PurchaseOrder> CancelPurchaseOrderAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Generates next purchase order number.
    /// </summary>
    Task<string> GeneratePurchaseOrderNumberAsync(CancellationToken ct = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets inventory statistics for a warehouse.
    /// </summary>
    Task<WarehouseStats> GetWarehouseStatsAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>
    /// Gets overall inventory statistics.
    /// </summary>
    Task<InventoryOverviewStats> GetOverviewStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets inventory valuation.
    /// </summary>
    Task<InventoryValuation> GetInventoryValuationAsync(Guid? warehouseId = null, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Item received in a transfer.
/// </summary>
public class TransferReceiveItem
{
    public Guid ItemId { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityDamaged { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Item received on a purchase order.
/// </summary>
public class PurchaseOrderReceiveItem
{
    public Guid ItemId { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityRejected { get; set; }
    public string? BinLocation { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Warehouse statistics.
/// </summary>
public class WarehouseStats
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalSkus { get; set; }
    public int TotalUnits { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public int ReorderItems { get; set; }
    public decimal TotalValue { get; set; }
    public int PendingTransfersIn { get; set; }
    public int PendingTransfersOut { get; set; }
    public int PendingAdjustments { get; set; }
}

/// <summary>
/// Overall inventory statistics.
/// </summary>
public class InventoryOverviewStats
{
    public int TotalWarehouses { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUnits { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public int PendingPurchaseOrders { get; set; }
    public int PendingTransfers { get; set; }
    public int PendingAdjustments { get; set; }
    public List<WarehouseStats> ByWarehouse { get; set; } = [];
}

/// <summary>
/// Inventory valuation.
/// </summary>
public class InventoryValuation
{
    public decimal TotalCostValue { get; set; }
    public decimal TotalRetailValue { get; set; }
    public decimal PotentialProfit { get; set; }
    public decimal MarginPercentage { get; set; }
    public int TotalUnits { get; set; }
    public List<CategoryValuation> ByCategory { get; set; } = [];
    public List<WarehouseValuation> ByWarehouse { get; set; } = [];
}

/// <summary>
/// Category inventory valuation.
/// </summary>
public class CategoryValuation
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public decimal CostValue { get; set; }
    public decimal RetailValue { get; set; }
}

/// <summary>
/// Warehouse inventory valuation.
/// </summary>
public class WarehouseValuation
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public decimal CostValue { get; set; }
    public decimal RetailValue { get; set; }
}
