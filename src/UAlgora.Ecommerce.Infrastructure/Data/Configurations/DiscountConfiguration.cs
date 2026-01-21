using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Discount.
/// </summary>
public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code)
            .HasMaxLength(100);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Scope)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Value)
            .HasPrecision(18, 4);

        builder.Property(d => d.MaxDiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(d => d.MinimumOrderAmount)
            .HasPrecision(18, 4);

        // JSON for applicable categories
        builder.Property(d => d.ApplicableCategoryIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        // JSON for applicable products
        builder.Property(d => d.ApplicableProductIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        // JSON for eligible customer IDs
        builder.Property(d => d.EligibleCustomerIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        // JSON for eligible customer tiers
        builder.Property(d => d.EligibleCustomerTiers)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for excluded products
        builder.Property(d => d.ExcludedProductIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        // JSON for excluded categories
        builder.Property(d => d.ExcludedCategoryIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        // JSON for Get product IDs (Buy X Get Y)
        builder.Property(d => d.GetProductIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(d => d.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        builder.HasIndex(d => d.IsActive);
        builder.HasIndex(d => d.StartDate);
        builder.HasIndex(d => d.EndDate);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => new { d.IsActive, d.StartDate, d.EndDate });
    }
}

/// <summary>
/// Entity configuration for DiscountUsage.
/// </summary>
public class DiscountUsageConfiguration : IEntityTypeConfiguration<DiscountUsage>
{
    public void Configure(EntityTypeBuilder<DiscountUsage> builder)
    {
        builder.ToTable("DiscountUsages");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.DiscountAmount)
            .HasPrecision(18, 4);

        // Relationships
        builder.HasOne(u => u.Discount)
            .WithMany()
            .HasForeignKey(u => u.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Customer)
            .WithMany()
            .HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Order)
            .WithMany()
            .HasForeignKey(u => u.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(u => u.DiscountId);
        builder.HasIndex(u => u.OrderId);
        builder.HasIndex(u => u.CustomerId);
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => new { u.DiscountId, u.CustomerId });
    }
}
