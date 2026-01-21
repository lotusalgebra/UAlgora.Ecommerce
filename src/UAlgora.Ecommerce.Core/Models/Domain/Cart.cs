namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a shopping cart.
/// </summary>
public class Cart : BaseEntity
{
    /// <summary>
    /// Customer ID if logged in.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Session ID for guest carts.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Currency code for this cart.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Cart items.
    /// </summary>
    public List<CartItem> Items { get; set; } = [];

    #region Pricing

    /// <summary>
    /// Subtotal before discounts and shipping.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Total discount amount.
    /// </summary>
    public decimal DiscountTotal { get; set; }

    /// <summary>
    /// Shipping cost.
    /// </summary>
    public decimal ShippingTotal { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal TaxTotal { get; set; }

    /// <summary>
    /// Grand total (subtotal - discounts + shipping + tax).
    /// </summary>
    public decimal GrandTotal { get; set; }

    #endregion

    #region Discounts

    /// <summary>
    /// Applied discounts.
    /// </summary>
    public List<AppliedDiscount> AppliedDiscounts { get; set; } = [];

    /// <summary>
    /// Applied coupon code.
    /// </summary>
    public string? CouponCode { get; set; }

    #endregion

    #region Shipping

    /// <summary>
    /// Shipping address.
    /// </summary>
    public Address? ShippingAddress { get; set; }

    /// <summary>
    /// Billing address.
    /// </summary>
    public Address? BillingAddress { get; set; }

    /// <summary>
    /// Whether billing address is same as shipping.
    /// </summary>
    public bool BillingSameAsShipping { get; set; } = true;

    /// <summary>
    /// Selected shipping method ID.
    /// </summary>
    public string? SelectedShippingMethod { get; set; }

    /// <summary>
    /// Selected shipping method name.
    /// </summary>
    public string? SelectedShippingMethodName { get; set; }

    #endregion

    #region Customer Info

    /// <summary>
    /// Customer email for guest checkout.
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Customer phone for guest checkout.
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Order notes from customer.
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    /// <summary>
    /// When this cart expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Navigation property to customer.
    /// </summary>
    public Customer? Customer { get; set; }

    #region Computed Properties

    /// <summary>
    /// Total number of items in cart.
    /// </summary>
    public int ItemCount => Items.Sum(i => i.Quantity);

    /// <summary>
    /// Number of unique products in cart.
    /// </summary>
    public int UniqueItemCount => Items.Count;

    /// <summary>
    /// Whether the cart is empty.
    /// </summary>
    public bool IsEmpty => Items.Count == 0;

    /// <summary>
    /// Whether the cart belongs to a guest.
    /// </summary>
    public bool IsGuest => !CustomerId.HasValue;

    /// <summary>
    /// Whether the cart has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    #endregion
}

/// <summary>
/// Represents an item in the shopping cart.
/// </summary>
public class CartItem : BaseEntity
{
    /// <summary>
    /// Parent cart ID.
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant ID if applicable.
    /// </summary>
    public Guid? VariantId { get; set; }

    /// <summary>
    /// Product name (snapshot at time of adding).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product/variant SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Variant name if applicable.
    /// </summary>
    public string? VariantName { get; set; }

    /// <summary>
    /// Variant options (e.g., {"Size": "Large", "Color": "Blue"}).
    /// </summary>
    public Dictionary<string, string>? VariantOptions { get; set; }

    /// <summary>
    /// Product/variant image ID.
    /// </summary>
    public Guid? ImageId { get; set; }

    /// <summary>
    /// Product/variant image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price at time of adding.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Original price before any discounts.
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Line total (quantity * unit price).
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Discount amount applied to this line.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Tax amount for this line.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Weight per unit (for shipping calculation).
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// When this item was added to cart.
    /// </summary>
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Navigation property to cart.
    /// </summary>
    public Cart? Cart { get; set; }

    #region Computed Properties

    /// <summary>
    /// Total weight for this line item.
    /// </summary>
    public decimal? TotalWeight => Weight.HasValue ? Weight.Value * Quantity : null;

    /// <summary>
    /// Final line total after discounts.
    /// </summary>
    public decimal FinalLineTotal => LineTotal - DiscountAmount;

    #endregion
}

/// <summary>
/// Represents an applied discount on a cart or order.
/// </summary>
public class AppliedDiscount
{
    /// <summary>
    /// Discount ID.
    /// </summary>
    public Guid DiscountId { get; set; }

    /// <summary>
    /// Discount code (if coupon-based).
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Discount name/description.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Discount amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Whether this is a coupon code discount.
    /// </summary>
    public bool IsCoupon { get; set; }
}
