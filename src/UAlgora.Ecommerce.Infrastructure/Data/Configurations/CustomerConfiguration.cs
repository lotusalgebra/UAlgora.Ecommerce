using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Customer.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Company)
            .HasMaxLength(200);

        builder.Property(c => c.TaxNumber)
            .HasMaxLength(100);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.PreferredCurrency)
            .HasMaxLength(10);

        builder.Property(c => c.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(c => c.Timezone)
            .HasMaxLength(100);

        builder.Property(c => c.TotalSpent)
            .HasPrecision(18, 4);

        builder.Property(c => c.AverageOrderValue)
            .HasPrecision(18, 4);

        builder.Property(c => c.StoreCreditBalance)
            .HasPrecision(18, 4);

        builder.Property(c => c.CustomerTier)
            .HasMaxLength(100);

        builder.Property(c => c.Source)
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasMaxLength(4000);

        // JSON for tags
        builder.Property(c => c.Tags)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Phone);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CustomerTier);
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.LastOrderAt);
        builder.HasIndex(c => c.LastLoginAt);
        builder.HasIndex(c => c.TotalSpent);
        builder.HasIndex(c => c.UmbracoMemberId);
        builder.HasIndex(c => new { c.LastName, c.FirstName });
    }
}
