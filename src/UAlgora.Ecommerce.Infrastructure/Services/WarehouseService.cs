using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for warehouse and inventory management.
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly EcommerceDbContext _context;
    private int _adjustmentCounter = 1;
    private int _transferCounter = 1;
    private int _purchaseOrderCounter = 1;

    public WarehouseService(EcommerceDbContext context)
    {
        _context = context;
    }

    #region Warehouses

    public async Task<Warehouse?> GetWarehouseByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);
    }

    public async Task<Warehouse?> GetWarehouseByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Code == code && !w.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllWarehousesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Warehouses.Where(w => !w.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        return await query.OrderBy(w => w.SortOrder).ThenBy(w => w.Name).ToListAsync(ct);
    }

    public async Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken ct = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsDefault && w.IsActive && !w.IsDeleted, ct);
    }

    public async Task<Warehouse> CreateWarehouseAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        warehouse.Id = Guid.NewGuid();
        warehouse.CreatedAt = DateTime.UtcNow;
        warehouse.UpdatedAt = DateTime.UtcNow;

        if (warehouse.IsDefault)
        {
            await ClearDefaultWarehouseAsync(ct);
        }

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(ct);

        return warehouse;
    }

    public async Task<Warehouse> UpdateWarehouseAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        warehouse.UpdatedAt = DateTime.UtcNow;

        if (warehouse.IsDefault)
        {
            await ClearDefaultWarehouseAsync(warehouse.Id, ct);
        }

        _context.Warehouses.Update(warehouse);
        await _context.SaveChangesAsync(ct);

        return warehouse;
    }

    public async Task DeleteWarehouseAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await GetWarehouseByIdAsync(id, ct);
        if (warehouse != null)
        {
            warehouse.IsDeleted = true;
            warehouse.DeletedAt = DateTime.UtcNow;
            warehouse.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task SetDefaultWarehouseAsync(Guid id, CancellationToken ct = default)
    {
        await ClearDefaultWarehouseAsync(ct);

        var warehouse = await GetWarehouseByIdAsync(id, ct);
        if (warehouse != null)
        {
            warehouse.IsDefault = true;
            warehouse.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public Task<ValidationResult> ValidateWarehouseAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(warehouse.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Name is required." });
        }

        if (string.IsNullOrWhiteSpace(warehouse.Code))
        {
            errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Code is required." });
        }

        return Task.FromResult(new ValidationResult { Errors = errors });
    }

    private async Task ClearDefaultWarehouseAsync(CancellationToken ct = default)
    {
        await ClearDefaultWarehouseAsync(null, ct);
    }

    private async Task ClearDefaultWarehouseAsync(Guid? exceptId, CancellationToken ct = default)
    {
        var defaults = await _context.Warehouses
            .Where(w => w.IsDefault && !w.IsDeleted && (exceptId == null || w.Id != exceptId))
            .ToListAsync(ct);

        foreach (var w in defaults)
        {
            w.IsDefault = false;
            w.UpdatedAt = DateTime.UtcNow;
        }
    }

    #endregion

    #region Warehouse Stock

    public async Task<IReadOnlyList<WarehouseStock>> GetWarehouseStockAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return await _context.WarehouseStocks
            .Include(s => s.Product)
            .Where(s => s.WarehouseId == warehouseId)
            .OrderBy(s => s.Product!.Name)
            .ToListAsync(ct);
    }

    public async Task<WarehouseStock?> GetStockAsync(Guid warehouseId, Guid productId, Guid? variantId = null, CancellationToken ct = default)
    {
        return await _context.WarehouseStocks
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId
                && s.ProductId == productId
                && s.VariantId == variantId, ct);
    }

    public async Task<IReadOnlyList<WarehouseStock>> GetProductStockAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default)
    {
        return await _context.WarehouseStocks
            .Include(s => s.Warehouse)
            .Where(s => s.ProductId == productId && s.VariantId == variantId)
            .ToListAsync(ct);
    }

    public async Task<WarehouseStock> UpdateStockAsync(WarehouseStock stock, CancellationToken ct = default)
    {
        var existing = await GetStockAsync(stock.WarehouseId, stock.ProductId, stock.VariantId, ct);

        if (existing == null)
        {
            stock.Id = Guid.NewGuid();
            stock.CreatedAt = DateTime.UtcNow;
            stock.UpdatedAt = DateTime.UtcNow;
            _context.WarehouseStocks.Add(stock);
        }
        else
        {
            existing.QuantityOnHand = stock.QuantityOnHand;
            existing.QuantityReserved = stock.QuantityReserved;
            existing.QuantityIncoming = stock.QuantityIncoming;
            existing.LowStockThreshold = stock.LowStockThreshold;
            existing.ReorderPoint = stock.ReorderPoint;
            existing.ReorderQuantity = stock.ReorderQuantity;
            existing.BinLocation = stock.BinLocation;
            existing.UpdatedAt = DateTime.UtcNow;
            stock = existing;
        }

        await _context.SaveChangesAsync(ct);
        return stock;
    }

    public async Task<IReadOnlyList<WarehouseStock>> GetLowStockItemsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        var query = _context.WarehouseStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.LowStockThreshold.HasValue && s.QuantityOnHand - s.QuantityReserved <= s.LowStockThreshold);

        if (warehouseId.HasValue)
        {
            query = query.Where(s => s.WarehouseId == warehouseId.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WarehouseStock>> GetOutOfStockItemsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        var query = _context.WarehouseStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.QuantityOnHand - s.QuantityReserved <= 0);

        if (warehouseId.HasValue)
        {
            query = query.Where(s => s.WarehouseId == warehouseId.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WarehouseStock>> GetReorderItemsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        var query = _context.WarehouseStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.ReorderPoint.HasValue && s.QuantityOnHand - s.QuantityReserved <= s.ReorderPoint);

        if (warehouseId.HasValue)
        {
            query = query.Where(s => s.WarehouseId == warehouseId.Value);
        }

        return await query.ToListAsync(ct);
    }

    #endregion

    #region Stock Adjustments

    public async Task<StockAdjustment?> GetAdjustmentByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.StockAdjustments
            .Include(a => a.Items)
            .Include(a => a.Warehouse)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IReadOnlyList<StockAdjustment>> GetWarehouseAdjustmentsAsync(Guid warehouseId, int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.StockAdjustments
            .Include(a => a.Items)
            .Where(a => a.WarehouseId == warehouseId && a.CreatedAt >= cutoff)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<StockAdjustment> CreateAdjustmentAsync(StockAdjustment adjustment, CancellationToken ct = default)
    {
        adjustment.Id = Guid.NewGuid();
        adjustment.CreatedAt = DateTime.UtcNow;
        adjustment.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(adjustment.ReferenceNumber))
        {
            adjustment.ReferenceNumber = await GenerateAdjustmentReferenceAsync(ct);
        }

        foreach (var item in adjustment.Items)
        {
            item.Id = Guid.NewGuid();
            item.StockAdjustmentId = adjustment.Id;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
        }

        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync(ct);

        return adjustment;
    }

    public async Task<StockAdjustment> UpdateAdjustmentAsync(StockAdjustment adjustment, CancellationToken ct = default)
    {
        adjustment.UpdatedAt = DateTime.UtcNow;
        _context.StockAdjustments.Update(adjustment);
        await _context.SaveChangesAsync(ct);
        return adjustment;
    }

    public async Task<StockAdjustment> ApproveAdjustmentAsync(Guid id, string approvedBy, CancellationToken ct = default)
    {
        var adjustment = await GetAdjustmentByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Adjustment not found.");

        adjustment.Status = StockAdjustmentStatus.Approved;
        adjustment.ApprovedBy = approvedBy;
        adjustment.ApprovedAt = DateTime.UtcNow;
        adjustment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return adjustment;
    }

    public async Task<StockAdjustment> CompleteAdjustmentAsync(Guid id, CancellationToken ct = default)
    {
        var adjustment = await GetAdjustmentByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Adjustment not found.");

        // Apply adjustments to warehouse stock
        foreach (var item in adjustment.Items)
        {
            var stock = await GetStockAsync(adjustment.WarehouseId, item.ProductId, item.VariantId, ct);
            if (stock == null)
            {
                stock = new WarehouseStock
                {
                    WarehouseId = adjustment.WarehouseId,
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    QuantityOnHand = item.QuantityAfter
                };
                await UpdateStockAsync(stock, ct);
            }
            else
            {
                stock.QuantityOnHand = item.QuantityAfter;
                await UpdateStockAsync(stock, ct);
            }
        }

        adjustment.Status = StockAdjustmentStatus.Completed;
        adjustment.CompletedAt = DateTime.UtcNow;
        adjustment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return adjustment;
    }

    public async Task<StockAdjustment> CancelAdjustmentAsync(Guid id, CancellationToken ct = default)
    {
        var adjustment = await GetAdjustmentByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Adjustment not found.");

        adjustment.Status = StockAdjustmentStatus.Cancelled;
        adjustment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return adjustment;
    }

    public Task<string> GenerateAdjustmentReferenceAsync(CancellationToken ct = default)
    {
        var reference = $"ADJ-{DateTime.UtcNow:yyyyMMdd}-{_adjustmentCounter++:D4}";
        return Task.FromResult(reference);
    }

    #endregion

    #region Stock Transfers

    public async Task<StockTransfer?> GetTransferByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.StockTransfers
            .Include(t => t.Items)
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<StockTransfer>> GetTransfersAsync(Guid? warehouseId = null, int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var query = _context.StockTransfers
            .Include(t => t.Items)
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Where(t => t.CreatedAt >= cutoff);

        if (warehouseId.HasValue)
        {
            query = query.Where(t => t.SourceWarehouseId == warehouseId || t.DestinationWarehouseId == warehouseId);
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
    }

    public async Task<StockTransfer> CreateTransferAsync(StockTransfer transfer, CancellationToken ct = default)
    {
        transfer.Id = Guid.NewGuid();
        transfer.CreatedAt = DateTime.UtcNow;
        transfer.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(transfer.ReferenceNumber))
        {
            transfer.ReferenceNumber = await GenerateTransferReferenceAsync(ct);
        }

        foreach (var item in transfer.Items)
        {
            item.Id = Guid.NewGuid();
            item.StockTransferId = transfer.Id;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
        }

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync(ct);

        return transfer;
    }

    public async Task<StockTransfer> UpdateTransferAsync(StockTransfer transfer, CancellationToken ct = default)
    {
        transfer.UpdatedAt = DateTime.UtcNow;
        _context.StockTransfers.Update(transfer);
        await _context.SaveChangesAsync(ct);
        return transfer;
    }

    public async Task<StockTransfer> ApproveTransferAsync(Guid id, string approvedBy, CancellationToken ct = default)
    {
        var transfer = await GetTransferByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Transfer not found.");

        transfer.Status = StockTransferStatus.Approved;
        transfer.ApprovedBy = approvedBy;
        transfer.ApprovedAt = DateTime.UtcNow;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return transfer;
    }

    public async Task<StockTransfer> ShipTransferAsync(Guid id, string? trackingNumber = null, CancellationToken ct = default)
    {
        var transfer = await GetTransferByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Transfer not found.");

        // Deduct from source warehouse
        foreach (var item in transfer.Items)
        {
            var stock = await GetStockAsync(transfer.SourceWarehouseId, item.ProductId, item.VariantId, ct);
            if (stock != null)
            {
                stock.QuantityOnHand -= item.QuantityRequested;
                await UpdateStockAsync(stock, ct);
            }
            item.QuantityShipped = item.QuantityRequested;
        }

        transfer.Status = StockTransferStatus.InTransit;
        transfer.ShippedAt = DateTime.UtcNow;
        transfer.TrackingNumber = trackingNumber;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return transfer;
    }

    public async Task<StockTransfer> ReceiveTransferAsync(Guid id, List<TransferReceiveItem> receivedItems, CancellationToken ct = default)
    {
        var transfer = await GetTransferByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Transfer not found.");

        foreach (var received in receivedItems)
        {
            var item = transfer.Items.FirstOrDefault(i => i.Id == received.ItemId);
            if (item != null)
            {
                item.QuantityReceived += received.QuantityReceived;
                item.QuantityDamaged += received.QuantityDamaged;
                item.Notes = received.Notes;

                // Add to destination warehouse
                var stock = await GetStockAsync(transfer.DestinationWarehouseId, item.ProductId, item.VariantId, ct);
                if (stock == null)
                {
                    stock = new WarehouseStock
                    {
                        WarehouseId = transfer.DestinationWarehouseId,
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        QuantityOnHand = received.QuantityReceived
                    };
                }
                else
                {
                    stock.QuantityOnHand += received.QuantityReceived;
                }
                await UpdateStockAsync(stock, ct);
            }
        }

        var allReceived = transfer.Items.All(i => i.QuantityReceived >= i.QuantityRequested);
        transfer.Status = allReceived ? StockTransferStatus.Completed : StockTransferStatus.PartiallyReceived;
        transfer.ReceivedAt = DateTime.UtcNow;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return transfer;
    }

    public async Task<StockTransfer> CancelTransferAsync(Guid id, CancellationToken ct = default)
    {
        var transfer = await GetTransferByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Transfer not found.");

        transfer.Status = StockTransferStatus.Cancelled;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return transfer;
    }

    public Task<string> GenerateTransferReferenceAsync(CancellationToken ct = default)
    {
        var reference = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{_transferCounter++:D4}";
        return Task.FromResult(reference);
    }

    #endregion

    #region Suppliers

    public async Task<Supplier?> GetSupplierByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
    }

    public async Task<Supplier?> GetSupplierByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Code == code && !s.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Supplier>> GetAllSuppliersAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Suppliers.Where(s => !s.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        return await query.OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SupplierProduct>> GetProductSuppliersAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default)
    {
        return await _context.SupplierProducts
            .Include(sp => sp.Supplier)
            .Where(sp => sp.ProductId == productId && sp.VariantId == variantId)
            .ToListAsync(ct);
    }

    public async Task<Supplier> CreateSupplierAsync(Supplier supplier, CancellationToken ct = default)
    {
        supplier.Id = Guid.NewGuid();
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.UpdatedAt = DateTime.UtcNow;

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(ct);

        return supplier;
    }

    public async Task<Supplier> UpdateSupplierAsync(Supplier supplier, CancellationToken ct = default)
    {
        supplier.UpdatedAt = DateTime.UtcNow;
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(ct);
        return supplier;
    }

    public async Task DeleteSupplierAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await GetSupplierByIdAsync(id, ct);
        if (supplier != null)
        {
            supplier.IsDeleted = true;
            supplier.DeletedAt = DateTime.UtcNow;
            supplier.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public Task<ValidationResult> ValidateSupplierAsync(Supplier supplier, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(supplier.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Name is required." });
        }

        if (string.IsNullOrWhiteSpace(supplier.Code))
        {
            errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Code is required." });
        }

        return Task.FromResult(new ValidationResult { Errors = errors });
    }

    public async Task<SupplierProduct> LinkProductToSupplierAsync(SupplierProduct supplierProduct, CancellationToken ct = default)
    {
        var existing = await _context.SupplierProducts
            .FirstOrDefaultAsync(sp => sp.SupplierId == supplierProduct.SupplierId
                && sp.ProductId == supplierProduct.ProductId
                && sp.VariantId == supplierProduct.VariantId, ct);

        if (existing != null)
        {
            existing.SupplierSku = supplierProduct.SupplierSku;
            existing.CostPrice = supplierProduct.CostPrice;
            existing.MinOrderQuantity = supplierProduct.MinOrderQuantity;
            existing.OrderIncrement = supplierProduct.OrderIncrement;
            existing.LeadTimeDays = supplierProduct.LeadTimeDays;
            existing.IsPrimary = supplierProduct.IsPrimary;
            existing.IsActive = supplierProduct.IsActive;
            existing.Notes = supplierProduct.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            supplierProduct = existing;
        }
        else
        {
            supplierProduct.Id = Guid.NewGuid();
            supplierProduct.CreatedAt = DateTime.UtcNow;
            supplierProduct.UpdatedAt = DateTime.UtcNow;
            _context.SupplierProducts.Add(supplierProduct);
        }

        await _context.SaveChangesAsync(ct);
        return supplierProduct;
    }

    public async Task UnlinkProductFromSupplierAsync(Guid supplierId, Guid productId, Guid? variantId = null, CancellationToken ct = default)
    {
        var existing = await _context.SupplierProducts
            .FirstOrDefaultAsync(sp => sp.SupplierId == supplierId
                && sp.ProductId == productId
                && sp.VariantId == variantId, ct);

        if (existing != null)
        {
            _context.SupplierProducts.Remove(existing);
            await _context.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Purchase Orders

    public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .FirstOrDefaultAsync(po => po.Id == id, ct);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(Guid? supplierId = null, int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var query = _context.PurchaseOrders
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Where(po => po.CreatedAt >= cutoff);

        if (supplierId.HasValue)
        {
            query = query.Where(po => po.SupplierId == supplierId);
        }

        return await query.OrderByDescending(po => po.OrderDate).ToListAsync(ct);
    }

    public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        order.Id = Guid.NewGuid();
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(order.OrderNumber))
        {
            order.OrderNumber = await GeneratePurchaseOrderNumberAsync(ct);
        }

        foreach (var item in order.Items)
        {
            item.Id = Guid.NewGuid();
            item.PurchaseOrderId = order.Id;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
        }

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync(ct);

        return order;
    }

    public async Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.PurchaseOrders.Update(order);
        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseOrder> ApprovePurchaseOrderAsync(Guid id, string approvedBy, CancellationToken ct = default)
    {
        var order = await GetPurchaseOrderByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        order.Status = PurchaseOrderStatus.Approved;
        order.ApprovedBy = approvedBy;
        order.ApprovedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseOrder> SendPurchaseOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await GetPurchaseOrderByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        order.Status = PurchaseOrderStatus.Sent;
        order.SentAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseOrder> ReceivePurchaseOrderAsync(Guid id, List<PurchaseOrderReceiveItem> receivedItems, CancellationToken ct = default)
    {
        var order = await GetPurchaseOrderByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        foreach (var received in receivedItems)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == received.ItemId);
            if (item != null)
            {
                item.QuantityReceived += received.QuantityReceived;
                item.QuantityRejected += received.QuantityRejected;
                item.BinLocation = received.BinLocation;
                item.Notes = received.Notes;

                // Add to warehouse stock
                var stock = await GetStockAsync(order.WarehouseId, item.ProductId, item.VariantId, ct);
                if (stock == null)
                {
                    stock = new WarehouseStock
                    {
                        WarehouseId = order.WarehouseId,
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        QuantityOnHand = received.QuantityReceived,
                        BinLocation = received.BinLocation,
                        LastRestockedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    stock.QuantityOnHand += received.QuantityReceived;
                    stock.LastRestockedAt = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(received.BinLocation))
                    {
                        stock.BinLocation = received.BinLocation;
                    }
                }
                await UpdateStockAsync(stock, ct);
            }
        }

        var allReceived = order.Items.All(i => i.QuantityReceived + i.QuantityRejected >= i.QuantityOrdered);
        order.Status = allReceived ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
        order.DeliveredAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseOrder> CompletePurchaseOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await GetPurchaseOrderByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        order.Status = PurchaseOrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseOrder> CancelPurchaseOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await GetPurchaseOrderByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        order.Status = PurchaseOrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return order;
    }

    public Task<string> GeneratePurchaseOrderNumberAsync(CancellationToken ct = default)
    {
        var orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{_purchaseOrderCounter++:D4}";
        return Task.FromResult(orderNumber);
    }

    #endregion

    #region Statistics

    public async Task<WarehouseStats> GetWarehouseStatsAsync(Guid warehouseId, CancellationToken ct = default)
    {
        var warehouse = await GetWarehouseByIdAsync(warehouseId, ct);
        var stocks = await GetWarehouseStockAsync(warehouseId, ct);

        return new WarehouseStats
        {
            WarehouseId = warehouseId,
            WarehouseName = warehouse?.Name ?? "",
            TotalProducts = stocks.Select(s => s.ProductId).Distinct().Count(),
            TotalSkus = stocks.Count,
            TotalUnits = stocks.Sum(s => s.QuantityOnHand),
            LowStockItems = stocks.Count(s => s.IsLowStock),
            OutOfStockItems = stocks.Count(s => s.AvailableQuantity <= 0),
            ReorderItems = stocks.Count(s => s.NeedsReorder),
            TotalValue = 0, // Would need cost data
            PendingTransfersIn = await _context.StockTransfers.CountAsync(t => t.DestinationWarehouseId == warehouseId && t.Status == StockTransferStatus.InTransit, ct),
            PendingTransfersOut = await _context.StockTransfers.CountAsync(t => t.SourceWarehouseId == warehouseId && t.Status == StockTransferStatus.Approved, ct),
            PendingAdjustments = await _context.StockAdjustments.CountAsync(a => a.WarehouseId == warehouseId && a.Status == StockAdjustmentStatus.Pending, ct)
        };
    }

    public async Task<InventoryOverviewStats> GetOverviewStatsAsync(CancellationToken ct = default)
    {
        var warehouses = await GetAllWarehousesAsync(false, ct);
        var warehouseStats = new List<WarehouseStats>();

        foreach (var warehouse in warehouses)
        {
            warehouseStats.Add(await GetWarehouseStatsAsync(warehouse.Id, ct));
        }

        return new InventoryOverviewStats
        {
            TotalWarehouses = warehouses.Count,
            TotalProducts = warehouseStats.Sum(w => w.TotalProducts),
            TotalUnits = warehouseStats.Sum(w => w.TotalUnits),
            TotalValue = warehouseStats.Sum(w => w.TotalValue),
            LowStockItems = warehouseStats.Sum(w => w.LowStockItems),
            OutOfStockItems = warehouseStats.Sum(w => w.OutOfStockItems),
            PendingPurchaseOrders = await _context.PurchaseOrders.CountAsync(po => po.Status == PurchaseOrderStatus.Sent || po.Status == PurchaseOrderStatus.Confirmed, ct),
            PendingTransfers = await _context.StockTransfers.CountAsync(t => t.Status == StockTransferStatus.InTransit, ct),
            PendingAdjustments = await _context.StockAdjustments.CountAsync(a => a.Status == StockAdjustmentStatus.Pending, ct),
            ByWarehouse = warehouseStats
        };
    }

    public Task<InventoryValuation> GetInventoryValuationAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        // Simplified - would need cost data from products
        return Task.FromResult(new InventoryValuation
        {
            TotalCostValue = 0,
            TotalRetailValue = 0,
            PotentialProfit = 0,
            MarginPercentage = 0,
            TotalUnits = 0,
            ByCategory = [],
            ByWarehouse = []
        });
    }

    #endregion
}
