using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for order operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Orders}")]
public class OrderManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IOrderService _orderService;

    public OrderManagementApiController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Gets the tree structure for orders organized by status.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType<OrderTreeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var counts = await _orderService.GetCountByStatusAsync();
        var nodes = new List<OrderTreeNodeModel>();

        var statuses = new (OrderStatus Status, string Name, string Icon)[]
        {
            (OrderStatus.Pending, "Pending", "icon-time"),
            (OrderStatus.Confirmed, "Confirmed", "icon-check"),
            (OrderStatus.Processing, "Processing", "icon-loading"),
            (OrderStatus.Shipped, "Shipped", "icon-truck"),
            (OrderStatus.Delivered, "Delivered", "icon-home"),
            (OrderStatus.Completed, "Completed", "icon-checkbox-dotted"),
            (OrderStatus.Cancelled, "Cancelled", "icon-delete"),
            (OrderStatus.Refunded, "Refunded", "icon-coin-dollar")
        };

        foreach (var (status, name, icon) in statuses)
        {
            var count = counts.TryGetValue(status, out var c) ? c : 0;
            var cssClasses = new List<string>();

            if (status == OrderStatus.Pending && count > 0)
            {
                cssClasses.Add("has-pending");
            }

            nodes.Add(new OrderTreeNodeModel
            {
                Id = $"status-{(int)status}",
                Name = name,
                Icon = GetStatusIcon(status),
                Status = status,
                Count = count,
                HasChildren = count > 0,
                CssClasses = cssClasses
            });
        }

        // Add "All Orders" node
        var totalCount = counts.Values.Sum();
        nodes.Add(new OrderTreeNodeModel
        {
            Id = "all-orders",
            Name = "All Orders",
            Icon = EcommerceConstants.Icons.Orders,
            Status = null,
            Count = totalCount,
            HasChildren = totalCount > 0
        });

        return Ok(new OrderTreeResponse { Nodes = nodes });
    }

    /// <summary>
    /// Gets orders for a specific status tree node.
    /// </summary>
    [HttpGet("tree/{nodeId}/children")]
    [ProducesResponseType<OrderListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTreeChildren(string nodeId, [FromQuery] int take = 50)
    {
        OrderStatus? statusFilter = null;
        if (nodeId.StartsWith("status-") && int.TryParse(nodeId.Replace("status-", ""), out var statusInt))
        {
            statusFilter = (OrderStatus)statusInt;
        }

        var orders = statusFilter.HasValue
            ? await _orderService.GetByStatusAsync(statusFilter.Value)
            : await _orderService.GetRecentAsync(take);

        var items = orders.Take(take).Select(MapToOrderItem).ToList();

        return Ok(new OrderListResponse
        {
            Items = items,
            Total = items.Count,
            Skip = 0,
            Take = take
        });
    }

    /// <summary>
    /// Gets a paged list of orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<OrderListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] OrderStatus? status = null)
    {
        IEnumerable<Core.Models.Domain.Order> orders;

        if (status.HasValue)
        {
            orders = await _orderService.GetByStatusAsync(status.Value);
        }
        else
        {
            orders = await _orderService.GetRecentAsync(skip + take);
        }

        var items = orders.Skip(skip).Take(take).Select(MapToOrderItem).ToList();

        return Ok(new OrderListResponse
        {
            Items = items,
            Total = items.Count,
            Skip = skip,
            Take = take
        });
    }

    /// <summary>
    /// Gets a single order by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(MapToOrderDetail(order));
    }

    /// <summary>
    /// Updates the order status.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            return BadRequest(new { message = "Invalid status value" });
        }

        var updated = await _orderService.UpdateStatusAsync(id, newStatus, request.Note);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Marks an order as shipped.
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsShipped(Guid id, [FromBody] ShipOrderRequest? request = null)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var updated = await _orderService.MarkAsShippedAsync(id, request?.TrackingNumber, request?.Carrier);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Marks an order as delivered.
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsDelivered(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var updated = await _orderService.MarkAsDeliveredAsync(id);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Marks an order as completed.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsCompleted(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var updated = await _orderService.MarkAsCompletedAsync(id);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest? request = null)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var canCancel = await _orderService.CanCancelAsync(id);
        if (!canCancel)
        {
            return BadRequest(new { message = "This order cannot be cancelled" });
        }

        var updated = await _orderService.CancelAsync(id, request?.Reason);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Adds a note to an order.
    /// </summary>
    [HttpPost("{id:guid}/note")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddOrderNoteRequest request)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return BadRequest(new { message = "Note content is required" });
        }

        var updated = await _orderService.AddNoteAsync(id, request.Note, request.IsInternal);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Puts an order on hold.
    /// </summary>
    [HttpPost("{id:guid}/hold")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HoldOrder(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = OrderStatus.OnHold;
        var updated = await _orderService.UpdateAsync(order);
        await _orderService.AddNoteAsync(id, "Order placed on hold", true);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Releases an order from hold.
    /// </summary>
    [HttpPost("{id:guid}/release")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseOrder(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.OnHold)
        {
            return BadRequest(new { message = "Order is not on hold" });
        }

        order.Status = OrderStatus.Pending;
        var updated = await _orderService.UpdateAsync(order);
        await _orderService.AddNoteAsync(id, "Order released from hold", true);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Gets order data for printing/export.
    /// </summary>
    [HttpGet("{id:guid}/print")]
    [ProducesResponseType<OrderPrintModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrintData(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(new OrderPrintModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            CustomerName = order.CustomerName ?? $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}".Trim(),
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            BillingAddress = MapAddress(order.BillingAddress),
            ShippingAddress = MapAddress(order.ShippingAddress),
            Lines = order.Lines.Select(MapOrderLine).ToList(),
            Subtotal = order.Subtotal,
            DiscountTotal = order.DiscountTotal,
            ShippingTotal = order.ShippingTotal,
            TaxTotal = order.TaxTotal,
            GrandTotal = order.GrandTotal,
            CurrencyCode = order.CurrencyCode,
            PaymentMethod = order.PaymentMethod,
            ShippingMethod = order.ShippingMethodName,
            CustomerNote = order.CustomerNote,
            CreatedAt = order.CreatedAt,
            PrintedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Marks an order as confirmed.
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmOrder(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.Pending)
        {
            return BadRequest(new { message = "Only pending orders can be confirmed" });
        }

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        var updated = await _orderService.UpdateAsync(order);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Marks an order as processing.
    /// </summary>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessOrder(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = OrderStatus.Processing;
        var updated = await _orderService.UpdateAsync(order);
        return Ok(MapToOrderDetail(updated));
    }

    /// <summary>
    /// Gets order statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType<OrderStatisticsModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var stats = await _orderService.GetStatisticsAsync(startDate, endDate);

        return Ok(new OrderStatisticsModel
        {
            TotalOrders = stats.TotalOrders,
            TotalRevenue = stats.TotalRevenue,
            AverageOrderValue = stats.AverageOrderValue,
            PendingOrders = stats.PendingOrders,
            ProcessingOrders = stats.ProcessingOrders,
            ShippedOrders = stats.ShippedOrders,
            CompletedOrders = stats.CompletedOrders,
            CancelledOrders = stats.CancelledOrders,
            RefundedAmount = stats.RefundedAmount
        });
    }

    #region Refunds

    /// <summary>
    /// Checks if an order can be refunded.
    /// </summary>
    [HttpGet("{id:guid}/can-refund")]
    [ProducesResponseType<RefundEligibilityModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CanRefund(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var canRefund = await _orderService.CanRefundAsync(id);
        var refundableAmount = order.PaidAmount - order.RefundedAmount;

        return Ok(new RefundEligibilityModel
        {
            CanRefund = canRefund,
            RefundableAmount = refundableAmount,
            PaidAmount = order.PaidAmount,
            RefundedAmount = order.RefundedAmount,
            CurrencyCode = order.CurrencyCode
        });
    }

    /// <summary>
    /// Processes a refund for an order.
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [ProducesResponseType<OrderDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundOrder(Guid id, [FromBody] RefundOrderRequest request)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var canRefund = await _orderService.CanRefundAsync(id);
        if (!canRefund)
        {
            return BadRequest(new { message = "This order cannot be refunded" });
        }

        var maxRefundable = order.PaidAmount - order.RefundedAmount;
        var refundAmount = request.Amount ?? maxRefundable;

        if (refundAmount <= 0 || refundAmount > maxRefundable)
        {
            return BadRequest(new { message = $"Invalid refund amount. Maximum refundable: {maxRefundable}" });
        }

        // Update order refunded amount
        order.RefundedAmount += refundAmount;

        if (order.RefundedAmount >= order.PaidAmount)
        {
            order.PaymentStatus = PaymentStatus.Refunded;
            order.Status = OrderStatus.Refunded;
        }
        else
        {
            order.PaymentStatus = PaymentStatus.PartiallyRefunded;
        }

        var updated = await _orderService.UpdateAsync(order);

        // Add note about refund
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            await _orderService.AddNoteAsync(id, $"Refund of {order.CurrencyCode} {refundAmount:F2}: {request.Reason}", true);
        }
        else
        {
            await _orderService.AddNoteAsync(id, $"Refund of {order.CurrencyCode} {refundAmount:F2} processed", true);
        }

        return Ok(MapToOrderDetail(updated));
    }

    #endregion

    #region Shipments

    /// <summary>
    /// Gets shipments for an order.
    /// </summary>
    [HttpGet("{id:guid}/shipments")]
    [ProducesResponseType<List<ShipmentModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderShipments(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var shipments = order.Shipments.Select(s => new ShipmentModel
        {
            Id = s.Id,
            ShipmentNumber = s.ShipmentNumber,
            Status = s.Status.ToString(),
            Carrier = s.Carrier,
            CarrierCode = s.CarrierCode,
            Service = s.Service,
            TrackingNumber = s.TrackingNumber,
            TrackingUrl = s.TrackingUrl,
            ShippingCost = s.ShippingCost,
            LabelUrl = s.LabelUrl,
            EstimatedDeliveryDate = s.EstimatedDeliveryDate,
            ShippedAt = s.ShippedAt,
            DeliveredAt = s.DeliveredAt,
            TotalItems = s.TotalItems,
            Items = s.Items.Select(i => new ShipmentItemModel
            {
                Id = i.Id,
                OrderLineId = i.OrderLineId,
                ProductName = i.OrderLine?.ProductName ?? "Unknown",
                Sku = i.OrderLine?.Sku ?? "",
                Quantity = i.Quantity
            }).ToList(),
            CreatedAt = s.CreatedAt
        }).ToList();

        return Ok(shipments);
    }

    /// <summary>
    /// Creates a shipment for an order.
    /// </summary>
    [HttpPost("{id:guid}/shipment")]
    [ProducesResponseType<ShipmentModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateShipment(Guid id, [FromBody] CreateShipmentRequest request)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        // Validate items
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "At least one item must be included in the shipment" });
        }

        foreach (var item in request.Items)
        {
            var orderLine = order.Lines.FirstOrDefault(l => l.Id == item.OrderLineId);
            if (orderLine == null)
            {
                return BadRequest(new { message = $"Order line {item.OrderLineId} not found" });
            }

            var remainingQuantity = orderLine.Quantity - orderLine.FulfilledQuantity;
            if (item.Quantity > remainingQuantity)
            {
                return BadRequest(new { message = $"Cannot ship {item.Quantity} of {orderLine.ProductName}. Only {remainingQuantity} remaining." });
            }
        }

        // Create shipment
        var shipmentNumber = $"SHP-{order.OrderNumber}-{order.Shipments.Count + 1}";
        var shipment = new Core.Models.Domain.Shipment
        {
            OrderId = order.Id,
            ShipmentNumber = shipmentNumber,
            Status = ShipmentStatus.Pending,
            Carrier = request.Carrier,
            CarrierCode = request.CarrierCode,
            Service = request.Service,
            TrackingNumber = request.TrackingNumber,
            TrackingUrl = request.TrackingUrl,
            Weight = request.Weight,
            WeightUnit = request.WeightUnit ?? "kg",
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            Notes = request.Notes
        };

        // Add items
        foreach (var item in request.Items)
        {
            shipment.Items.Add(new Core.Models.Domain.ShipmentItem
            {
                OrderLineId = item.OrderLineId,
                Quantity = item.Quantity
            });

            // Update fulfilled quantity on order line
            var orderLine = order.Lines.First(l => l.Id == item.OrderLineId);
            orderLine.FulfilledQuantity += item.Quantity;
        }

        order.Shipments.Add(shipment);

        // Update order fulfillment status
        var totalFulfilled = order.Lines.Sum(l => l.FulfilledQuantity);
        var totalOrdered = order.Lines.Sum(l => l.Quantity);

        if (totalFulfilled >= totalOrdered)
        {
            order.FulfillmentStatus = FulfillmentStatus.Fulfilled;
        }
        else if (totalFulfilled > 0)
        {
            order.FulfillmentStatus = FulfillmentStatus.PartiallyFulfilled;
        }

        await _orderService.UpdateAsync(order);

        return Ok(new ShipmentModel
        {
            Id = shipment.Id,
            ShipmentNumber = shipment.ShipmentNumber,
            Status = shipment.Status.ToString(),
            Carrier = shipment.Carrier,
            TrackingNumber = shipment.TrackingNumber,
            TotalItems = shipment.TotalItems,
            CreatedAt = shipment.CreatedAt
        });
    }

    /// <summary>
    /// Updates a shipment status (e.g., mark as shipped, delivered).
    /// </summary>
    [HttpPut("{orderId:guid}/shipment/{shipmentId:guid}")]
    [ProducesResponseType<ShipmentModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShipment(Guid orderId, Guid shipmentId, [FromBody] UpdateShipmentRequest request)
    {
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        var shipment = order.Shipments.FirstOrDefault(s => s.Id == shipmentId);
        if (shipment == null)
        {
            return NotFound(new { message = "Shipment not found" });
        }

        if (!string.IsNullOrEmpty(request.TrackingNumber))
        {
            shipment.TrackingNumber = request.TrackingNumber;
        }

        if (!string.IsNullOrEmpty(request.TrackingUrl))
        {
            shipment.TrackingUrl = request.TrackingUrl;
        }

        if (!string.IsNullOrEmpty(request.Carrier))
        {
            shipment.Carrier = request.Carrier;
        }

        if (Enum.TryParse<ShipmentStatus>(request.Status, out var newStatus))
        {
            shipment.Status = newStatus;

            // Mark as shipped when status indicates package is moving
            if ((newStatus == ShipmentStatus.PickedUp || newStatus == ShipmentStatus.InTransit) && !shipment.ShippedAt.HasValue)
            {
                shipment.ShippedAt = DateTime.UtcNow;
            }
            else if (newStatus == ShipmentStatus.Delivered && !shipment.DeliveredAt.HasValue)
            {
                shipment.DeliveredAt = DateTime.UtcNow;
            }
        }

        await _orderService.UpdateAsync(order);

        return Ok(new ShipmentModel
        {
            Id = shipment.Id,
            ShipmentNumber = shipment.ShipmentNumber,
            Status = shipment.Status.ToString(),
            Carrier = shipment.Carrier,
            TrackingNumber = shipment.TrackingNumber,
            TrackingUrl = shipment.TrackingUrl,
            ShippedAt = shipment.ShippedAt,
            DeliveredAt = shipment.DeliveredAt,
            TotalItems = shipment.TotalItems,
            CreatedAt = shipment.CreatedAt
        });
    }

    #endregion

    #region Payments

    /// <summary>
    /// Gets payments for an order.
    /// </summary>
    [HttpGet("{id:guid}/payments")]
    [ProducesResponseType<List<PaymentModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderPayments(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        var payments = order.Payments.Select(p => new PaymentModel
        {
            Id = p.Id,
            Status = p.Status.ToString(),
            MethodType = p.MethodType.ToString(),
            Provider = p.Provider,
            MethodName = p.MethodName,
            Amount = p.Amount,
            CurrencyCode = p.CurrencyCode,
            TransactionId = p.TransactionId,
            IsRefund = p.IsRefund,
            CardBrand = p.CardBrand,
            CardLast4 = p.CardLast4,
            ErrorMessage = p.ErrorMessage,
            CapturedAt = p.CapturedAt,
            RefundedAt = p.RefundedAt,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Ok(payments);
    }

    #endregion

    private static string GetStatusIcon(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "icon-time color-yellow",
        OrderStatus.Confirmed => "icon-check color-blue",
        OrderStatus.Processing => "icon-loading color-blue",
        OrderStatus.Shipped => "icon-truck color-blue",
        OrderStatus.Delivered => "icon-home color-green",
        OrderStatus.Completed => "icon-checkbox-dotted color-green",
        OrderStatus.Cancelled => "icon-delete color-red",
        OrderStatus.Refunded => "icon-coin-dollar color-orange",
        OrderStatus.OnHold => "icon-pause color-grey",
        OrderStatus.Failed => "icon-alert color-red",
        _ => EcommerceConstants.Icons.Order
    };

    private static OrderItemModel MapToOrderItem(Core.Models.Domain.Order order)
    {
        return new OrderItemModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            StatusIcon = GetStatusIcon(order.Status),
            CustomerName = order.CustomerName ?? $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}".Trim(),
            CustomerEmail = order.CustomerEmail,
            Subtotal = order.Subtotal,
            TaxTotal = order.TaxTotal,
            ShippingTotal = order.ShippingTotal,
            DiscountTotal = order.DiscountTotal,
            GrandTotal = order.GrandTotal,
            CurrencyCode = order.CurrencyCode,
            ItemCount = order.Lines.Count,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    private static OrderDetailModel MapToOrderDetail(Core.Models.Domain.Order order)
    {
        return new OrderDetailModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            StatusIcon = GetStatusIcon(order.Status),
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName ?? $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}".Trim(),
            CustomerEmail = order.CustomerEmail,
            Subtotal = order.Subtotal,
            TaxTotal = order.TaxTotal,
            ShippingTotal = order.ShippingTotal,
            DiscountTotal = order.DiscountTotal,
            GrandTotal = order.GrandTotal,
            CurrencyCode = order.CurrencyCode,
            ItemCount = order.Lines.Count,
            PaymentStatus = order.PaymentStatus.ToString(),
            PaymentMethod = order.PaymentMethod,
            CustomerNote = order.CustomerNote,
            InternalNote = order.InternalNote,
            BillingAddress = MapAddress(order.BillingAddress),
            ShippingAddress = MapAddress(order.ShippingAddress),
            Lines = order.Lines.Select(MapOrderLine).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    private static AddressModel? MapAddress(Core.Models.Domain.Address? address)
    {
        if (address == null) return null;

        return new AddressModel
        {
            FirstName = address.FirstName,
            LastName = address.LastName,
            Company = address.Company,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            StateProvince = address.StateProvince,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Phone = address.Phone
        };
    }

    private static OrderLineModel MapOrderLine(Core.Models.Domain.OrderLine line)
    {
        return new OrderLineModel
        {
            Id = line.Id,
            ProductId = line.ProductId,
            VariantId = line.VariantId,
            ProductName = line.ProductName,
            VariantName = line.VariantName,
            Sku = line.Sku,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            DiscountAmount = line.DiscountAmount,
            TaxAmount = line.TaxAmount,
            LineTotal = line.LineTotal,
            FinalLineTotal = line.FinalLineTotal,
            ImageUrl = line.ImageUrl
        };
    }
}

#region Response Models

public class OrderTreeResponse
{
    public List<OrderTreeNodeModel> Nodes { get; set; } = [];
}

public class OrderTreeNodeModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public OrderStatus? Status { get; set; }
    public int Count { get; set; }
    public bool HasChildren { get; set; }
    public List<string> CssClasses { get; set; } = [];
}

public class OrderListResponse
{
    public List<OrderItemModel> Items { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class OrderItemModel
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string Status { get; set; }
    public required string StatusIcon { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public required string CurrencyCode { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OrderDetailModel : OrderItemModel
{
    public Guid? CustomerId { get; set; }
    public required string PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    public AddressModel? BillingAddress { get; set; }
    public AddressModel? ShippingAddress { get; set; }
    public List<OrderLineModel> Lines { get; set; } = [];
}

public class AddressModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Company { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? StateProvince { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
    public string? Phone { get; set; }
}

public class OrderLineModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public required string ProductName { get; set; }
    public string? VariantName { get; set; }
    public required string Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal FinalLineTotal { get; set; }
    public string? ImageUrl { get; set; }
}

public class OrderStatisticsModel
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal RefundedAmount { get; set; }
}

public class RefundEligibilityModel
{
    public bool CanRefund { get; set; }
    public decimal RefundableAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public required string CurrencyCode { get; set; }
}

public class ShipmentModel
{
    public Guid Id { get; set; }
    public string? ShipmentNumber { get; set; }
    public required string Status { get; set; }
    public string? Carrier { get; set; }
    public string? CarrierCode { get; set; }
    public string? Service { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public decimal ShippingCost { get; set; }
    public string? LabelUrl { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int TotalItems { get; set; }
    public List<ShipmentItemModel> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class ShipmentItemModel
{
    public Guid Id { get; set; }
    public Guid OrderLineId { get; set; }
    public required string ProductName { get; set; }
    public required string Sku { get; set; }
    public int Quantity { get; set; }
}

public class PaymentModel
{
    public Guid Id { get; set; }
    public required string Status { get; set; }
    public required string MethodType { get; set; }
    public required string Provider { get; set; }
    public string? MethodName { get; set; }
    public decimal Amount { get; set; }
    public required string CurrencyCode { get; set; }
    public string? TransactionId { get; set; }
    public bool IsRefund { get; set; }
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderPrintModel
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string Status { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public AddressModel? BillingAddress { get; set; }
    public AddressModel? ShippingAddress { get; set; }
    public List<OrderLineModel> Lines { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public required string CurrencyCode { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ShippingMethod { get; set; }
    public string? CustomerNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PrintedAt { get; set; }
}

#endregion

#region Request Models

public class UpdateOrderStatusRequest
{
    public required string Status { get; set; }
    public string? Note { get; set; }
}

public class ShipOrderRequest
{
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
}

public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

public class AddOrderNoteRequest
{
    public required string Note { get; set; }
    public bool IsInternal { get; set; } = true;
}

public class RefundOrderRequest
{
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}

public class CreateShipmentRequest
{
    public string? Carrier { get; set; }
    public string? CarrierCode { get; set; }
    public string? Service { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateShipmentItemRequest> Items { get; set; } = [];
}

public class CreateShipmentItemRequest
{
    public Guid OrderLineId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateShipmentRequest
{
    public string? Status { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
}

#endregion
