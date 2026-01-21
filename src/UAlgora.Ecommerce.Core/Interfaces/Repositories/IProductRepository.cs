using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for product operations.
/// </summary>
public interface IProductRepository : ISoftDeleteRepository<Product>
{
    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);

    /// <summary>
    /// Gets a product by slug.
    /// </summary>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Gets a product by Umbraco node ID.
    /// </summary>
    Task<Product?> GetByUmbracoNodeIdAsync(int nodeId, CancellationToken ct = default);

    /// <summary>
    /// Gets a product with all variants loaded.
    /// </summary>
    Task<Product?> GetWithVariantsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated products with filtering and sorting.
    /// </summary>
    Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters parameters, CancellationToken ct = default);

    /// <summary>
    /// Gets products by category.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, bool includeSubcategories = false, CancellationToken ct = default);

    /// <summary>
    /// Gets featured products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets products on sale.
    /// </summary>
    Task<IReadOnlyList<Product>> GetOnSaleAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets new arrivals.
    /// </summary>
    Task<IReadOnlyList<Product>> GetNewArrivalsAsync(int count = 10, int daysBack = 30, CancellationToken ct = default);

    /// <summary>
    /// Gets best selling products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetBestSellersAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets related products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetRelatedAsync(Guid productId, int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets products by IDs.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Checks if a SKU exists.
    /// </summary>
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a slug exists.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets low stock products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets out of stock products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetOutOfStockAsync(CancellationToken ct = default);

    /// <summary>
    /// Searches products by term.
    /// </summary>
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken ct = default);

    /// <summary>
    /// Updates product stock quantity.
    /// </summary>
    Task UpdateStockAsync(Guid productId, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Updates variant stock quantity.
    /// </summary>
    Task UpdateVariantStockAsync(Guid variantId, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Gets all unique brands.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllBrandsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all unique tags.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default);
}

/// <summary>
/// Query parameters for product listing.
/// </summary>
public class ProductQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public bool IncludeSubcategories { get; set; } = true;
    public string? Brand { get; set; }
    public List<string>? Tags { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public bool? OnSale { get; set; }
    public bool? IsFeatured { get; set; }
    public ProductStatus? Status { get; set; }
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Newest;
    public bool IncludeVariants { get; set; } = false;
}
