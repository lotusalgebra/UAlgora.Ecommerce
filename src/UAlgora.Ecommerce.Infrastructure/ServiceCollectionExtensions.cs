using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Infrastructure.Data;
using UAlgora.Ecommerce.Infrastructure.Repositories;
using UAlgora.Ecommerce.Infrastructure.Services;

namespace UAlgora.Ecommerce.Infrastructure;

/// <summary>
/// Extension methods for registering e-commerce services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all e-commerce infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEcommerceInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext
        services.AddDbContext<EcommerceDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(EcommerceDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Register repositories
        services.AddRepositories();

        // Register services
        services.AddServices();

        return services;
    }

    /// <summary>
    /// Adds all repository implementations to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();

        // Enterprise feature repositories
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IGiftCardRepository, GiftCardRepository>();
        services.AddScoped<IReturnRepository, ReturnRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();
        services.AddScoped<IPaymentLinkRepository, PaymentLinkRepository>();

        return services;
    }

    /// <summary>
    /// Adds all service implementations to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IShippingService, ShippingService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IWishlistService, WishlistService>();

        // Enterprise feature services
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IGiftCardService, GiftCardService>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IPaymentLinkService, PaymentLinkService>();

        // Register HttpClient for webhook delivery
        services.AddHttpClient("WebhookClient")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 3
            });

        return services;
    }

    /// <summary>
    /// Adds e-commerce infrastructure with a pre-configured DbContext.
    /// Use this when the DbContext is already registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEcommerceServices(this IServiceCollection services)
    {
        services.AddRepositories();
        services.AddServices();

        return services;
    }
}
