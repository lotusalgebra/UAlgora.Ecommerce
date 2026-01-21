namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a customer wishlist.
/// </summary>
public class Wishlist : BaseEntity
{
    /// <summary>
    /// Customer ID who owns this wishlist.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Wishlist name.
    /// </summary>
    public string Name { get; set; } = "My Wishlist";

    /// <summary>
    /// Whether this is the default wishlist.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this wishlist is public/shareable.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Share token for public wishlists.
    /// </summary>
    public string? ShareToken { get; set; }

    /// <summary>
    /// Wishlist items.
    /// </summary>
    public List<WishlistItem> Items { get; set; } = [];

    /// <summary>
    /// Navigation property to customer.
    /// </summary>
    public Customer? Customer { get; set; }

    #region Computed Properties

    /// <summary>
    /// Number of items in wishlist.
    /// </summary>
    public int ItemCount => Items.Count;

    /// <summary>
    /// Whether the wishlist is empty.
    /// </summary>
    public bool IsEmpty => Items.Count == 0;

    #endregion
}

/// <summary>
/// Represents an item in a wishlist.
/// </summary>
public class WishlistItem : BaseEntity
{
    /// <summary>
    /// Wishlist ID.
    /// </summary>
    public Guid WishlistId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant ID if applicable.
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Price when item was added.
    /// </summary>
    public decimal PriceWhenAdded { get; set; }

    /// <summary>
    /// Notes about the item.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Desired quantity.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Priority (1 = highest).
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Navigation property to wishlist.
    /// </summary>
    public Wishlist? Wishlist { get; set; }

    /// <summary>
    /// Navigation property to product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Navigation property to variant.
    /// </summary>
    public ProductVariant? Variant { get; set; }

    #region Computed Properties

    /// <summary>
    /// Whether the price has dropped since adding.
    /// </summary>
    public bool HasPriceDropped(decimal currentPrice) => currentPrice < PriceWhenAdded;

    /// <summary>
    /// Price drop amount.
    /// </summary>
    public decimal GetPriceDrop(decimal currentPrice) =>
        currentPrice < PriceWhenAdded ? PriceWhenAdded - currentPrice : 0;

    #endregion
}
