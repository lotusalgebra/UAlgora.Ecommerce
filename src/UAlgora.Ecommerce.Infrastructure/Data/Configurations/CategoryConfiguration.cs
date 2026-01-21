using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Category.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Slug)
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(1000);

        builder.Property(c => c.Path)
            .HasMaxLength(1000);

        builder.Property(c => c.MetaTitle)
            .HasMaxLength(200);

        builder.Property(c => c.MetaDescription)
            .HasMaxLength(500);

        // Self-referencing relationship for hierarchy
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore navigation properties that are computed/manual
        builder.Ignore(c => c.Products);

        // Indexes
        builder.HasIndex(c => c.Slug);
        builder.HasIndex(c => c.ParentId);
        builder.HasIndex(c => c.SortOrder);
        builder.HasIndex(c => c.IsVisible);
        builder.HasIndex(c => c.Level);
        builder.HasIndex(c => c.UmbracoNodeId);
        builder.HasIndex(c => new { c.ParentId, c.SortOrder });
    }
}
