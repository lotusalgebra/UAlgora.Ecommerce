using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Infrastructure;
using UAlgora.Ecommerce.Web;
using UAlgora.Ecommerce.Site.Data;
using UAlgora.Ecommerce.Infrastructure.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Workaround for Umbraco 15.4.4 service scope validation bug
// See: https://github.com/umbraco/Umbraco-CMS/issues/
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});

// Add E-commerce services
var connectionString = builder.Configuration.GetConnectionString("umbracoDbDSN")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=UAlgora.Ecommerce;Trusted_Connection=True;";
builder.Services.AddEcommerceInfrastructure(connectionString);
builder.Services.AddEcommerceWeb();
builder.Services.AddScoped<DemoDataSeeder>();

// Add MVC for storefront controllers
builder.Services.AddControllersWithViews();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

// Apply e-commerce migrations and seed demo data
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Apply pending migrations - let errors propagate so we can debug
    var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();

    // Get pending migrations
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    logger.LogInformation("Pending e-commerce migrations: {Count} - {Migrations}",
        pendingMigrations.Count(),
        string.Join(", ", pendingMigrations));

    logger.LogInformation("Applying database migrations...");
    await dbContext.Database.MigrateAsync();
    logger.LogInformation("Database migrations applied successfully.");

    // Seed demo data
    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    await seeder.SeedAsync();
    logger.LogInformation("Demo data seeded successfully.");
}

await app.BootUmbracoAsync();

// Use routing for MVC controllers
app.UseRouting();

// Configure Umbraco - BackOffice middleware only, but all endpoints (needed for install)
app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        // Note: We intentionally DON'T call UseWebsite() here
        // because our StorefrontController handles all frontend routes
    })
    .WithEndpoints(u =>
    {
        // Map our storefront controllers BEFORE Umbraco endpoints
        // This ensures our routes take precedence
        u.EndpointRouteBuilder.MapControllers();

        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints(); // Needed for Umbraco install/upgrade process
    });

await app.RunAsync();
