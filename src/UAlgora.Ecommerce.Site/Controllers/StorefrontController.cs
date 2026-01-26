using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Site.ViewModels;

namespace UAlgora.Ecommerce.Site.Controllers;

public class StorefrontController : Controller
{
    private const string AuthScheme = "EcommerceCustomer";
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly IWishlistService _wishlistService;
    private readonly IReviewService _reviewService;

    public StorefrontController(
        IProductService productService,
        ICategoryService categoryService,
        ICartService cartService,
        ICustomerService customerService,
        IOrderService orderService,
        IWishlistService wishlistService,
        IReviewService reviewService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _cartService = cartService;
        _customerService = customerService;
        _orderService = orderService;
        _wishlistService = wishlistService;
        _reviewService = reviewService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var featuredParams = new ProductQueryParameters { Page = 1, PageSize = 8, IsFeatured = true };
        var newParams = new ProductQueryParameters { Page = 1, PageSize = 8, SortBy = ProductSortBy.Newest };
        var saleParams = new ProductQueryParameters { Page = 1, PageSize = 4, OnSale = true };

        var featured = await _productService.GetPagedAsync(featuredParams);
        var newArrivals = await _productService.GetPagedAsync(newParams);
        var deals = await _productService.GetPagedAsync(saleParams);
        var categories = await _categoryService.GetRootCategoriesAsync();

        var viewModel = new HomeViewModel
        {
            FeaturedProducts = featured.Items.ToList(),
            NewArrivals = newArrivals.Items.ToList(),
            DealsOfTheDay = deals.Items.ToList(),
            Categories = categories.Where(c => c.IsVisible).Take(8).ToList()
        };

        ViewData["Title"] = "Home";
        return View("Home", viewModel);
    }

    [HttpGet("/products")]
    public async Task<IActionResult> Products(
        string? search,
        Guid? category,
        string? sort,
        decimal? minPrice,
        decimal? maxPrice,
        bool? sale,
        bool? inStock,
        int page = 1)
    {
        var parameters = new ProductQueryParameters
        {
            Page = page,
            PageSize = 12,
            SearchTerm = search,
            CategoryId = category,
            OnSale = sale,
            InStock = inStock,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SortBy = sort switch
            {
                "price-asc" => ProductSortBy.PriceLowToHigh,
                "price-desc" => ProductSortBy.PriceHighToLow,
                "name" => ProductSortBy.NameAscending,
                "newest" => ProductSortBy.Newest,
                "rating" => ProductSortBy.TopRated,
                _ => ProductSortBy.Newest
            }
        };

        var result = await _productService.GetPagedAsync(parameters);
        var categories = await _categoryService.GetAllAsync();

        string? categoryName = null;
        if (category.HasValue)
        {
            var cat = await _categoryService.GetByIdAsync(category.Value);
            categoryName = cat?.Name;
        }

        var viewModel = new ProductListViewModel
        {
            Products = result.Items.ToList(),
            Categories = categories.Where(c => c.IsVisible).ToList(),
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = 12,
            SearchTerm = search,
            CategoryId = category,
            CategoryName = categoryName,
            SortBy = sort,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            OnSale = sale,
            InStock = inStock
        };

        ViewData["Title"] = categoryName ?? "All Products";
        return View("Products", viewModel);
    }

    [HttpGet("/products/{slug}")]
    public async Task<IActionResult> ProductDetail(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);
        if (product == null)
        {
            return NotFound();
        }

        var reviews = await _reviewService.GetReviewsByProductIdAsync(product.Id, approvedOnly: true);
        var relatedParams = new ProductQueryParameters
        {
            Page = 1,
            PageSize = 4,
            CategoryId = product.CategoryIds.FirstOrDefault()
        };
        var related = await _productService.GetPagedAsync(relatedParams);

        Category? category = null;
        if (product.CategoryIds.Any())
        {
            category = await _categoryService.GetByIdAsync(product.CategoryIds.First());
        }

        var viewModel = new ProductDetailViewModel
        {
            Product = product,
            RelatedProducts = related.Items.Where(p => p.Id != product.Id).Take(4).ToList(),
            Reviews = reviews,
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
            ReviewCount = reviews.Count,
            Category = category
        };

