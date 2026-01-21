using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UAlgora.Ecommerce.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating EcommerceDbContext during migrations.
/// This factory is used by EF Core tools (dotnet ef migrations) when no startup project is available.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EcommerceDbContext>
{
    public EcommerceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EcommerceDbContext>();

        // Use a placeholder connection string for design-time operations
        // This will be replaced by the actual connection string at runtime
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=UAlgora_Ecommerce_Dev;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(EcommerceDbContext).Assembly.FullName);
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ecommerce");
        });

        return new EcommerceDbContext(optionsBuilder.Options);
    }
}
