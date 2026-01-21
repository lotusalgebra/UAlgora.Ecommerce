using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for inventory management.
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Gets current stock quantity for a product/variant.
    /// </summary>
    Task<int> GetStockAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets inventory status for a product/variant.
    /// </summary>
    Task<InventoryStatus> GetStatusAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a product/variant is in stock.
    /// </summary>
    Task<bool> IsInStockAsync(Guid productId, Guid? variantId = null, int quantity = 1, CancellationToken ct = default);

    /// <summary>
    /// Checks if requested quantities are available.
    /// </summary>
    Task<StockCheckResult> CheckAvailabilityAsync(IEnumerable<StockCheckRequest> requests, CancellationToken ct = default);

    /// <summary>
    /// Reserves stock for an order.
    /// </summary>
    Task<StockReservation> ReserveStockAsync(Guid orderId, IEnumerable<StockReservationItem> items, CancellationToken ct = default);

    /// <summary>
    /// Commits a stock reservation (after order completion).
    /// </summary>
    Task CommitReservationAsync(Guid reservationId, CancellationToken ct = default);

    /// <summary>
    /// Releases a stock reservation (order cancelled).
    /// </summary>
    Task ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default);

    /// <summary>
    /// Adjusts stock quantity.
    /// </summary>
    Task<int> AdjustStockAsync(Guid productId, Guid? variantId, int adjustment, string reason, CancellationToken ct = default);

    /// <summary>
    /// Sets stock quantity.
    /// </summary>
    Task<int> SetStockAsync(Guid productId, Guid? variantId, int quantity, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Gets low stock products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets out of stock products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetOutOfStockAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets stock movement history.
    /// </summary>
    Task<IReadOnlyList<StockMovement>> GetMovementHistoryAsync(Guid productId, Guid? variantId = null, int days = 30, CancellationToken ct = default);

    /// <summary>
    /// Restocks a product (e.g., from return).
    /// </summary>
    Task<int> RestockAsync(Guid productId, Guid? variantId, int quantity, string? reason = null, CancellationToken ct = default);
}

/// <summary>
/// Inventory status information.
/// </summary>
public class InventoryStatus
{
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public bool AllowBackorders { get; set; }
    public bool TrackInventory { get; set; }
    public int? LowStockThreshold { get; set; }
}

/// <summary>
/// Stock check request.
/// </summary>
public class StockCheckRequest
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Stock check result.
/// </summary>
public class StockCheckResult
{
    public bool AllAvailable => UnavailableItems.Count == 0;
    public List<StockCheckItem> AvailableItems { get; set; } = [];
    public List<StockCheckItem> UnavailableItems { get; set; } = [];
}

/// <summary>
/// Stock check item result.
/// </summary>
public class StockCheckItem
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable => AvailableQuantity >= RequestedQuantity;
}

/// <summary>
/// Stock reservation.
/// </summary>
public class StockReservation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public List<StockReservationItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsCommitted { get; set; }
    public bool IsReleased { get; set; }
}

/// <summary>
/// Stock reservation item.
/// </summary>
public class StockReservationItem
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Stock movement record.
/// </summary>
public class StockMovement
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public int Change { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
