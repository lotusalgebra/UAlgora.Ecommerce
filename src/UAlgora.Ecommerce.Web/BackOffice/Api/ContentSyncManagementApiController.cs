using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using UAlgora.Ecommerce.Web.Services;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Algora Content Sync Management API Controller
///
/// Provides endpoints for syncing Algora Commerce data to Umbraco content tree.
/// This allows products and categories to appear in the Content section.
/// Supports bidirectional sync: Database ↔ Content Tree
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/content-sync")]
public class ContentSyncManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ProductContentSyncService _productSyncService;
    private readonly CategoryContentSyncService _categorySyncService;
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<ContentSyncManagementApiController> _logger;

    public ContentSyncManagementApiController(
        ProductContentSyncService productSyncService,
        CategoryContentSyncService categorySyncService,
        IContentService contentService,
        IContentTypeService contentTypeService,
        IProductService productService,
        ICategoryService categoryService,
        ILogger<ContentSyncManagementApiController> logger)
    {
        _productSyncService = productSyncService;
        _categorySyncService = categorySyncService;
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _productService = productService;
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Syncs all products from the database to Umbraco content tree
    /// </summary>
    [HttpPost("products")]
    [ProducesResponseType(typeof(SyncResultDto), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SyncProducts()
    {
        try
        {
            var result = await _productSyncService.SyncAllProductsAsync();

            return Ok(new SyncResultDto
            {
                Success = result.Success,
                TotalProducts = result.TotalProducts,
                Created = result.Created,
                Updated = result.Updated,
                Errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Syncs all products from Umbraco content tree to the database.
    /// This ensures products created via the content tree appear in the product dashboard.
    /// </summary>
    [HttpPost("content-to-database")]
    [ProducesResponseType(typeof(SyncResultDto), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SyncContentToDatabase()
    {
        var result = new SyncResultDto();

        try
        {
            _logger.LogInformation("Starting content-to-database sync...");

            // Get the Algora Product document type
            var productDocType = _contentTypeService.Get(AlgoraDocumentTypeConstants.ProductAlias);
            if (productDocType == null)
            {
                result.Errors.Add("Algora Product document type not found");
                return Ok(result);
            }

            // Find all content nodes of type Algora Product
            var productContents = _contentService.GetPagedOfType(
                productDocType.Id,
                0,
                int.MaxValue,
                out var totalRecords,
                null);

            result.TotalProducts = (int)totalRecords;
            _logger.LogInformation("Found {Count} product content nodes to sync", result.TotalProducts);

            foreach (var content in productContents)
            {
                try
                {
                    var sku = content.GetValue<string>("sku");
                    if (string.IsNullOrWhiteSpace(sku))
                    {
                        _logger.LogWarning("Skipping product content {Id} - no SKU", content.Id);
                        result.Errors.Add($"Skipped content {content.Id} - no SKU");
                        continue;
                    }

                    // Check if product exists in database by SKU
                    var existingProduct = await _productService.GetBySkuAsync(sku);

                    if (existingProduct != null)
                    {
                        // Update existing product
                        MapContentToProduct(content, existingProduct);
                        await _productService.UpdateAsync(existingProduct);
                        result.Updated++;
                        _logger.LogDebug("Updated product: {Sku}", sku);
                    }
                    else
                    {
                        // Create new product
                        var newProduct = new Product();
                        MapContentToProduct(content, newProduct);
                        await _productService.CreateAsync(newProduct);
                        result.Created++;
                        _logger.LogDebug("Created product: {Sku}", sku);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing product content {Id}", content.Id);
                    result.Errors.Add($"Failed to sync content {content.Id}: {ex.Message}");
                }
            }

            result.Success = result.Errors.Count == 0;
            _logger.LogInformation("Content-to-database sync complete. Created: {Created}, Updated: {Updated}, Errors: {Errors}",
                result.Created, result.Updated, result.Errors.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during content-to-database sync");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Syncs all categories from the database to Umbraco content tree
    /// </summary>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(CategorySyncResultDto), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SyncCategories()
    {
        try
        {
            var result = await _categorySyncService.SyncAllCategoriesAsync();

            return Ok(new CategorySyncResultDto
            {
                Success = result.Success,
                TotalCategories = result.TotalCategories,
                Created = result.Created,
                Updated = result.Updated,
                Errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Syncs all categories from Umbraco content tree to the database.
    /// This ensures categories created via the content tree appear in the category dashboard.
    /// </summary>
    [HttpPost("categories-to-database")]
    [ProducesResponseType(typeof(CategorySyncResultDto), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SyncCategoriesToDatabase()
    {
        var result = new CategorySyncResultDto();

        try
        {
            _logger.LogInformation("Starting category content-to-database sync...");

            // Get the Algora Category document type
            var categoryDocType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CategoryAlias);
            if (categoryDocType == null)
            {
                result.Errors.Add("Algora Category document type not found");
                return Ok(result);
            }

            // Find all content nodes of type Algora Category
            var categoryContents = _contentService.GetPagedOfType(
                categoryDocType.Id,
                0,
                int.MaxValue,
                out var totalRecords,
                null);

            result.TotalCategories = (int)totalRecords;
            _logger.LogInformation("Found {Count} category content nodes to sync", result.TotalCategories);

            foreach (var content in categoryContents)
            {
                try
                {
                    var name = content.GetValue<string>("categoryName") ?? content.Name;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _logger.LogWarning("Skipping category content {Id} - no name", content.Id);
                        result.Errors.Add($"Skipped content {content.Id} - no name");
                        continue;
                    }

                    var slug = content.GetValue<string>("slug") ?? GenerateSlug(name);

                    // Check if category exists in database by UmbracoNodeId or slug
                    var allCategories = await _categoryService.GetAllAsync();
                    var existingCategory = allCategories.FirstOrDefault(c => c.UmbracoNodeId == content.Id)
                        ?? await _categoryService.GetBySlugAsync(slug);

                    if (existingCategory != null)
                    {
                        // Update existing category
                        MapContentToCategory(content, existingCategory);
                        await _categoryService.UpdateAsync(existingCategory);
                        result.Updated++;
                        _logger.LogDebug("Updated category: {Name}", name);
                    }
                    else
                    {
                        // Create new category
                        var newCategory = new Category();
                        MapContentToCategory(content, newCategory);
                        await _categoryService.CreateAsync(newCategory);
                        result.Created++;
                        _logger.LogDebug("Created category: {Name}", name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing category content {Id}", content.Id);
                    result.Errors.Add($"Failed to sync content {content.Id}: {ex.Message}");
                }
            }

            result.Success = result.Errors.Count == 0;
            _logger.LogInformation("Category content-to-database sync complete. Created: {Created}, Updated: {Updated}, Errors: {Errors}",
                result.Created, result.Updated, result.Errors.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during category content-to-database sync");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Syncs all data (products and categories) bidirectionally
    /// </summary>
    [HttpPost("sync-all")]
    [ProducesResponseType(typeof(FullSyncResultDto), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SyncAll()
    {
        var result = new FullSyncResultDto();

        try
        {
            _logger.LogInformation("Starting full bidirectional sync...");

            // Sync categories first (DB → Content)
            var categoryResult = await _categorySyncService.SyncAllCategoriesAsync();
            result.Categories = new CategorySyncResultDto
            {
                Success = categoryResult.Success,
                TotalCategories = categoryResult.TotalCategories,
                Created = categoryResult.Created,
                Updated = categoryResult.Updated,
                Errors = categoryResult.Errors
            };

            // Sync products (DB → Content)
            var productResult = await _productSyncService.SyncAllProductsAsync();
            result.Products = new SyncResultDto
            {
                Success = productResult.Success,
                TotalProducts = productResult.TotalProducts,
                Created = productResult.Created,
                Updated = productResult.Updated,
                Errors = productResult.Errors
            };

            result.Success = result.Categories.Success && result.Products.Success;

            _logger.LogInformation("Full sync complete. Categories: {CatCreated}/{CatUpdated}, Products: {ProdCreated}/{ProdUpdated}",
                result.Categories.Created, result.Categories.Updated,
                result.Products.Created, result.Products.Updated);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full sync");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private void MapContentToCategory(Umbraco.Cms.Core.Models.IContent content, Category category)
    {
        // Set Umbraco Node ID for reverse lookup
        category.UmbracoNodeId = content.Id;

        // Content properties
        category.Name = content.GetValue<string>("categoryName") ?? content.Name ?? "";
        category.Slug = content.GetValue<string>("slug") ?? GenerateSlug(category.Name);
        category.Description = content.GetValue<string>("description");

        // Settings properties
        category.IsVisible = content.GetValue<bool>("isVisible");
        category.SortOrder = content.GetValue<int>("sortOrder");

        // SEO properties
        category.MetaTitle = content.GetValue<string>("metaTitle");
        category.MetaDescription = content.GetValue<string>("metaDescription");

        // Media - store as URL string
        var imageValue = content.GetValue<string>("image");
        if (!string.IsNullOrWhiteSpace(imageValue))
        {
            category.ImageUrl = imageValue;
        }

        // Handle parent category relationship
        if (content.ParentId > 0)
        {
            var parentContent = _contentService.GetById(content.ParentId);
            if (parentContent != null && parentContent.ContentType.Alias == AlgoraDocumentTypeConstants.CategoryAlias)
            {
                // Find the parent category by Umbraco node ID
                var allCategories = _categoryService.GetAllAsync().GetAwaiter().GetResult();
                var parentCategory = allCategories.FirstOrDefault(c => c.UmbracoNodeId == content.ParentId);

                if (parentCategory != null)
                {
                    category.ParentId = parentCategory.Id;
                }
            }
            else
            {
                // Parent is not a category (could be Catalog), so this is a root category
                category.ParentId = null;
            }
        }

        // Calculate level based on parent
        category.Level = category.ParentId.HasValue ? GetCategoryLevel(category.ParentId.Value) + 1 : 0;

        // Build path
        category.Path = BuildCategoryPath(category);
    }

    private int GetCategoryLevel(Guid parentId)
    {
        var parent = _categoryService.GetByIdAsync(parentId).GetAwaiter().GetResult();
        return parent?.Level ?? 0;
    }

    private string BuildCategoryPath(Category category)
    {
        var path = category.Name;
        var parentId = category.ParentId;

        while (parentId.HasValue)
        {
            var parent = _categoryService.GetByIdAsync(parentId.Value).GetAwaiter().GetResult();
            if (parent == null) break;

            path = $"{parent.Name}/{path}";
            parentId = parent.ParentId;
        }

        return path ?? "";
    }

    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        return slug.Trim('-');
    }

    private void MapContentToProduct(Umbraco.Cms.Core.Models.IContent content, Product product)
    {
        // Set Umbraco Node ID for reverse lookup
        product.UmbracoNodeId = content.Id;

        // Content tab properties
        product.Name = content.GetValue<string>("productName") ?? content.Name ?? "";
        product.Sku = content.GetValue<string>("sku") ?? "";
        product.Slug = content.GetValue<string>("slug");
        product.ShortDescription = content.GetValue<string>("shortDescription");
        product.Description = content.GetValue<string>("description");
        product.Brand = content.GetValue<string>("brand");
        product.Manufacturer = content.GetValue<string>("manufacturer");
        product.Mpn = content.GetValue<string>("mpn");
        product.Gtin = content.GetValue<string>("gtin");

        // Commerce tab - Pricing
        product.BasePrice = content.GetValue<decimal>("basePrice");
        var salePrice = content.GetValue<decimal>("salePrice");
        product.SalePrice = salePrice == 0 ? null : salePrice;
        var costPrice = content.GetValue<decimal>("costPrice");
        product.CostPrice = costPrice == 0 ? null : costPrice;
        var compareAtPrice = content.GetValue<decimal>("compareAtPrice");
        product.CompareAtPrice = compareAtPrice == 0 ? null : compareAtPrice;
        product.CurrencyCode = content.GetValue<string>("currencyCode") ?? "USD";
        product.TaxIncluded = content.GetValue<bool>("taxIncluded");
        product.TaxClass = content.GetValue<string>("taxClass");

        // Commerce tab - Inventory
        product.TrackInventory = content.GetValue<bool>("trackInventory");
        product.StockQuantity = content.GetValue<int>("stockQuantity");
        product.LowStockThreshold = content.GetValue<int>("lowStockThreshold");
        product.AllowBackorders = content.GetValue<bool>("allowBackorders");

        // Update stock status based on inventory
        if (product.TrackInventory)
        {
            if (product.StockQuantity <= 0)
            {
                product.StockStatus = product.AllowBackorders ? StockStatus.OnBackorder : StockStatus.OutOfStock;
            }
            else
            {
                product.StockStatus = StockStatus.InStock;
            }
        }
        else
        {
            product.StockStatus = StockStatus.InStock;
        }

        // Commerce tab - Shipping
        var weight = content.GetValue<decimal>("weight");
        product.Weight = weight == 0 ? null : weight;
        var length = content.GetValue<decimal>("length");
        product.Length = length == 0 ? null : length;
        var width = content.GetValue<decimal>("width");
        product.Width = width == 0 ? null : width;
        var height = content.GetValue<decimal>("height");
        product.Height = height == 0 ? null : height;

        // Settings tab
        product.IsVisible = content.GetValue<bool>("isVisible");
        product.IsFeatured = content.GetValue<bool>("isFeatured");
        product.SortOrder = content.GetValue<int>("sortOrder");

        // Parse status from string
        var statusString = content.GetValue<string>("status");
        if (!string.IsNullOrEmpty(statusString) && Enum.TryParse<ProductStatus>(statusString, true, out var status))
        {
            product.Status = status;
        }
        else
        {
            // If published in Umbraco, default to Published status
            product.Status = content.Published ? ProductStatus.Published : ProductStatus.Draft;
        }

        // SEO
        product.MetaTitle = content.GetValue<string>("metaTitle");
        product.MetaDescription = content.GetValue<string>("metaDescription");
        product.MetaKeywords = content.GetValue<string>("metaKeywords");
    }
}

/// <summary>
/// DTO for product sync operation result
/// </summary>
public class SyncResultDto
{
    public bool Success { get; set; }
    public int TotalProducts { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// DTO for category sync operation result
/// </summary>
public class CategorySyncResultDto
{
    public bool Success { get; set; }
    public int TotalCategories { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// DTO for full bidirectional sync result
/// </summary>
public class FullSyncResultDto
{
    public bool Success { get; set; }
    public SyncResultDto Products { get; set; } = new();
    public CategorySyncResultDto Categories { get; set; } = new();
}