        ViewData["Title"] = product.Name;
        ViewData["Description"] = product.MetaDescription ?? product.ShortDescription;
        return View("ProductDetail", viewModel);
    }

    [HttpGet("/cart")]
    public IActionResult Cart()
    {
        ViewData["Title"] = "Shopping Cart";
        return View("Cart", new CartViewModel());
    }

    [HttpGet("/checkout")]
    public IActionResult Checkout()
    {
        var viewModel = new CheckoutViewModel
        {
            ShippingMethods =
            [
                new ShippingMethodModel { Id = "standard", Name = "Standard Shipping", Description = "5-7 business days", Price = 5.99m, EstimatedDelivery = "5-7 business days" },
                new ShippingMethodModel { Id = "express", Name = "Express Shipping", Description = "2-3 business days", Price = 12.99m, EstimatedDelivery = "2-3 business days" },
                new ShippingMethodModel { Id = "overnight", Name = "Overnight Shipping", Description = "Next business day", Price = 24.99m, EstimatedDelivery = "Next business day" }
            ],
            PaymentMethods =
            [
                new PaymentMethodModel { Id = "card", Name = "Credit/Debit Card", Icon = "credit-card" },
                new PaymentMethodModel { Id = "paypal", Name = "PayPal", Icon = "paypal" }
            ]
        };

        ViewData["Title"] = "Checkout";
        return View("Checkout", viewModel);
    }

    [HttpGet("/order-confirmation/{orderNumber}")]
    public async Task<IActionResult> OrderConfirmation(string orderNumber)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber);
        if (order == null)
        {
            return NotFound();
        }

        var viewModel = new OrderConfirmationViewModel
        {
            Order = order,
            OrderNumber = orderNumber,
            Email = order.CustomerEmail ?? string.Empty
        };

        ViewData["Title"] = "Order Confirmation";
        return View("OrderConfirmation", viewModel);
    }

    [HttpGet("/account")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> Account()
    {
        var customerId = GetCurrentCustomerId();
        if (customerId == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var customer = await _customerService.GetByIdAsync(customerId.Value);
        if (customer == null)
        {
            await HttpContext.SignOutAsync(AuthScheme);
            return RedirectToAction(nameof(Login));
        }

        // Get recent orders
        var recentOrders = await _orderService.GetByCustomerIdAsync(customerId.Value);
        var addresses = customer.Addresses?.ToList() ?? [];

        var viewModel = new AccountViewModel
        {
            CustomerId = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            MemberSince = customer.CreatedAt,
            TotalOrders = recentOrders.Count(),
            LoyaltyPoints = customer.LoyaltyPoints,
            StoreCreditBalance = customer.StoreCreditBalance,
            Addresses = addresses,
            RecentOrders = recentOrders.OrderByDescending(o => o.PlacedAt ?? o.CreatedAt).Take(5).ToList()
        };

        ViewData["Title"] = "My Account";
        return View("Account", viewModel);
    }

    [HttpGet("/account/orders")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> Orders()
    {
        var customerId = GetCurrentCustomerId();
        if (customerId == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var orders = await _orderService.GetByCustomerIdAsync(customerId.Value);
        var orderedOrders = orders.OrderByDescending(o => o.PlacedAt ?? o.CreatedAt).ToList();

        ViewData["Title"] = "My Orders";
        return View("Orders", orderedOrders);
    }

    [HttpGet("/account/orders/{orderNumber}")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> OrderDetail(string orderNumber)
    {
        var customerId = GetCurrentCustomerId();
        if (customerId == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var order = await _orderService.GetByOrderNumberAsync(orderNumber);
        if (order == null)
        {
            return NotFound();
        }

        // Verify the order belongs to the current customer
        if (order.CustomerId != customerId)
        {
            return Forbid();
        }

        ViewData["Title"] = $"Order {orderNumber}";
        return View("OrderDetail", order);
    }

    [HttpGet("/wishlist")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> Wishlist()
    {
        var customerId = GetCurrentCustomerId();
        if (customerId == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var wishlist = await _wishlistService.GetDefaultWishlistAsync(customerId.Value);
        var viewModel = new WishlistViewModel
        {
            Wishlist = wishlist,
            Items = wishlist?.Items?.ToList() ?? []
        };

        ViewData["Title"] = "My Wishlist";
        return View("Wishlist", viewModel);
    }

    [HttpGet("/deals")]
    public async Task<IActionResult> Deals()
    {
        var parameters = new ProductQueryParameters
        {
            Page = 1,
            PageSize = 24,
            OnSale = true,
            SortBy = ProductSortBy.Newest
        };

        var result = await _productService.GetPagedAsync(parameters);
        var categories = await _categoryService.GetAllAsync();

        var viewModel = new ProductListViewModel
        {
            Products = result.Items.ToList(),
            Categories = categories.Where(c => c.IsVisible).ToList(),
            TotalCount = result.TotalCount,
            Page = 1,
            PageSize = 24,
            OnSale = true
        };

        ViewData["Title"] = "Deals & Offers";
        return View("Deals", viewModel);
    }

    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl)
    {
        // Redirect if already authenticated
        if (GetCurrentCustomerId() != null)
        {
            return Redirect(returnUrl ?? "/account");
        }

        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl ?? "/account";
        return View("Login");
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        // Redirect if already authenticated
        if (GetCurrentCustomerId() != null)
        {
            return RedirectToAction(nameof(Account));
        }

        ViewData["Title"] = "Create Account";
        return View("Register");
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthScheme);
        return RedirectToAction(nameof(Index));
    }

    #region Helper Methods

    private Guid? GetCurrentCustomerId()
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return null;
        }

        return customerId;
    }

    #endregion
}
