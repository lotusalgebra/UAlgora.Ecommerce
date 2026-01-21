using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Review.
/// </summary>
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .HasMaxLength(200);

        builder.Property(r => r.Content)
            .HasMaxLength(4000);

        builder.Property(r => r.Pros)
            .HasMaxLength(2000);

        builder.Property(r => r.Cons)
            .HasMaxLength(2000);

        builder.Property(r => r.ReviewerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ReviewerEmail)
            .HasMaxLength(500);

        builder.Property(r => r.MerchantResponse)
            .HasMaxLength(2000);

        // JSON for images
        builder.Property(r => r.ImageUrls)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.Rating);
        builder.HasIndex(r => r.IsApproved);
        builder.HasIndex(r => r.IsVerifiedPurchase);
        builder.HasIndex(r => r.IsFeatured);
        builder.HasIndex(r => r.CreatedAt);
        builder.HasIndex(r => new { r.ProductId, r.IsApproved });
        builder.HasIndex(r => new { r.ProductId, r.Rating });
    }
}
