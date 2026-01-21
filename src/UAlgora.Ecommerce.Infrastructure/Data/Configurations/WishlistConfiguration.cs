using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Wishlist.
/// </summary>
public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.ShareToken)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(w => w.Customer)
            .WithMany()
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Items)
            .WithOne(i => i.Wishlist)
            .HasForeignKey(i => i.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => w.CustomerId);
        builder.HasIndex(w => w.IsDefault);
        builder.HasIndex(w => w.IsPublic);
        builder.HasIndex(w => w.ShareToken).IsUnique().HasFilter("[ShareToken] IS NOT NULL");
        builder.HasIndex(w => new { w.CustomerId, w.IsDefault });
    }
}

/// <summary>
/// Entity configuration for WishlistItem.
/// </summary>
public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.PriceWhenAdded)
            .HasPrecision(18, 4);

        // Relationships
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Variant)
            .WithMany()
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(i => i.WishlistId);
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.CreatedAt);
        builder.HasIndex(i => new { i.WishlistId, i.ProductId, i.VariantId }).IsUnique();
    }
}
