using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for LicenseSubscription.
/// </summary>
public class LicenseSubscriptionConfiguration : IEntityTypeConfiguration<LicenseSubscription>
{
    public void Configure(EntityTypeBuilder<LicenseSubscription> builder)
    {
        builder.ToTable("LicenseSubscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CustomerEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.CustomerName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.PaymentProvider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.ProviderSubscriptionId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.ProviderCustomerId)
            .HasMaxLength(500);

        builder.Property(s => s.ProviderPriceId)
            .HasMaxLength(500);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.Amount)
            .HasPrecision(18, 4);

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(s => s.BillingInterval)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("year");

        builder.Property(s => s.LicensedDomain)
            .HasMaxLength(500);

        builder.Property(s => s.LicenseType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.LastFailureReason)
            .HasMaxLength(1000);

        builder.Property(s => s.MetadataJson)
            .HasColumnType("nvarchar(max)");

        // Relationship to License (Restrict to avoid multiple cascade paths)
        builder.HasOne(s => s.License)
            .WithMany()
            .HasForeignKey(s => s.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.LicenseId).IsUnique();
        builder.HasIndex(s => s.ProviderSubscriptionId).IsUnique();
        builder.HasIndex(s => s.ProviderCustomerId);
        builder.HasIndex(s => s.CustomerEmail);
        builder.HasIndex(s => s.PaymentProvider);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.LicenseType);
        builder.HasIndex(s => s.CurrentPeriodEnd);
        builder.HasIndex(s => s.NextPaymentDate);
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => new { s.Status, s.AutoRenew });
        builder.HasIndex(s => new { s.PaymentProvider, s.Status });
    }
}

/// <summary>
/// Entity configuration for LicensePayment.
/// </summary>
public class LicensePaymentConfiguration : IEntityTypeConfiguration<LicensePayment>
{
    public void Configure(EntityTypeBuilder<LicensePayment> builder)
    {
        builder.ToTable("LicensePayments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PaymentProvider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ProviderPaymentId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.ProviderCustomerId)
            .HasMaxLength(500);

        builder.Property(p => p.ProviderInvoiceId)
            .HasMaxLength(500);

        builder.Property(p => p.ProviderChargeId)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 4);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(p => p.CustomerEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.CustomerName)
            .HasMaxLength(256);

        builder.Property(p => p.ReceiptUrl)
            .HasMaxLength(2000);

        builder.Property(p => p.InvoiceUrl)
            .HasMaxLength(2000);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(1000);

        builder.Property(p => p.FailureCode)
            .HasMaxLength(100);

        builder.Property(p => p.RefundedAmount)
            .HasPrecision(18, 4);

        builder.Property(p => p.RefundReason)
            .HasMaxLength(500);

        builder.Property(p => p.PaymentType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("subscription");

        builder.Property(p => p.LicenseType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.CardBrand)
            .HasMaxLength(50);

        builder.Property(p => p.CardLast4)
            .HasMaxLength(4);

        builder.Property(p => p.CardCountry)
            .HasMaxLength(10);

        builder.Property(p => p.RawResponseJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.MetadataJson)
            .HasColumnType("nvarchar(max)");

        // Relationship to Subscription (Restrict to avoid multiple cascade paths)
        builder.HasOne(p => p.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to License (for one-time purchases, Restrict to avoid multiple cascade paths)
        builder.HasOne(p => p.License)
            .WithMany()
            .HasForeignKey(p => p.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.SubscriptionId);
        builder.HasIndex(p => p.LicenseId);
        builder.HasIndex(p => p.ProviderPaymentId).IsUnique();
        builder.HasIndex(p => p.ProviderInvoiceId);
        builder.HasIndex(p => p.ProviderChargeId);
        builder.HasIndex(p => p.ProviderCustomerId);
        builder.HasIndex(p => p.CustomerEmail);
        builder.HasIndex(p => p.PaymentProvider);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.LicenseType);
        builder.HasIndex(p => p.PaidAt);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => new { p.PaymentProvider, p.Status });
        builder.HasIndex(p => new { p.Status, p.CreatedAt });
    }
}
