using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for TaxCategory.
/// </summary>
public class TaxCategoryConfiguration : IEntityTypeConfiguration<TaxCategory>
{
    public void Configure(EntityTypeBuilder<TaxCategory> builder)
    {
        builder.ToTable("TaxCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.ExternalTaxCode)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.IsDefault);
        builder.HasIndex(c => c.SortOrder);
        builder.HasIndex(c => new { c.IsActive, c.SortOrder });
    }
}

/// <summary>
/// Entity configuration for TaxZone.
/// </summary>
public class TaxZoneConfiguration : IEntityTypeConfiguration<TaxZone>
{
    public void Configure(EntityTypeBuilder<TaxZone> builder)
    {
        builder.ToTable("TaxZones");

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
        builder.HasIndex(z => z.Priority);
        builder.HasIndex(z => z.SortOrder);
        builder.HasIndex(z => new { z.IsActive, z.Priority });
    }
}

/// <summary>
/// Entity configuration for TaxRate.
/// </summary>
public class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("TaxRates");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.RateType)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Decimal properties
        builder.Property(r => r.Rate).HasPrecision(18, 4);
        builder.Property(r => r.FlatAmount).HasPrecision(18, 4);
        builder.Property(r => r.MinimumAmount).HasPrecision(18, 4);
        builder.Property(r => r.MaximumAmount).HasPrecision(18, 4);
        builder.Property(r => r.MaximumTax).HasPrecision(18, 4);

        // Jurisdiction properties
        builder.Property(r => r.JurisdictionType).HasMaxLength(100);
        builder.Property(r => r.JurisdictionName).HasMaxLength(200);
        builder.Property(r => r.JurisdictionCode).HasMaxLength(100);

        // Relationships
        builder.HasOne(r => r.TaxZone)
            .WithMany(z => z.Rates)
            .HasForeignKey(r => r.TaxZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.TaxCategory)
            .WithMany(c => c.Rates)
            .HasForeignKey(r => r.TaxCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.TaxZoneId);
        builder.HasIndex(r => r.TaxCategoryId);
        builder.HasIndex(r => r.IsActive);
        builder.HasIndex(r => r.Priority);
        builder.HasIndex(r => new { r.TaxZoneId, r.TaxCategoryId });
        builder.HasIndex(r => new { r.IsActive, r.Priority });
        builder.HasIndex(r => new { r.EffectiveFrom, r.EffectiveTo });
    }
}
