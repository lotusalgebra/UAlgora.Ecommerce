using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Order.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.CustomerEmail)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(o => o.CustomerName)
            .HasMaxLength(200);

        builder.Property(o => o.CurrencyCode)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(o => o.Subtotal)
            .HasPrecision(18, 4);

        builder.Property(o => o.DiscountTotal)
            .HasPrecision(18, 4);

        builder.Property(o => o.ShippingTotal)
            .HasPrecision(18, 4);

        builder.Property(o => o.TaxTotal)
            .HasPrecision(18, 4);

        builder.Property(o => o.GrandTotal)
            .HasPrecision(18, 4);

        builder.Property(o => o.PaidAmount)
            .HasPrecision(18, 4);

        builder.Property(o => o.RefundedAmount)
            .HasPrecision(18, 4);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.FulfillmentStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.ShippingMethod)
            .HasMaxLength(100);

        builder.Property(o => o.ShippingMethodName)
            .HasMaxLength(200);

        builder.Property(o => o.PaymentMethod)
            .HasMaxLength(200);

        builder.Property(o => o.PaymentProvider)
            .HasMaxLength(100);

        builder.Property(o => o.PaymentIntentId)
            .HasMaxLength(500);

        builder.Property(o => o.CouponCode)
            .HasMaxLength(100);

        builder.Property(o => o.CustomerNote)
            .HasMaxLength(2000);

        builder.Property(o => o.InternalNote)
            .HasMaxLength(4000);

        builder.Property(o => o.TrackingNumber)
            .HasMaxLength(200);

        builder.Property(o => o.Carrier)
            .HasMaxLength(100);

        builder.Property(o => o.IpAddress)
            .HasMaxLength(50);

        builder.Property(o => o.UserAgent)
            .HasMaxLength(500);

        builder.Property(o => o.Source)
            .HasMaxLength(100);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(1000);

        // JSON for applied discounts
        builder.Property(o => o.AppliedDiscounts)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<AppliedDiscount>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<AppliedDiscount>())
            .HasColumnType("nvarchar(max)");

        // Relationships - Address navigation properties
        builder.HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Lines)
            .WithOne(l => l.Order)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Shipments)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.CustomerEmail);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);
        builder.HasIndex(o => o.FulfillmentStatus);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => o.PaymentIntentId);
        builder.HasIndex(o => o.PlacedAt);
        builder.HasIndex(o => new { o.Status, o.CreatedAt });
        builder.HasIndex(o => new { o.CustomerId, o.CreatedAt });
    }
}

/// <summary>
/// Entity configuration for OrderLine.
/// </summary>
public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.VariantName)
            .HasMaxLength(500);

        builder.Property(l => l.Sku)
            .HasMaxLength(100);

        builder.Property(l => l.ImageUrl)
            .HasMaxLength(1000);

        builder.Property(l => l.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(l => l.OriginalPrice)
            .HasPrecision(18, 4);

        builder.Property(l => l.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(l => l.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(l => l.LineTotal)
            .HasPrecision(18, 4);

        builder.Property(l => l.Weight)
            .HasPrecision(18, 4);

        // JSON for variant options
        builder.Property(l => l.VariantOptions)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null))
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(l => l.OrderId);
        builder.HasIndex(l => l.ProductId);
        builder.HasIndex(l => l.Sku);
    }
}
