# Umbraco 15+ E-Commerce Plugin Development Guide

> A comprehensive reference for building e-commerce plugins for Umbraco 15+ CMS using .NET 9 and modern architecture patterns.

---

## 1. Platform Overview

### 1.1 Technology Stack

```yaml
framework:
  cms: Umbraco 15+
  runtime: .NET 9
  language: C# 13
  backoffice_ui: Lit + Web Components
  frontend: Razor / Blazor / Headless
  database: SQL Server / PostgreSQL / SQLite
  orm: Entity Framework Core 9
  
plugin_types:
  - Composers (DI registration)
  - Notification Handlers (events)
  - Property Editors (custom fields)
  - Content Apps (contextual panels)
  - Sections (navigation areas)
  - Dashboards (section landing)
  - Trees (hierarchical nav)
  - API Controllers
```

### 1.2 Required NuGet Packages

```xml
<PackageReference Include="Umbraco.Cms" Version="15.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.*" />
<PackageReference Include="Stripe.net" Version="45.*" />
<PackageReference Include="MailKit" Version="4.*" />
<PackageReference Include="QuestPDF" Version="2024.*" />
<PackageReference Include="FluentValidation" Version="11.*" />
<PackageReference Include="Mapster" Version="7.*" />
```

---

## 2. Project Structure

```
UmbracoEcommerce/
├── src/
│   ├── UmbracoEcommerce.Core/
│   │   ├── Models/
│   │   │   ├── Domain/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── ProductVariant.cs
│   │   │   │   ├── Category.cs
│   │   │   │   ├── Cart.cs
│   │   │   │   ├── CartItem.cs
│   │   │   │   ├── Order.cs
│   │   │   │   ├── OrderLine.cs
│   │   │   │   ├── Customer.cs
│   │   │   │   ├── Address.cs
│   │   │   │   ├── Payment.cs
│   │   │   │   ├── Shipment.cs
│   │   │   │   └── Discount.cs
│   │   │   └── Dtos/
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   ├── Services/
│   │   │   └── Providers/
│   │   ├── Events/
│   │   ├── Exceptions/
│   │   └── Constants/
│   │
│   ├── UmbracoEcommerce.Infrastructure/
│   │   ├── Data/
│   │   │   ├── EcommerceDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   ├── Providers/
│   │   │   ├── Payment/
│   │   │   ├── Shipping/
│   │   │   └── Tax/
│   │   └── Caching/
│   │
│   ├── UmbracoEcommerce.Web/
│   │   ├── Composers/
│   │   ├── Controllers/
│   │   │   ├── Api/
│   │   │   ├── Backoffice/
│   │   │   └── Surface/
│   │   ├── NotificationHandlers/
│   │   ├── PropertyEditors/
│   │   ├── ContentApps/
│   │   ├── Sections/
│   │   └── wwwroot/App_Plugins/
│   │
│   └── UmbracoEcommerce.Site/
│       ├── Program.cs
│       ├── appsettings.json
│       └── Views/
│
└── tests/
```

---

## 3. Composer Registration

```csharp
// UmbracoEcommerce.Web/Composers/EcommerceComposer.cs
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbracoEcommerce.Web.Composers;

public class EcommerceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Database Context
        builder.Services.AddDbContext<EcommerceDbContext>(options =>
        {
            var connectionString = builder.Config.GetConnectionString("umbracoDbDSN");
            options.UseSqlServer(connectionString);
        });

        // Repositories
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();

        // Services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<ICheckoutService, CheckoutService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IInventoryService, InventoryService>();
        builder.Services.AddScoped<IDiscountService, DiscountService>();
        builder.Services.AddScoped<IPricingService, PricingService>();

        // Providers
        builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        builder.Services.AddScoped<IShippingProvider, FlatRateShippingProvider>();
        builder.Services.AddScoped<ITaxProvider, SimpleTaxProvider>();

        // Configuration
        builder.Services.Configure<EcommerceSettings>(
            builder.Config.GetSection("Ecommerce"));
        builder.Services.Configure<StripeSettings>(
            builder.Config.GetSection("Ecommerce:Stripe"));

        // Notification Handlers
        builder.AddNotificationHandler<ContentPublishedNotification, 
            ProductContentPublishedHandler>();
        builder.AddNotificationHandler<OrderCreatedNotification, 
            OrderCreatedNotificationHandler>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMemoryCache();
    }
}
```

