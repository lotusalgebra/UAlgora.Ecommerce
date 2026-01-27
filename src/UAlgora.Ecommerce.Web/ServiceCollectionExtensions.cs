using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Web.Authorization;
using UAlgora.Ecommerce.Web.Licensing;
using UAlgora.Ecommerce.Web.Providers;
using UAlgora.Ecommerce.Web.Services;

namespace UAlgora.Ecommerce.Web;

/// <summary>
/// Extension methods for registering e-commerce web services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Policy name for e-commerce admin access.
    /// </summary>
    public const string EcommerceAdminPolicy = "EcommerceAdmin";

    /// <summary>
    /// Adds e-commerce web layer services including HTTP context providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEcommerceWeb(this IServiceCollection services)
    {
        // Ensure HttpContextAccessor is registered
        services.AddHttpContextAccessor();

        // Register cart context provider
        services.AddScoped<ICartContextProvider, HttpCartContextProvider>();

        // Register content sync services for bidirectional sync
        // Database ↔ Umbraco Content Tree
        services.AddScoped<ProductContentSyncService>();
        services.AddScoped<CategoryContentSyncService>();

        // Register catalog content seeder for demo/testing
        services.AddScoped<CatalogContentSeeder>();
        services.AddScoped<EnterpriseContentSeeder>();

        // Register PDF generation service
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();

        // Register license context (singleton to hold current license state)
        services.AddSingleton<LicenseContext>();

        return services;
    }

    /// <summary>
    /// Adds Algora Commerce licensing services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAlgoraLicensing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure license options from configuration
        services.Configure<LicenseOptions>(configuration.GetSection(LicenseOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Adds e-commerce authorization services and policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEcommerceAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure authorization options from configuration
        services.Configure<EcommerceAuthorizationOptions>(
            configuration.GetSection(EcommerceAuthorizationOptions.SectionName));

        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, EcommerceAdminAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, EcommerceAdminApiKeyHandler>();

        // Add API key authentication scheme
        services.AddAuthentication()
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                EcommerceAuthenticationSchemes.ApiKey,
                options => { });

        // Configure authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy(EcommerceAdminPolicy, policy =>
            {
                policy.AddRequirements(new EcommerceAdminRequirement());
                policy.AddAuthenticationSchemes(
                    EcommerceAuthenticationSchemes.ApiKey,
                    "Umbraco.BackOffice.Cookie");
            });

        return services;
    }
}
