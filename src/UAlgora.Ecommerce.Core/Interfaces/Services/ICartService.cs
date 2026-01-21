using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for cart operations.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Gets the current cart for the session/user.
    /// </summary>
    Task<Cart> GetCartAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a cart by ID.
    /// </summary>
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds an item to the cart.
    /// </summary>
    Task<Cart> AddItemAsync(AddToCartRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates the quantity of a cart item.
    /// </summary>
    Task<Cart> UpdateItemQuantityAsync(Guid itemId, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    Task<Cart> RemoveItemAsync(Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    Task<Cart> ClearCartAsync(CancellationToken ct = default);

    /// <summary>
    /// Applies a coupon code to the cart.
    /// </summary>
    Task<Cart> ApplyCouponAsync(string couponCode, CancellationToken ct = default);

    /// <summary>
    /// Removes the applied coupon from the cart.
    /// </summary>
    Task<Cart> RemoveCouponAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets the shipping address for the cart.
    /// </summary>
    Task<Cart> SetShippingAddressAsync(Address address, CancellationToken ct = default);

    /// <summary>
    /// Sets the billing address for the cart.
    /// </summary>
    Task<Cart> SetBillingAddressAsync(Address address, CancellationToken ct = default);

    /// <summary>
    /// Gets available shipping options for the cart.
    /// </summary>
    Task<IReadOnlyList<ShippingOption>> GetShippingOptionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets the shipping method for the cart.
    /// </summary>
    Task<Cart> SetShippingMethodAsync(string shippingMethodId, CancellationToken ct = default);

    /// <summary>
    /// Recalculates cart totals.
    /// </summary>
    Task<Cart> RecalculateAsync(CancellationToken ct = default);

    /// <summary>
    /// Merges a guest cart with a customer cart.
    /// </summary>
    Task<Cart> MergeCartsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Validates the cart for checkout readiness.
    /// </summary>
    Task<CartValidationResult> ValidateForCheckoutAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the count of items in the cart.
    /// </summary>
    Task<int> GetItemCountAsync(CancellationToken ct = default);

    #region Management Methods (Backoffice)

    /// <summary>
    /// Gets all carts with optional filtering (backoffice).
    /// </summary>
    Task<PagedResult<Cart>> GetPagedCartsAsync(
        int page = 1,
        int pageSize = 20,
        Guid? customerId = null,
        bool? isGuest = null,
        bool? isAbandoned = null,
        string? sortBy = null,
        bool descending = true,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a cart by ID with full details (backoffice).
    /// </summary>
    Task<Cart?> GetCartByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets carts by customer ID (backoffice).
    /// </summary>
    Task<List<Cart>> GetCartsByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets abandoned carts (backoffice).
    /// </summary>
    Task<List<Cart>> GetAbandonedCartsAsync(int daysOld = 7, CancellationToken ct = default);

    /// <summary>
    /// Updates cart notes (backoffice).
    /// </summary>
    Task<Cart> UpdateCartNotesAsync(Guid cartId, string? notes, CancellationToken ct = default);

    /// <summary>
    /// Deletes a cart (backoffice).
    /// </summary>
    Task DeleteCartAsync(Guid cartId, CancellationToken ct = default);

    /// <summary>
    /// Deletes expired carts (backoffice).
    /// </summary>
    Task<int> DeleteExpiredCartsAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes abandoned carts older than specified days (backoffice).
    /// </summary>
    Task<int> DeleteAbandonedCartsAsync(int daysOld = 30, CancellationToken ct = default);

    /// <summary>
    /// Gets cart statistics (backoffice).
    /// </summary>
    Task<CartStatistics> GetStatisticsAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets cart expiration (backoffice).
    /// </summary>
    Task<Cart> SetCartExpirationAsync(Guid cartId, DateTime? expiresAt, CancellationToken ct = default);

    /// <summary>
    /// Clears a cart by ID (backoffice).
    /// </summary>
    Task<Cart> ClearCartByIdAsync(Guid cartId, CancellationToken ct = default);

    /// <summary>
    /// Assigns a cart to a customer (backoffice).
    /// </summary>
    Task<Cart> AssignCartToCustomerAsync(Guid cartId, Guid customerId, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Cart statistics summary.
/// </summary>
public class CartStatistics
{
    public int TotalCarts { get; set; }
    public int ActiveCarts { get; set; }
    public int AbandonedCarts { get; set; }
    public int ExpiredCarts { get; set; }
    public int GuestCarts { get; set; }
    public int CustomerCarts { get; set; }
    public int EmptyCarts { get; set; }
    public int CartsWithItems { get; set; }
    public decimal TotalCartValue { get; set; }
    public decimal AverageCartValue { get; set; }
    public int TotalItems { get; set; }
    public double AverageItemsPerCart { get; set; }
    public int TodayCreated { get; set; }
    public int ThisWeekCreated { get; set; }
    public int ThisMonthCreated { get; set; }
    public decimal AbandonedCartValue { get; set; }
}

/// <summary>
/// Request for adding an item to cart.
/// </summary>
public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Represents a shipping option.
/// </summary>
public class ShippingOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? EstimatedDelivery { get; set; }
    public string? Carrier { get; set; }
}

/// <summary>
/// Result of cart validation.
/// </summary>
public class CartValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<CartValidationError> Errors { get; set; } = [];

    public static CartValidationResult Success() => new();

    public static CartValidationResult Failure(string errorCode, string message) => new()
    {
        Errors = [new CartValidationError { ErrorCode = errorCode, Message = message }]
    };
}

/// <summary>
/// Cart validation error.
/// </summary>
public class CartValidationError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? ItemId { get; set; }
}
