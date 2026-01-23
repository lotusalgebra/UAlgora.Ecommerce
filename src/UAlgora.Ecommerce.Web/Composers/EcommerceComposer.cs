using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UAlgora.Ecommerce.Infrastructure;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UAlgora.Ecommerce.Web.Composers;

/// <summary>
/// Algora Commerce Composer
///
/// Umbraco composer that registers all Algora Commerce services.
/// This composer runs during Umbraco startup and configures DI for the entire commerce system.
///
/// Architecture:
/// - Registers Infrastructure layer (DbContext, repositories, services)
/// - Registers Web layer (HttpCartContextProvider, etc.)
/// - Registers Authorization (admin API protection)
///
/// Note: Document types are handled by AlgoraDocumentTypeComposer (Clean Architecture)
/// </summary>
public class EcommerceComposer : IComposer
{
    /// <summary>
    /// Composes the Algora Commerce services into the Umbraco DI container.
    /// </summary>
    public void Compose(IUmbracoBuilder builder)
    {
        // Get connection string from configuration
        var connectionString = builder.Config.GetConnectionString("EcommerceDb");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fall back to default Umbraco connection string if Algora-specific one isn't configured
            connectionString = builder.Config.GetConnectionString("umbracoDbDSN");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Algora Commerce: No database connection string found. Please configure 'EcommerceDb' or 'umbracoDbDSN' in appsettings.json.");
        }

        // Register infrastructure layer (DbContext, repositories, services)
        builder.Services.AddEcommerceInfrastructure(connectionString);

        // Register web layer (HttpCartContextProvider, etc.)
        builder.Services.AddEcommerceWeb();

        // Register authorization (admin API protection)
        builder.Services.AddEcommerceAuthorization(builder.Config);

        // Note: Document type installation is handled by AlgoraDocumentTypeComposer
        // which follows Clean Architecture patterns with proper separation of concerns
    }
}
