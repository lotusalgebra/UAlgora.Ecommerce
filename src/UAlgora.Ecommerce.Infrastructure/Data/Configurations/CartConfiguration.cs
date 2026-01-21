using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Cart.
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SessionId)
            .HasMaxLength(200);

        builder.Property(c => c.CurrencyCode)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(c => c.CouponCode)
            .HasMaxLength(100);

        builder.Property(c => c.CustomerEmail)
            .HasMaxLength(500);

        builder.Property(c => c.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.Property(c => c.SelectedShippingMethod)
            .HasMaxLength(100);

        builder.Property(c => c.SelectedShippingMethodName)
            .HasMaxLength(200);

        builder.Property(c => c.Subtotal)
            .HasPrecision(18, 4);

        builder.Property(c => c.DiscountTotal)
            .HasPrecision(18, 4);

        builder.Property(c => c.ShippingTotal)
            .HasPrecision(18, 4);

        builder.Property(c => c.TaxTotal)
            .HasPrecision(18, 4);

        builder.Property(c => c.GrandTotal)
            .HasPrecision(18, 4);

        // Relationships
        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation properties to Address entities
        builder.HasOne(c => c.ShippingAddress)
            .WithMany()
            .HasForeignKey("ShippingAddressId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.BillingAddress)
            .WithMany()
            .HasForeignKey("BillingAddressId")
            .OnDelete(DeleteBehavior.SetNull);

        // JSON for applied discounts
        builder.Property(c => c.AppliedDiscounts)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<AppliedDiscount>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<AppliedDiscount>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(c => c.SessionId);
        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.ExpiresAt);
    }
}

/// <summary>
/// Entity configuration for CartItem.
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.VariantName)
            .HasMaxLength(500);

        builder.Property(i => i.Sku)
            .HasMaxLength(100);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(1000);

        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(i => i.OriginalPrice)
            .HasPrecision(18, 4);

        builder.Property(i => i.LineTotal)
            .HasPrecision(18, 4);

        builder.Property(i => i.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(i => i.Weight)
            .HasPrecision(18, 4);

        // JSON for variant options
        builder.Property(i => i.VariantOptions)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null))
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(i => i.CartId);
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.AddedAt);
        builder.HasIndex(i => new { i.CartId, i.ProductId, i.VariantId });
    }
}
