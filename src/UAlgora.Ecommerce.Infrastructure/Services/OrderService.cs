using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for order operations.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IInventoryService _inventoryService;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IInventoryService inventoryService)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _inventoryService = inventoryService;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _orderRepository.GetWithDetailsAsync(id, ct);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _orderRepository.GetByOrderNumberAsync(orderNumber, ct);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(
        OrderQueryParameters parameters,
        CancellationToken ct = default)
    {
        return await _orderRepository.GetPagedAsync(parameters, ct);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _orderRepository.GetByCustomerIdAsync(customerId, ct);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerEmailAsync(string email, CancellationToken ct = default)
    {
        return await _orderRepository.GetByCustomerEmailAsync(email, ct);
    }

    public async Task<IReadOnlyList<Order>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        return await _orderRepository.GetRecentAsync(count, ct);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default)
    {
        return await _orderRepository.GetByStatusAsync(status, ct);
    }

    public async Task<Order> CreateFromCartAsync(
        Cart cart,
        CreateOrderRequest request,
        CancellationToken ct = default)
    {
        // Generate order number
        var orderNumber = await _orderRepository.GenerateOrderNumberAsync(ct);

        // Create order from cart
        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            CartId = cart.Id,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            CustomerName = request.CustomerName,
            CurrencyCode = cart.CurrencyCode,
            Subtotal = cart.Subtotal,
            DiscountTotal = cart.DiscountTotal,
            ShippingTotal = cart.ShippingTotal,
            TaxTotal = cart.TaxTotal,
            GrandTotal = cart.GrandTotal,
            AppliedDiscounts = cart.AppliedDiscounts,
            CouponCode = cart.CouponCode,
            ShippingMethod = request.ShippingMethod ?? cart.SelectedShippingMethod,
            ShippingMethodName = cart.SelectedShippingMethodName,
            PaymentMethod = request.PaymentMethod,
            PaymentIntentId = request.PaymentIntentId,
            CustomerNote = request.CustomerNote,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Source = request.Source,
            BillingSameAsShipping = request.BillingSameAsShipping,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            FulfillmentStatus = FulfillmentStatus.Unfulfilled,
            PlacedAt = DateTime.UtcNow
        };

        // Create order lines from cart items
        foreach (var item in cart.Items)
        {
            order.Lines.Add(new OrderLine
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                ProductName = item.ProductName,
                Sku = item.Sku,
                VariantName = item.VariantName,
                VariantOptions = item.VariantOptions,
                ImageUrl = item.ImageUrl,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                OriginalPrice = item.OriginalPrice,
                LineTotal = item.LineTotal,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount,
                Weight = item.Weight
            });
        }

        // Save order
        order = await _orderRepository.AddAsync(order, ct);

        // Reserve inventory
        var reservationItems = order.Lines.Select(l => new StockReservationItem
        {
            ProductId = l.ProductId,
            VariantId = l.VariantId,
            Quantity = l.Quantity
        });

        await _inventoryService.ReserveStockAsync(order.Id, reservationItems, ct);

        // Update customer statistics if logged in
        if (request.CustomerId.HasValue)
        {
            await _customerRepository.UpdateStatisticsAsync(request.CustomerId.Value, ct);
        }

        return order;
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken ct = default)
    {
        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order> UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        string? note = null,
        CancellationToken ct = default)
    {
        await _orderRepository.UpdateStatusAsync(orderId, status, ct);

        if (!string.IsNullOrWhiteSpace(note))
        {
            await AddNoteAsync(orderId, note, true, ct);
        }

        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        return order!;
    }

    public async Task<Order> ConfirmAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found.");
        }

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;

        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order> CancelAsync(Guid orderId, string? reason = null, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found.");
        }

        if (!order.CanCancel)
        {
            throw new InvalidOperationException("Order cannot be cancelled in its current state.");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;

        // Release inventory reservation
        // TODO: Implement reservation tracking and release

        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order> MarkAsPaidAsync(
        Guid orderId,
        string? transactionId = null,
        CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found.");
        }

        order.PaymentStatus = PaymentStatus.Captured;
        order.PaidAmount = order.GrandTotal;
        order.PaidAt = DateTime.UtcNow;

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Processing;
        }

        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order> MarkAsShippedAsync(
        Guid orderId,
        string? trackingNumber = null,
        string? carrier = null,
        CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found.");
        }

        order.Status = OrderStatus.Shipped;
        order.TrackingNumber = trackingNumber;
        order.Carrier = carrier;
        order.ShippedAt = DateTime.UtcNow;
        order.FulfillmentStatus = FulfillmentStatus.Fulfilled;

        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order> MarkAsDeliveredAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found.");
        }

        order.Status = OrderStatus.Delivered;
        order.DeliveredAt = DateTime.UtcNow;

        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order> MarkAsCompletedAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found.");
        }

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order> AddNoteAsync(
        Guid orderId,
        string note,
        bool isInternal = true,
        CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found.");
        }

        if (isInternal)
        {
            order.InternalNote = string.IsNullOrEmpty(order.InternalNote)
                ? $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {note}"
                : $"{order.InternalNote}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {note}";
        }
        else
        {
            order.CustomerNote = string.IsNullOrEmpty(order.CustomerNote)
                ? note
                : $"{order.CustomerNote}\n{note}";
        }

        return await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Dictionary<OrderStatus, int>> GetCountByStatusAsync(CancellationToken ct = default)
    {
        return await _orderRepository.GetCountByStatusAsync(ct);
    }

    public async Task<OrderStatistics> GetStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var totalRevenue = await _orderRepository.GetTotalRevenueAsync(startDate, endDate, ct);
        var averageOrderValue = await _orderRepository.GetAverageOrderValueAsync(startDate, endDate, ct);
        var countByStatus = await _orderRepository.GetCountByStatusAsync(ct);

        return new OrderStatistics
        {
            TotalOrders = countByStatus.Values.Sum(),
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            PendingOrders = countByStatus.GetValueOrDefault(OrderStatus.Pending),
            ProcessingOrders = countByStatus.GetValueOrDefault(OrderStatus.Processing),
            ShippedOrders = countByStatus.GetValueOrDefault(OrderStatus.Shipped),
            CompletedOrders = countByStatus.GetValueOrDefault(OrderStatus.Completed),
            CancelledOrders = countByStatus.GetValueOrDefault(OrderStatus.Cancelled),
            RefundedAmount = 0 // TODO: Calculate from refund records
        };
    }

    public Task SendConfirmationEmailAsync(Guid orderId, CancellationToken ct = default)
    {
        // TODO: Implement email service integration
        return Task.CompletedTask;
    }

    public Task SendShippingNotificationAsync(Guid orderId, CancellationToken ct = default)
    {
        // TODO: Implement email service integration
        return Task.CompletedTask;
    }

    public async Task<bool> CanCancelAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        return order?.CanCancel ?? false;
    }

    public async Task<bool> CanRefundAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        return order?.CanRefund ?? false;
    }
}
