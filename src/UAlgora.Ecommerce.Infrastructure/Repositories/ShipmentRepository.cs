using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for shipment operations.
/// </summary>
public class ShipmentRepository : Repository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Shipment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.OrderId == orderId)
            .Include(s => s.Items)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, ct);
    }

    public async Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.ShipmentNumber == shipmentNumber, ct);
    }

    public async Task<Shipment?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .ThenInclude(i => i.OrderLine)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Status == status)
            .Include(s => s.Items)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetByCarrierAsync(string carrier, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Carrier == carrier || s.CarrierCode == carrier)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetPendingAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Status == ShipmentStatus.Pending || s.Status == ShipmentStatus.LabelCreated)
            .Include(s => s.Items)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetInTransitAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.Status == ShipmentStatus.InTransit)
            .Include(s => s.Items)
            .OrderBy(s => s.ShippedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateShipmentNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var prefix = $"SHP-{year}{month:D2}-";

        // Get the last shipment number for this month
        var lastShipment = await DbSet
            .Where(s => s.ShipmentNumber != null && s.ShipmentNumber.StartsWith(prefix))
            .OrderByDescending(s => s.ShipmentNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastShipment?.ShipmentNumber != null)
        {
            var lastNumber = lastShipment.ShipmentNumber.Replace(prefix, "");
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }

    public async Task UpdateStatusAsync(Guid shipmentId, ShipmentStatus status, CancellationToken ct = default)
    {
        var shipment = await GetByIdAsync(shipmentId, ct);
        if (shipment != null)
        {
            shipment.Status = status;

            // Update timestamps based on status
            switch (status)
            {
                case ShipmentStatus.LabelCreated:
                    shipment.LabelCreatedAt = DateTime.UtcNow;
                    break;
                case ShipmentStatus.PickedUp:
                    shipment.PickedUpAt = DateTime.UtcNow;
                    break;
                case ShipmentStatus.InTransit:
                    shipment.ShippedAt ??= DateTime.UtcNow;
                    break;
                case ShipmentStatus.Delivered:
                    shipment.DeliveredAt = DateTime.UtcNow;
                    break;
            }

            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task AddTrackingEventAsync(
        Guid shipmentId,
        TrackingEvent trackingEvent,
        CancellationToken ct = default)
    {
        var shipment = await GetByIdAsync(shipmentId, ct);
        if (shipment != null)
        {
            shipment.TrackingEvents.Add(trackingEvent);
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<ShipmentItem> AddItemAsync(ShipmentItem item, CancellationToken ct = default)
    {
        await Context.ShipmentItems.AddAsync(item, ct);
        await Context.SaveChangesAsync(ct);

        // Update order line fulfilled quantity
        var orderLine = await Context.OrderLines.FindAsync(new object[] { item.OrderLineId }, ct);
        if (orderLine != null)
        {
            orderLine.FulfilledQuantity += item.Quantity;
            await Context.SaveChangesAsync(ct);
        }

        return item;
    }

    public async Task<int> GetFulfilledQuantityAsync(Guid orderLineId, CancellationToken ct = default)
    {
        return await Context.ShipmentItems
            .Where(si => si.OrderLineId == orderLineId)
            .SumAsync(si => si.Quantity, ct);
    }
}
