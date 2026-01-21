using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for inventory management in the backoffice.
/// </summary>
[ApiController]
[BackOfficeRoute("ecommerce/inventory")]
[MapToApi("ecommerce-management-api")]
public class InventoryManagementApiController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public InventoryManagementApiController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    #region Dashboard & Stats

    /// <summary>
    /// Gets inventory overview statistics.
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct = default)
    {
        var stats = await _warehouseService.GetOverviewStatsAsync(ct);
        return Ok(stats);
    }

    /// <summary>
    /// Gets inventory tree stats.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var warehouses = await _warehouseService.GetAllWarehousesAsync(false, ct);
        var suppliers = await _warehouseService.GetAllSuppliersAsync(false, ct);

        return Ok(new
        {
            warehouses = warehouses.Count,
            suppliers = suppliers.Count,
            purchaseOrders = 0,
            stockAdjustments = 0,
            stockTransfers = 0,
            lowStock = 0
        });
    }

    /// <summary>
    /// Gets low stock items.
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock(
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var items = await _warehouseService.GetLowStockItemsAsync(null, ct);
        var result = items.Take(limit).Select(s => new
        {
            productId = s.ProductId,
            productName = s.Product?.Name ?? "Unknown",
            sku = s.Product?.Sku,
            warehouseId = s.WarehouseId,
            warehouseName = s.Warehouse?.Name ?? "Unknown",
            quantity = s.QuantityOnHand - s.QuantityReserved,
            threshold = s.LowStockThreshold
        });

        return Ok(result);
    }

    /// <summary>
    /// Gets recent inventory activity.
    /// </summary>
    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity(
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        // Return empty for now - can be enhanced with audit logging
        await Task.CompletedTask;
        return Ok(new List<object>());
    }

    #endregion

    #region Warehouses

    /// <summary>
    /// Gets all warehouses.
    /// </summary>
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetAllWarehouses(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var warehouses = await _warehouseService.GetAllWarehousesAsync(includeInactive, ct);
        return Ok(warehouses.Select(MapWarehouseToResponse));
    }

    /// <summary>
    /// Gets a warehouse by ID.
    /// </summary>
    [HttpGet("warehouse/{id:guid}")]
    public async Task<IActionResult> GetWarehouse(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        return Ok(MapWarehouseToResponse(warehouse));
    }

    /// <summary>
    /// Creates a new warehouse.
    /// </summary>
    [HttpPost("warehouse")]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = new Warehouse
        {
            Name = request.Name,
            Code = request.Code,
            Type = Enum.TryParse<WarehouseType>(request.Type, out var type) ? type : WarehouseType.Warehouse,
            Description = request.Description,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            CanFulfillOrders = request.CanFulfillOrders,
            AcceptsReturns = request.AcceptsReturns,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Priority = request.Priority,
            ShippingCountries = request.ShippingCountries ?? [],
            OperatingHours = request.OperatingHours ?? new Dictionary<string, string>()
        };

        var created = await _warehouseService.CreateWarehouseAsync(warehouse, ct);
        return Ok(MapWarehouseToResponse(created));
    }

    /// <summary>
    /// Updates a warehouse.
    /// </summary>
    [HttpPut("warehouse/{id:guid}")]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        warehouse.Name = request.Name;
        warehouse.Code = request.Code;
        warehouse.Type = Enum.TryParse<WarehouseType>(request.Type, out var type) ? type : WarehouseType.Warehouse;
        warehouse.Description = request.Description;
        warehouse.IsActive = request.IsActive;
        warehouse.IsDefault = request.IsDefault;
        warehouse.CanFulfillOrders = request.CanFulfillOrders;
        warehouse.AcceptsReturns = request.AcceptsReturns;
        warehouse.AddressLine1 = request.AddressLine1;
        warehouse.AddressLine2 = request.AddressLine2;
        warehouse.City = request.City;
        warehouse.State = request.State;
        warehouse.PostalCode = request.PostalCode;
        warehouse.Country = request.Country;
        warehouse.ContactName = request.ContactName;
        warehouse.ContactEmail = request.ContactEmail;
        warehouse.ContactPhone = request.ContactPhone;
        warehouse.Priority = request.Priority;
        warehouse.ShippingCountries = request.ShippingCountries ?? [];
        warehouse.OperatingHours = request.OperatingHours ?? new Dictionary<string, string>();

        var updated = await _warehouseService.UpdateWarehouseAsync(warehouse, ct);
        return Ok(MapWarehouseToResponse(updated));
    }

    /// <summary>
    /// Deletes a warehouse.
    /// </summary>
    [HttpDelete("warehouse/{id:guid}")]
    public async Task<IActionResult> DeleteWarehouse(Guid id, CancellationToken ct = default)
    {
        await _warehouseService.DeleteWarehouseAsync(id, ct);
        return Ok();
    }

    /// <summary>
    /// Gets stock levels for a warehouse.
    /// </summary>
    [HttpGet("warehouse/{id:guid}/stock")]
    public async Task<IActionResult> GetWarehouseStock(Guid id, CancellationToken ct = default)
    {
        var stock = await _warehouseService.GetWarehouseStockAsync(id, ct);
        return Ok(stock.Select(s => new
        {
            s.Id,
            s.WarehouseId,
            s.ProductId,
            s.VariantId,
            productName = s.Product?.Name ?? "Unknown",
            sku = s.Product?.Sku,
            s.QuantityOnHand,
            s.QuantityReserved,
            s.LowStockThreshold,
            s.ReorderPoint,
            s.BinLocation
        }));
    }

    /// <summary>
    /// Sets a warehouse as default.
    /// </summary>
    [HttpPost("warehouse/{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultWarehouse(Guid id, CancellationToken ct = default)
    {
        await _warehouseService.SetDefaultWarehouseAsync(id, ct);
        return Ok();
    }

    /// <summary>
    /// Toggles warehouse status.
    /// </summary>
    [HttpPost("warehouse/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleWarehouseStatus(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        warehouse.IsActive = !warehouse.IsActive;
        var updated = await _warehouseService.UpdateWarehouseAsync(warehouse, ct);
        return Ok(MapWarehouseToResponse(updated));
    }

    /// <summary>
    /// Updates warehouse sort order.
    /// </summary>
    [HttpPost("warehouse/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateWarehouseSort(Guid id, [FromBody] InventorySortOrderRequest request, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        warehouse.SortOrder = request.SortOrder;
        var updated = await _warehouseService.UpdateWarehouseAsync(warehouse, ct);
        return Ok(MapWarehouseToResponse(updated));
    }

    /// <summary>
    /// Updates warehouse priority.
    /// </summary>
    [HttpPost("warehouse/{id:guid}/update-priority")]
    public async Task<IActionResult> UpdateWarehousePriority(Guid id, [FromBody] InventoryPriorityRequest request, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        warehouse.Priority = request.Priority;
        var updated = await _warehouseService.UpdateWarehouseAsync(warehouse, ct);
        return Ok(MapWarehouseToResponse(updated));
    }

    /// <summary>
    /// Toggles warehouse fulfillment capability.
    /// </summary>
    [HttpPost("warehouse/{id:guid}/toggle-fulfillment")]
    public async Task<IActionResult> ToggleWarehouseFulfillment(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        warehouse.CanFulfillOrders = !warehouse.CanFulfillOrders;
        var updated = await _warehouseService.UpdateWarehouseAsync(warehouse, ct);
        return Ok(MapWarehouseToResponse(updated));
    }

    /// <summary>
    /// Toggles warehouse returns acceptance.
    /// </summary>
    [HttpPost("warehouse/{id:guid}/toggle-returns")]
    public async Task<IActionResult> ToggleWarehouseReturns(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        warehouse.AcceptsReturns = !warehouse.AcceptsReturns;
        var updated = await _warehouseService.UpdateWarehouseAsync(warehouse, ct);
        return Ok(MapWarehouseToResponse(updated));
    }

    /// <summary>
    /// Duplicates a warehouse.
    /// </summary>
    [HttpPost("warehouse/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateWarehouse(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, ct);
        if (warehouse == null)
        {
            return NotFound();
        }

        var duplicate = new Warehouse
        {
            Name = $"{warehouse.Name} (Copy)",
            Code = $"{warehouse.Code}-copy-{DateTime.UtcNow.Ticks}",
            Description = warehouse.Description,
            Type = warehouse.Type,
            IsActive = false, // Start inactive
            IsDefault = false, // Don't copy default status
            Priority = warehouse.Priority + 1,
            SortOrder = warehouse.SortOrder + 1,
            AddressLine1 = warehouse.AddressLine1,
            AddressLine2 = warehouse.AddressLine2,
            City = warehouse.City,
            State = warehouse.State,
            PostalCode = warehouse.PostalCode,
            Country = warehouse.Country,
            ContactName = warehouse.ContactName,
            ContactEmail = warehouse.ContactEmail,
            ContactPhone = warehouse.ContactPhone,
            CanFulfillOrders = warehouse.CanFulfillOrders,
            AcceptsReturns = warehouse.AcceptsReturns,
            AllowNegativeStock = warehouse.AllowNegativeStock,
            ShippingCountries = warehouse.ShippingCountries.ToList(),
            OperatingHours = new Dictionary<string, string>(warehouse.OperatingHours)
        };

        var created = await _warehouseService.CreateWarehouseAsync(duplicate, ct);
        return Ok(MapWarehouseToResponse(created));
    }

    #endregion

    #region Suppliers

    /// <summary>
    /// Gets all suppliers.
    /// </summary>
    [HttpGet("suppliers")]
    public async Task<IActionResult> GetAllSuppliers(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var suppliers = await _warehouseService.GetAllSuppliersAsync(includeInactive, ct);
        return Ok(suppliers.Select(MapSupplierToResponse));
    }

    /// <summary>
    /// Gets a supplier by ID.
    /// </summary>
    [HttpGet("supplier/{id:guid}")]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        return Ok(MapSupplierToResponse(supplier));
    }

    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    [HttpPost("supplier")]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request, CancellationToken ct = default)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            Code = request.Code,
            Type = Enum.TryParse<SupplierType>(request.Type, out var type) ? type : SupplierType.Manufacturer,
            Description = request.Description,
            IsActive = request.IsActive,
            ContactName = request.ContactName,
            Email = request.Email,
            Phone = request.Phone,
            Website = request.Website,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            TaxId = request.TaxId,
            PaymentTerms = request.PaymentTerms,
            CurrencyCode = request.CurrencyCode ?? "USD",
            LeadTimeDays = request.LeadTimeDays,
            MinOrderValue = request.MinOrderValue,
            Notes = request.Notes
        };

        var created = await _warehouseService.CreateSupplierAsync(supplier, ct);
        return Ok(MapSupplierToResponse(created));
    }

    /// <summary>
    /// Updates a supplier.
    /// </summary>
    [HttpPut("supplier/{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        supplier.Name = request.Name;
        supplier.Code = request.Code;
        supplier.Type = Enum.TryParse<SupplierType>(request.Type, out var type) ? type : SupplierType.Manufacturer;
        supplier.Description = request.Description;
        supplier.IsActive = request.IsActive;
        supplier.ContactName = request.ContactName;
        supplier.Email = request.Email;
        supplier.Phone = request.Phone;
        supplier.Website = request.Website;
        supplier.AddressLine1 = request.AddressLine1;
        supplier.AddressLine2 = request.AddressLine2;
        supplier.City = request.City;
        supplier.State = request.State;
        supplier.PostalCode = request.PostalCode;
        supplier.Country = request.Country;
        supplier.TaxId = request.TaxId;
        supplier.PaymentTerms = request.PaymentTerms;
        supplier.CurrencyCode = request.CurrencyCode ?? "USD";
        supplier.LeadTimeDays = request.LeadTimeDays;
        supplier.MinOrderValue = request.MinOrderValue;
        supplier.Notes = request.Notes;

        var updated = await _warehouseService.UpdateSupplierAsync(supplier, ct);
        return Ok(MapSupplierToResponse(updated));
    }

    /// <summary>
    /// Deletes a supplier.
    /// </summary>
    [HttpDelete("supplier/{id:guid}")]
    public async Task<IActionResult> DeleteSupplier(Guid id, CancellationToken ct = default)
    {
        await _warehouseService.DeleteSupplierAsync(id, ct);
        return Ok();
    }

    /// <summary>
    /// Gets products for a supplier.
    /// </summary>
    [HttpGet("supplier/{id:guid}/products")]
    public async Task<IActionResult> GetSupplierProducts(Guid id, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        return Ok(supplier.Products.Select(p => new
        {
            p.Id,
            p.SupplierId,
            p.ProductId,
            p.VariantId,
            productName = p.Product?.Name ?? "Unknown",
            sku = p.Product?.Sku,
            p.SupplierSku,
            p.CostPrice,
            p.MinOrderQuantity,
            p.LeadTimeDays,
            p.IsPrimary
        }));
    }

    /// <summary>
    /// Toggles supplier status.
    /// </summary>
    [HttpPost("supplier/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleSupplierStatus(Guid id, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        supplier.IsActive = !supplier.IsActive;
        var updated = await _warehouseService.UpdateSupplierAsync(supplier, ct);
        return Ok(MapSupplierToResponse(updated));
    }

    /// <summary>
    /// Updates supplier sort order.
    /// </summary>
    [HttpPost("supplier/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateSupplierSort(Guid id, [FromBody] InventorySortOrderRequest request, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        supplier.SortOrder = request.SortOrder;
        var updated = await _warehouseService.UpdateSupplierAsync(supplier, ct);
        return Ok(MapSupplierToResponse(updated));
    }

    /// <summary>
    /// Updates supplier lead time.
    /// </summary>
    [HttpPost("supplier/{id:guid}/update-lead-time")]
    public async Task<IActionResult> UpdateSupplierLeadTime(Guid id, [FromBody] SupplierLeadTimeRequest request, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        supplier.LeadTimeDays = request.LeadTimeDays;
        var updated = await _warehouseService.UpdateSupplierAsync(supplier, ct);
        return Ok(MapSupplierToResponse(updated));
    }

    /// <summary>
    /// Updates supplier rating.
    /// </summary>
    [HttpPost("supplier/{id:guid}/update-rating")]
    public async Task<IActionResult> UpdateSupplierRating(Guid id, [FromBody] SupplierRatingRequest request, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        supplier.Rating = request.Rating;
        var updated = await _warehouseService.UpdateSupplierAsync(supplier, ct);
        return Ok(MapSupplierToResponse(updated));
    }

    /// <summary>
    /// Duplicates a supplier.
    /// </summary>
    [HttpPost("supplier/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateSupplier(Guid id, CancellationToken ct = default)
    {
        var supplier = await _warehouseService.GetSupplierByIdAsync(id, ct);
        if (supplier == null)
        {
            return NotFound();
        }

        var duplicate = new Supplier
        {
            Name = $"{supplier.Name} (Copy)",
            Code = $"{supplier.Code}-copy-{DateTime.UtcNow.Ticks}",
            Description = supplier.Description,
            Type = supplier.Type,
            IsActive = false, // Start inactive
            SortOrder = supplier.SortOrder + 1,
            ContactName = supplier.ContactName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Website = supplier.Website,
            AddressLine1 = supplier.AddressLine1,
            AddressLine2 = supplier.AddressLine2,
            City = supplier.City,
            State = supplier.State,
            PostalCode = supplier.PostalCode,
            Country = supplier.Country,
            TaxId = supplier.TaxId,
            PaymentTerms = supplier.PaymentTerms,
            LeadTimeDays = supplier.LeadTimeDays,
            MinOrderValue = supplier.MinOrderValue,
            CurrencyCode = supplier.CurrencyCode,
            Notes = supplier.Notes
        };

        var created = await _warehouseService.CreateSupplierAsync(duplicate, ct);
        return Ok(MapSupplierToResponse(created));
    }

    #endregion

    #region Stock Adjustments

    /// <summary>
    /// Gets a stock adjustment by ID.
    /// </summary>
    [HttpGet("stock-adjustment/{id:guid}")]
    public async Task<IActionResult> GetStockAdjustment(Guid id, CancellationToken ct = default)
    {
        var adjustment = await _warehouseService.GetAdjustmentByIdAsync(id, ct);
        if (adjustment == null)
        {
            return NotFound();
        }

        return Ok(MapStockAdjustmentToResponse(adjustment));
    }

    /// <summary>
    /// Creates a new stock adjustment.
    /// </summary>
    [HttpPost("stock-adjustment")]
    public async Task<IActionResult> CreateStockAdjustment([FromBody] CreateStockAdjustmentRequest request, CancellationToken ct = default)
    {
        var adjustment = new StockAdjustment
        {
            WarehouseId = request.WarehouseId,
            Type = Enum.TryParse<StockAdjustmentType>(request.Type, out var type) ? type : StockAdjustmentType.InventoryCount,
            Notes = request.Notes,
            ExternalReference = request.ExternalReference
        };

        var created = await _warehouseService.CreateAdjustmentAsync(adjustment, ct);
        return Ok(MapStockAdjustmentToResponse(created));
    }

    /// <summary>
    /// Updates a stock adjustment.
    /// </summary>
    [HttpPut("stock-adjustment/{id:guid}")]
    public async Task<IActionResult> UpdateStockAdjustment(Guid id, [FromBody] UpdateStockAdjustmentRequest request, CancellationToken ct = default)
    {
        var adjustment = await _warehouseService.GetAdjustmentByIdAsync(id, ct);
        if (adjustment == null)
        {
            return NotFound();
        }

        adjustment.Type = Enum.TryParse<StockAdjustmentType>(request.Type, out var type) ? type : StockAdjustmentType.InventoryCount;
        adjustment.Notes = request.Notes;
        adjustment.ExternalReference = request.ExternalReference;

        var updated = await _warehouseService.UpdateAdjustmentAsync(adjustment, ct);
        return Ok(MapStockAdjustmentToResponse(updated));
    }

    /// <summary>
    /// Approves a stock adjustment.
    /// </summary>
    [HttpPost("stock-adjustment/{id:guid}/approve")]
    public async Task<IActionResult> ApproveStockAdjustment(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.ApproveAdjustmentAsync(id, "admin", ct);
            return Ok(MapStockAdjustmentToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Completes a stock adjustment.
    /// </summary>
    [HttpPost("stock-adjustment/{id:guid}/complete")]
    public async Task<IActionResult> CompleteStockAdjustment(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.CompleteAdjustmentAsync(id, ct);
            return Ok(MapStockAdjustmentToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a stock adjustment.
    /// </summary>
    [HttpPost("stock-adjustment/{id:guid}/cancel")]
    public async Task<IActionResult> CancelStockAdjustment(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.CancelAdjustmentAsync(id, ct);
            return Ok(MapStockAdjustmentToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submits a stock adjustment for approval.
    /// </summary>
    [HttpPost("stock-adjustment/{id:guid}/submit")]
    public async Task<IActionResult> SubmitStockAdjustment(Guid id, CancellationToken ct = default)
    {
        var adjustment = await _warehouseService.GetAdjustmentByIdAsync(id, ct);
        if (adjustment == null)
        {
            return NotFound();
        }

        if (adjustment.Status != StockAdjustmentStatus.Draft)
        {
            return BadRequest(new { message = "Only draft adjustments can be submitted" });
        }

        adjustment.Status = StockAdjustmentStatus.Pending;
        var updated = await _warehouseService.UpdateAdjustmentAsync(adjustment, ct);
        return Ok(MapStockAdjustmentToResponse(updated));
    }

    /// <summary>
    /// Rejects a stock adjustment.
    /// </summary>
    [HttpPost("stock-adjustment/{id:guid}/reject")]
    public async Task<IActionResult> RejectStockAdjustment(Guid id, CancellationToken ct = default)
    {
        var adjustment = await _warehouseService.GetAdjustmentByIdAsync(id, ct);
        if (adjustment == null)
        {
            return NotFound();
        }

        if (adjustment.Status != StockAdjustmentStatus.Pending)
        {
            return BadRequest(new { message = "Only pending adjustments can be rejected" });
        }

        adjustment.Status = StockAdjustmentStatus.Rejected;
        var updated = await _warehouseService.UpdateAdjustmentAsync(adjustment, ct);
        return Ok(MapStockAdjustmentToResponse(updated));
    }

    /// <summary>
    /// Updates stock adjustment type.
    /// </summary>
    [HttpPost("stock-adjustment/{id:guid}/update-type")]
    public async Task<IActionResult> UpdateStockAdjustmentType(Guid id, [FromBody] StockAdjustmentTypeRequest request, CancellationToken ct = default)
    {
        var adjustment = await _warehouseService.GetAdjustmentByIdAsync(id, ct);
        if (adjustment == null)
        {
            return NotFound();
        }

        if (adjustment.Status != StockAdjustmentStatus.Draft)
        {
            return BadRequest(new { message = "Only draft adjustments can be modified" });
        }

        adjustment.Type = Enum.TryParse<StockAdjustmentType>(request.Type, out var type) ? type : StockAdjustmentType.ManualCorrection;
        var updated = await _warehouseService.UpdateAdjustmentAsync(adjustment, ct);
        return Ok(MapStockAdjustmentToResponse(updated));
    }

    #endregion

    #region Stock Transfers

    /// <summary>
    /// Gets a stock transfer by ID.
    /// </summary>
    [HttpGet("stock-transfer/{id:guid}")]
    public async Task<IActionResult> GetStockTransfer(Guid id, CancellationToken ct = default)
    {
        var transfer = await _warehouseService.GetTransferByIdAsync(id, ct);
        if (transfer == null)
        {
            return NotFound();
        }

        return Ok(MapStockTransferToResponse(transfer));
    }

    /// <summary>
    /// Creates a new stock transfer.
    /// </summary>
    [HttpPost("stock-transfer")]
    public async Task<IActionResult> CreateStockTransfer([FromBody] CreateStockTransferRequest request, CancellationToken ct = default)
    {
        var transfer = new StockTransfer
        {
            SourceWarehouseId = request.SourceWarehouseId,
            DestinationWarehouseId = request.DestinationWarehouseId,
            ExpectedArrivalDate = request.ExpectedArrivalDate,
            Notes = request.Notes
        };

        var created = await _warehouseService.CreateTransferAsync(transfer, ct);
        return Ok(MapStockTransferToResponse(created));
    }

    /// <summary>
    /// Updates a stock transfer.
    /// </summary>
    [HttpPut("stock-transfer/{id:guid}")]
    public async Task<IActionResult> UpdateStockTransfer(Guid id, [FromBody] UpdateStockTransferRequest request, CancellationToken ct = default)
    {
        var transfer = await _warehouseService.GetTransferByIdAsync(id, ct);
        if (transfer == null)
        {
            return NotFound();
        }

        transfer.ExpectedArrivalDate = request.ExpectedArrivalDate;
        transfer.Notes = request.Notes;
        transfer.TrackingNumber = request.TrackingNumber;
        transfer.Carrier = request.Carrier;

        var updated = await _warehouseService.UpdateTransferAsync(transfer, ct);
        return Ok(MapStockTransferToResponse(updated));
    }

    /// <summary>
    /// Approves a stock transfer.
    /// </summary>
    [HttpPost("stock-transfer/{id:guid}/approve")]
    public async Task<IActionResult> ApproveStockTransfer(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.ApproveTransferAsync(id, "admin", ct);
            return Ok(MapStockTransferToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Ships a stock transfer.
    /// </summary>
    [HttpPost("stock-transfer/{id:guid}/ship")]
    public async Task<IActionResult> ShipStockTransfer(Guid id, [FromBody] ShipTransferRequest request, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.ShipTransferAsync(id, request.TrackingNumber, ct);
            return Ok(MapStockTransferToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Receives a stock transfer.
    /// </summary>
    [HttpPost("stock-transfer/{id:guid}/receive")]
    public async Task<IActionResult> ReceiveStockTransfer(Guid id, [FromBody] ReceiveTransferRequest request, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.ReceiveTransferAsync(id, request.Items ?? [], ct);
            return Ok(MapStockTransferToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a stock transfer.
    /// </summary>
    [HttpPost("stock-transfer/{id:guid}/cancel")]
    public async Task<IActionResult> CancelStockTransfer(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.CancelTransferAsync(id, ct);
            return Ok(MapStockTransferToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submits a stock transfer for approval.
    /// </summary>
    [HttpPost("stock-transfer/{id:guid}/submit")]
    public async Task<IActionResult> SubmitStockTransfer(Guid id, CancellationToken ct = default)
    {
        var transfer = await _warehouseService.GetTransferByIdAsync(id, ct);
        if (transfer == null)
        {
            return NotFound();
        }

        if (transfer.Status != StockTransferStatus.Draft)
        {
            return BadRequest(new { message = "Only draft transfers can be submitted" });
        }

        transfer.Status = StockTransferStatus.Pending;
        var updated = await _warehouseService.UpdateTransferAsync(transfer, ct);
        return Ok(MapStockTransferToResponse(updated));
    }

    /// <summary>
    /// Updates stock transfer tracking information.
    /// </summary>
    [HttpPost("stock-transfer/{id:guid}/update-tracking")]
    public async Task<IActionResult> UpdateStockTransferTracking(Guid id, [FromBody] StockTransferTrackingRequest request, CancellationToken ct = default)
    {
        var transfer = await _warehouseService.GetTransferByIdAsync(id, ct);
        if (transfer == null)
        {
            return NotFound();
        }

        transfer.TrackingNumber = request.TrackingNumber;
        transfer.Carrier = request.Carrier;
        var updated = await _warehouseService.UpdateTransferAsync(transfer, ct);
        return Ok(MapStockTransferToResponse(updated));
    }

    #endregion

    #region Purchase Orders

    /// <summary>
    /// Gets a purchase order by ID.
    /// </summary>
    [HttpGet("purchase-order/{id:guid}")]
    public async Task<IActionResult> GetPurchaseOrder(Guid id, CancellationToken ct = default)
    {
        var order = await _warehouseService.GetPurchaseOrderByIdAsync(id, ct);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(MapPurchaseOrderToResponse(order));
    }

    /// <summary>
    /// Creates a new purchase order.
    /// </summary>
    [HttpPost("purchase-order")]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        var order = new PurchaseOrder
        {
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            OrderDate = request.OrderDate ?? DateTime.UtcNow,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            CurrencyCode = request.CurrencyCode ?? "USD",
            Notes = request.Notes,
            InternalNotes = request.InternalNotes,
            PaymentTerms = request.PaymentTerms,
            ShippingMethod = request.ShippingMethod
        };

        var created = await _warehouseService.CreatePurchaseOrderAsync(order, ct);
        return Ok(MapPurchaseOrderToResponse(created));
    }

    /// <summary>
    /// Updates a purchase order.
    /// </summary>
    [HttpPut("purchase-order/{id:guid}")]
    public async Task<IActionResult> UpdatePurchaseOrder(Guid id, [FromBody] UpdatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        var order = await _warehouseService.GetPurchaseOrderByIdAsync(id, ct);
        if (order == null)
        {
            return NotFound();
        }

        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        order.Notes = request.Notes;
        order.InternalNotes = request.InternalNotes;
        order.PaymentTerms = request.PaymentTerms;
        order.ShippingMethod = request.ShippingMethod;
        order.TrackingNumber = request.TrackingNumber;
        order.SupplierReference = request.SupplierReference;

        var updated = await _warehouseService.UpdatePurchaseOrderAsync(order, ct);
        return Ok(MapPurchaseOrderToResponse(updated));
    }

    /// <summary>
    /// Approves a purchase order.
    /// </summary>
    [HttpPost("purchase-order/{id:guid}/approve")]
    public async Task<IActionResult> ApprovePurchaseOrder(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.ApprovePurchaseOrderAsync(id, "admin", ct);
            return Ok(MapPurchaseOrderToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Sends a purchase order to the supplier.
    /// </summary>
    [HttpPost("purchase-order/{id:guid}/send")]
    public async Task<IActionResult> SendPurchaseOrder(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.SendPurchaseOrderAsync(id, ct);
            return Ok(MapPurchaseOrderToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Receives items from a purchase order.
    /// </summary>
    [HttpPost("purchase-order/{id:guid}/receive")]
    public async Task<IActionResult> ReceivePurchaseOrder(Guid id, [FromBody] ReceivePurchaseOrderRequest request, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.ReceivePurchaseOrderAsync(id, request.Items ?? [], ct);
            return Ok(MapPurchaseOrderToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Completes a purchase order.
    /// </summary>
    [HttpPost("purchase-order/{id:guid}/complete")]
    public async Task<IActionResult> CompletePurchaseOrder(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.CompletePurchaseOrderAsync(id, ct);
            return Ok(MapPurchaseOrderToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a purchase order.
    /// </summary>
    [HttpPost("purchase-order/{id:guid}/cancel")]
    public async Task<IActionResult> CancelPurchaseOrder(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _warehouseService.CancelPurchaseOrderAsync(id, ct);
            return Ok(MapPurchaseOrderToResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets purchase orders for a supplier.
    /// </summary>
    [HttpGet("supplier/{id:guid}/purchase-orders")]
    public async Task<IActionResult> GetSupplierPurchaseOrders(Guid id, CancellationToken ct = default)
    {
        var orders = await _warehouseService.GetPurchaseOrdersAsync(id, 365, ct);
        return Ok(orders.Select(MapPurchaseOrderToResponse));
    }

    #endregion

    #region Mapping Helpers

    private static object MapWarehouseToResponse(Warehouse w) => new
    {
        w.Id,
        w.Name,
        w.Code,
        Type = w.Type.ToString(),
        w.Description,
        w.IsActive,
        w.IsDefault,
        w.CanFulfillOrders,
        w.AcceptsReturns,
        w.AddressLine1,
        w.AddressLine2,
        w.City,
        w.State,
        w.PostalCode,
        w.Country,
        w.ContactName,
        w.ContactEmail,
        w.ContactPhone,
        w.Priority,
        w.ShippingCountries,
        w.OperatingHours,
        w.CreatedAt,
        w.UpdatedAt
    };

    private static object MapSupplierToResponse(Supplier s) => new
    {
        s.Id,
        s.Name,
        s.Code,
        Type = s.Type.ToString(),
        s.Description,
        s.IsActive,
        s.ContactName,
        s.Email,
        s.Phone,
        s.Website,
        s.AddressLine1,
        s.AddressLine2,
        s.City,
        s.State,
        s.PostalCode,
        s.Country,
        s.TaxId,
        s.PaymentTerms,
        s.CurrencyCode,
        s.LeadTimeDays,
        s.MinOrderValue,
        s.Rating,
        s.Notes,
        s.CreatedAt,
        s.UpdatedAt
    };

    private static object MapStockAdjustmentToResponse(StockAdjustment a) => new
    {
        a.Id,
        a.ReferenceNumber,
        a.WarehouseId,
        WarehouseName = a.Warehouse?.Name,
        Type = a.Type.ToString(),
        Status = a.Status.ToString(),
        a.Notes,
        a.ExternalReference,
        a.CreatedBy,
        a.ApprovedBy,
        a.ApprovedAt,
        a.CompletedAt,
        Items = a.Items?.Select(i => new
        {
            i.Id,
            i.ProductId,
            i.VariantId,
            i.Sku,
            i.ProductName,
            i.QuantityBefore,
            i.QuantityAdjusted,
            i.UnitCost,
            i.BinLocation,
            i.Notes
        }),
        a.CreatedAt,
        a.UpdatedAt
    };

    private static object MapStockTransferToResponse(StockTransfer t) => new
    {
        t.Id,
        t.ReferenceNumber,
        t.SourceWarehouseId,
        SourceWarehouseName = t.SourceWarehouse?.Name,
        t.DestinationWarehouseId,
        DestinationWarehouseName = t.DestinationWarehouse?.Name,
        Status = t.Status.ToString(),
        t.ExpectedArrivalDate,
        t.ShippedAt,
        t.ReceivedAt,
        t.Notes,
        t.TrackingNumber,
        t.Carrier,
        t.CreatedBy,
        t.ApprovedBy,
        t.ApprovedAt,
        Items = t.Items?.Select(i => new
        {
            i.Id,
            i.ProductId,
            i.VariantId,
            i.Sku,
            i.ProductName,
            i.QuantityRequested,
            i.QuantityShipped,
            i.QuantityReceived,
            i.QuantityDamaged,
            i.SourceBinLocation,
            i.DestinationBinLocation,
            i.Notes
        }),
        t.CreatedAt,
        t.UpdatedAt
    };

    private static object MapPurchaseOrderToResponse(PurchaseOrder o) => new
    {
        o.Id,
        o.OrderNumber,
        o.SupplierId,
        SupplierName = o.Supplier?.Name,
        o.WarehouseId,
        WarehouseName = o.Warehouse?.Name,
        Status = o.Status.ToString(),
        o.OrderDate,
        o.ExpectedDeliveryDate,
        o.CurrencyCode,
        o.Subtotal,
        o.TaxAmount,
        o.ShippingCost,
        o.DiscountAmount,
        o.Total,
        o.SupplierReference,
        o.PaymentTerms,
        o.ShippingMethod,
        o.TrackingNumber,
        o.Notes,
        o.InternalNotes,
        o.CreatedBy,
        o.ApprovedBy,
        o.ApprovedAt,
        ItemCount = o.Items?.Count ?? 0,
        Items = o.Items?.Select(i => new
        {
            i.Id,
            i.ProductId,
            i.VariantId,
            i.Sku,
            i.ProductName,
            i.SupplierSku,
            i.QuantityOrdered,
            i.QuantityReceived,
            i.QuantityRejected,
            i.UnitCost,
            i.DiscountPercent,
            i.TaxRate,
            i.LineTotal,
            i.BinLocation,
            i.Notes
        }),
        o.CreatedAt,
        o.UpdatedAt
    };

    #endregion
}

#region Request Models

public class CreateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool CanFulfillOrders { get; set; } = true;
    public bool AcceptsReturns { get; set; } = true;
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int Priority { get; set; }
    public List<string>? ShippingCountries { get; set; }
    public Dictionary<string, string>? OperatingHours { get; set; }
}

public class UpdateWarehouseRequest : CreateWarehouseRequest { }

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxId { get; set; }
    public string? PaymentTerms { get; set; }
    public string? CurrencyCode { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinOrderValue { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSupplierRequest : CreateSupplierRequest { }

public class CreateStockAdjustmentRequest
{
    public Guid WarehouseId { get; set; }
    public string? Type { get; set; }
    public string? Notes { get; set; }
    public string? ExternalReference { get; set; }
}

public class UpdateStockAdjustmentRequest : CreateStockAdjustmentRequest { }

public class CreateStockTransferRequest
{
    public Guid SourceWarehouseId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public DateTime? ExpectedArrivalDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockTransferRequest
{
    public DateTime? ExpectedArrivalDate { get; set; }
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
}

public class ShipTransferRequest
{
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
}

public class ReceiveTransferRequest
{
    public bool ReceiveAll { get; set; }
    public List<TransferReceiveItem>? Items { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? PaymentTerms { get; set; }
    public string? ShippingMethod { get; set; }
}

public class UpdatePurchaseOrderRequest
{
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? PaymentTerms { get; set; }
    public string? ShippingMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public string? SupplierReference { get; set; }
}

public class ReceivePurchaseOrderRequest
{
    public List<PurchaseOrderReceiveItem>? Items { get; set; }
}

public class InventorySortOrderRequest
{
    public int SortOrder { get; set; }
}

public class InventoryPriorityRequest
{
    public int Priority { get; set; }
}

public class SupplierLeadTimeRequest
{
    public int? LeadTimeDays { get; set; }
}

public class SupplierRatingRequest
{
    public decimal? Rating { get; set; }
}

public class StockAdjustmentTypeRequest
{
    public string Type { get; set; } = "ManualCorrection";
}

public class StockTransferTrackingRequest
{
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
}

#endregion
