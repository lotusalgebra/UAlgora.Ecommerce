using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Payment.
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PaymentIntentId)
            .HasMaxLength(500);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(500);

        builder.Property(p => p.ChargeId)
            .HasMaxLength(500);

        builder.Property(p => p.RefundId)
            .HasMaxLength(500);

        builder.Property(p => p.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.MethodName)
            .HasMaxLength(200);

        builder.Property(p => p.MethodType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 4);

        builder.Property(p => p.CurrencyCode)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.ErrorCode)
            .HasMaxLength(100);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(p => p.CardBrand)
            .HasMaxLength(50);

        builder.Property(p => p.CardLast4)
            .HasMaxLength(4);

        builder.Property(p => p.RiskLevel)
            .HasMaxLength(50);

        builder.Property(p => p.AvsResult)
            .HasMaxLength(50);

        builder.Property(p => p.CvvResult)
            .HasMaxLength(50);

        builder.Property(p => p.RawResponse)
            .HasColumnType("nvarchar(max)");

        // Self-referencing for refunds
        builder.HasOne(p => p.ParentPayment)
            .WithMany(p => p.Refunds)
            .HasForeignKey(p => p.ParentPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.PaymentIntentId);
        builder.HasIndex(p => p.TransactionId);
        builder.HasIndex(p => p.ChargeId);
        builder.HasIndex(p => p.Provider);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.ParentPaymentId);
        builder.HasIndex(p => new { p.OrderId, p.Status });
    }
}

/// <summary>
/// Entity configuration for StoredPaymentMethod.
/// </summary>
public class StoredPaymentMethodConfiguration : IEntityTypeConfiguration<StoredPaymentMethod>
{
    public void Configure(EntityTypeBuilder<StoredPaymentMethod> builder)
    {
        builder.ToTable("StoredPaymentMethods");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ProviderMethodId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.DisplayName)
            .HasMaxLength(200);

        builder.Property(m => m.CardBrand)
            .HasMaxLength(50);

        builder.Property(m => m.CardLast4)
            .HasMaxLength(4);

        // Relationship to billing address
        builder.HasOne(m => m.BillingAddress)
            .WithMany()
            .HasForeignKey(m => m.BillingAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Customer)
            .WithMany()
            .HasForeignKey(m => m.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(m => m.CustomerId);
        builder.HasIndex(m => m.ProviderMethodId);
        builder.HasIndex(m => m.Provider);
        builder.HasIndex(m => m.IsDefault);
        builder.HasIndex(m => new { m.CustomerId, m.IsDefault });
    }
}
