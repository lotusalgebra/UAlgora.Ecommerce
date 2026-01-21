using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for inventory management.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;
    private readonly Dictionary<Guid, StockReservation> _reservations = new();
    private readonly List<StockMovement> _movements = new();
    private readonly object _lock = new();

    public InventoryService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<int> GetStockAsync(
        Guid productId,
        Guid? variantId = null,
        CancellationToken ct = default)
    {
        var product = variantId.HasValue
            ? await _productRepository.GetWithVariantsAsync(productId, ct)
            : await _productRepository.GetByIdAsync(productId, ct);

        if (product == null)
        {
            return 0;
        }

        if (variantId.HasValue)
        {
            var variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Value);
            return variant?.StockQuantity ?? 0;
        }

        return product.StockQuantity;
    }

    public async Task<InventoryStatus> GetStatusAsync(
        Guid productId,
        Guid? variantId = null,
        CancellationToken ct = default)
    {
        var product = variantId.HasValue
            ? await _productRepository.GetWithVariantsAsync(productId, ct)
            : await _productRepository.GetByIdAsync(productId, ct);

        if (product == null)
        {
            return new InventoryStatus
            {
                StockQuantity = 0,
                ReservedQuantity = 0,
                IsInStock = false,
                IsLowStock = false,
                AllowBackorders = false,
                TrackInventory = false
            };
        }

        int stockQty;
        bool trackInventory;
        bool allowBackorders;
        int? lowStockThreshold;

        if (variantId.HasValue)
        {
            var variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Value);
            if (variant == null)
            {
                return new InventoryStatus();
            }

            // Variants use their own stock, but inherit tracking settings from product
            stockQty = variant.StockQuantity;
            trackInventory = product.TrackInventory;
            allowBackorders = product.AllowBackorders;
            lowStockThreshold = product.LowStockThreshold;
        }
        else
        {
            stockQty = product.StockQuantity;
            trackInventory = product.TrackInventory;
            allowBackorders = product.AllowBackorders;
            lowStockThreshold = product.LowStockThreshold;
        }

        var reservedQty = GetReservedQuantity(productId, variantId);

        return new InventoryStatus
        {
            StockQuantity = stockQty,
            ReservedQuantity = reservedQty,
            IsInStock = !trackInventory || stockQty - reservedQty > 0 || allowBackorders,
            IsLowStock = lowStockThreshold.HasValue && stockQty <= lowStockThreshold.Value,
            AllowBackorders = allowBackorders,
            TrackInventory = trackInventory,
            LowStockThreshold = lowStockThreshold
        };
    }

    public async Task<bool> IsInStockAsync(
        Guid productId,
        Guid? variantId = null,
        int quantity = 1,
        CancellationToken ct = default)
    {
        var status = await GetStatusAsync(productId, variantId, ct);

        if (!status.TrackInventory)
        {
            return true;
        }

        if (status.AllowBackorders)
        {
            return true;
        }

        return status.AvailableQuantity >= quantity;
    }

    public async Task<StockCheckResult> CheckAvailabilityAsync(
        IEnumerable<StockCheckRequest> requests,
        CancellationToken ct = default)
    {
        var result = new StockCheckResult();

        foreach (var request in requests)
        {
            var status = await GetStatusAsync(request.ProductId, request.VariantId, ct);

            var item = new StockCheckItem
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                RequestedQuantity = request.Quantity,
                AvailableQuantity = status.AvailableQuantity
            };

            if (!status.TrackInventory || status.AllowBackorders || item.IsAvailable)
            {
                result.AvailableItems.Add(item);
            }
            else
            {
                result.UnavailableItems.Add(item);
            }
        }

        return result;
    }

    public Task<StockReservation> ReserveStockAsync(
        Guid orderId,
        IEnumerable<StockReservationItem> items,
        CancellationToken ct = default)
    {
        lock (_lock)
        {
            var reservation = new StockReservation
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Items = items.ToList(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            _reservations[reservation.Id] = reservation;

            return Task.FromResult(reservation);
        }
    }

    public async Task CommitReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        StockReservation? reservation;

        lock (_lock)
        {
            if (!_reservations.TryGetValue(reservationId, out reservation))
            {
                throw new InvalidOperationException($"Reservation {reservationId} not found.");
            }

            if (reservation.IsCommitted || reservation.IsReleased)
            {
                throw new InvalidOperationException("Reservation has already been committed or released.");
            }

            reservation.IsCommitted = true;
        }

        // Deduct stock for each item
        foreach (var item in reservation.Items)
        {
            await AdjustStockAsync(item.ProductId, item.VariantId, -item.Quantity, $"Order {reservation.OrderId}", ct);
        }
    }

    public Task ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_reservations.TryGetValue(reservationId, out var reservation))
            {
                throw new InvalidOperationException($"Reservation {reservationId} not found.");
            }

            if (reservation.IsCommitted)
            {
                throw new InvalidOperationException("Cannot release a committed reservation.");
            }

            reservation.IsReleased = true;
            _reservations.Remove(reservationId);
        }

        return Task.CompletedTask;
    }

    public async Task<int> AdjustStockAsync(
        Guid productId,
        Guid? variantId,
        int adjustment,
        string reason,
        CancellationToken ct = default)
    {
        var product = await _productRepository.GetWithVariantsAsync(productId, ct);
        if (product == null)
        {
            throw new InvalidOperationException($"Product {productId} not found.");
        }

        int currentStock;
        int newStock;

        if (variantId.HasValue)
        {
            var variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Value);
            if (variant == null)
            {
                throw new InvalidOperationException($"Variant {variantId} not found.");
            }

            currentStock = variant.StockQuantity;
            newStock = currentStock + adjustment;
            variant.StockQuantity = Math.Max(0, newStock);
        }
        else
        {
            currentStock = product.StockQuantity;
            newStock = currentStock + adjustment;
            product.StockQuantity = Math.Max(0, newStock);
        }

        await _productRepository.UpdateAsync(product, ct);

        // Record movement
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VariantId = variantId,
            QuantityBefore = currentStock,
            QuantityAfter = Math.Max(0, newStock),
            Change = adjustment,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        lock (_lock)
        {
            _movements.Add(movement);
        }

        return Math.Max(0, newStock);
    }

    public async Task<int> SetStockAsync(
        Guid productId,
        Guid? variantId,
        int quantity,
        string? reason = null,
        CancellationToken ct = default)
    {
        var currentStock = await GetStockAsync(productId, variantId, ct);
        var adjustment = quantity - currentStock;

        return await AdjustStockAsync(productId, variantId, adjustment, reason ?? "Stock set", ct);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default)
    {
        return await _productRepository.GetLowStockAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetOutOfStockAsync(CancellationToken ct = default)
    {
        return await _productRepository.GetOutOfStockAsync(ct);
    }

    public Task<IReadOnlyList<StockMovement>> GetMovementHistoryAsync(
        Guid productId,
        Guid? variantId = null,
        int days = 30,
        CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        IReadOnlyList<StockMovement> movements;

        lock (_lock)
        {
            movements = _movements
                .Where(m => m.ProductId == productId
                    && (!variantId.HasValue || m.VariantId == variantId)
                    && m.CreatedAt >= cutoff)
                .OrderByDescending(m => m.CreatedAt)
                .ToList();
        }

        return Task.FromResult(movements);
    }

    public async Task<int> RestockAsync(
        Guid productId,
        Guid? variantId,
        int quantity,
        string? reason = null,
        CancellationToken ct = default)
    {
        return await AdjustStockAsync(productId, variantId, quantity, reason ?? "Restock", ct);
    }

    private int GetReservedQuantity(Guid productId, Guid? variantId)
    {
        lock (_lock)
        {
            return _reservations.Values
                .Where(r => !r.IsCommitted && !r.IsReleased && (!r.ExpiresAt.HasValue || r.ExpiresAt > DateTime.UtcNow))
                .SelectMany(r => r.Items)
                .Where(i => i.ProductId == productId && i.VariantId == variantId)
                .Sum(i => i.Quantity);
        }
    }
}
