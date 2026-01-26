using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Site.ViewModels;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace UAlgora.Ecommerce.Site.Controllers;

public class StorefrontController : Controller
{
    private const string AuthScheme = "EcommerceCustomer";
    private const string LoginPageAlias = "algoraLoginPage";
    private const string RegisterPageAlias = "algoraRegisterPage";
    private const string CheckoutPageAlias = "algoraCheckoutPage";
    private const string CartPageAlias = "algoraCartPage";
    private const string ProductsPageAlias = "algoraProductsPage";
    private const string ProductDetailPageAlias = "algoraProductDetailPage";
    private const string AccountPageAlias = "algoraAccountPage";
    private const string OrdersPageAlias = "algoraOrdersPage";
    private const string OrderDetailPageAlias = "algoraOrderDetailPage";
    private const string WishlistPageAlias = "algoraWishlistPage";
    private const string DealsPageAlias = "algoraDealsPage";
    private const string OrderConfirmationPageAlias = "algoraOrderConfirmationPage";
    private const string AddressesPageAlias = "algoraAddressesPage";
    private const string SettingsPageAlias = "algoraSettingsPage";

    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly IWishlistService _wishlistService;
    private readonly IReviewService _reviewService;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public StorefrontController(
        IProductService productService,
        ICategoryService categoryService,
        ICartService cartService,
        ICustomerService customerService,
        IOrderService orderService,
        IWishlistService wishlistService,
        IReviewService reviewService,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _productService = productService;
        _categoryService = categoryService;
        _cartService = cartService;
        _customerService = customerService;
        _orderService = orderService;
        _wishlistService = wishlistService;
        _reviewService = reviewService;
        _umbracoContextAccessor = umbracoContextAccessor;
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
        // Look for CMS-managed Products page
        var productsPage = FindPageByAlias(ProductsPageAlias);
        if (productsPage != null)
        {
            // Build query string to preserve filters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (category.HasValue) queryParams.Add($"category={category}");
            if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={sort}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");
            if (sale == true) queryParams.Add("sale=true");
            if (inStock == true) queryParams.Add("inStock=true");
            if (page > 1) queryParams.Add($"page={page}");

            var url = productsPage.Url();
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }
            return Redirect(url);
        }

        // Fallback to hardcoded view
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
        // Look for CMS-managed Product Detail page
        var productDetailPage = FindPageByAlias(ProductDetailPageAlias);
        if (productDetailPage != null)
        {
            // Redirect to the Umbraco-managed product detail page with slug as query parameter
            return Redirect(productDetailPage.Url() + $"?slug={Uri.EscapeDataString(slug)}");
        }

        // Fallback to hardcoded view
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
        // Look for CMS-managed Cart page
        var cartPage = FindPageByAlias(CartPageAlias);
        if (cartPage != null)
        {
            // Redirect to the Umbraco-managed cart page
            return Redirect(cartPage.Url());
        }

