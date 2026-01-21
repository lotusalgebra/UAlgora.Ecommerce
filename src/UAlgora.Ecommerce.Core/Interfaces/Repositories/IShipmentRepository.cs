using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for shipment operations.
/// </summary>
public interface IShipmentRepository : IRepository<Shipment>
{
    /// <summary>
    /// Gets shipments by order ID.
    /// </summary>
    Task<IReadOnlyList<Shipment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets a shipment by tracking number.
    /// </summary>
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default);

    /// <summary>
    /// Gets a shipment by shipment number.
    /// </summary>
    Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default);

    /// <summary>
    /// Gets a shipment with items loaded.
    /// </summary>
    Task<Shipment?> GetWithItemsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets shipments by status.
    /// </summary>
    Task<IReadOnlyList<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets shipments by carrier.
    /// </summary>
    Task<IReadOnlyList<Shipment>> GetByCarrierAsync(string carrier, CancellationToken ct = default);

    /// <summary>
    /// Gets pending shipments (awaiting pickup).
    /// </summary>
    Task<IReadOnlyList<Shipment>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets shipments in transit.
    /// </summary>
    Task<IReadOnlyList<Shipment>> GetInTransitAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets shipments within a date range.
    /// </summary>
    Task<IReadOnlyList<Shipment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Generates a new unique shipment number.
    /// </summary>
    Task<string> GenerateShipmentNumberAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates shipment status.
    /// </summary>
    Task UpdateStatusAsync(Guid shipmentId, ShipmentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Adds a tracking event.
    /// </summary>
    Task AddTrackingEventAsync(Guid shipmentId, TrackingEvent trackingEvent, CancellationToken ct = default);

    /// <summary>
    /// Adds a shipment item.
    /// </summary>
    Task<ShipmentItem> AddItemAsync(ShipmentItem item, CancellationToken ct = default);

    /// <summary>
    /// Gets fulfilled quantity for an order line.
    /// </summary>
    Task<int> GetFulfilledQuantityAsync(Guid orderLineId, CancellationToken ct = default);
}
