using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for ShippingMethod.
/// </summary>
public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable("ShippingMethods");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.CalculationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.WeightUnit)
            .HasMaxLength(20);

        // Decimal properties
        builder.Property(m => m.FlatRate).HasPrecision(18, 4);
        builder.Property(m => m.WeightBaseRate).HasPrecision(18, 4);
        builder.Property(m => m.WeightPerUnitRate).HasPrecision(18, 4);
        builder.Property(m => m.PricePercentage).HasPrecision(18, 4);
        builder.Property(m => m.MinimumCost).HasPrecision(18, 4);
        builder.Property(m => m.MaximumCost).HasPrecision(18, 4);
        builder.Property(m => m.PerItemRate).HasPrecision(18, 4);
        builder.Property(m => m.HandlingFee).HasPrecision(18, 4);
        builder.Property(m => m.FreeShippingThreshold).HasPrecision(18, 4);
        builder.Property(m => m.MinWeight).HasPrecision(18, 4);
        builder.Property(m => m.MaxWeight).HasPrecision(18, 4);
        builder.Property(m => m.MinOrderAmount).HasPrecision(18, 4);
        builder.Property(m => m.MaxOrderAmount).HasPrecision(18, 4);

        // String properties
        builder.Property(m => m.DeliveryEstimateText).HasMaxLength(200);
        builder.Property(m => m.CarrierProviderId).HasMaxLength(100);
        builder.Property(m => m.CarrierServiceCode).HasMaxLength(100);
        builder.Property(m => m.IconName).HasMaxLength(100);
        builder.Property(m => m.ImageUrl).HasMaxLength(500);
        builder.Property(m => m.TaxClass).HasMaxLength(100);

        // Indexes
        builder.HasIndex(m => m.Code).IsUnique();
        builder.HasIndex(m => m.IsActive);
        builder.HasIndex(m => m.SortOrder);
        builder.HasIndex(m => new { m.IsActive, m.SortOrder });
    }
}

/// <summary>
/// Entity configuration for ShippingZone.
/// </summary>
public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZones");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(z => z.Description)
            .HasMaxLength(1000);

        builder.Property(z => z.Code)
            .IsRequired()
            .HasMaxLength(100);

        // JSON for countries
        builder.Property(z => z.Countries)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for states
        builder.Property(z => z.States)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for postal code patterns
        builder.Property(z => z.PostalCodePatterns)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for cities
        builder.Property(z => z.Cities)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for excluded countries
        builder.Property(z => z.ExcludedCountries)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for excluded states
        builder.Property(z => z.ExcludedStates)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // JSON for excluded postal codes
        builder.Property(z => z.ExcludedPostalCodes)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(z => z.Code).IsUnique();
        builder.HasIndex(z => z.IsActive);
        builder.HasIndex(z => z.IsDefault);
        builder.HasIndex(z => z.SortOrder);
    }
}

/// <summary>
/// Entity configuration for ShippingRate.
/// </summary>
public class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRate>
{
    public void Configure(EntityTypeBuilder<ShippingRate> builder)
    {
        builder.ToTable("ShippingRates");

        builder.HasKey(r => r.Id);

        // Decimal properties
        builder.Property(r => r.BaseRate).HasPrecision(18, 4);
        builder.Property(r => r.PerWeightRate).HasPrecision(18, 4);
        builder.Property(r => r.PerItemRate).HasPrecision(18, 4);
        builder.Property(r => r.PercentageRate).HasPrecision(18, 4);
        builder.Property(r => r.HandlingFee).HasPrecision(18, 4);
        builder.Property(r => r.MinWeight).HasPrecision(18, 4);
        builder.Property(r => r.MaxWeight).HasPrecision(18, 4);
        builder.Property(r => r.MinOrderAmount).HasPrecision(18, 4);
        builder.Property(r => r.MaxOrderAmount).HasPrecision(18, 4);
        builder.Property(r => r.FreeShippingThreshold).HasPrecision(18, 4);

        // Relationships
        builder.HasOne(r => r.Zone)
            .WithMany(z => z.Rates)
            .HasForeignKey(r => r.ShippingZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Method)
            .WithMany(m => m.Rates)
            .HasForeignKey(r => r.ShippingMethodId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.ShippingZoneId);
        builder.HasIndex(r => r.ShippingMethodId);
        builder.HasIndex(r => r.IsActive);
        builder.HasIndex(r => new { r.ShippingZoneId, r.ShippingMethodId }).IsUnique();
    }
}
