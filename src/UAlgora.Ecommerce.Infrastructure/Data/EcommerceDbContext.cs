using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for the e-commerce system.
/// </summary>
public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Suppress pending model changes warning for migrations
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    // Products
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();

    // Cart
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    // Customers
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();

    // Discounts
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<DiscountUsage> DiscountUsages => Set<DiscountUsage>();

    // Payments
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StoredPaymentMethod> StoredPaymentMethods => Set<StoredPaymentMethod>();
    public DbSet<PaymentMethodConfig> PaymentMethodConfigs => Set<PaymentMethodConfig>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();

    // Shipments
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();

    // Shipping Configuration
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();

    // Tax Configuration
    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();
    public DbSet<TaxZone> TaxZones => Set<TaxZone>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();

    // Reviews & Wishlists
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Inventory & Warehouse Management
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseStock> WarehouseStocks => Set<WarehouseStock>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentItem> StockAdjustmentItems => Set<StockAdjustmentItem>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierProduct> SupplierProducts => Set<SupplierProduct>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    // Currency Management
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    // Multi-Store
    public DbSet<Store> Stores => Set<Store>();

    // Gift Cards
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<GiftCardTransaction> GiftCardTransactions => Set<GiftCardTransaction>();

    // Returns & Refunds
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();

    // Email Templates
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    // Audit Logging
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Licensing
    public DbSet<License> Licenses => Set<License>();

    // Webhooks
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();

    // Payment Links
    public DbSet<PaymentLink> PaymentLinks => Set<PaymentLink>();
    public DbSet<PaymentLinkPayment> PaymentLinkPayments => Set<PaymentLinkPayment>();

    // Checkout Steps
    public DbSet<CheckoutStepConfiguration> CheckoutSteps => Set<CheckoutStepConfiguration>();

    // Geography
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<StateProvince> StateProvinces => Set<StateProvince>();

    // Invoices
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceTemplate> InvoiceTemplates => Set<InvoiceTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcommerceDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(SoftDeleteEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}
