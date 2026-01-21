using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for cart operations.
/// </summary>
public interface ICartRepository : IRepository<Cart>
{
    /// <summary>
    /// Gets a cart by session ID.
    /// </summary>
    Task<Cart?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets a cart by customer ID.
    /// </summary>
    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets a cart with all items loaded.
    /// </summary>
    Task<Cart?> GetWithItemsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a cart by session ID with all items loaded.
    /// </summary>
    Task<Cart?> GetBySessionIdWithItemsAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets a cart by customer ID with all items loaded.
    /// </summary>
    Task<Cart?> GetByCustomerIdWithItemsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets or creates a cart for a session.
    /// </summary>
    Task<Cart> GetOrCreateBySessionAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets or creates a cart for a customer.
    /// </summary>
    Task<Cart> GetOrCreateByCustomerAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Merges a guest cart into a customer cart.
    /// </summary>
    Task<Cart> MergeCartsAsync(string sessionId, Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Adds an item to a cart.
    /// </summary>
    Task<CartItem> AddItemAsync(Guid cartId, CartItem item, CancellationToken ct = default);

    /// <summary>
    /// Updates a cart item.
    /// </summary>
    Task<CartItem> UpdateItemAsync(CartItem item, CancellationToken ct = default);

    /// <summary>
    /// Removes an item from a cart.
    /// </summary>
    Task RemoveItemAsync(Guid cartId, Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Clears all items from a cart.
    /// </summary>
    Task ClearItemsAsync(Guid cartId, CancellationToken ct = default);

    /// <summary>
    /// Gets expired carts.
    /// </summary>
    Task<IReadOnlyList<Cart>> GetExpiredCartsAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes expired carts.
    /// </summary>
    Task DeleteExpiredCartsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets abandoned carts (not modified within specified hours).
    /// </summary>
    Task<IReadOnlyList<Cart>> GetAbandonedCartsAsync(int hoursAgo = 24, CancellationToken ct = default);
}