---

## 4. Domain Models

### 4.1 Product

```csharp
namespace UmbracoEcommerce.Core.Models.Domain;

public class Product
{
    public Guid Id { get; set; }
    public int UmbracoNodeId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    
    // Pricing
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool TaxIncluded { get; set; }
    public string? TaxClass { get; set; }
    
    // Inventory
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public bool AllowBackorders { get; set; }
    public StockStatus StockStatus { get; set; } = StockStatus.InStock;
    
    // Physical Properties
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    
    // Organization
    public Guid? PrimaryImageId { get; set; }
    public List<Guid> ImageIds { get; set; } = [];
    public List<Guid> CategoryIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string? Brand { get; set; }
    
    // Variants
    public bool HasVariants { get; set; }
    public List<ProductVariant> Variants { get; set; } = [];
    public List<ProductAttribute> Attributes { get; set; } = [];
    
    // Status
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Computed
    public decimal CurrentPrice => SalePrice ?? BasePrice;
    public bool IsOnSale => SalePrice.HasValue && SalePrice < BasePrice;
    public bool IsInStock => !TrackInventory || StockQuantity > 0 || AllowBackorders;
}

public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Options { get; set; } = [];
    public decimal? Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public Guid? ImageId { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public Product? Product { get; set; }
}

public enum StockStatus { InStock, OutOfStock, OnBackorder, PreOrder }
public enum ProductStatus { Draft, Published, Archived }
```

### 4.2 Cart

```csharp
public class Cart
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string? SessionId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    
    public List<CartItem> Items { get; set; } = [];
    
    // Pricing
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    
    // Discounts
    public List<AppliedDiscount> AppliedDiscounts { get; set; } = [];
    public string? CouponCode { get; set; }
    
    // Shipping
    public Address? ShippingAddress { get; set; }
    public string? SelectedShippingMethod { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int ItemCount => Items.Sum(i => i.Quantity);
    public bool IsEmpty => Items.Count == 0;
}

public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public Dictionary<string, string>? VariantOptions { get; set; }
    public Guid? ImageId { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    
    public DateTime AddedAt { get; set; }
}
```

### 4.3 Order

```csharp
public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    
    // Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.Unfulfilled;
    
    // Customer Info
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }
    
    // Addresses
    public Address ShippingAddress { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
    public bool BillingSameAsShipping { get; set; } = true;
    
    // Line Items
    public List<OrderLine> Lines { get; set; } = [];
    
    // Pricing
    public string CurrencyCode { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    
    // Payment
    public string? PaymentMethod { get; set; }
    public string? PaymentIntentId { get; set; }
    public List<Payment> Payments { get; set; } = [];
    
    // Shipping
    public string? ShippingMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public List<Shipment> Shipments { get; set; } = [];
    
    // Notes
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public Dictionary<string, string>? VariantOptions { get; set; }
    
    public int Quantity { get; set; }
    public int FulfilledQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public class Address
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? StateProvince { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public enum OrderStatus { Pending, Confirmed, Processing, Shipped, Delivered, Completed, Cancelled, Refunded }
public enum PaymentStatus { Pending, Authorized, Captured, PartiallyRefunded, Refunded, Failed, Voided }
public enum FulfillmentStatus { Unfulfilled, PartiallyFulfilled, Fulfilled, Returned }
```

---

## 5. Repository Pattern

### 5.1 Product Repository Interface

```csharp
namespace UmbracoEcommerce.Core.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters parameters, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetFeaturedAsync(int count = 10, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);
}

public class ProductQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public bool? OnSale { get; set; }
    public ProductStatus? Status { get; set; }
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Newest;
}

public enum ProductSortBy { Newest, PriceLowToHigh, PriceHighToLow, Name, BestSelling }
```

### 5.2 Repository Implementation

