using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for dashboard and statistics in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Dashboard}")]
public class DashboardManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IDiscountService _discountService;

    public DashboardManagementApiController(
        IOrderService orderService,
        IProductService productService,
        ICustomerService customerService,
        IDiscountService discountService)
    {
        _orderService = orderService;
        _productService = productService;
        _customerService = customerService;
        _discountService = discountService;
    }

    /// <summary>
    /// Gets the main dashboard overview statistics.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType<DashboardOverviewModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        // Get order statistics
        var orderCounts = await _orderService.GetCountByStatusAsync();
        var pendingOrders = orderCounts.TryGetValue(OrderStatus.Pending, out var pending) ? pending : 0;
        var totalOrders = orderCounts.Values.Sum();

        // Get product count
        var productParams = new ProductQueryParameters { Page = 1, PageSize = 1 };
        var productResult = await _productService.GetPagedAsync(productParams);

        // Get customer count
        var customerParams = new CustomerQueryParameters { Page = 1, PageSize = 1 };
        var customerResult = await _customerService.GetPagedAsync(customerParams);

        // Get recent revenue (orders from last 30 days)
        var recentOrders = await _orderService.GetRecentAsync(100);
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var monthlyRevenue = recentOrders
            .Where(o => o.CreatedAt >= thirtyDaysAgo &&
                        (o.Status == OrderStatus.Completed ||
                         o.Status == OrderStatus.Delivered ||
                         o.Status == OrderStatus.Shipped))
            .Sum(o => o.GrandTotal);

        var todayOrders = recentOrders.Count(o => o.CreatedAt.Date == DateTime.UtcNow.Date);

        return Ok(new DashboardOverviewModel
        {
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TodayOrders = todayOrders,
            TotalProducts = productResult.TotalCount,
            TotalCustomers = customerResult.TotalCount,
            MonthlyRevenue = monthlyRevenue,
            CurrencyCode = "USD" // Could be made configurable
        });
    }

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
    /// Gets low stock products alert.
    /// </summary>
    [HttpGet("products/low-stock")]
    [ProducesResponseType<LowStockProductsModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10, [FromQuery] int take = 20)
    {
        // Get products and filter by low stock
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
    /// Gets top customers by spending.
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
}

#region Response Models

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
