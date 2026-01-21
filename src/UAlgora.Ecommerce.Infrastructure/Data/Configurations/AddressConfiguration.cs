using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Address.
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Company)
            .HasMaxLength(200);

        builder.Property(a => a.AddressLine1)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(500);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.StateProvince)
            .HasMaxLength(200);

        builder.Property(a => a.StateProvinceCode)
            .HasMaxLength(20);

        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.Phone)
            .HasMaxLength(50);

        builder.Property(a => a.Label)
            .HasMaxLength(100);

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(a => a.CustomerId);
        builder.HasIndex(a => a.IsDefaultShipping);
        builder.HasIndex(a => a.IsDefaultBilling);
        builder.HasIndex(a => a.CountryCode);
        builder.HasIndex(a => new { a.CustomerId, a.IsDefaultShipping });
        builder.HasIndex(a => new { a.CustomerId, a.IsDefaultBilling });
    }
}
