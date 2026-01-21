using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for pricing calculations.
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Gets the current price for a product.
    /// </summary>
    Task<decimal> GetProductPriceAsync(Guid productId, Guid? variantId = null, Guid? customerId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets pricing details for a product.
    /// </summary>
    Task<PricingDetails> GetPricingDetailsAsync(Guid productId, Guid? variantId = null, Guid? customerId = null, CancellationToken ct = default);

    /// <summary>
    /// Calculates line item price.
    /// </summary>
    Task<LineItemPricing> CalculateLineItemPriceAsync(Guid productId, Guid? variantId, int quantity, Guid? customerId = null, CancellationToken ct = default);

    /// <summary>
    /// Calculates cart subtotal.
    /// </summary>
    Task<decimal> CalculateSubtotalAsync(Cart cart, CancellationToken ct = default);

    /// <summary>
    /// Gets tiered pricing for a product.
    /// </summary>
    Task<IReadOnlyList<TierPrice>> GetTieredPricingAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets customer-specific pricing for a product.
    /// </summary>
    Task<decimal?> GetCustomerPriceAsync(Guid productId, Guid customerId, Guid? variantId = null, CancellationToken ct = default);

    /// <summary>
    /// Converts price to different currency.
    /// </summary>
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken ct = default);

    /// <summary>
    /// Formats price for display.
    /// </summary>
    string FormatPrice(decimal amount, string? currencyCode = null);
}

/// <summary>
/// Detailed pricing information.
/// </summary>
public class PricingDetails
{
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public bool IsOnSale { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool TaxIncluded { get; set; }
    public IReadOnlyList<TierPrice>? TierPrices { get; set; }
}

/// <summary>
/// Line item pricing calculation.
/// </summary>
public class LineItemPricing
{
    public decimal UnitPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? AppliedTier { get; set; }
}

/// <summary>
/// Tiered pricing.
/// </summary>
public class TierPrice
{
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
}
