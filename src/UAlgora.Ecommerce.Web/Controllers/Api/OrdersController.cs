using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using static UAlgora.Ecommerce.Web.ServiceCollectionExtensions;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for order operations.
/// </summary>
public class OrdersController : EcommerceApiController
{
    private readonly IOrderService _orderService;
    private readonly ICartContextProvider _contextProvider;

    public OrdersController(
        IOrderService orderService,
        ICartContextProvider contextProvider)
    {
        _orderService = orderService;
        _contextProvider = contextProvider;
    }

    /// <summary>
    /// Gets orders for the current customer.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "You must be logged in to view orders." });
        }

        var orders = await _orderService.GetByCustomerIdAsync(customerId.Value, ct);
        return ApiSuccess(orders);
    }

    /// <summary>
    /// Gets an order by ID (for authenticated customer).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Order not found." });
        }

        // Verify ownership if customer is logged in
        var customerId = _contextProvider.GetCustomerId();
        if (customerId.HasValue && order.CustomerId != customerId.Value)
        {
            return Forbid();
        }

        return ApiSuccess(order);
    }

    /// <summary>
    /// Gets an order by order number.
    /// </summary>
    [HttpGet("by-number/{orderNumber}")]
    public async Task<IActionResult> GetOrderByNumber(string orderNumber, CancellationToken ct = default)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber, ct);
        if (order == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Order not found." });
        }

        // Verify ownership if customer is logged in
        var customerId = _contextProvider.GetCustomerId();
        if (customerId.HasValue && order.CustomerId != customerId.Value)
        {
            return Forbid();
        }

        return ApiSuccess(order);
    }

    /// <summary>
    /// Looks up an order by order number and email (for guest orders).
    /// </summary>
    [HttpPost("lookup")]
    public async Task<IActionResult> LookupOrder(
        [FromBody] OrderLookupRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.OrderNumber) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new ApiErrorResponse { Message = "Order number and email are required." });
        }

        var order = await _orderService.GetByOrderNumberAsync(request.OrderNumber, ct);
        if (order == null || !order.CustomerEmail.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new ApiErrorResponse { Message = "Order not found." });
        }

        return ApiSuccess(order);
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest? request,
        CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Order not found." });
        }

        // Verify ownership
        var customerId = _contextProvider.GetCustomerId();
        if (customerId.HasValue && order.CustomerId != customerId.Value)
        {
            return Forbid();
        }

        var canCancel = await _orderService.CanCancelAsync(id, ct);
        if (!canCancel)
        {
            return ApiError("This order cannot be cancelled.");
        }

        try
        {
            var updatedOrder = await _orderService.CancelAsync(id, request?.Reason, ct);
            return ApiSuccess(updatedOrder, "Order cancelled successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Tracks order shipment.
    /// </summary>
    [HttpGet("{id:guid}/tracking")]
    public async Task<IActionResult> GetTracking(Guid id, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Order not found." });
        }

        // Verify ownership
        var customerId = _contextProvider.GetCustomerId();
        if (customerId.HasValue && order.CustomerId != customerId.Value)
        {
            return Forbid();
        }

        var tracking = new
        {
            order.Status,
            order.FulfillmentStatus,
            order.TrackingNumber,
            order.Carrier,
            order.ShippedAt,
            order.DeliveredAt,
            order.EstimatedDeliveryDate
        };

        return ApiSuccess(tracking);
    }
}

#region Admin Endpoints

/// <summary>
/// Admin API controller for order management.
/// </summary>
[Route("api/ecommerce/admin/orders")]
[Authorize(Policy = EcommerceAdminPolicy)]
public class OrdersAdminController : EcommerceApiController
{
    private readonly IOrderService _orderService;

    public OrdersAdminController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Gets paginated orders (admin).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var parameters = new OrderQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            SearchTerm = search,
            StartDate = fromDate,
            EndDate = toDate
        };

        var result = await _orderService.GetPagedAsync(parameters, ct);
        return ApiPaged(result.Items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <summary>
    /// Gets recent orders (admin).
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentOrders(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var orders = await _orderService.GetRecentAsync(count, ct);
        return ApiSuccess(orders);
    }

    /// <summary>
    /// Gets orders by status (admin).
    /// </summary>
    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(
        OrderStatus status,
        CancellationToken ct = default)
    {
        var orders = await _orderService.GetByStatusAsync(status, ct);
        return ApiSuccess(orders);
    }

    /// <summary>
    /// Gets order counts by status (admin).
    /// </summary>
    [HttpGet("counts")]
    public async Task<IActionResult> GetOrderCounts(CancellationToken ct = default)
    {
        var counts = await _orderService.GetCountByStatusAsync(ct);
        return ApiSuccess(counts);
    }

    /// <summary>
    /// Gets order statistics (admin).
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var stats = await _orderService.GetStatisticsAsync(startDate, endDate, ct);
        return ApiSuccess(stats);
    }

    /// <summary>
    /// Updates order status (admin).
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var order = await _orderService.UpdateStatusAsync(id, request.Status, request.Note, ct);
            return ApiSuccess(order, "Order status updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Confirms an order (admin).
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmOrder(Guid id, CancellationToken ct = default)
    {
        try
        {
            var order = await _orderService.ConfirmAsync(id, ct);
            return ApiSuccess(order, "Order confirmed.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Marks order as paid (admin).
    /// </summary>
    [HttpPost("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(
        Guid id,
        [FromBody] MarkPaidRequest? request,
        CancellationToken ct = default)
    {
        try
        {
            var order = await _orderService.MarkAsPaidAsync(id, request?.TransactionId, ct);
            return ApiSuccess(order, "Order marked as paid.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Marks order as shipped (admin).
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> ShipOrder(
        Guid id,
        [FromBody] ShipOrderRequest? request,
        CancellationToken ct = default)
    {
        try
        {
            var order = await _orderService.MarkAsShippedAsync(
                id,
                request?.TrackingNumber,
                request?.Carrier,
                ct);
            return ApiSuccess(order, "Order shipped.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Marks order as delivered (admin).
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> DeliverOrder(Guid id, CancellationToken ct = default)
    {
        try
        {
            var order = await _orderService.MarkAsDeliveredAsync(id, ct);
            return ApiSuccess(order, "Order marked as delivered.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Marks order as completed (admin).
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteOrder(Guid id, CancellationToken ct = default)
    {
        try
        {
            var order = await _orderService.MarkAsCompletedAsync(id, ct);
            return ApiSuccess(order, "Order completed.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Adds a note to an order (admin).
    /// </summary>
    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(
        Guid id,
        [FromBody] AddOrderNoteRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return BadRequest(new ApiErrorResponse { Message = "Note is required." });
        }

        try
        {
            var order = await _orderService.AddNoteAsync(id, request.Note, request.IsInternal, ct);
            return ApiSuccess(order, "Note added.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }
}

#endregion

#region Request Models

public class OrderLookupRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
}

public class MarkPaidRequest
{
    public string? TransactionId { get; set; }
}

public class ShipOrderRequest
{
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
}

public class AddOrderNoteRequest
{
    public string Note { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = true;
}

#endregion
