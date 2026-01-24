using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UAlgora.Ecommerce.Web.Licensing;

/// <summary>
/// Extension methods for adding license validation services.
/// </summary>
public static class LicenseServiceCollectionExtensions
{
    /// <summary>
    /// Adds Algora Commerce license validation services.
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

        // Register license context as singleton (holds current license state)
        services.AddSingleton<LicenseContext>();

        return services;
    }

    /// <summary>
    /// Adds the license validation middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseAlgoraLicenseValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LicenseValidationMiddleware>();
    }
}
