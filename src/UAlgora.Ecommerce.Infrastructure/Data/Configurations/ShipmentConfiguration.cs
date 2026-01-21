using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Shipment.
/// </summary>
public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShipmentNumber)
            .HasMaxLength(100);

        builder.Property(s => s.TrackingNumber)
            .HasMaxLength(200);

        builder.Property(s => s.TrackingUrl)
            .HasMaxLength(1000);

        builder.Property(s => s.Carrier)
            .HasMaxLength(100);

        builder.Property(s => s.CarrierCode)
            .HasMaxLength(50);

        builder.Property(s => s.Service)
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.LabelUrl)
            .HasMaxLength(1000);

        builder.Property(s => s.LabelFormat)
            .HasMaxLength(20);

        builder.Property(s => s.CommercialInvoiceUrl)
            .HasMaxLength(1000);

        builder.Property(s => s.ShippingCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.InsuranceCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.DeclaredValue)
            .HasPrecision(18, 4);

        builder.Property(s => s.Weight)
            .HasPrecision(18, 4);

        builder.Property(s => s.WeightUnit)
            .HasMaxLength(10);

        builder.Property(s => s.Length)
            .HasPrecision(18, 4);

        builder.Property(s => s.Width)
            .HasPrecision(18, 4);

        builder.Property(s => s.Height)
            .HasPrecision(18, 4);

        builder.Property(s => s.DimensionUnit)
            .HasMaxLength(10);

        builder.Property(s => s.DeliveryInstructions)
            .HasMaxLength(1000);

        builder.Property(s => s.SignedBy)
            .HasMaxLength(200);

        builder.Property(s => s.ProofOfDeliveryUrl)
            .HasMaxLength(1000);

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(s => s.ShipFromAddress)
            .WithMany()
            .HasForeignKey(s => s.ShipFromAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.ShipToAddress)
            .WithMany()
            .HasForeignKey(s => s.ShipToAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Shipment)
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // JSON for tracking events (not entities)
        builder.Property(s => s.TrackingEvents)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<TrackingEvent>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<TrackingEvent>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.ShipmentNumber);
        builder.HasIndex(s => s.TrackingNumber);
        builder.HasIndex(s => s.Carrier);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ShippedAt);
        builder.HasIndex(s => s.DeliveredAt);
    }
}

/// <summary>
/// Entity configuration for ShipmentItem.
/// </summary>
public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.ToTable("ShipmentItems");

        builder.HasKey(i => i.Id);

        // Relationships
        builder.HasOne(i => i.OrderLine)
            .WithMany()
            .HasForeignKey(i => i.OrderLineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(i => i.ShipmentId);
        builder.HasIndex(i => i.OrderLineId);
    }
}
