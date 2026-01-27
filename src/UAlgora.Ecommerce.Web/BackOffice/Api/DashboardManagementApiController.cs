using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for dashboard and statistics in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Dashboard}")]
public class DashboardManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IDiscountService _discountService;

    public DashboardManagementApiController(
        IOrderService orderService,
        IOrderRepository orderRepository,
        IProductService productService,
        ICustomerService customerService,
        IDiscountService discountService)
    {
        _orderService = orderService;
        _orderRepository = orderRepository;
        _productService = productService;
        _customerService = customerService;
        _discountService = discountService;
    }

    /// <summary>
    /// Gets the main dashboard overview statistics.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType<DashboardOverviewResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.AddDays(-(int)now.DayOfWeek).Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var yearStart = new DateTime(now.Year, 1, 1);

        // Get all orders for calculations
        var allOrders = await _orderRepository.GetAllAsync();
        var orders = allOrders.Where(o => !o.IsDeleted).ToList();

        // Calculate revenue by period
        var todayOrders = orders.Where(o => o.CreatedAt >= todayStart).ToList();
        var weekOrders = orders.Where(o => o.CreatedAt >= weekStart).ToList();
        var monthOrders = orders.Where(o => o.CreatedAt >= monthStart).ToList();
        var yearOrders = orders.Where(o => o.CreatedAt >= yearStart).ToList();

        var completedStatuses = new[] { OrderStatus.Completed, OrderStatus.Delivered, OrderStatus.Shipped, OrderStatus.Processing };

        decimal CalculateRevenue(IEnumerable<Order> orderList) =>
            orderList.Where(o => completedStatuses.Contains(o.Status)).Sum(o => o.GrandTotal);

        var todayRevenue = CalculateRevenue(todayOrders);
        var weekRevenue = CalculateRevenue(weekOrders);
        var monthRevenue = CalculateRevenue(monthOrders);
        var yearRevenue = CalculateRevenue(yearOrders);

        // Calculate changes (compare to previous period)
        var yesterdayStart = todayStart.AddDays(-1);
        var yesterdayOrders = orders.Where(o => o.CreatedAt >= yesterdayStart && o.CreatedAt < todayStart).ToList();
        var yesterdayRevenue = CalculateRevenue(yesterdayOrders);
        var todayChange = yesterdayRevenue > 0 ? ((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100 : 0;

        var lastWeekStart = weekStart.AddDays(-7);
        var lastWeekOrders = orders.Where(o => o.CreatedAt >= lastWeekStart && o.CreatedAt < weekStart).ToList();
        var lastWeekRevenue = CalculateRevenue(lastWeekOrders);
        var weekChange = lastWeekRevenue > 0 ? ((weekRevenue - lastWeekRevenue) / lastWeekRevenue) * 100 : 0;

        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthOrders = orders.Where(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < monthStart).ToList();
        var lastMonthRevenue = CalculateRevenue(lastMonthOrders);
        var monthChange = lastMonthRevenue > 0 ? ((monthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

        var lastYearStart = yearStart.AddYears(-1);
        var lastYearOrders = orders.Where(o => o.CreatedAt >= lastYearStart && o.CreatedAt < yearStart).ToList();
        var lastYearRevenue = CalculateRevenue(lastYearOrders);
        var yearChange = lastYearRevenue > 0 ? ((yearRevenue - lastYearRevenue) / lastYearRevenue) * 100 : 0;

        // Order funnel counts
        var cartCount = orders.Count(o => o.Status == OrderStatus.Pending);
        var checkoutCount = orders.Count(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing);
        var paidCount = orders.Count(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered);
        var shippedCount = orders.Count(o => o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered);

        // Get product count
        var productParams = new ProductQueryParameters { Page = 1, PageSize = 1 };
        var productResult = await _productService.GetPagedAsync(productParams);

        // Get customer count
        var customerParams = new CustomerQueryParameters { Page = 1, PageSize = 1 };
        var customerResult = await _customerService.GetPagedAsync(customerParams);

        // Get active discounts count
        var discounts = await _discountService.GetActiveAsync();
        var activeDiscounts = discounts.Count(d => d.IsActive &&
            (!d.StartDate.HasValue || d.StartDate <= now) &&
            (!d.EndDate.HasValue || d.EndDate > now));

        return Ok(new DashboardOverviewResponse
        {
            TodayRevenue = todayRevenue,
            TodayOrders = todayOrders.Count,
            TodayChange = Math.Round(todayChange, 1),
            WeekRevenue = weekRevenue,
            WeekOrders = weekOrders.Count,
            WeekChange = Math.Round(weekChange, 1),
            MonthRevenue = monthRevenue,
            MonthOrders = monthOrders.Count,
            MonthChange = Math.Round(monthChange, 1),
            YearRevenue = yearRevenue,
            YearOrders = yearOrders.Count,
            YearChange = Math.Round(yearChange, 1),
            CartCount = cartCount,
            CheckoutCount = checkoutCount,
            PaidCount = paidCount,
            ShippedCount = shippedCount,
            TotalProducts = productResult.TotalCount,
            TotalOrders = orders.Count,
            TotalCustomers = customerResult.TotalCount,
            ActiveDiscounts = activeDiscounts,
            CurrencyCode = "USD"
        });
    }

    /// <summary>
    /// Gets top selling products for the dashboard.
    /// </summary>
    [HttpGet("top-products")]
    [ProducesResponseType<List<TopProductResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 5)
    {
        // Get all orders with items to calculate top products
        var orders = await _orderRepository.GetAllWithLinesAsync();
        var completedOrders = orders.Where(o => !o.IsDeleted &&
            (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped));

        // Aggregate sales by product
        var productSales = completedOrders
            .SelectMany(o => o.Lines)
            .GroupBy(i => new { i.ProductId, i.ProductName, i.Sku })
            .Select(g => new TopProductResponse
            {
                Name = g.Key.ProductName,
                Sku = g.Key.Sku ?? "N/A",
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
                Quantity = g.Sum(i => i.Quantity)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(limit)
            .ToList();

        // If no sales data, get products from catalog
        if (!productSales.Any())
        {
            var products = await _productService.GetBestSellersAsync(limit);
            productSales = products.Select(p => new TopProductResponse
            {
                Name = p.Name,
                Sku = p.Sku,
                Revenue = 0,
                Quantity = 0
            }).ToList();
        }

        return Ok(productSales);
    }

    /// <summary>
    /// Gets low stock products alert.
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType<List<LowStockResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        var parameters = new ProductQueryParameters { Page = 1, PageSize = 100 };
        var result = await _productService.GetPagedAsync(parameters);

        var lowStockProducts = result.Items
            .Where(p => p.TrackInventory && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
            .Select(p => new LowStockResponse
            {
                Name = p.Name,
                Sku = p.Sku,
                Stock = p.StockQuantity,
                Threshold = p.LowStockThreshold > 0 ? p.LowStockThreshold : threshold
            })
            .ToList();

        return Ok(lowStockProducts);
    }

    /// <summary>
    /// Gets top buyers by spending (calculated from orders).
    /// </summary>
    [HttpGet("top-buyers")]
    [ProducesResponseType<List<TopBuyerResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopBuyers([FromQuery] int limit = 5)
    {
        // Calculate top buyers directly from valid orders (excluding pending, cancelled, refunded, failed)
        var orders = await _orderRepository.GetAllAsync();
        var validStatuses = new[] { OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Completed };
        var validOrders = orders.Where(o => !o.IsDeleted && validStatuses.Contains(o.Status)).ToList();

        // Aggregate spending by customer email
        var topBuyers = validOrders
            .Where(o => !string.IsNullOrEmpty(o.CustomerEmail))
            .GroupBy(o => o.CustomerEmail.ToLower())
            .Select(g => new TopBuyerResponse
            {
                Name = g.First().CustomerName ?? g.First().CustomerEmail,
                Email = g.First().CustomerEmail,
                Total = g.Sum(o => o.GrandTotal - o.RefundedAmount),
                Orders = g.Count()
            })
            .OrderByDescending(b => b.Total)
            .Take(limit)
            .ToList();

        // If no orders, fall back to customers from service (for display purposes)
        if (!topBuyers.Any())
        {
            var customers = await _customerService.GetTopBySpentAsync(limit);
            topBuyers = customers.Select(c => new TopBuyerResponse
            {
                Name = $"{c.FirstName} {c.LastName}".Trim(),
                Email = c.Email,
                Total = c.TotalSpent,
                Orders = c.TotalOrders
            }).ToList();
        }

        return Ok(topBuyers);
    }

    /// <summary>
    /// Gets recent activity feed for the dashboard.
    /// </summary>
    [HttpGet("recent-activity")]
    [ProducesResponseType<List<ActivityResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 10)
    {
        var recentOrders = await _orderService.GetRecentAsync(limit);
        var activities = new List<ActivityResponse>();

        foreach (var order in recentOrders)
        {
            var timeAgo = GetTimeAgo(order.CreatedAt);
            var customerName = order.CustomerName ?? $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}".Trim();

            string type;
            string text;

            switch (order.Status)
            {
                case OrderStatus.Pending:
                    type = "order";
                    text = $"New order <strong>#{order.OrderNumber}</strong> from {customerName}";
                    break;
                case OrderStatus.Completed:
                case OrderStatus.Delivered:
                    type = "payment";
                    text = $"Payment received for order <strong>#{order.OrderNumber}</strong>";
                    break;
                case OrderStatus.Shipped:
                    type = "order";
                    text = $"Order <strong>#{order.OrderNumber}</strong> shipped";
                    break;
                case OrderStatus.Cancelled:
                    type = "refund";
                    text = $"Order <strong>#{order.OrderNumber}</strong> cancelled";
                    break;
                default:
                    type = "order";
                    text = $"Order <strong>#{order.OrderNumber}</strong> updated to {order.Status}";
                    break;
            }

            activities.Add(new ActivityResponse
            {
                Type = type,
                Text = text,
                Amount = order.GrandTotal,
                Time = timeAgo
            });
        }

        return Ok(activities);
    }

    /// <summary>
    /// Gets refund statistics for the dashboard.
    /// </summary>
    [HttpGet("refund-stats")]
    [ProducesResponseType<RefundStatsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRefundStats()
    {
        var orders = await _orderRepository.GetAllAsync();
        var allOrders = orders.Where(o => !o.IsDeleted).ToList();

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var sixtyDaysAgo = DateTime.UtcNow.AddDays(-60);

        var recentOrders = allOrders.Where(o => o.CreatedAt >= thirtyDaysAgo).ToList();
        var previousOrders = allOrders.Where(o => o.CreatedAt >= sixtyDaysAgo && o.CreatedAt < thirtyDaysAgo).ToList();

        var recentRefunds = recentOrders.Where(o => o.Status == OrderStatus.Refunded || o.Status == OrderStatus.Cancelled).ToList();
        var previousRefunds = previousOrders.Where(o => o.Status == OrderStatus.Refunded || o.Status == OrderStatus.Cancelled).ToList();

        var recentRate = recentOrders.Count > 0 ? (decimal)recentRefunds.Count / recentOrders.Count * 100 : 0;
        var previousRate = previousOrders.Count > 0 ? (decimal)previousRefunds.Count / previousOrders.Count * 100 : 0;
        var rateChange = recentRate - previousRate;

        var totalRefunds = recentRefunds.Sum(o => o.GrandTotal);

        return Ok(new RefundStatsResponse
        {
            Rate = Math.Round(recentRate, 1),
            RateChange = Math.Round(rateChange, 1),
            TotalRefunds = totalRefunds,
            RefundCount = recentRefunds.Count
        });
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;

        if (span.TotalMinutes < 1)
            return "just now";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} mins ago";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} hours ago";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays} days ago";

        return dateTime.ToString("MMM dd");
    }

    #region Legacy Endpoints (kept for backward compatibility)

    /// <summary>
    /// Gets order statistics by status.
    /// </summary>
    [HttpGet("orders/by-status")]
    [ProducesResponseType<OrderStatusStatisticsModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByStatus()
    {
        var counts = await _orderService.GetCountByStatusAsync();

        return Ok(new OrderStatusStatisticsModel
        {
            StatusCounts = counts.Select(kvp => new StatusCountModel
            {
                Status = kvp.Key.ToString(),
                Count = kvp.Value
            }).ToList(),
            Total = counts.Values.Sum()
        });
    }

    /// <summary>
    /// Gets recent orders for the dashboard.
    /// </summary>
    [HttpGet("orders/recent")]
    [ProducesResponseType<RecentOrdersModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int take = 10)
    {
        var orders = await _orderService.GetRecentAsync(take);

        return Ok(new RecentOrdersModel
        {
            Orders = orders.Select(o => new RecentOrderItemModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.CustomerName ?? $"{o.BillingAddress?.FirstName} {o.BillingAddress?.LastName}".Trim(),
                GrandTotal = o.GrandTotal,
                CurrencyCode = o.CurrencyCode,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            }).ToList()
        });
    }

    /// <summary>
    /// Gets top selling products.
    /// </summary>
    [HttpGet("products/top-selling")]
    [ProducesResponseType<TopProductsModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int take = 10)
    {
        var products = await _productService.GetBestSellersAsync(take);

        return Ok(new TopProductsModel
        {
            Products = products.Select(p => new TopProductItemModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Price = p.CurrentPrice,
                ImageUrl = p.PrimaryImageUrl
            }).ToList()
        });
    }

    /// <summary>
    /// Gets low stock products alert (legacy).
    /// </summary>
    [HttpGet("products/low-stock")]
    [ProducesResponseType<LowStockProductsModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10, [FromQuery] int take = 20)
    {
        var parameters = new ProductQueryParameters { Page = 1, PageSize = 100 };
        var result = await _productService.GetPagedAsync(parameters);

        var lowStockProducts = result.Items
            .Where(p => p.TrackInventory && p.IsLowStock)
            .Take(take)
            .ToList();

        return Ok(new LowStockProductsModel
        {
            Products = lowStockProducts.Select(p => new LowStockProductItemModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                ImageUrl = p.PrimaryImageUrl
            }).ToList(),
            TotalCount = lowStockProducts.Count
        });
    }

    /// <summary>
    /// Gets top customers by spending (legacy).
    /// </summary>
    [HttpGet("customers/top")]
    [ProducesResponseType<TopCustomersModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopCustomers([FromQuery] int take = 10)
    {
        var customers = await _customerService.GetTopBySpentAsync(take);

        return Ok(new TopCustomersModel
        {
            Customers = customers.Select(c => new TopCustomerItemModel
            {
                Id = c.Id,
                Name = $"{c.FirstName} {c.LastName}".Trim(),
                Email = c.Email,
                TotalSpent = c.TotalSpent,
                TotalOrders = c.TotalOrders
            }).ToList()
        });
    }

    /// <summary>
    /// Gets active discount codes.
    /// </summary>
    [HttpGet("discounts/active")]
    [ProducesResponseType<ActiveDiscountsModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveDiscounts()
    {
        var discounts = await _discountService.GetActiveAsync();
        var now = DateTime.UtcNow;

        var activeDiscounts = discounts
            .Where(d => d.IsActive &&
                (!d.StartDate.HasValue || d.StartDate <= now) &&
                (!d.EndDate.HasValue || d.EndDate > now))
            .ToList();

        return Ok(new ActiveDiscountsModel
        {
            Discounts = activeDiscounts.Select(d => new ActiveDiscountItemModel
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Type = d.Type.ToString(),
                Value = d.Value,
                UsageCount = d.UsageCount,
                TotalUsageLimit = d.TotalUsageLimit,
                EndDate = d.EndDate
            }).ToList(),
            TotalCount = activeDiscounts.Count
        });
    }

    #endregion
}

#region Dashboard Response Models

public class DashboardOverviewResponse
{
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayChange { get; set; }
    public decimal WeekRevenue { get; set; }
    public int WeekOrders { get; set; }
    public decimal WeekChange { get; set; }
    public decimal MonthRevenue { get; set; }
    public int MonthOrders { get; set; }
    public decimal MonthChange { get; set; }
    public decimal YearRevenue { get; set; }
    public int YearOrders { get; set; }
    public decimal YearChange { get; set; }
    public int CartCount { get; set; }
    public int CheckoutCount { get; set; }
    public int PaidCount { get; set; }
    public int ShippedCount { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int ActiveDiscounts { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class TopProductResponse
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Quantity { get; set; }
}

public class LowStockResponse
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int Threshold { get; set; }
}

public class TopBuyerResponse
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Orders { get; set; }
}

public class ActivityResponse
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Time { get; set; } = string.Empty;
}

public class RefundStatsResponse
{
    public decimal Rate { get; set; }
    public decimal RateChange { get; set; }
    public decimal TotalRefunds { get; set; }
    public int RefundCount { get; set; }
}

#endregion

#region Legacy Response Models

public class DashboardOverviewModel
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TodayOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public required string CurrencyCode { get; set; }
}

