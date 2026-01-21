using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Site.ViewModels;

namespace UAlgora.Ecommerce.Site.Controllers;

public class StorefrontController : Controller
{
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
    public IActionResult Account()
    {
        ViewData["Title"] = "My Account";
        return View("Account", new AccountViewModel());
    }

    [HttpGet("/account/orders")]
    public IActionResult Orders()
    {
        ViewData["Title"] = "My Orders";
        return View("Orders", new List<Order>());
    }

    [HttpGet("/account/orders/{orderNumber}")]
    public async Task<IActionResult> OrderDetail(string orderNumber)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber);
        if (order == null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Order {orderNumber}";
        return View("OrderDetail", order);
    }

    [HttpGet("/wishlist")]
    public IActionResult Wishlist()
    {
        ViewData["Title"] = "My Wishlist";
        return View("Wishlist", new WishlistViewModel());
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
        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl;
        return View("Login");
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        ViewData["Title"] = "Create Account";
        return View("Register");
    }
}
