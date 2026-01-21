using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for order operations.
/// </summary>
public interface IOrderRepository : ISoftDeleteRepository<Order>
{
    /// <summary>
    /// Gets an order by order number.
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);

    /// <summary>
    /// Gets an order with all related data loaded.
    /// </summary>
    Task<Order?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated orders with filtering and sorting.
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
    /// Gets orders by status.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets orders by payment status.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByPaymentStatusAsync(PaymentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets orders by fulfillment status.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByFulfillmentStatusAsync(FulfillmentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets orders created within a date range.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Gets recent orders.
    /// </summary>
    Task<IReadOnlyList<Order>> GetRecentAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets pending orders requiring attention.
    /// </summary>
    Task<IReadOnlyList<Order>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets orders ready for fulfillment.
    /// </summary>
    Task<IReadOnlyList<Order>> GetReadyForFulfillmentAsync(CancellationToken ct = default);

    /// <summary>
    /// Generates a new unique order number.
    /// </summary>
    Task<string> GenerateOrderNumberAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates order status.
    /// </summary>
    Task UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default);

    /// <summary>
    /// Updates payment status.
    /// </summary>
    Task UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Updates fulfillment status.
    /// </summary>
    Task UpdateFulfillmentStatusAsync(Guid orderId, FulfillmentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets order count by status.
    /// </summary>
    Task<Dictionary<OrderStatus, int>> GetCountByStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets total revenue for a date range.
    /// </summary>
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);

    /// <summary>
    /// Gets average order value for a date range.
    /// </summary>
    Task<decimal> GetAverageOrderValueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
}

/// <summary>
/// Query parameters for order listing.
/// </summary>
public class OrderQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerEmail { get; set; }
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public FulfillmentStatus? FulfillmentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public OrderSortBy SortBy { get; set; } = OrderSortBy.Newest;
    public bool IncludeLines { get; set; } = false;
}
