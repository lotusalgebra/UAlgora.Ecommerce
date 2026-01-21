using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for product operations.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);

    /// <summary>
    /// Gets a product by slug.
    /// </summary>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated products.
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
    /// Gets new arrival products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetNewArrivalsAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets best selling products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetBestSellersAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets related products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetRelatedAsync(Guid productId, int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Searches products.
    /// </summary>
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken ct = default);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds a variant to a product.
    /// </summary>
    Task<ProductVariant> AddVariantAsync(Guid productId, ProductVariant variant, CancellationToken ct = default);

    /// <summary>
    /// Updates a product variant.
    /// </summary>
    Task<ProductVariant> UpdateVariantAsync(ProductVariant variant, CancellationToken ct = default);

    /// <summary>
    /// Deletes a product variant.
    /// </summary>
    Task DeleteVariantAsync(Guid variantId, CancellationToken ct = default);

    /// <summary>
    /// Generates a unique SKU.
    /// </summary>
    Task<string> GenerateSkuAsync(string baseSku, CancellationToken ct = default);

    /// <summary>
    /// Generates a unique slug.
    /// </summary>
    Task<string> GenerateSlugAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Validates a product.
    /// </summary>
    Task<ValidationResult> ValidateAsync(Product product, CancellationToken ct = default);

    /// <summary>
    /// Gets all brands.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllBrandsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all tags.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default);
}

/// <summary>
/// Validation result for service operations.
/// </summary>
public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<ValidationError> Errors { get; set; } = [];

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(string propertyName, string errorMessage) => new()
    {
        Errors = [new ValidationError { PropertyName = propertyName, ErrorMessage = errorMessage }]
    };

    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => new()
    {
        Errors = errors.ToList()
    };
}

/// <summary>
/// Validation error.
/// </summary>
public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