```csharp
using Microsoft.EntityFrameworkCore;

namespace UmbracoEcommerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly EcommerceDbContext _context;

    public ProductRepository(EcommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        ProductQueryParameters parameters, 
        CancellationToken ct = default)
    {
        var query = _context.Products.Include(p => p.Variants).AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(term) ||
                p.Sku.ToLower().Contains(term));
        }

        if (parameters.CategoryId.HasValue)
            query = query.Where(p => p.CategoryIds.Contains(parameters.CategoryId.Value));

        if (!string.IsNullOrWhiteSpace(parameters.Brand))
            query = query.Where(p => p.Brand == parameters.Brand);

        if (parameters.MinPrice.HasValue)
            query = query.Where(p => p.CurrentPrice >= parameters.MinPrice.Value);

        if (parameters.MaxPrice.HasValue)
            query = query.Where(p => p.CurrentPrice <= parameters.MaxPrice.Value);

        if (parameters.InStock == true)
            query = query.Where(p => !p.TrackInventory || p.StockQuantity > 0);

        if (parameters.OnSale == true)
            query = query.Where(p => p.SalePrice.HasValue && p.SalePrice < p.BasePrice);

        if (parameters.Status.HasValue)
            query = query.Where(p => p.Status == parameters.Status.Value);

        var totalCount = await query.CountAsync(ct);

        // Apply sorting
        query = parameters.SortBy switch
        {
            ProductSortBy.PriceLowToHigh => query.OrderBy(p => p.CurrentPrice),
            ProductSortBy.PriceHighToLow => query.OrderByDescending(p => p.CurrentPrice),
            ProductSortBy.Name => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
        return product;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync([id], ct);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(ct);
        }
    }
}
```

---

## 6. Service Layer

### 6.1 Cart Service

```csharp
namespace UmbracoEcommerce.Core.Interfaces.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(CancellationToken ct = default);
    Task<CartDto> AddItemAsync(AddToCartRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemQuantityAsync(Guid itemId, int quantity, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(Guid itemId, CancellationToken ct = default);
    Task<CartDto> ClearCartAsync(CancellationToken ct = default);
    Task<CartDto> ApplyCouponAsync(string couponCode, CancellationToken ct = default);
    Task<CartDto> RemoveCouponAsync(CancellationToken ct = default);
    Task<CartDto> SetShippingAddressAsync(AddressDto address, CancellationToken ct = default);
    Task<IEnumerable<ShippingOptionDto>> GetShippingOptionsAsync(CancellationToken ct = default);
    Task<CartDto> SetShippingMethodAsync(string shippingMethodId, CancellationToken ct = default);
    Task<CartDto> RecalculateAsync(Guid cartId, CancellationToken ct = default);
}

public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; } = 1;
}
```

### 6.2 Cart Service Implementation

