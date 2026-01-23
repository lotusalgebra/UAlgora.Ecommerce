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
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/content-sync")]
public class ContentSyncManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ProductContentSyncService _syncService;
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IProductService _productService;
    private readonly ILogger<ContentSyncManagementApiController> _logger;

    public ContentSyncManagementApiController(
        ProductContentSyncService syncService,
        IContentService contentService,
        IContentTypeService contentTypeService,
        IProductService productService,
        ILogger<ContentSyncManagementApiController> logger)
    {
        _syncService = syncService;
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _productService = productService;
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
            var result = await _syncService.SyncAllProductsAsync();

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
/// DTO for sync operation result
/// </summary>
public class SyncResultDto
{
    public bool Success { get; set; }
    public int TotalProducts { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();
}
