using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Warehouse entity.
/// </summary>
public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Ecommerce_Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(w => w.Code)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.AddressLine1)
            .HasMaxLength(200);

        builder.Property(w => w.AddressLine2)
            .HasMaxLength(200);

        builder.Property(w => w.City)
            .HasMaxLength(100);

        builder.Property(w => w.State)
            .HasMaxLength(100);

        builder.Property(w => w.PostalCode)
            .HasMaxLength(20);

        builder.Property(w => w.Country)
            .HasMaxLength(2);

        builder.Property(w => w.ContactName)
            .HasMaxLength(200);

        builder.Property(w => w.ContactEmail)
            .HasMaxLength(256);

        builder.Property(w => w.ContactPhone)
            .HasMaxLength(50);

        builder.Property(w => w.ShippingCountries)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.OperatingHours)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)");

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}

/// <summary>
/// EF Core configuration for WarehouseStock entity.
/// </summary>
public class WarehouseStockConfiguration : IEntityTypeConfiguration<WarehouseStock>
{
    public void Configure(EntityTypeBuilder<WarehouseStock> builder)
    {
        builder.ToTable("Ecommerce_WarehouseStocks");

        builder.HasKey(ws => ws.Id);

        builder.HasIndex(ws => new { ws.WarehouseId, ws.ProductId, ws.VariantId })
            .IsUnique();

        builder.Property(ws => ws.BinLocation)
            .HasMaxLength(50);

        builder.HasOne(ws => ws.Warehouse)
            .WithMany(w => w.StockLevels)
            .HasForeignKey(ws => ws.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ws => ws.Product)
            .WithMany()
            .HasForeignKey(ws => ws.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for StockAdjustment entity.
/// </summary>
public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("Ecommerce_StockAdjustments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.ReferenceNumber)
            .IsUnique();

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.Property(a => a.ExternalReference)
            .HasMaxLength(100);

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(200);

        builder.Property(a => a.ApprovedBy)
            .HasMaxLength(200);

        builder.HasOne(a => a.Warehouse)
            .WithMany()
            .HasForeignKey(a => a.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Items)
            .WithOne(i => i.StockAdjustment)
            .HasForeignKey(i => i.StockAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for StockAdjustmentItem entity.
/// </summary>
public class StockAdjustmentItemConfiguration : IEntityTypeConfiguration<StockAdjustmentItem>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentItem> builder)
    {
        builder.ToTable("Ecommerce_StockAdjustmentItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.Property(i => i.BinLocation)
            .HasMaxLength(50);

        builder.Property(i => i.UnitCost)
            .HasPrecision(18, 4);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// EF Core configuration for StockTransfer entity.
/// </summary>
public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("Ecommerce_StockTransfers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.ReferenceNumber)
            .IsUnique();

        builder.Property(t => t.Notes)
            .HasMaxLength(2000);

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(200);

        builder.Property(t => t.ApprovedBy)
            .HasMaxLength(200);

        builder.Property(t => t.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(t => t.Carrier)
            .HasMaxLength(100);

        builder.HasOne(t => t.SourceWarehouse)
            .WithMany()
            .HasForeignKey(t => t.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(t => t.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Items)
            .WithOne(i => i.StockTransfer)
            .HasForeignKey(i => i.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for StockTransferItem entity.
/// </summary>
public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        builder.ToTable("Ecommerce_StockTransferItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.SourceBinLocation)
            .HasMaxLength(50);

        builder.Property(i => i.DestinationBinLocation)
            .HasMaxLength(50);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// EF Core configuration for Supplier entity.
/// </summary>
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Ecommerce_Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.ContactName)
            .HasMaxLength(200);

        builder.Property(s => s.Email)
            .HasMaxLength(256);

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.Website)
            .HasMaxLength(500);

        builder.Property(s => s.AddressLine1)
            .HasMaxLength(200);

        builder.Property(s => s.AddressLine2)
            .HasMaxLength(200);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .HasMaxLength(100);

        builder.Property(s => s.PostalCode)
            .HasMaxLength(20);

        builder.Property(s => s.Country)
            .HasMaxLength(2);

        builder.Property(s => s.TaxId)
            .HasMaxLength(50);

        builder.Property(s => s.PaymentTerms)
            .HasMaxLength(100);

        builder.Property(s => s.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(s => s.MinOrderValue)
            .HasPrecision(18, 4);

        builder.Property(s => s.Rating)
            .HasPrecision(3, 2);

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

/// <summary>
/// EF Core configuration for SupplierProduct entity.
/// </summary>
public class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
{
    public void Configure(EntityTypeBuilder<SupplierProduct> builder)
    {
        builder.ToTable("Ecommerce_SupplierProducts");

        builder.HasKey(sp => sp.Id);

        builder.HasIndex(sp => new { sp.SupplierId, sp.ProductId, sp.VariantId })
            .IsUnique();

        builder.Property(sp => sp.SupplierSku)
            .HasMaxLength(100);

        builder.Property(sp => sp.CostPrice)
            .HasPrecision(18, 4);

        builder.Property(sp => sp.Notes)
            .HasMaxLength(500);

        builder.HasOne(sp => sp.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(sp => sp.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Product)
            .WithMany()
            .HasForeignKey(sp => sp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for PurchaseOrder entity.
/// </summary>
public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("Ecommerce_PurchaseOrders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(po => po.OrderNumber)
            .IsUnique();

        builder.Property(po => po.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(po => po.Subtotal)
            .HasPrecision(18, 4);

        builder.Property(po => po.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(po => po.ShippingCost)
            .HasPrecision(18, 4);

        builder.Property(po => po.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(po => po.Total)
            .HasPrecision(18, 4);

        builder.Property(po => po.SupplierReference)
            .HasMaxLength(100);

        builder.Property(po => po.PaymentTerms)
            .HasMaxLength(100);

        builder.Property(po => po.ShippingMethod)
            .HasMaxLength(100);

        builder.Property(po => po.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(po => po.Notes)
            .HasMaxLength(2000);

        builder.Property(po => po.InternalNotes)
            .HasMaxLength(2000);

        builder.Property(po => po.CreatedBy)
            .HasMaxLength(200);

        builder.Property(po => po.ApprovedBy)
            .HasMaxLength(200);

        builder.HasOne(po => po.Supplier)
            .WithMany()
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Warehouse)
            .WithMany()
            .HasForeignKey(po => po.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for PurchaseOrderItem entity.
/// </summary>
public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("Ecommerce_PurchaseOrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.SupplierSku)
            .HasMaxLength(100);

        builder.Property(i => i.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(i => i.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(i => i.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(i => i.LineTotal)
            .HasPrecision(18, 4);

        builder.Property(i => i.BinLocation)
            .HasMaxLength(50);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
