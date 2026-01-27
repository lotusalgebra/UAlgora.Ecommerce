# Architecture Overview

Technical architecture guide for developers working with Algora Commerce.

## Clean Architecture

Algora Commerce follows Clean Architecture principles with four distinct layers:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│              (UAlgora.Ecommerce.Web)                    │
│    Controllers, Views, Backoffice UI, Umbraco Integration│
├─────────────────────────────────────────────────────────┤
│                   Application Layer                      │
│           (UAlgora.Ecommerce.Infrastructure)            │
│         Services, Repositories, Providers               │
├─────────────────────────────────────────────────────────┤
│                     Domain Layer                         │
│              (UAlgora.Ecommerce.Core)                   │
│       Entities, Interfaces, Events, Exceptions          │
├─────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                    │
│           (UAlgora.Ecommerce.Infrastructure)            │
│    EF Core, Database, External Services, Caching        │
└─────────────────────────────────────────────────────────┘
```

## Project Structure

```
src/
├── UAlgora.Ecommerce.Core/
│   ├── Models/
│   │   └── Domain/           # Entity classes
│   │       ├── Product.cs
│   │       ├── ProductVariant.cs
│   │       ├── Category.cs
│   │       ├── Cart.cs
│   │       ├── CartItem.cs
│   │       ├── Order.cs
│   │       ├── OrderLine.cs
│   │       ├── Customer.cs
│   │       ├── Address.cs
│   │       ├── Discount.cs
│   │       ├── Payment.cs
│   │       ├── Shipment.cs
│   │       └── ...
│   ├── Interfaces/
│   │   ├── Repositories/     # Data access contracts
│   │   │   ├── IProductRepository.cs
│   │   │   ├── IOrderRepository.cs
│   │   │   └── ...
│   │   ├── Services/         # Business logic contracts
│   │   │   ├── ICartService.cs
│   │   │   ├── ICheckoutService.cs
│   │   │   └── ...
│   │   └── Providers/        # External service contracts
│   │       ├── IPaymentProvider.cs
│   │       ├── IShippingProvider.cs
│   │       └── ITaxProvider.cs
│   ├── Events/               # Domain events
│   ├── Exceptions/           # Custom exceptions
│   └── Constants/            # Enums and constants
│
├── UAlgora.Ecommerce.Infrastructure/
│   ├── Data/
│   │   ├── EcommerceDbContext.cs
│   │   ├── Configurations/   # EF Core entity configs
│   │   └── Migrations/
│   ├── Repositories/         # Repository implementations
│   ├── Services/             # Service implementations
│   ├── Providers/
│   │   ├── Payment/          # Stripe, Razorpay, etc.
│   │   ├── Shipping/         # Carrier integrations
│   │   └── Tax/              # Tax calculators
│   └── Caching/
│
├── UAlgora.Ecommerce.Web/
│   ├── Composers/            # Umbraco DI registration
│   ├── Controllers/
│   │   ├── Api/              # Storefront API
│   │   └── Backoffice/       # Management API
│   ├── NotificationHandlers/ # Umbraco events
│   ├── DocumentTypes/        # CMS content types
│   ├── ContentApps/          # Backoffice panels
│   └── wwwroot/App_Plugins/  # Backoffice UI
│
├── UAlgora.Ecommerce.Site/   # Demo Umbraco site
│
└── UAlgora.Ecommerce.LicensePortal/  # License management
```

## Core Domain Entities

### Entity Relationships

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Product   │────<│  Variant    │     │  Category   │
└─────────────┘     └─────────────┘     └─────────────┘
       │                                       │
       └───────────────────┬───────────────────┘
                           │ many-to-many
                    ┌──────┴──────┐
                    │ProductCategory│
                    └─────────────┘

┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Customer   │────<│    Order    │────<│  OrderLine  │
└─────────────┘     └─────────────┘     └─────────────┘
       │                   │
       │                   ├────<┌─────────────┐
       │                   │     │   Payment   │
       │                   │     └─────────────┘
       │                   │
       │                   └────<┌─────────────┐
       │                         │  Shipment   │
       │                         └─────────────┘
       │
       └────<┌─────────────┐     ┌─────────────┐
             │    Cart     │────<│  CartItem   │
             └─────────────┘     └─────────────┘
```

### Base Entity

All entities inherit from `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }  // Soft delete
}
```

## Repository Pattern

### Interface Definition

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters parameters, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### Implementation

```csharp
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
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
    }

    // ... other methods
}
```

## Service Layer

Services encapsulate business logic:

```csharp
public interface ICartService
{
    Task<CartDto> GetCartAsync(CancellationToken ct = default);
    Task<CartDto> AddItemAsync(AddToCartRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemQuantityAsync(Guid itemId, int quantity, CancellationToken ct = default);
    Task<CartDto> ApplyCouponAsync(string code, CancellationToken ct = default);
}

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDiscountService _discountService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Business logic implementation
}
```

## Provider Pattern

External integrations use the provider pattern for flexibility:

### Payment Provider

