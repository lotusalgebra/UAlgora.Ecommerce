using System.Text.RegularExpressions;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for product operations.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _productRepository.GetByIdAsync(id, ct);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        return await _productRepository.GetBySkuAsync(sku, ct);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _productRepository.GetBySlugAsync(slug, ct);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        ProductQueryParameters parameters,
        CancellationToken ct = default)
    {
        return await _productRepository.GetPagedAsync(parameters, ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        bool includeSubcategories = false,
        CancellationToken ct = default)
    {
        return await _productRepository.GetByCategoryAsync(categoryId, includeSubcategories, ct);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 10, CancellationToken ct = default)
    {
        return await _productRepository.GetFeaturedAsync(count, ct);
    }

    public async Task<IReadOnlyList<Product>> GetOnSaleAsync(int count = 10, CancellationToken ct = default)
    {
        return await _productRepository.GetOnSaleAsync(count, ct);
    }

    public async Task<IReadOnlyList<Product>> GetNewArrivalsAsync(int count = 10, CancellationToken ct = default)
    {
        return await _productRepository.GetNewArrivalsAsync(count, 30, ct);
    }

    public async Task<IReadOnlyList<Product>> GetBestSellersAsync(int count = 10, CancellationToken ct = default)
    {
        return await _productRepository.GetBestSellersAsync(count, ct);
    }

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(
        Guid productId,
        int count = 10,
        CancellationToken ct = default)
    {
        return await _productRepository.GetRelatedAsync(productId, count, ct);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken ct = default)
    {
        return await _productRepository.SearchAsync(searchTerm, maxResults, ct);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        // Generate SKU if not provided
        if (string.IsNullOrWhiteSpace(product.Sku))
        {
            product.Sku = await GenerateSkuAsync(product.Name, ct);
        }

        // Generate slug if not provided
        if (string.IsNullOrWhiteSpace(product.Slug))
        {
            product.Slug = await GenerateSlugAsync(product.Name, ct);
        }

        return await _productRepository.AddAsync(product, ct);
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        return await _productRepository.UpdateAsync(product, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _productRepository.SoftDeleteAsync(id, ct);
    }

    public async Task<ProductVariant> AddVariantAsync(
        Guid productId,
        ProductVariant variant,
        CancellationToken ct = default)
    {
        var product = await _productRepository.GetWithVariantsAsync(productId, ct);
        if (product == null)
        {
            throw new InvalidOperationException($"Product {productId} not found.");
        }

        variant.ProductId = productId;

        // Generate variant SKU if not provided
        if (string.IsNullOrWhiteSpace(variant.Sku))
        {
            var baseSku = product.Sku ?? product.Name;
            var suffix = variant.Name ?? Guid.NewGuid().ToString()[..6];
            variant.Sku = await GenerateSkuAsync($"{baseSku}-{suffix}", ct);
        }

        product.Variants.Add(variant);
        await _productRepository.UpdateAsync(product, ct);

        return variant;
    }

    public async Task<ProductVariant> UpdateVariantAsync(
        ProductVariant variant,
        CancellationToken ct = default)
    {
        var product = await _productRepository.GetWithVariantsAsync(variant.ProductId, ct);
        if (product == null)
        {
            throw new InvalidOperationException($"Product {variant.ProductId} not found.");
        }

        var existingVariant = product.Variants.FirstOrDefault(v => v.Id == variant.Id);
        if (existingVariant == null)
        {
            throw new InvalidOperationException($"Variant {variant.Id} not found.");
        }

        // Update variant properties
        existingVariant.Sku = variant.Sku;
        existingVariant.Name = variant.Name;
        existingVariant.Price = variant.Price;
        existingVariant.SalePrice = variant.SalePrice;
        existingVariant.StockQuantity = variant.StockQuantity;
        existingVariant.Options = variant.Options;
        existingVariant.ImageId = variant.ImageId;
        existingVariant.Weight = variant.Weight;
        existingVariant.IsAvailable = variant.IsAvailable;

        await _productRepository.UpdateAsync(product, ct);
        return existingVariant;
    }

    public async Task DeleteVariantAsync(Guid variantId, CancellationToken ct = default)
    {
        // Find the product that contains this variant
        var products = await _productRepository.GetAllAsync(ct);
        foreach (var product in products)
        {
            var fullProduct = await _productRepository.GetWithVariantsAsync(product.Id, ct);
            if (fullProduct == null) continue;

            var variant = fullProduct.Variants.FirstOrDefault(v => v.Id == variantId);
            if (variant != null)
            {
                fullProduct.Variants.Remove(variant);
                await _productRepository.UpdateAsync(fullProduct, ct);
                return;
            }
        }

        throw new InvalidOperationException($"Variant {variantId} not found.");
    }

    public async Task<string> GenerateSkuAsync(string baseSku, CancellationToken ct = default)
    {
        var sku = GenerateBaseSku(baseSku);
        var counter = 0;
        var uniqueSku = sku;

        while (await _productRepository.SkuExistsAsync(uniqueSku, null, ct))
        {
            counter++;
            uniqueSku = $"{sku}-{counter}";
        }

        return uniqueSku;
    }

    public async Task<string> GenerateSlugAsync(string name, CancellationToken ct = default)
    {
        var slug = GenerateBaseSlug(name);
        var counter = 0;
        var uniqueSlug = slug;

        while (await _productRepository.SlugExistsAsync(uniqueSlug, null, ct))
        {
            counter++;
            uniqueSlug = $"{slug}-{counter}";
        }

        return uniqueSlug;
    }

    public async Task<ValidationResult> ValidateAsync(Product product, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Product name is required." });
        }

        if (product.BasePrice < 0)
        {
            errors.Add(new ValidationError { PropertyName = "BasePrice", ErrorMessage = "Price cannot be negative." });
        }

        if (!string.IsNullOrWhiteSpace(product.Sku))
        {
            if (await _productRepository.SkuExistsAsync(product.Sku, product.Id == Guid.Empty ? null : product.Id, ct))
            {
                errors.Add(new ValidationError { PropertyName = "Sku", ErrorMessage = "SKU already exists." });
            }
        }

        if (!string.IsNullOrWhiteSpace(product.Slug))
        {
            if (await _productRepository.SlugExistsAsync(product.Slug, product.Id == Guid.Empty ? null : product.Id, ct))
            {
                errors.Add(new ValidationError { PropertyName = "Slug", ErrorMessage = "Slug already exists." });
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    public async Task<IReadOnlyList<string>> GetAllBrandsAsync(CancellationToken ct = default)
    {
        return await _productRepository.GetAllBrandsAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default)
    {
        return await _productRepository.GetAllTagsAsync(ct);
    }

    private static string GenerateBaseSku(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Guid.NewGuid().ToString()[..8].ToUpperInvariant();

        // Remove special characters and convert to uppercase
        var sku = Regex.Replace(input, @"[^a-zA-Z0-9\-]", "");
        sku = sku.ToUpperInvariant();

        // Limit length
        if (sku.Length > 20)
            sku = sku[..20];

        return sku;
    }

    private static string GenerateBaseSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Guid.NewGuid().ToString()[..8].ToLowerInvariant();

        // Convert to lowercase
        var slug = input.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove special characters except hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove multiple consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 100)
            slug = slug[..100].TrimEnd('-');

        return slug;
    }
}
