using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for wishlist operations.
/// </summary>
public interface IWishlistRepository : IRepository<Wishlist>
{
    /// <summary>
    /// Gets wishlists by customer ID.
    /// </summary>
    Task<IReadOnlyList<Wishlist>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets the default wishlist for a customer.
    /// </summary>
    Task<Wishlist?> GetDefaultByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets or creates the default wishlist for a customer.
    /// </summary>
    Task<Wishlist> GetOrCreateDefaultAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets a wishlist by share token.
    /// </summary>
    Task<Wishlist?> GetByShareTokenAsync(string shareToken, CancellationToken ct = default);

    /// <summary>
    /// Gets a wishlist with items loaded.
    /// </summary>
    Task<Wishlist?> GetWithItemsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds an item to a wishlist.
    /// </summary>
    Task<WishlistItem> AddItemAsync(WishlistItem item, CancellationToken ct = default);

    /// <summary>
    /// Removes an item from a wishlist.
    /// </summary>
    Task RemoveItemAsync(Guid wishlistId, Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Removes a product from a wishlist.
    /// </summary>
    Task RemoveProductAsync(Guid wishlistId, Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a product is in a wishlist.
    /// </summary>
    Task<bool> ContainsProductAsync(Guid wishlistId, Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a product is in any of a customer's wishlists.
    /// </summary>
    Task<bool> IsProductInCustomerWishlistAsync(Guid customerId, Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Clears all items from a wishlist.
    /// </summary>
    Task ClearItemsAsync(Guid wishlistId, CancellationToken ct = default);

    /// <summary>
    /// Generates a unique share token.
    /// </summary>
    Task<string> GenerateShareTokenAsync(CancellationToken ct = default);
}
