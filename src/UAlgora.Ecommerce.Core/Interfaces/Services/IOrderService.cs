using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for order operations.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets an order by order number.
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated orders.
    /// </summary>
    Task<PagedResult<Order>> GetPagedAsync(OrderQueryParameters parameters, CancellationToken ct = default);

    /// <summary>
    /// Gets orders by customer ID.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets orders by customer email.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByCustomerEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets recent orders.
    /// </summary>
    Task<IReadOnlyList<Order>> GetRecentAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets orders by status.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default);

    /// <summary>
    /// Creates a new order from a cart.
    /// </summary>
    Task<Order> CreateFromCartAsync(Cart cart, CreateOrderRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an order.
    /// </summary>
    Task<Order> UpdateAsync(Order order, CancellationToken ct = default);

    /// <summary>
    /// Updates order status.
    /// </summary>
    Task<Order> UpdateStatusAsync(Guid orderId, OrderStatus status, string? note = null, CancellationToken ct = default);

    /// <summary>
    /// Confirms an order.
    /// </summary>
    Task<Order> ConfirmAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Cancels an order.
    /// </summary>
    Task<Order> CancelAsync(Guid orderId, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Marks an order as paid.
    /// </summary>
    Task<Order> MarkAsPaidAsync(Guid orderId, string? transactionId = null, CancellationToken ct = default);

    /// <summary>
    /// Marks an order as shipped.
    /// </summary>
    Task<Order> MarkAsShippedAsync(Guid orderId, string? trackingNumber = null, string? carrier = null, CancellationToken ct = default);

    /// <summary>
    /// Marks an order as delivered.
    /// </summary>
    Task<Order> MarkAsDeliveredAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Marks an order as completed.
    /// </summary>
    Task<Order> MarkAsCompletedAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Adds a note to an order.
    /// </summary>
    Task<Order> AddNoteAsync(Guid orderId, string note, bool isInternal = true, CancellationToken ct = default);

    /// <summary>
    /// Gets order count by status.
    /// </summary>
    Task<Dictionary<OrderStatus, int>> GetCountByStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets order statistics.
    /// </summary>
    Task<OrderStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);

    /// <summary>
    /// Sends order confirmation email.
    /// </summary>
    Task SendConfirmationEmailAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Sends shipping notification email.
    /// </summary>
    Task SendShippingNotificationAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Can the order be cancelled.
    /// </summary>
    Task<bool> CanCancelAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Can the order be refunded.
    /// </summary>
    Task<bool> CanRefundAsync(Guid orderId, CancellationToken ct = default);
}

/// <summary>
/// Request to create an order.
/// </summary>
public class CreateOrderRequest
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }
    public Guid? CustomerId { get; set; }
    public Address ShippingAddress { get; set; } = new();
    public Address? BillingAddress { get; set; }
    public bool BillingSameAsShipping { get; set; } = true;
    public string? ShippingMethod { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? CustomerNote { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Order statistics.
/// </summary>
public class OrderStatistics
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
