using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Product.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Slug)
            .HasMaxLength(500);

        builder.Property(p => p.Sku)
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(1000);

        builder.Property(p => p.BasePrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.SalePrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.CompareAtPrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.CurrencyCode)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(p => p.TaxClass)
            .HasMaxLength(100);

        builder.Property(p => p.Weight)
            .HasPrecision(18, 4);

        builder.Property(p => p.WeightUnit)
            .HasMaxLength(10);

        builder.Property(p => p.Length)
            .HasPrecision(18, 4);

        builder.Property(p => p.Width)
            .HasPrecision(18, 4);

        builder.Property(p => p.Height)
            .HasPrecision(18, 4);

        builder.Property(p => p.DimensionUnit)
            .HasMaxLength(10);

        builder.Property(p => p.PrimaryImageUrl)
            .HasMaxLength(1000);

        builder.Property(p => p.Brand)
            .HasMaxLength(200);

        builder.Property(p => p.Manufacturer)
            .HasMaxLength(200);

        builder.Property(p => p.Mpn)
            .HasMaxLength(100);

        builder.Property(p => p.Gtin)
            .HasMaxLength(100);

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(200);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.MetaKeywords)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.StockStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        // JSON columns for collections
        builder.Property(p => p.ImageIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.CategoryIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Tags)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for product attributes (not entities)
        builder.Property(p => p.Attributes)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<ProductAttribute>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<ProductAttribute>())
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore navigation properties that are computed/manual
        builder.Ignore(p => p.Categories);

        // Indexes
        builder.HasIndex(p => p.Slug);
        builder.HasIndex(p => p.Sku);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsVisible);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.UmbracoNodeId);
        builder.HasIndex(p => new { p.IsVisible, p.Status });
    }
}

/// <summary>
/// Entity configuration for ProductVariant.
/// </summary>
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Name)
            .HasMaxLength(500);

        builder.Property(v => v.Price)
            .HasPrecision(18, 4);

        builder.Property(v => v.SalePrice)
            .HasPrecision(18, 4);

        builder.Property(v => v.CostPrice)
            .HasPrecision(18, 4);

        builder.Property(v => v.Weight)
            .HasPrecision(18, 4);

        builder.Property(v => v.Gtin)
            .HasMaxLength(100);

        builder.Property(v => v.ImageUrl)
            .HasMaxLength(1000);

        // JSON for options
        builder.Property(v => v.Options)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(v => v.Sku);
        builder.HasIndex(v => v.ProductId);
        builder.HasIndex(v => v.Gtin);
        builder.HasIndex(v => new { v.ProductId, v.Sku }).IsUnique();
    }
}
