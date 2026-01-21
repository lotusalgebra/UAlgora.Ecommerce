using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UAlgora.Ecommerce.Infrastructure;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UAlgora.Ecommerce.Web.Composers;

/// <summary>
/// Umbraco composer that registers all e-commerce services.
/// This composer runs during Umbraco startup and configures DI for the entire e-commerce system.
/// </summary>
public class EcommerceComposer : IComposer
{
    /// <summary>
    /// Composes the e-commerce services into the Umbraco DI container.
    /// </summary>
    public void Compose(IUmbracoBuilder builder)
    {
        // Get connection string from configuration
        var connectionString = builder.Config.GetConnectionString("EcommerceDb");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fall back to default Umbraco connection string if ecommerce-specific one isn't configured
            connectionString = builder.Config.GetConnectionString("umbracoDbDSN");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "No database connection string found. Please configure 'EcommerceDb' or 'umbracoDbDSN' in appsettings.json.");
        }

        // Register infrastructure layer (DbContext, repositories, services)
        builder.Services.AddEcommerceInfrastructure(connectionString);

        // Register web layer (HttpCartContextProvider, etc.)
        builder.Services.AddEcommerceWeb();

        // Register authorization (admin API protection)
        builder.Services.AddEcommerceAuthorization(builder.Config);
    }
}
