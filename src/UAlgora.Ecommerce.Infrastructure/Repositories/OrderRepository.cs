using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for order operations.
/// </summary>
public class OrderRepository : SoftDeleteRepository<Order>, IOrderRepository
{
    public OrderRepository(EcommerceDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all orders with their line items included.
    /// </summary>
    public async Task<IReadOnlyList<Order>> GetAllWithLinesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .ToListAsync(ct);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
    }

    public async Task<Order?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .Include(o => o.Payments)
            .Include(o => o.Shipments)
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(
        OrderQueryParameters parameters,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                o.CustomerEmail.ToLower().Contains(term) ||
                (o.CustomerName != null && o.CustomerName.ToLower().Contains(term)));
        }

        if (parameters.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == parameters.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.CustomerEmail))
        {
            query = query.Where(o => o.CustomerEmail == parameters.CustomerEmail);
        }

        if (parameters.Status.HasValue)
        {
            query = query.Where(o => o.Status == parameters.Status.Value);
        }

        if (parameters.PaymentStatus.HasValue)
        {
            query = query.Where(o => o.PaymentStatus == parameters.PaymentStatus.Value);
        }

        if (parameters.FulfillmentStatus.HasValue)
        {
            query = query.Where(o => o.FulfillmentStatus == parameters.FulfillmentStatus.Value);
        }

        if (parameters.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= parameters.StartDate.Value);
        }

        if (parameters.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= parameters.EndDate.Value);
        }

        if (parameters.MinTotal.HasValue)
        {
            query = query.Where(o => o.GrandTotal >= parameters.MinTotal.Value);
        }

        if (parameters.MaxTotal.HasValue)
        {
            query = query.Where(o => o.GrandTotal <= parameters.MaxTotal.Value);
        }

        // Apply sorting
        query = parameters.SortBy switch
        {
            OrderSortBy.Newest => query.OrderByDescending(o => o.CreatedAt),
            OrderSortBy.Oldest => query.OrderBy(o => o.CreatedAt),
            OrderSortBy.TotalHighToLow => query.OrderByDescending(o => o.GrandTotal),
            OrderSortBy.TotalLowToHigh => query.OrderBy(o => o.GrandTotal),
            OrderSortBy.Status => query.OrderBy(o => o.Status),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(ct);

        // Include lines if requested
        if (parameters.IncludeLines && items.Any())
        {
            var orderIds = items.Select(o => o.Id).ToList();
            var lines = await Context.OrderLines
                .Where(l => orderIds.Contains(l.OrderId))
                .ToListAsync(ct);

            foreach (var order in items)
            {
                order.Lines = lines.Where(l => l.OrderId == order.Id).ToList();
            }
        }

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerEmailAsync(string email, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .Where(o => o.CustomerEmail == email)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByPaymentStatusAsync(PaymentStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(o => o.PaymentStatus == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByFulfillmentStatusAsync(FulfillmentStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(o => o.FulfillmentStatus == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.Lines)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetPendingAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetReadyForFulfillmentAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(o => o.PaymentStatus == PaymentStatus.Captured)
            .Where(o => o.FulfillmentStatus == FulfillmentStatus.Unfulfilled)
            .Where(o => o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.CreatedAt)
            .Include(o => o.Lines)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"ORD-{year}-";

        // Get the last order number for this year
        var lastOrder = await DbSet
            .IgnoreQueryFilters()
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastOrder != null)
        {
            var lastNumber = lastOrder.OrderNumber.Replace(prefix, "");
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D6}";
    }

    public async Task UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct);
        if (order != null)
        {
            order.Status = status;

            // Update timestamp based on status
            switch (status)
            {
                case OrderStatus.Confirmed:
                    order.ConfirmedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Shipped:
                    order.ShippedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Completed:
                    order.CompletedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledAt = DateTime.UtcNow;
                    break;
            }

            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct);
        if (order != null)
        {
            order.PaymentStatus = status;

            if (status == PaymentStatus.Captured)
            {
                order.PaidAt = DateTime.UtcNow;
            }

            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateFulfillmentStatusAsync(Guid orderId, FulfillmentStatus status, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct);
        if (order != null)
        {
            order.FulfillmentStatus = status;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<Dictionary<OrderStatus, int>> GetCountByStatusAsync(CancellationToken ct = default)
    {
        return await DbSet
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
    }

    public async Task<decimal> GetTotalRevenueAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Where(o => o.PaymentStatus == PaymentStatus.Captured);

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endDate.Value);
        }

        return await query.SumAsync(o => o.GrandTotal - o.RefundedAmount, ct);
    }

    public async Task<decimal> GetAverageOrderValueAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Where(o => o.PaymentStatus == PaymentStatus.Captured);

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endDate.Value);
        }

        var count = await query.CountAsync(ct);
        if (count == 0)
            return 0;

        return await query.AverageAsync(o => o.GrandTotal, ct);
    }
}