```csharp
public interface IPaymentProvider
{
    string Name { get; }
    string DisplayName { get; }
    bool SupportsRefunds { get; }

    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currencyCode,
        PaymentIntentOptions options,
        CancellationToken ct = default);

    Task<PaymentResult> CapturePaymentAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken ct = default);

    Task<RefundResult> RefundPaymentAsync(
        string paymentIntentId,
        decimal amount,
        string? reason = null,
        CancellationToken ct = default);
}
```

### Registering Custom Providers

```csharp
// In your Composer or Program.cs
builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
builder.Services.AddScoped<IPaymentProvider, RazorpayPaymentProvider>();
builder.Services.AddScoped<IPaymentProvider, MyCustomPaymentProvider>();
```

## Database Layer

### EF Core DbContext

```csharp
public class EcommerceDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
    // ... more DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcommerceDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        // ...
    }
}
```

### Entity Configuration

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("EcommerceProducts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(500);
        builder.Property(p => p.BasePrice).HasColumnType("decimal(18,2)");

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Slug);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Table Naming Convention

All tables are prefixed with `Ecommerce`:

| Entity | Table Name |
|--------|------------|
| Product | EcommerceProducts |
| Category | EcommerceCategories |
| Order | EcommerceOrders |
| Customer | EcommerceCustomers |
| Cart | EcommerceCarts |

## Dependency Injection

### Service Registration

```csharp
// Infrastructure layer
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEcommerceInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // Database
        services.AddDbContext<EcommerceDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        // ...

        // Services
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IOrderService, OrderService>();
        // ...

        return services;
    }
}
```

### Umbraco Composer

```csharp
public class EcommerceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var connectionString = builder.Config.GetConnectionString("umbracoDbDSN");

        builder.Services.AddEcommerceInfrastructure(connectionString);
        builder.Services.AddEcommerceWeb();

        // Notification handlers
        builder.AddNotificationHandler<ContentPublishedNotification,
            ProductContentPublishedHandler>();
    }
}
```

## Event System

### Domain Events

```csharp
public class OrderCreatedEvent : INotification
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(Guid orderId, string orderNumber, decimal totalAmount)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
    }
}
```

### Event Handlers

```csharp
public class OrderCreatedHandler : INotificationAsyncHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;

    public async Task HandleAsync(OrderCreatedEvent notification, CancellationToken ct)
    {
        // Send confirmation email
        await _emailService.SendOrderConfirmationAsync(notification.OrderId, ct);

        // Commit inventory reservation
        await _inventoryService.CommitReservationAsync(notification.OrderId, ct);
    }
}
```

## API Controllers

### Storefront API

```csharp
[ApiController]
[Route("api/ecommerce/cart")]
public class CartApiController : ControllerBase
{
    private readonly ICartService _cartService;

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(ct);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(
        [FromBody] AddToCartRequest request,
        CancellationToken ct)
    {
        var cart = await _cartService.AddItemAsync(request, ct);
        return Ok(cart);
    }
}
```

### Management API

```csharp
[ApiController]
[ApiExplorerSettings(GroupName = "Ecommerce")]
[Authorize(Policy = "EcommerceBackoffice")]
public class ProductManagementApiController : EcommerceManagementApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] ProductQueryParameters parameters,
        CancellationToken ct)
    {
        // ...
    }
}
```

## Umbraco Integration

### Content Sync

Bidirectional sync between database and Umbraco content:

```csharp
public class ProductContentSyncService : IContentSyncService<Product>
{
    public async Task SyncToContentAsync(Product product)
    {
        // Create/update Umbraco content node from product
    }

    public async Task SyncFromContentAsync(IContent content)
    {
        // Update product from Umbraco content node
    }
}
```

### Document Types

```csharp
public class ProductDocumentTypeProvider : IDocumentTypeDefinitionProvider
{
    public string Alias => "product";
    public string Name => "Product";

    public IEnumerable<PropertyType> GetPropertyTypes()
    {
        yield return new PropertyType("sku", "Textstring");
        yield return new PropertyType("price", "Decimal");
        yield return new PropertyType("description", "RichText");
    }
}
```

## Caching Strategy

```csharp
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"product:{id}";

        if (_cache.TryGetValue(cacheKey, out Product? product))
            return product;

        product = await _inner.GetByIdAsync(id, ct);

        if (product != null)
            _cache.Set(cacheKey, product, _cacheExpiry);

        return product;
    }
}
```

## Testing

### Unit Tests

```csharp
public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepo;
    private readonly Mock<IProductRepository> _productRepo;
    private readonly CartService _service;

    [Fact]
    public async Task AddItem_ValidProduct_AddsToCart()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), StockQuantity = 10 };
        _productRepo.Setup(x => x.GetByIdAsync(product.Id, default))
            .ReturnsAsync(product);

        // Act
        var result = await _service.AddItemAsync(new AddToCartRequest
        {
            ProductId = product.Id,
            Quantity = 2
        });

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);
    }
}
```

---

## Related Documentation

- [API Reference](./api-reference.md)
- [Extending the Plugin](./extending.md)
- [Custom Payment Providers](./payment-providers.md)