public class OrderStatusStatisticsModel
{
    public List<StatusCountModel> StatusCounts { get; set; } = [];
    public int Total { get; set; }
}

public class StatusCountModel
{
    public required string Status { get; set; }
    public int Count { get; set; }
}

public class RecentOrdersModel
{
    public List<RecentOrderItemModel> Orders { get; set; } = [];
}

public class RecentOrderItemModel
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public decimal GrandTotal { get; set; }
    public required string CurrencyCode { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TopProductsModel
{
    public List<TopProductItemModel> Products { get; set; } = [];
}

public class TopProductItemModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}

public class LowStockProductsModel
{
    public List<LowStockProductItemModel> Products { get; set; } = [];
    public int TotalCount { get; set; }
}

public class LowStockProductItemModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public string? ImageUrl { get; set; }
}

public class TopCustomersModel
{
    public List<TopCustomerItemModel> Customers { get; set; } = [];
}

public class TopCustomerItemModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
}

public class ActiveDiscountsModel
{
    public List<ActiveDiscountItemModel> Discounts { get; set; } = [];
    public int TotalCount { get; set; }
}

public class ActiveDiscountItemModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public required string Type { get; set; }
    public decimal Value { get; set; }
    public int UsageCount { get; set; }
    public int? TotalUsageLimit { get; set; }
    public DateTime? EndDate { get; set; }
}

#endregion
