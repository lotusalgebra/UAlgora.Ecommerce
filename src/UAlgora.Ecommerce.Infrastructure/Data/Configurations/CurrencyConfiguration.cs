using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Currency.
/// </summary>
public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Symbol)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.NativeName)
            .HasMaxLength(100);

        builder.Property(c => c.DecimalSeparator)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue(".");

        builder.Property(c => c.ThousandsSeparator)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue(",");

        builder.Property(c => c.DecimalPlaces)
            .HasDefaultValue(2);

        builder.Property(c => c.RoundingIncrement)
            .HasPrecision(18, 6);

        // JSON for countries
        builder.Property(c => c.Countries)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasMany(c => c.ExchangeRatesFrom)
            .WithOne(r => r.FromCurrency)
            .HasForeignKey(r => r.FromCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.ExchangeRatesTo)
            .WithOne(r => r.ToCurrency)
            .HasForeignKey(r => r.ToCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.IsDefault);
        builder.HasIndex(c => c.SortOrder);
        builder.HasIndex(c => new { c.IsActive, c.SortOrder });

        // Query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

/// <summary>
/// Entity configuration for ExchangeRate.
/// </summary>
public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rate)
            .IsRequired()
            .HasPrecision(18, 8);

        builder.Property(r => r.MarkupPercent)
            .HasPrecision(10, 4);

        builder.Property(r => r.Source)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(r => r.IsActive);
        builder.HasIndex(r => r.EffectiveFrom);
        builder.HasIndex(r => r.EffectiveTo);
        builder.HasIndex(r => new { r.FromCurrencyId, r.ToCurrencyId });
        builder.HasIndex(r => new { r.FromCurrencyId, r.ToCurrencyId, r.IsActive, r.EffectiveFrom });
    }
}
