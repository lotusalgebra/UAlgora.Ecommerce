using UAlgora.Ecommerce.Infrastructure;
using UAlgora.Ecommerce.Web;
using UAlgora.Ecommerce.Site.Data;
using UAlgora.Ecommerce.Infrastructure.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not seed demo data. This is normal on first run before database is created.");
    }
}

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();

        // Map storefront controllers
        u.EndpointRouteBuilder.MapControllerRoute(
            name: "default",
            pattern: "{controller=Storefront}/{action=Index}/{id?}");
    });

await app.RunAsync();