```csharp
namespace UmbracoEcommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDiscountService _discountService;
    private readonly IShippingService _shippingService;
    private readonly ITaxService _taxService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string CartSessionKey = "EcommerceCartId";

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IDiscountService discountService,
        IShippingService shippingService,
        ITaxService taxService,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _discountService = discountService;
        _shippingService = shippingService;
        _taxService = taxService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CartDto> AddItemAsync(AddToCartRequest request, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(ct);
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct)
            ?? throw new ProductNotFoundException(request.ProductId);

        // Check stock
        if (product.TrackInventory && !product.AllowBackorders)
        {
            var availableStock = request.VariantId.HasValue
                ? product.Variants.FirstOrDefault(v => v.Id == request.VariantId)?.StockQuantity ?? 0
                : product.StockQuantity;

            var existingQty = cart.Items
                .Where(i => i.ProductId == request.ProductId && i.VariantId == request.VariantId)
                .Sum(i => i.Quantity);

            if (existingQty + request.Quantity > availableStock)
                throw new InsufficientStockException(product.Sku, availableStock);
        }

        var variant = request.VariantId.HasValue
            ? product.Variants.FirstOrDefault(v => v.Id == request.VariantId)
            : null;

        var existingItem = cart.Items.FirstOrDefault(i => 
            i.ProductId == request.ProductId && i.VariantId == request.VariantId);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.LineTotal = existingItem.Quantity * existingItem.UnitPrice;
        }
        else
        {
            var unitPrice = variant?.Price ?? variant?.SalePrice ?? product.CurrentPrice;
            cart.Items.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                VariantId = variant?.Id,
                ProductName = product.Name,
                Sku = variant?.Sku ?? product.Sku,
                VariantName = variant?.Name,
                VariantOptions = variant?.Options,
                ImageId = variant?.ImageId ?? product.PrimaryImageId,
                Quantity = request.Quantity,
                UnitPrice = unitPrice,
                LineTotal = request.Quantity * unitPrice,
                AddedAt = DateTime.UtcNow
            });
        }

        await RecalculateTotalsAsync(cart, ct);
        await _cartRepository.UpdateAsync(cart, ct);

        return cart.Adapt<CartDto>();
    }

    private async Task RecalculateTotalsAsync(Cart cart, CancellationToken ct)
    {
        cart.Subtotal = cart.Items.Sum(i => i.LineTotal);
        cart.DiscountTotal = 0;

        if (!string.IsNullOrEmpty(cart.CouponCode))
        {
            var discount = await _discountService.CalculateDiscountAsync(cart.CouponCode, cart, ct);
            if (discount != null)
                cart.DiscountTotal = discount.Amount;
        }

        if (!string.IsNullOrEmpty(cart.SelectedShippingMethod) && cart.ShippingAddress != null)
            cart.ShippingTotal = await _shippingService.CalculateShippingAsync(cart, cart.SelectedShippingMethod, ct);

        cart.TaxTotal = await _taxService.CalculateTaxAsync(cart, ct);
        cart.GrandTotal = cart.Subtotal - cart.DiscountTotal + cart.ShippingTotal + cart.TaxTotal;
        cart.UpdatedAt = DateTime.UtcNow;
    }
}
```

---

## 7. Payment Integration

### 7.1 Payment Provider Interface

```csharp
namespace UmbracoEcommerce.Core.Interfaces.Providers;

public interface IPaymentProvider
{
    string Name { get; }
    string DisplayName { get; }
    bool SupportsRefunds { get; }

    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount, string currencyCode, PaymentIntentOptions options, CancellationToken ct = default);

    Task<PaymentResult> CapturePaymentAsync(
        string paymentIntentId, decimal? amount = null, CancellationToken ct = default);

    Task<RefundResult> RefundPaymentAsync(
        string paymentIntentId, decimal amount, string? reason = null, CancellationToken ct = default);

    Task<bool> ValidateWebhookAsync(string payload, string signature, CancellationToken ct = default);
}

public class PaymentIntentResult
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentStatus Status { get; set; }
}
```

### 7.2 Stripe Implementation

```csharp
using Microsoft.Extensions.Options;
using Stripe;

namespace UmbracoEcommerce.Infrastructure.Providers.Payment;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly StripeSettings _settings;

    public string Name => "stripe";
    public string DisplayName => "Credit Card (Stripe)";
    public bool SupportsRefunds => true;

    public StripePaymentProvider(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount, string currencyCode, PaymentIntentOptions options, CancellationToken ct = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var createOptions = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currencyCode.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                Metadata = options.Metadata,
                ReceiptEmail = options.CustomerEmail
            };

            var paymentIntent = await service.CreateAsync(createOptions, cancellationToken: ct);

            return new PaymentIntentResult
            {
                Success = true,
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret
            };
        }
        catch (StripeException ex)
        {
            return new PaymentIntentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<PaymentResult> CapturePaymentAsync(
        string paymentIntentId, decimal? amount = null, CancellationToken ct = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentCaptureOptions();
            if (amount.HasValue)
                options.AmountToCapture = (long)(amount.Value * 100);

            var paymentIntent = await service.CaptureAsync(paymentIntentId, options, cancellationToken: ct);

            return new PaymentResult
            {
                Success = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.LatestChargeId,
                Status = paymentIntent.Status == "succeeded" ? PaymentStatus.Captured : PaymentStatus.Failed
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message, Status = PaymentStatus.Failed };
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(
        string paymentIntentId, decimal amount, string? reason = null, CancellationToken ct = default)
    {
        try
        {
            var service = new RefundService();
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = (long)(amount * 100),
                Reason = "requested_by_customer"
            };

            var refund = await service.CreateAsync(options, cancellationToken: ct);

            return new RefundResult
            {
                Success = refund.Status == "succeeded",
                RefundId = refund.Id,
                RefundedAmount = refund.Amount / 100m
            };
        }
        catch (StripeException ex)
        {
            return new RefundResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public Task<bool> ValidateWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        try
        {
            EventUtility.ConstructEvent(payload, signature, _settings.WebhookSecret);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
```

