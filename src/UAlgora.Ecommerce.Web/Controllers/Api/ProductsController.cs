using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using static UAlgora.Ecommerce.Web.ServiceCollectionExtensions;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for product operations.
/// </summary>
public class ProductsController : EcommerceApiController
{
    private readonly IProductService _productService;
    private readonly IPricingService _pricingService;

    public ProductsController(
        IProductService productService,
        IPricingService pricingService)
    {
        _productService = productService;
        _pricingService = pricingService;
    }

    /// <summary>
    /// Gets a paginated list of products.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? brand = null,
        [FromQuery] ProductSortBy? sortBy = null,
        CancellationToken ct = default)
    {
        var parameters = new ProductQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = categoryId,
            SearchTerm = search,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Brand = brand,
            SortBy = sortBy ?? ProductSortBy.Newest
        };

        var result = await _productService.GetPagedAsync(parameters, ct);

        return ApiPaged(result.Items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        return ApiSuccess(product);
    }

    /// <summary>
    /// Gets a product by slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug, CancellationToken ct = default)
    {
        var product = await _productService.GetBySlugAsync(slug, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        return ApiSuccess(product);
    }

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    [HttpGet("by-sku/{sku}")]
    public async Task<IActionResult> GetProductBySku(string sku, CancellationToken ct = default)
    {
        var product = await _productService.GetBySkuAsync(sku, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        return ApiSuccess(product);
    }

    /// <summary>
    /// Gets products by category.
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] bool includeSubcategories = false,
        CancellationToken ct = default)
    {
        var products = await _productService.GetByCategoryAsync(categoryId, includeSubcategories, ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets featured products.
    /// </summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedProducts(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var products = await _productService.GetFeaturedAsync(count, ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets products on sale.
    /// </summary>
    [HttpGet("on-sale")]
    public async Task<IActionResult> GetOnSaleProducts(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var products = await _productService.GetOnSaleAsync(count, ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets new arrival products.
    /// </summary>
    [HttpGet("new-arrivals")]
    public async Task<IActionResult> GetNewArrivals(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var products = await _productService.GetNewArrivalsAsync(count, ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets best selling products.
    /// </summary>
    [HttpGet("best-sellers")]
    public async Task<IActionResult> GetBestSellers(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var products = await _productService.GetBestSellersAsync(count, ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets related products.
    /// </summary>
    [HttpGet("{id:guid}/related")]
    public async Task<IActionResult> GetRelatedProducts(
        Guid id,
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var products = await _productService.GetRelatedAsync(id, count, ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Searches products.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string q,
        [FromQuery] int maxResults = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ApiErrorResponse { Message = "Search query is required." });
        }

        var products = await _productService.SearchAsync(q, maxResults, ct);
        return ApiSuccess(products);
    }

    /// <summary>
    /// Gets pricing details for a product.
    /// </summary>
    [HttpGet("{id:guid}/pricing")]
    public async Task<IActionResult> GetProductPricing(
        Guid id,
        [FromQuery] Guid? variantId = null,
        CancellationToken ct = default)
    {
        try
        {
            var pricing = await _pricingService.GetPricingDetailsAsync(id, variantId, null, ct);
            return ApiSuccess(pricing);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all available brands.
    /// </summary>
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken ct = default)
    {
        var brands = await _productService.GetAllBrandsAsync(ct);
        return ApiSuccess(brands);
    }

    /// <summary>
    /// Gets all available tags.
    /// </summary>
    [HttpGet("tags")]
    public async Task<IActionResult> GetTags(CancellationToken ct = default)
    {
        var tags = await _productService.GetAllTagsAsync(ct);
        return ApiSuccess(tags);
    }
}

#region Admin Endpoints

/// <summary>
/// Admin API controller for product management.
/// </summary>
[Route("api/ecommerce/admin/products")]
[Authorize(Policy = EcommerceAdminPolicy)]
public class ProductsAdminController : EcommerceApiController
{
    private readonly IProductService _productService;

    public ProductsAdminController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Gets paginated products (admin).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] ProductStatus? status = null,
        [FromQuery] string? brand = null,
        [FromQuery] ProductSortBy? sortBy = null,
        CancellationToken ct = default)
    {
        var parameters = new ProductQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = categoryId,
            SearchTerm = search,
            Status = status,
            Brand = brand,
            SortBy = sortBy ?? ProductSortBy.Newest
        };

        var result = await _productService.GetPagedAsync(parameters, ct);
        return ApiPaged(result.Items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <summary>
    /// Gets a product by ID (admin).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        return ApiSuccess(product);
    }

    /// <summary>
    /// Gets a product by SKU (admin).
    /// </summary>
    [HttpGet("by-sku/{sku}")]
    public async Task<IActionResult> GetProductBySku(string sku, CancellationToken ct = default)
    {
        var product = await _productService.GetBySkuAsync(sku, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        return ApiSuccess(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken ct = default)
    {
        var product = new Product
        {
            Sku = request.Sku ?? await _productService.GenerateSkuAsync(request.Name, ct),
            Name = request.Name,
            Slug = request.Slug ?? await _productService.GenerateSlugAsync(request.Name, ct),
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            BasePrice = request.BasePrice,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            CompareAtPrice = request.CompareAtPrice,
            CurrencyCode = request.CurrencyCode ?? "USD",
            TaxIncluded = request.TaxIncluded,
            TaxClass = request.TaxClass,
            TrackInventory = request.TrackInventory,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold ?? 5,
            AllowBackorders = request.AllowBackorders,
            Weight = request.Weight,
            WeightUnit = request.WeightUnit ?? "kg",
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            DimensionUnit = request.DimensionUnit ?? "cm",
            PrimaryImageId = request.PrimaryImageId,
            PrimaryImageUrl = request.PrimaryImageUrl,
            ImageIds = request.ImageIds ?? [],
            CategoryIds = request.CategoryIds ?? [],
            Tags = request.Tags ?? [],
            Brand = request.Brand,
            Manufacturer = request.Manufacturer,
            Mpn = request.Mpn,
            Gtin = request.Gtin,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            Status = request.Status,
            IsFeatured = request.IsFeatured,
            IsVisible = request.IsVisible,
            SortOrder = request.SortOrder,
            UmbracoNodeId = request.UmbracoNodeId
        };

        // Add attributes if provided
        if (request.Attributes != null)
        {
            product.Attributes = request.Attributes.Select(a => new ProductAttribute
            {
                Name = a.Name,
                Values = a.Values ?? [],
                SortOrder = a.SortOrder
            }).ToList();
        }

        var validation = await _productService.ValidateAsync(product, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var created = await _productService.CreateAsync(product, ct);
        return ApiSuccess(created, "Product created.");
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        // Update basic info
        product.Name = request.Name ?? product.Name;
        product.Sku = request.Sku ?? product.Sku;
        product.Slug = request.Slug ?? product.Slug;
        product.ShortDescription = request.ShortDescription ?? product.ShortDescription;
        product.Description = request.Description ?? product.Description;

        // Update pricing
        if (request.BasePrice.HasValue)
            product.BasePrice = request.BasePrice.Value;
        product.SalePrice = request.SalePrice ?? product.SalePrice;
        product.CostPrice = request.CostPrice ?? product.CostPrice;
        product.CompareAtPrice = request.CompareAtPrice ?? product.CompareAtPrice;
        product.CurrencyCode = request.CurrencyCode ?? product.CurrencyCode;
        if (request.TaxIncluded.HasValue)
            product.TaxIncluded = request.TaxIncluded.Value;
        product.TaxClass = request.TaxClass ?? product.TaxClass;

        // Update inventory
        if (request.TrackInventory.HasValue)
            product.TrackInventory = request.TrackInventory.Value;
        if (request.StockQuantity.HasValue)
            product.StockQuantity = request.StockQuantity.Value;
        if (request.LowStockThreshold.HasValue)
            product.LowStockThreshold = request.LowStockThreshold.Value;
        if (request.AllowBackorders.HasValue)
            product.AllowBackorders = request.AllowBackorders.Value;
        if (request.StockStatus.HasValue)
            product.StockStatus = request.StockStatus.Value;

        // Update physical properties
        product.Weight = request.Weight ?? product.Weight;
        product.WeightUnit = request.WeightUnit ?? product.WeightUnit;
        product.Length = request.Length ?? product.Length;
        product.Width = request.Width ?? product.Width;
        product.Height = request.Height ?? product.Height;
        product.DimensionUnit = request.DimensionUnit ?? product.DimensionUnit;

        // Update organization
        if (request.PrimaryImageId.HasValue)
            product.PrimaryImageId = request.PrimaryImageId;
        product.PrimaryImageUrl = request.PrimaryImageUrl ?? product.PrimaryImageUrl;
        if (request.ImageIds != null)
            product.ImageIds = request.ImageIds;
        if (request.CategoryIds != null)
            product.CategoryIds = request.CategoryIds;
        if (request.Tags != null)
            product.Tags = request.Tags;
        product.Brand = request.Brand ?? product.Brand;
        product.Manufacturer = request.Manufacturer ?? product.Manufacturer;
        product.Mpn = request.Mpn ?? product.Mpn;
        product.Gtin = request.Gtin ?? product.Gtin;

        // Update SEO
        product.MetaTitle = request.MetaTitle ?? product.MetaTitle;
        product.MetaDescription = request.MetaDescription ?? product.MetaDescription;
        product.MetaKeywords = request.MetaKeywords ?? product.MetaKeywords;

        // Update status
        if (request.Status.HasValue)
            product.Status = request.Status.Value;
        if (request.IsFeatured.HasValue)
            product.IsFeatured = request.IsFeatured.Value;
        if (request.IsVisible.HasValue)
            product.IsVisible = request.IsVisible.Value;
        if (request.SortOrder.HasValue)
            product.SortOrder = request.SortOrder.Value;

        // Update attributes if provided
        if (request.Attributes != null)
        {
            product.Attributes = request.Attributes.Select(a => new ProductAttribute
            {
                Name = a.Name,
                Values = a.Values ?? [],
                SortOrder = a.SortOrder
            }).ToList();
        }

        var validation = await _productService.ValidateAsync(product, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var updated = await _productService.UpdateAsync(product, ct);
        return ApiSuccess(updated, "Product updated.");
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        await _productService.DeleteAsync(id, ct);
        return ApiSuccess(new { }, "Product deleted.");
    }

    /// <summary>
    /// Updates product status.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateProductStatusRequest request,
        CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        product.Status = request.Status;
        if (request.IsVisible.HasValue)
            product.IsVisible = request.IsVisible.Value;

        var updated = await _productService.UpdateAsync(product, ct);
        return ApiSuccess(updated, "Product status updated.");
    }

    /// <summary>
    /// Generates a unique SKU.
    /// </summary>
    [HttpPost("generate-sku")]
    public async Task<IActionResult> GenerateSku(
        [FromBody] GenerateSkuRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.BaseSku))
        {
            return BadRequest(new ApiErrorResponse { Message = "Base SKU is required." });
        }

        var sku = await _productService.GenerateSkuAsync(request.BaseSku, ct);
        return ApiSuccess(new { sku });
    }

    /// <summary>
    /// Generates a unique slug.
    /// </summary>
    [HttpPost("generate-slug")]
    public async Task<IActionResult> GenerateSlug(
        [FromBody] GenerateSlugRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiErrorResponse { Message = "Name is required." });
        }

        var slug = await _productService.GenerateSlugAsync(request.Name, ct);
        return ApiSuccess(new { slug });
    }

    #region Variants

    /// <summary>
    /// Gets all variants for a product.
    /// </summary>
    [HttpGet("{productId:guid}/variants")]
    public async Task<IActionResult> GetVariants(Guid productId, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(productId, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        return ApiSuccess(product.Variants);
    }

    /// <summary>
    /// Adds a variant to a product.
    /// </summary>
    [HttpPost("{productId:guid}/variants")]
    public async Task<IActionResult> AddVariant(
        Guid productId,
        [FromBody] CreateVariantRequest request,
        CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(productId, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        var variant = new ProductVariant
        {
            ProductId = productId,
            Sku = request.Sku ?? await _productService.GenerateSkuAsync($"{product.Sku}-{request.Name}", ct),
            Name = request.Name,
            Options = request.Options ?? [],
            Price = request.Price,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            StockQuantity = request.StockQuantity,
            ImageId = request.ImageId,
            ImageUrl = request.ImageUrl,
            Gtin = request.Gtin,
            Weight = request.Weight,
            IsDefault = request.IsDefault,
            IsAvailable = request.IsAvailable,
            SortOrder = request.SortOrder
        };

        try
        {
            var created = await _productService.AddVariantAsync(productId, variant, ct);
            return ApiSuccess(created, "Variant added.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Updates a product variant.
    /// </summary>
    [HttpPut("{productId:guid}/variants/{variantId:guid}")]
    public async Task<IActionResult> UpdateVariant(
        Guid productId,
        Guid variantId,
        [FromBody] UpdateVariantRequest request,
        CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(productId, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Variant not found." });
        }

        // Update variant properties
        variant.Name = request.Name ?? variant.Name;
        variant.Sku = request.Sku ?? variant.Sku;
        if (request.Options != null)
            variant.Options = request.Options;
        variant.Price = request.Price ?? variant.Price;
        variant.SalePrice = request.SalePrice ?? variant.SalePrice;
        variant.CostPrice = request.CostPrice ?? variant.CostPrice;
        if (request.StockQuantity.HasValue)
            variant.StockQuantity = request.StockQuantity.Value;
        if (request.ImageId.HasValue)
            variant.ImageId = request.ImageId;
        variant.ImageUrl = request.ImageUrl ?? variant.ImageUrl;
        variant.Gtin = request.Gtin ?? variant.Gtin;
        variant.Weight = request.Weight ?? variant.Weight;
        if (request.IsDefault.HasValue)
            variant.IsDefault = request.IsDefault.Value;
        if (request.IsAvailable.HasValue)
            variant.IsAvailable = request.IsAvailable.Value;
        if (request.SortOrder.HasValue)
            variant.SortOrder = request.SortOrder.Value;

        try
        {
            var updated = await _productService.UpdateVariantAsync(variant, ct);
            return ApiSuccess(updated, "Variant updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a product variant.
    /// </summary>
    [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
    public async Task<IActionResult> DeleteVariant(
        Guid productId,
        Guid variantId,
        CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(productId, ct);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Product not found." });
        }

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Variant not found." });
        }

        try
        {
            await _productService.DeleteVariantAsync(variantId, ct);
            return ApiSuccess(new { }, "Variant deleted.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Updates multiple products' status.
    /// </summary>
    [HttpPost("bulk/status")]
    public async Task<IActionResult> BulkUpdateStatus(
        [FromBody] BulkUpdateStatusRequest request,
        CancellationToken ct = default)
    {
        if (request.ProductIds == null || request.ProductIds.Count == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "At least one product ID is required." });
        }

        var updatedCount = 0;
        var errors = new List<string>();

        foreach (var productId in request.ProductIds)
        {
            var product = await _productService.GetByIdAsync(productId, ct);
            if (product == null)
            {
                errors.Add($"Product {productId} not found.");
                continue;
            }

            product.Status = request.Status;
            if (request.IsVisible.HasValue)
                product.IsVisible = request.IsVisible.Value;

            await _productService.UpdateAsync(product, ct);
            updatedCount++;
        }

        return ApiSuccess(new { updatedCount, errors }, $"{updatedCount} products updated.");
    }

    /// <summary>
    /// Deletes multiple products.
    /// </summary>
    [HttpPost("bulk/delete")]
    public async Task<IActionResult> BulkDelete(
        [FromBody] BulkDeleteRequest request,
        CancellationToken ct = default)
    {
        if (request.ProductIds == null || request.ProductIds.Count == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "At least one product ID is required." });
        }

        var deletedCount = 0;
        var errors = new List<string>();

        foreach (var productId in request.ProductIds)
        {
            try
            {
                await _productService.DeleteAsync(productId, ct);
                deletedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to delete product {productId}: {ex.Message}");
            }
        }

        return ApiSuccess(new { deletedCount, errors }, $"{deletedCount} products deleted.");
    }

    /// <summary>
    /// Updates category assignment for multiple products.
    /// </summary>
    [HttpPost("bulk/categories")]
    public async Task<IActionResult> BulkUpdateCategories(
        [FromBody] BulkUpdateCategoriesRequest request,
        CancellationToken ct = default)
    {
        if (request.ProductIds == null || request.ProductIds.Count == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "At least one product ID is required." });
        }

        var updatedCount = 0;
        var errors = new List<string>();

        foreach (var productId in request.ProductIds)
        {
            var product = await _productService.GetByIdAsync(productId, ct);
            if (product == null)
            {
                errors.Add($"Product {productId} not found.");
                continue;
            }

            if (request.Mode == BulkCategoryMode.Replace)
            {
                product.CategoryIds = request.CategoryIds ?? [];
            }
            else if (request.Mode == BulkCategoryMode.Add)
            {
                var newCategories = (request.CategoryIds ?? []).Except(product.CategoryIds);
                product.CategoryIds.AddRange(newCategories);
            }
            else if (request.Mode == BulkCategoryMode.Remove)
            {
                product.CategoryIds = product.CategoryIds.Except(request.CategoryIds ?? []).ToList();
            }

            await _productService.UpdateAsync(product, ct);
            updatedCount++;
        }

        return ApiSuccess(new { updatedCount, errors }, $"{updatedCount} products updated.");
    }

    #endregion
}

#endregion

#region Request Models

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    // Pricing
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? CurrencyCode { get; set; }
    public bool TaxIncluded { get; set; }
    public string? TaxClass { get; set; }

    // Inventory
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool AllowBackorders { get; set; }

    // Physical properties
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; }

    // Organization
    public Guid? PrimaryImageId { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<Guid>? ImageIds { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public List<string>? Tags { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Mpn { get; set; }
    public string? Gtin { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    // Status
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public bool IsFeatured { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }

    // Umbraco
    public int? UmbracoNodeId { get; set; }

    // Attributes
    public List<ProductAttributeRequest>? Attributes { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    // Pricing
    public decimal? BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? CurrencyCode { get; set; }
    public bool? TaxIncluded { get; set; }
    public string? TaxClass { get; set; }

    // Inventory
    public bool? TrackInventory { get; set; }
    public int? StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool? AllowBackorders { get; set; }
    public StockStatus? StockStatus { get; set; }

    // Physical properties
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; }

    // Organization
    public Guid? PrimaryImageId { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<Guid>? ImageIds { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public List<string>? Tags { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Mpn { get; set; }
    public string? Gtin { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    // Status
    public ProductStatus? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsVisible { get; set; }
    public int? SortOrder { get; set; }

    // Attributes
    public List<ProductAttributeRequest>? Attributes { get; set; }
}

public class ProductAttributeRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string>? Values { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateProductStatusRequest
{
    public ProductStatus Status { get; set; }
    public bool? IsVisible { get; set; }
}

public class GenerateSkuRequest
{
    public string BaseSku { get; set; } = string.Empty;
}

public class GenerateSlugRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CreateVariantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public Dictionary<string, string>? Options { get; set; }
    public decimal? Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public Guid? ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Gtin { get; set; }
    public decimal? Weight { get; set; }
    public bool IsDefault { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateVariantRequest
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public Dictionary<string, string>? Options { get; set; }
    public decimal? Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int? StockQuantity { get; set; }
    public Guid? ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Gtin { get; set; }
    public decimal? Weight { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsAvailable { get; set; }
    public int? SortOrder { get; set; }
}

public class BulkUpdateStatusRequest
{
    public List<Guid> ProductIds { get; set; } = [];
    public ProductStatus Status { get; set; }
    public bool? IsVisible { get; set; }
}

public class BulkDeleteRequest
{
    public List<Guid> ProductIds { get; set; } = [];
}

public class BulkUpdateCategoriesRequest
{
    public List<Guid> ProductIds { get; set; } = [];
    public List<Guid>? CategoryIds { get; set; }
    public BulkCategoryMode Mode { get; set; } = BulkCategoryMode.Replace;
}

public enum BulkCategoryMode
{
    Replace = 0,
    Add = 1,
    Remove = 2
}

#endregion
