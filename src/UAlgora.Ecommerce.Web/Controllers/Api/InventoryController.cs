using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using static UAlgora.Ecommerce.Web.ServiceCollectionExtensions;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for public inventory/stock operations.
/// </summary>
public class InventoryController : EcommerceApiController
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Gets stock status for a product.
    /// </summary>
    [HttpGet("status/{productId:guid}")]
    public async Task<IActionResult> GetStatus(
        Guid productId,
        [FromQuery] Guid? variantId = null,
        CancellationToken ct = default)
    {
        var status = await _inventoryService.GetStatusAsync(productId, variantId, ct);
        return ApiSuccess(status);
    }

    /// <summary>
    /// Checks if a product is in stock.
    /// </summary>
    [HttpGet("in-stock/{productId:guid}")]
    public async Task<IActionResult> IsInStock(
        Guid productId,
        [FromQuery] Guid? variantId = null,
        [FromQuery] int quantity = 1,
        CancellationToken ct = default)
    {
        var inStock = await _inventoryService.IsInStockAsync(productId, variantId, quantity, ct);
        return ApiSuccess(new { inStock, productId, variantId, requestedQuantity = quantity });
    }

    /// <summary>
    /// Checks availability for multiple products.
    /// </summary>
    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability(
        [FromBody] List<StockCheckApiRequest> requests,
        CancellationToken ct = default)
    {
        var checkRequests = requests.Select(r => new StockCheckRequest
        {
            ProductId = r.ProductId,
            VariantId = r.VariantId,
            Quantity = r.Quantity
        });

        var result = await _inventoryService.CheckAvailabilityAsync(checkRequests, ct);
        return ApiSuccess(result);
    }
}

/// <summary>
/// Admin API controller for inventory management.
/// </summary>
[Route("api/ecommerce/admin/inventory")]
[Authorize(Policy = EcommerceAdminPolicy)]
public class InventoryAdminController : EcommerceApiController
{
    private readonly IInventoryService _inventoryService;

    public InventoryAdminController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Gets current stock for a product.
    /// </summary>
    [HttpGet("stock/{productId:guid}")]
    public async Task<IActionResult> GetStock(
        Guid productId,
        [FromQuery] Guid? variantId = null,
        CancellationToken ct = default)
    {
        var stock = await _inventoryService.GetStockAsync(productId, variantId, ct);
        return ApiSuccess(new { productId, variantId, stockQuantity = stock });
    }

    /// <summary>
    /// Gets detailed inventory status.
    /// </summary>
    [HttpGet("status/{productId:guid}")]
    public async Task<IActionResult> GetDetailedStatus(
        Guid productId,
        [FromQuery] Guid? variantId = null,
        CancellationToken ct = default)
    {
        var status = await _inventoryService.GetStatusAsync(productId, variantId, ct);
        return ApiSuccess(status);
    }

    /// <summary>
    /// Gets low stock products.
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock(CancellationToken ct = default)
    {
        var products = await _inventoryService.GetLowStockAsync(ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets out of stock products.
    /// </summary>
    [HttpGet("out-of-stock")]
    public async Task<IActionResult> GetOutOfStock(CancellationToken ct = default)
    {
        var products = await _inventoryService.GetOutOfStockAsync(ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets stock movement history.
    /// </summary>
    [HttpGet("movements/{productId:guid}")]
    public async Task<IActionResult> GetMovementHistory(
        Guid productId,
        [FromQuery] Guid? variantId = null,
        [FromQuery] int days = 30,
        CancellationToken ct = default)
    {
        var movements = await _inventoryService.GetMovementHistoryAsync(productId, variantId, days, ct);
        return ApiSuccess(movements);
    }

    /// <summary>
    /// Sets stock quantity.
    /// </summary>
    [HttpPut("stock/{productId:guid}")]
    public async Task<IActionResult> SetStock(
        Guid productId,
        [FromBody] SetStockRequest request,
        CancellationToken ct = default)
    {
        if (request.Quantity < 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Quantity cannot be negative." });
        }

        try
        {
            var newStock = await _inventoryService.SetStockAsync(
                productId,
                request.VariantId,
                request.Quantity,
                request.Reason,
                ct);

            return ApiSuccess(new { productId, variantId = request.VariantId, stockQuantity = newStock }, "Stock updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Adjusts stock quantity (add or subtract).
    /// </summary>
    [HttpPost("stock/{productId:guid}/adjust")]
    public async Task<IActionResult> AdjustStock(
        Guid productId,
        [FromBody] AdjustStockRequest request,
        CancellationToken ct = default)
    {
        if (request.Adjustment == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Adjustment cannot be zero." });
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new ApiErrorResponse { Message = "Reason is required." });
        }

        try
        {
            var newStock = await _inventoryService.AdjustStockAsync(
                productId,
                request.VariantId,
                request.Adjustment,
                request.Reason,
                ct);

            return ApiSuccess(new { productId, variantId = request.VariantId, stockQuantity = newStock }, "Stock adjusted.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Restocks a product (typically from a return).
    /// </summary>
    [HttpPost("stock/{productId:guid}/restock")]
    public async Task<IActionResult> Restock(
        Guid productId,
        [FromBody] RestockRequest request,
        CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Quantity must be greater than 0." });
        }

        try
        {
            var newStock = await _inventoryService.RestockAsync(
                productId,
                request.VariantId,
                request.Quantity,
                request.Reason,
                ct);

            return ApiSuccess(new { productId, variantId = request.VariantId, stockQuantity = newStock }, "Stock restocked.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Reserves stock for an order.
    /// </summary>
    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock(
        [FromBody] ReserveStockRequest request,
        CancellationToken ct = default)
    {
        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(new ApiErrorResponse { Message = "At least one item is required." });
        }

        var items = request.Items.Select(i => new StockReservationItem
        {
            ProductId = i.ProductId,
            VariantId = i.VariantId,
            Quantity = i.Quantity
        });

        var reservation = await _inventoryService.ReserveStockAsync(request.OrderId, items, ct);
        return ApiSuccess(reservation, "Stock reserved.");
    }

    /// <summary>
    /// Commits a stock reservation.
    /// </summary>
    [HttpPost("reservations/{reservationId:guid}/commit")]
    public async Task<IActionResult> CommitReservation(
        Guid reservationId,
        CancellationToken ct = default)
    {
        try
        {
            await _inventoryService.CommitReservationAsync(reservationId, ct);
            return ApiSuccess(new { }, "Reservation committed.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Releases a stock reservation.
    /// </summary>
    [HttpPost("reservations/{reservationId:guid}/release")]
    public async Task<IActionResult> ReleaseReservation(
        Guid reservationId,
        CancellationToken ct = default)
    {
        try
        {
            await _inventoryService.ReleaseReservationAsync(reservationId, ct);
            return ApiSuccess(new { }, "Reservation released.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }
}

#region Request Models

public class StockCheckApiRequest
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class SetStockRequest
{
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}

public class AdjustStockRequest
{
    public Guid? VariantId { get; set; }
    public int Adjustment { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RestockRequest
{
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}

public class ReserveStockRequest
{
    public Guid OrderId { get; set; }
    public List<ReserveStockItemRequest> Items { get; set; } = [];
}

public class ReserveStockItemRequest
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; }
}

#endregion