---

## 8. API Controllers

### 8.1 Cart API Controller

```csharp
using Microsoft.AspNetCore.Mvc;

namespace UmbracoEcommerce.Web.Controllers.Api;

[ApiController]
[Route("api/ecommerce/cart")]
public class CartApiController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartApiController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(ct);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        try
        {
            var cart = await _cartService.AddItemAsync(request, ct);
            return Ok(cart);
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { error = ex.Message, availableStock = ex.AvailableStock });
        }
    }

    [HttpPut("items/{itemId}")]
    public async Task<ActionResult<CartDto>> UpdateItemQuantity(Guid itemId, [FromBody] UpdateQuantityRequest request, CancellationToken ct)
    {
        try
        {
            var cart = await _cartService.UpdateItemQuantityAsync(itemId, request.Quantity, ct);
            return Ok(cart);
        }
        catch (CartItemNotFoundException)
        {
            return NotFound(new { error = "Cart item not found" });
        }
    }

    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid itemId, CancellationToken ct)
    {
        var cart = await _cartService.RemoveItemAsync(itemId, ct);
        return Ok(cart);
    }

    [HttpPost("coupon")]
    public async Task<ActionResult<CartDto>> ApplyCoupon([FromBody] ApplyCouponRequest request, CancellationToken ct)
    {
        try
        {
            var cart = await _cartService.ApplyCouponAsync(request.Code, ct);
            return Ok(cart);
        }
        catch (InvalidCouponException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("shipping-options")]
    public async Task<ActionResult<IEnumerable<ShippingOptionDto>>> GetShippingOptions(CancellationToken ct)
    {
        var options = await _cartService.GetShippingOptionsAsync(ct);
        return Ok(options);
    }
}
```

### 8.2 Checkout API Controller

```csharp
[ApiController]
[Route("api/ecommerce/checkout")]
public class CheckoutApiController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutApiController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost("initialize")]
    public async Task<ActionResult<CheckoutSessionDto>> Initialize(CancellationToken ct)
    {
        var session = await _checkoutService.InitializeCheckoutAsync(ct);
        return Ok(session);
    }

    [HttpPut("{sessionId}/shipping-address")]
    public async Task<ActionResult<CheckoutSessionDto>> UpdateShippingAddress(
        Guid sessionId, [FromBody] AddressDto address, CancellationToken ct)
    {
        var session = await _checkoutService.UpdateShippingAddressAsync(sessionId, address, ct);
        return Ok(session);
    }

    [HttpPost("{sessionId}/payment-intent")]
    public async Task<ActionResult<PaymentIntentDto>> CreatePaymentIntent(Guid sessionId, CancellationToken ct)
    {
        var paymentIntent = await _checkoutService.CreatePaymentIntentAsync(sessionId, ct);
        return Ok(paymentIntent);
    }

    [HttpPost("{sessionId}/complete")]
    public async Task<ActionResult<OrderDto>> Complete(
        Guid sessionId, [FromBody] CompleteCheckoutRequest request, CancellationToken ct)
    {
        try
        {
            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var order = await _checkoutService.CompleteCheckoutAsync(sessionId, request, ct);
            return Ok(order);
        }
        catch (CheckoutValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (PaymentFailedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

---

## 9. Backoffice Extensions

### 9.1 Commerce Section

```csharp
using Umbraco.Cms.Core.Sections;

namespace UmbracoEcommerce.Web.Sections;

