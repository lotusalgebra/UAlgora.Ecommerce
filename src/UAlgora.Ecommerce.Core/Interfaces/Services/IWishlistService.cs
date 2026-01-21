using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for wishlist management.
/// </summary>
public interface IWishlistService
{
    #region Wishlists

    /// <summary>
    /// Gets all wishlists with optional filtering.
    /// </summary>
    Task<List<Wishlist>> GetAllWishlistsAsync(bool includeItems = false, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated wishlists with filtering.
    /// </summary>
    Task<PagedResult<Wishlist>> GetPagedWishlistsAsync(
        int page = 1,
        int pageSize = 20,
        Guid? customerId = null,
        bool? isPublic = null,
        string? sortBy = null,
        bool descending = true,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a wishlist by ID.
    /// </summary>
    Task<Wishlist?> GetWishlistByIdAsync(Guid id, bool includeItems = true, CancellationToken ct = default);

    /// <summary>
    /// Gets wishlists by customer ID.
    /// </summary>
    Task<List<Wishlist>> GetWishlistsByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets the default wishlist for a customer.
    /// </summary>
    Task<Wishlist?> GetDefaultWishlistAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets a wishlist by share token.
    /// </summary>
    Task<Wishlist?> GetWishlistByShareTokenAsync(string shareToken, CancellationToken ct = default);

    /// <summary>
    /// Creates a new wishlist.
    /// </summary>
    Task<Wishlist> CreateWishlistAsync(Wishlist wishlist, CancellationToken ct = default);

    /// <summary>
    /// Updates a wishlist.
    /// </summary>
    Task<Wishlist> UpdateWishlistAsync(Wishlist wishlist, CancellationToken ct = default);

    /// <summary>
    /// Deletes a wishlist.
    /// </summary>
    Task DeleteWishlistAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Wishlist Actions

    /// <summary>
    /// Sets a wishlist as default for a customer.
    /// </summary>
    Task<Wishlist> SetDefaultWishlistAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Toggles the public/private status of a wishlist.
    /// </summary>
    Task<Wishlist> TogglePublicAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Generates a share token for a wishlist.
    /// </summary>
    Task<Wishlist> GenerateShareTokenAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Removes the share token from a wishlist.
    /// </summary>
    Task<Wishlist> RemoveShareTokenAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Renames a wishlist.
    /// </summary>
    Task<Wishlist> RenameWishlistAsync(Guid id, string name, CancellationToken ct = default);

    /// <summary>
    /// Duplicates a wishlist.
    /// </summary>
    Task<Wishlist> DuplicateWishlistAsync(Guid id, string? newName = null, CancellationToken ct = default);

    /// <summary>
    /// Clears all items from a wishlist.
    /// </summary>
    Task<Wishlist> ClearWishlistAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Merges items from one wishlist into another.
    /// </summary>
    Task<Wishlist> MergeWishlistsAsync(Guid sourceId, Guid targetId, bool deleteSource = false, CancellationToken ct = default);

    #endregion

    #region Wishlist Items

    /// <summary>
    /// Gets a wishlist item by ID.
    /// </summary>
    Task<WishlistItem?> GetWishlistItemByIdAsync(Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Adds an item to a wishlist.
    /// </summary>
    Task<WishlistItem> AddItemAsync(Guid wishlistId, Guid productId, Guid? variantId = null, decimal priceWhenAdded = 0, string? notes = null, int quantity = 1, int? priority = null, CancellationToken ct = default);

    /// <summary>
    /// Updates a wishlist item.
    /// </summary>
    Task<WishlistItem> UpdateItemAsync(WishlistItem item, CancellationToken ct = default);

    /// <summary>
    /// Removes an item from a wishlist.
    /// </summary>
    Task RemoveItemAsync(Guid wishlistId, Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Updates item notes.
    /// </summary>
    Task<WishlistItem> UpdateItemNotesAsync(Guid itemId, string? notes, CancellationToken ct = default);

    /// <summary>
    /// Updates item quantity.
    /// </summary>
    Task<WishlistItem> UpdateItemQuantityAsync(Guid itemId, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Updates item priority.
    /// </summary>
    Task<WishlistItem> UpdateItemPriorityAsync(Guid itemId, int? priority, CancellationToken ct = default);

    /// <summary>
    /// Moves an item to another wishlist.
    /// </summary>
    Task<WishlistItem> MoveItemAsync(Guid itemId, Guid targetWishlistId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a product is in a wishlist.
    /// </summary>
    Task<bool> IsProductInWishlistAsync(Guid wishlistId, Guid productId, Guid? variantId = null, CancellationToken ct = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets wishlist statistics.
    /// </summary>
    Task<WishlistStatistics> GetStatisticsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the most wishlisted products.
    /// </summary>
    Task<List<ProductWishlistCount>> GetMostWishlistedProductsAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets wishlist count for a customer.
    /// </summary>
    Task<int> GetCustomerWishlistCountAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets total items across all wishlists for a customer.
    /// </summary>
    Task<int> GetCustomerTotalItemsAsync(Guid customerId, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Wishlist statistics summary.
/// </summary>
public class WishlistStatistics
{
    public int TotalWishlists { get; set; }
    public int TotalItems { get; set; }
    public int PublicWishlists { get; set; }
    public int PrivateWishlists { get; set; }
    public int CustomersWithWishlists { get; set; }
    public double AverageItemsPerWishlist { get; set; }
    public int EmptyWishlists { get; set; }
    public int TodayCreated { get; set; }
    public int ThisWeekCreated { get; set; }
    public int ThisMonthCreated { get; set; }
}

/// <summary>
/// Product wishlist count for most wishlisted products.
/// </summary>
public class ProductWishlistCount
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int WishlistCount { get; set; }
}
