using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a product in the e-commerce catalog.
/// </summary>
public class Product : SoftDeleteEntity
{
    /// <summary>
    /// Store this product belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Reference to the Umbraco content node ID.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Stock Keeping Unit - unique product identifier.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug for the product.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Brief product description for listings.
    /// </summary>
    public string? ShortDescription { get; set; }

    /// <summary>
    /// Full product description with rich text support.
    /// </summary>
    public string? Description { get; set; }

    #region Pricing

    /// <summary>
    /// Base retail price.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Sale/discounted price (if on sale).
    /// </summary>
    public decimal? SalePrice { get; set; }

    /// <summary>
    /// Cost price for margin calculations.
    /// </summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Original MSRP/compare-at price for showing discounts.
    /// </summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary>
    /// Currency code (ISO 4217).
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Whether prices include tax.
    /// </summary>
    public bool TaxIncluded { get; set; }

    /// <summary>
    /// Tax class for tax calculation.
    /// </summary>
    public string? TaxClass { get; set; }

    #endregion

    #region Inventory

    /// <summary>
    /// Whether to track inventory for this product.
    /// </summary>
    public bool TrackInventory { get; set; } = true;

    /// <summary>
    /// Current stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Threshold for low stock alerts.
    /// </summary>
    public int LowStockThreshold { get; set; } = 5;

    /// <summary>
    /// Whether to allow orders when out of stock.
    /// </summary>
    public bool AllowBackorders { get; set; }

    /// <summary>
    /// Current stock status.
    /// </summary>
    public StockStatus StockStatus { get; set; } = StockStatus.InStock;

    #endregion

    #region Physical Properties

    /// <summary>
    /// Product weight for shipping calculations.
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Weight unit (kg, lb, oz, g).
    /// </summary>
    public string WeightUnit { get; set; } = "kg";

    /// <summary>
    /// Product length for shipping calculations.
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// Product width for shipping calculations.
    /// </summary>
    public decimal? Width { get; set; }

    /// <summary>
    /// Product height for shipping calculations.
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Dimension unit (cm, in, m).
    /// </summary>
    public string DimensionUnit { get; set; } = "cm";

    #endregion

    #region Organization

    /// <summary>
    /// Primary product image ID.
    /// </summary>
    public Guid? PrimaryImageId { get; set; }

    /// <summary>
    /// Primary image URL.
    /// </summary>
    public string? PrimaryImageUrl { get; set; }

    /// <summary>
    /// Additional product image IDs.
    /// </summary>
    public List<Guid> ImageIds { get; set; } = [];

    /// <summary>
    /// Category IDs this product belongs to.
    /// </summary>
    public List<Guid> CategoryIds { get; set; } = [];

    /// <summary>
    /// Product tags for filtering and search.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Brand name.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Manufacturer name.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Manufacturer Part Number.
    /// </summary>
    public string? Mpn { get; set; }

    /// <summary>
    /// Global Trade Item Number (barcode).
    /// </summary>
    public string? Gtin { get; set; }

    #endregion

    #region Variants

    /// <summary>
    /// Whether this product has variants.
    /// </summary>
    public bool HasVariants { get; set; }

    /// <summary>
    /// Product variants (size, color, etc.).
    /// </summary>
    public List<ProductVariant> Variants { get; set; } = [];

    /// <summary>
    /// Product attributes/options configuration.
    /// </summary>
    public List<ProductAttribute> Attributes { get; set; } = [];

    #endregion

    #region SEO

    /// <summary>
    /// Meta title for SEO.
    /// </summary>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Meta description for SEO.
    /// </summary>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Meta keywords for SEO.
    /// </summary>
    public string? MetaKeywords { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Product publication status.
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    /// <summary>
    /// Whether this product is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Whether this product is visible in listings.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Categories this product belongs to.
    /// </summary>
    public List<Category> Categories { get; set; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Current effective price (sale price if available, otherwise base price).
    /// </summary>
    public decimal CurrentPrice => SalePrice ?? BasePrice;

    /// <summary>
    /// Whether the product is currently on sale.
    /// </summary>
    public bool IsOnSale => SalePrice.HasValue && SalePrice < BasePrice;

    /// <summary>
    /// Whether the product is in stock.
    /// </summary>
    public bool IsInStock => !TrackInventory || StockQuantity > 0 || AllowBackorders;

    /// <summary>
    /// Whether the stock is low.
    /// </summary>
    public bool IsLowStock => TrackInventory && StockQuantity > 0 && StockQuantity <= LowStockThreshold;

    /// <summary>
    /// Discount percentage if on sale.
    /// </summary>
    public decimal? DiscountPercentage => IsOnSale && BasePrice > 0
        ? Math.Round((1 - (SalePrice!.Value / BasePrice)) * 100, 2)
        : null;

    /// <summary>
    /// Profit margin percentage.
    /// </summary>
    public decimal? MarginPercentage => CostPrice.HasValue && CostPrice > 0 && CurrentPrice > 0
        ? Math.Round(((CurrentPrice - CostPrice.Value) / CurrentPrice) * 100, 2)
        : null;

    #endregion
}

/// <summary>
/// Represents a product variant (e.g., different size, color).
/// </summary>
public class ProductVariant : BaseEntity
{
    /// <summary>
    /// Parent product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant-specific SKU.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Variant name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Option values (e.g., {"Size": "Large", "Color": "Blue"}).
    /// </summary>
    public Dictionary<string, string> Options { get; set; } = [];

    /// <summary>
    /// Variant-specific base price (overrides product price if set).
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Variant-specific sale price.
    /// </summary>
    public decimal? SalePrice { get; set; }

    /// <summary>
    /// Variant-specific cost price.
    /// </summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Variant-specific stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Variant-specific image ID.
    /// </summary>
    public Guid? ImageId { get; set; }

    /// <summary>
    /// Variant image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// GTIN/barcode for this variant.
    /// </summary>
    public string? Gtin { get; set; }

    /// <summary>
    /// Variant-specific weight.
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Whether this is the default variant.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this variant is available.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Navigation property to parent product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Current effective price for this variant.
    /// </summary>
    public decimal GetCurrentPrice(Product product) =>
        SalePrice ?? Price ?? product.SalePrice ?? product.BasePrice;

    /// <summary>
    /// Whether this variant is in stock.
    /// </summary>
    public bool IsInStock(Product product) =>
        !product.TrackInventory || StockQuantity > 0 || product.AllowBackorders;
}

/// <summary>
/// Represents a product attribute/option configuration (e.g., Size, Color).
/// </summary>
public class ProductAttribute
{
    /// <summary>
    /// Attribute name (e.g., "Size", "Color").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Available values for this attribute.
    /// </summary>
    public List<string> Values { get; set; } = [];

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }
}