public class CommerceSection : ISection
{
    public string Alias => "commerce";
    public string Name => "Commerce";
}
```

### 9.2 Order Tree Controller

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;

namespace UmbracoEcommerce.Web.Controllers.Backoffice.Trees;

[Tree("commerce", "orders", TreeTitle = "Orders", SortOrder = 1)]
public class OrderTreeController : TreeController
{
    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var nodes = new TreeNodeCollection();

        if (id == Constants.System.RootString)
        {
            nodes.Add(CreateTreeNode("pending", id, queryStrings, "Pending", "icon-hourglass", false));
            nodes.Add(CreateTreeNode("processing", id, queryStrings, "Processing", "icon-loading", false));
            nodes.Add(CreateTreeNode("shipped", id, queryStrings, "Shipped", "icon-truck", false));
            nodes.Add(CreateTreeNode("completed", id, queryStrings, "Completed", "icon-check", false));
            nodes.Add(CreateTreeNode("cancelled", id, queryStrings, "Cancelled", "icon-delete", false));
        }

        return nodes;
    }

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
    {
        return new MenuItemCollection();
    }
}
```

### 9.3 Package Manifest (umbraco-package.json)

```json
{
    "$schema": "../../umbraco-package-schema.json",
    "id": "umbraco-ecommerce",
    "name": "Umbraco Ecommerce",
    "version": "1.0.0",
    "extensions": [
        {
            "type": "section",
            "alias": "Ecommerce.Section.Commerce",
            "name": "Commerce Section",
            "element": "/App_Plugins/UmbracoEcommerce/sections/commerce-section.element.js",
            "meta": {
                "label": "Commerce",
                "pathname": "commerce"
            }
        },
        {
            "type": "sectionView",
            "alias": "Ecommerce.SectionView.Orders",
            "name": "Orders View",
            "element": "/App_Plugins/UmbracoEcommerce/views/orders-view.element.js",
            "meta": {
                "label": "Orders",
                "pathname": "orders",
                "icon": "icon-shopping-basket"
            },
            "conditions": [{ "alias": "Umb.Condition.SectionAlias", "match": "commerce" }]
        },
        {
            "type": "dashboard",
            "alias": "Ecommerce.Dashboard.Sales",
            "name": "Sales Dashboard",
            "element": "/App_Plugins/UmbracoEcommerce/dashboards/sales-dashboard.element.js",
            "meta": { "label": "Sales Overview" },
            "conditions": [{ "alias": "Umb.Condition.SectionAlias", "match": "commerce" }]
        },
        {
            "type": "contentApp",
            "alias": "Ecommerce.ContentApp.ProductStock",
            "name": "Product Stock",
            "element": "/App_Plugins/UmbracoEcommerce/content-apps/product-stock.element.js",
            "meta": { "label": "Stock", "icon": "icon-box" },
            "conditions": [{ "alias": "Umb.Condition.WorkspaceContentTypeAlias", "match": "product" }]
        }
    ]
}
```

---

## 10. Entity Framework Configuration

### 10.1 DbContext

```csharp
using Microsoft.EntityFrameworkCore;

namespace UmbracoEcommerce.Infrastructure.Data;

public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcommerceDbContext).Assembly);
    }
}
```

### 10.2 Product Configuration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UmbracoEcommerce.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("EcommerceProducts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Slug).HasMaxLength(500);
        builder.Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Slug);
        builder.HasIndex(p => p.UmbracoNodeId);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## 11. Frontend JavaScript