        // Fallback to hardcoded view
        ViewData["Title"] = "Shopping Cart";
        return View("Cart", new CartViewModel());
    }

    [HttpGet("/checkout")]
    public IActionResult Checkout()
    {
        // Look for CMS-managed Checkout page
        var checkoutPage = FindPageByAlias(CheckoutPageAlias);
        if (checkoutPage != null)
        {
            // Redirect to the Umbraco-managed checkout page
            return Redirect(checkoutPage.Url());
        }

        // Fallback to hardcoded view
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
        // Look for CMS-managed Order Confirmation page
        var orderConfirmationPage = FindPageByAlias(OrderConfirmationPageAlias);
        if (orderConfirmationPage != null)
        {
            // Redirect to the Umbraco-managed order confirmation page with order number as query parameter
            return Redirect(orderConfirmationPage.Url() + $"?orderNumber={Uri.EscapeDataString(orderNumber)}");
        }

        // Try to load the order from database
        var order = await _orderService.GetByOrderNumberAsync(orderNumber);

        // Get customer email from authentication if order not found
        string customerEmail = order?.CustomerEmail ?? string.Empty;
        if (string.IsNullOrEmpty(customerEmail))
        {
            var authResult = await HttpContext.AuthenticateAsync(AuthScheme);
            if (authResult.Succeeded)
            {
                customerEmail = authResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
            }
        }

        var viewModel = new OrderConfirmationViewModel
        {
            Order = order, // May be null for demo/test orders
            OrderNumber = orderNumber,
            Email = customerEmail
        };

        ViewData["Title"] = "Order Confirmation";
        return View("OrderConfirmation", viewModel);
    }

    [HttpGet("/account")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> Account()
    {
        // Look for CMS-managed Account page
        var accountPage = FindPageByAlias(AccountPageAlias);
        if (accountPage != null)
        {
            // Redirect to the Umbraco-managed account page
            return Redirect(accountPage.Url());
        }

        // Fallback to hardcoded view
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

    [HttpGet("/account/addresses")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> Addresses()
    {
        // Look for CMS-managed Addresses page
        var addressesPage = FindPageByAlias(AddressesPageAlias);
        if (addressesPage != null)
        {
            return Redirect(addressesPage.Url());
        }

        // Fallback to hardcoded view
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

        var viewModel = new AddressesViewModel
        {
            CustomerId = customer.Id,
            Addresses = customer.Addresses?.ToList() ?? []
        };

        ViewData["Title"] = "My Addresses";
        return View("Addresses", viewModel);
    }

    [HttpGet("/account/settings")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> Settings()
    {
        // Look for CMS-managed Settings page
        var settingsPage = FindPageByAlias(SettingsPageAlias);
        if (settingsPage != null)
        {
            return Redirect(settingsPage.Url());
        }

        // Fallback to hardcoded view
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

        var viewModel = new SettingsViewModel
        {
            CustomerId = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            AcceptsMarketing = customer.AcceptsMarketing
        };

        ViewData["Title"] = "Account Settings";
        return View("Settings", viewModel);
    }

    [HttpGet("/account/orders")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> Orders()
    {
        // Look for CMS-managed Orders page
        var ordersPage = FindPageByAlias(OrdersPageAlias);
        if (ordersPage != null)
        {
            // Redirect to the Umbraco-managed orders page
            return Redirect(ordersPage.Url());
        }

        // Fallback to hardcoded view
        var customerId = GetCurrentCustomerId();
        if (customerId == null)
        {
            return RedirectToAction(nameof(Login));
        }

        // Get orders by customer ID
        var ordersByCustomerId = await _orderService.GetByCustomerIdAsync(customerId.Value);

        // Also get guest orders by email (for orders placed before login)
        var customer = await _customerService.GetByIdAsync(customerId.Value);
        var guestOrders = new List<Order>();
        if (customer != null && !string.IsNullOrEmpty(customer.Email))
        {
            var ordersByEmail = await _orderService.GetByCustomerEmailAsync(customer.Email);
            guestOrders = ordersByEmail.Where(o => o.CustomerId == null).ToList();
        }

        // Merge and deduplicate orders
        var allOrders = ordersByCustomerId.Union(guestOrders).DistinctBy(o => o.Id)
            .OrderByDescending(o => o.PlacedAt ?? o.CreatedAt).ToList();

        ViewData["Title"] = "My Orders";
        return View("Orders", allOrders);
    }

    [HttpGet("/account/orders/{orderNumber}")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> OrderDetail(string orderNumber)
    {
        // Look for CMS-managed Order Detail page
        var orderDetailPage = FindPageByAlias(OrderDetailPageAlias);
        if (orderDetailPage != null)
        {
            // Redirect to the Umbraco-managed order detail page with order number as query parameter
            return Redirect(orderDetailPage.Url() + $"?orderNumber={Uri.EscapeDataString(orderNumber)}");
        }

        // Fallback to hardcoded view
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

        // Verify the order belongs to the current customer (by ID or email for guest orders)
        var customer = await _customerService.GetByIdAsync(customerId.Value);
        var isOwner = order.CustomerId == customerId ||
                      (order.CustomerId == null && !string.IsNullOrEmpty(order.CustomerEmail) &&
                       customer != null && string.Equals(order.CustomerEmail, customer.Email, StringComparison.OrdinalIgnoreCase));

        if (!isOwner)
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
        // Look for CMS-managed Wishlist page
        var wishlistPage = FindPageByAlias(WishlistPageAlias);
        if (wishlistPage != null)
        {
            // Redirect to the Umbraco-managed wishlist page
            return Redirect(wishlistPage.Url());
        }

        // Fallback to hardcoded view
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
    public async Task<IActionResult> Deals(string? sort, string? discount, string? category, int page = 1)
    {
        // Look for CMS-managed Deals page
        var dealsPage = FindPageByAlias(DealsPageAlias);
        if (dealsPage != null)
        {
            // Build query string to preserve filters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={sort}");
            if (!string.IsNullOrEmpty(discount)) queryParams.Add($"discount={discount}");
            if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");
            if (page > 1) queryParams.Add($"page={page}");

            var url = dealsPage.Url();
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }
            return Redirect(url);
        }

        // Fallback to hardcoded view
        var parameters = new ProductQueryParameters
        {
            Page = page,
            PageSize = 24,
            OnSale = true,
            SortBy = sort switch
            {
                "price-asc" => ProductSortBy.PriceLowToHigh,
                "price-desc" => ProductSortBy.PriceHighToLow,
                "newest" => ProductSortBy.Newest,
                _ => ProductSortBy.Newest
            }
        };

        var result = await _productService.GetPagedAsync(parameters);
        var categories = await _categoryService.GetAllAsync();

        var viewModel = new ProductListViewModel
        {
            Products = result.Items.ToList(),
            Categories = categories.Where(c => c.IsVisible).ToList(),
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = 24,
            OnSale = true,
            SortBy = sort
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

        // Look for CMS-managed Login page
        var loginPage = FindPageByAlias(LoginPageAlias);
        if (loginPage != null)
        {
            // Redirect to the Umbraco-managed login page
            return Redirect(loginPage.Url() + (string.IsNullOrEmpty(returnUrl) ? "" : $"?returnUrl={Uri.EscapeDataString(returnUrl)}"));
        }

        // Fallback to hardcoded view
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

        // Look for CMS-managed Register page
        var registerPage = FindPageByAlias(RegisterPageAlias);
        if (registerPage != null)
        {
            // Redirect to the Umbraco-managed register page
            return Redirect(registerPage.Url());
        }

        // Fallback to hardcoded view
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

    /// <summary>
    /// Finds a published page by its document type alias.
    /// Searches the entire content tree for the first matching page.
    /// </summary>
    private IPublishedContent? FindPageByAlias(string alias)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
        {
            return null;
        }

        var content = umbracoContext.Content;
        if (content == null)
        {
            return null;
        }

        // Search all root content and their descendants
        foreach (var root in content.GetAtRoot())
        {
            // Check root
            if (root.ContentType.Alias == alias)
            {
                return root;
            }

            // Check descendants
            var found = root.DescendantsOrSelf().FirstOrDefault(x => x.ContentType.Alias == alias);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    #endregion
}