```javascript
// wwwroot/js/ecommerce/cart.js
class EcommerceCart {
    constructor() {
        this.apiBase = '/api/ecommerce/cart';
        this.cart = null;
        this.listeners = [];
        this.init();
    }
    
    async init() {
        await this.loadCart();
        this.bindEvents();
    }
    
    async loadCart() {
        const response = await fetch(this.apiBase);
        this.cart = await response.json();
        this.notifyListeners();
    }
    
    async addItem(productId, variantId = null, quantity = 1) {
        const response = await fetch(`${this.apiBase}/items`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId, variantId, quantity })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error);
        }
        
        this.cart = await response.json();
        this.notifyListeners();
        return this.cart;
    }
    
    async updateQuantity(itemId, quantity) {
        const response = await fetch(`${this.apiBase}/items/${itemId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ quantity })
        });
        
        this.cart = await response.json();
        this.notifyListeners();
        return this.cart;
    }
    
    async removeItem(itemId) {
        const response = await fetch(`${this.apiBase}/items/${itemId}`, { method: 'DELETE' });
        this.cart = await response.json();
        this.notifyListeners();
        return this.cart;
    }
    
    async applyCoupon(code) {
        const response = await fetch(`${this.apiBase}/coupon`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ code })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error);
        }
        
        this.cart = await response.json();
        this.notifyListeners();
        return this.cart;
    }
    
    subscribe(callback) {
        this.listeners.push(callback);
        return () => { this.listeners = this.listeners.filter(l => l !== callback); };
    }
    
    notifyListeners() {
        this.listeners.forEach(cb => cb(this.cart));
    }
    
    bindEvents() {
        document.addEventListener('click', async (e) => {
            const btn = e.target.closest('[data-add-to-cart]');
            if (btn) {
                e.preventDefault();
                const { productId, variantId, quantity } = btn.dataset;
                await this.addItem(productId, variantId || null, parseInt(quantity || '1'));
            }
        });
    }
    
    get itemCount() { return this.cart?.itemCount || 0; }
    get total() { return this.cart?.grandTotal || 0; }
    get isEmpty() { return !this.cart || this.cart.items.length === 0; }
}

window.ecommerceCart = new EcommerceCart();
```

---

## 12. Configuration (appsettings.json)

```json
{
  "Ecommerce": {
    "Store": {
      "Name": "My Store",
      "DefaultCurrency": "USD",
      "PricesIncludeTax": false
    },
    "Stripe": {
      "PublishableKey": "pk_test_...",
      "SecretKey": "sk_test_...",
      "WebhookSecret": "whsec_..."
    },
    "Shipping": {
      "StandardRate": 5.99,
      "ExpressRate": 12.99,
      "FreeShippingThreshold": 50.00
    },
    "Tax": {
      "DefaultRate": 0.0,
      "TaxShipping": false,
      "TaxRates": [
        { "CountryCode": "US", "StateCode": "NY", "Rate": 0.08875 },
        { "CountryCode": "US", "StateCode": "CA", "Rate": 0.0725 }
      ]
    },
    "Email": {
      "SmtpHost": "smtp.example.com",
      "SmtpPort": 587,
      "FromEmail": "orders@example.com",
      "FromName": "My Store"
    },
    "Inventory": {
      "LowStockThreshold": 5,
      "AllowBackorders": false
    },
    "Cart": {
      "ExpirationDays": 30
    }
  }
}
```

---

## 13. Notification Handlers

```csharp
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UmbracoEcommerce.Web.NotificationHandlers;

public class OrderCreatedNotificationHandler : INotificationAsyncHandler<OrderCreatedNotification>
{
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;

    public OrderCreatedNotificationHandler(
        IOrderService orderService,
        IEmailService emailService,
        IInventoryService inventoryService)
    {
        _orderService = orderService;
        _emailService = emailService;
        _inventoryService = inventoryService;
    }

    public async Task HandleAsync(OrderCreatedNotification notification, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(notification.OrderId, ct);
        if (order == null) return;

        // Send confirmation email
        await _emailService.SendOrderConfirmationAsync(order, ct);

        // Commit inventory reservation
        await _inventoryService.CommitReservationAsync(order.Id, ct);
    }
}

public class OrderCreatedNotification : INotification
{
    public Guid OrderId { get; }
    public OrderCreatedNotification(Guid orderId) => OrderId = orderId;
}
```

---

## Quick Reference Commands

```bash
# Create migration
dotnet ef migrations add InitialCreate -p src/UmbracoEcommerce.Infrastructure -s src/UmbracoEcommerce.Site

# Update database
dotnet ef database update -p src/UmbracoEcommerce.Infrastructure -s src/UmbracoEcommerce.Site

# Run tests
dotnet test

# Build packages
dotnet pack -c Release
```

---

*This guide provides patterns for building e-commerce plugins for Umbraco 15+. Adapt to your specific requirements.*
