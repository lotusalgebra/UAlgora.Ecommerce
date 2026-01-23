using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Algora Product Content Sync Service
///
/// Syncs products from the Algora Commerce database to Umbraco content nodes.
/// This allows products to appear in the Umbraco Content section alongside
/// regular content, enabling full CMS management capabilities.
///
/// Architecture:
/// - Creates "Algora Products" root container on first sync
/// - Creates individual product nodes using Algora Product document type
/// - Maps all product properties to document type properties
/// - Supports incremental sync (only creates new/updates changed)
/// </summary>
public class ProductContentSyncService
{
    #region Dependencies

    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IProductService _productService;
    private readonly ILogger<ProductContentSyncService> _logger;

    #endregion

    #region Constants

    private const string ProductsRootName = "Algora Products";
    private const string CategoriesRootName = "Algora Categories";

    #endregion

    #region Constructor

    public ProductContentSyncService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        IProductService productService,
        ILogger<ProductContentSyncService> logger)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _productService = productService;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Syncs all products from the database to Umbraco content tree
    /// </summary>
    public async Task<SyncResult> SyncAllProductsAsync()
    {
        var result = new SyncResult();

        try
        {
            _logger.LogInformation("Algora Commerce: Starting product content sync...");

            // Verify document type exists
            var productDocType = _contentTypeService.Get(AlgoraDocumentTypeConstants.ProductAlias);
            if (productDocType == null)
            {
                _logger.LogError("Algora Product document type not found. Please restart the application.");
                result.Errors.Add("Algora Product document type not found");
                return result;
            }

            // Get or create root container
            var rootContent = GetOrCreateProductsRoot(productDocType);
            if (rootContent == null)
            {
                result.Errors.Add("Failed to create Products root container");
                return result;
            }

            // Validate root ID
            if (rootContent.Id <= 0)
            {
                _logger.LogError("Products root container has invalid ID: {Id}", rootContent.Id);
                result.Errors.Add($"Products root container has invalid ID: {rootContent.Id}");
                return result;
            }

            _logger.LogInformation("Using Products root container with ID: {Id}", rootContent.Id);

            // Get all products from database (fetch all in one query)
            var pagedResult = await _productService.GetPagedAsync(new ProductQueryParameters
            {
                Page = 1,
                PageSize = 10000 // Get all products
            });
            var products = pagedResult.Items;
            result.TotalProducts = pagedResult.TotalCount;

            _logger.LogInformation("Algora Commerce: Found {Count} products to sync", result.TotalProducts);

            // Get existing product content nodes (to avoid duplicates)
            var existingNodes = _contentService.GetPagedChildren(rootContent.Id, 0, int.MaxValue, out _)
                .ToDictionary(c => c.GetValue<string>("sku") ?? "", c => c);

            foreach (var product in products)
            {
                try
                {
                    // Check if product already exists
                    if (existingNodes.TryGetValue(product.Sku, out var existingNode))
                    {
                        // Update existing node
                        UpdateProductNode(existingNode, product);
                        _contentService.Save(existingNode);
                        result.Updated++;
                    }
                    else
                    {
                        // Create new node
                        var newNode = CreateProductNode(rootContent.Id, productDocType, product);
                        if (newNode != null)
                        {
                            var saveResult = _contentService.Save(newNode);
                            if (saveResult.Success)
                            {
                                _contentService.Publish(newNode, Array.Empty<string>());
                                result.Created++;
                            }
                            else
                            {
                                _logger.LogError("Failed to save product {Sku}: {Errors}",
                                    product.Sku, string.Join(", ", saveResult.EventMessages.GetAll().Select(m => m.Message)));
                                result.Errors.Add($"Failed to save product {product.Sku}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing product {Sku}", product.Sku);
                    result.Errors.Add($"Failed to sync product {product.Sku}: {ex.Message}");
                }
            }

            _logger.LogInformation("Algora Commerce: Product sync complete. Created: {Created}, Updated: {Updated}",
                result.Created, result.Updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Algora Commerce: Error during product sync");
            result.Errors.Add($"Sync failed: {ex.Message}");
        }

        return result;
    }

    #endregion

    #region Private Methods

    private IContent? GetOrCreateProductsRoot(IContentType productDocType)
    {
        // Try to find existing root
        var rootNodes = _contentService.GetRootContent();
        var existingRoot = rootNodes.FirstOrDefault(c => c.Name == ProductsRootName);

        if (existingRoot != null)
        {
            _logger.LogInformation("Found existing Products root container with ID: {Id}", existingRoot.Id);
            return existingRoot;
        }

        // Create new root container
        _logger.LogInformation("Creating Products root container...");

        try
        {
            var root = _contentService.Create(ProductsRootName, Constants.System.Root, productDocType);
            root.SetValue("productName", ProductsRootName);
            root.SetValue("sku", "ROOT-PRODUCTS");
            root.SetValue("description", "Container for all Algora Commerce products");
            root.SetValue("isVisible", false);
            root.SetValue("basePrice", 0m);

            // First save the content
            var saveResult = _contentService.Save(root);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save Products root: {Errors}",
                    string.Join(", ", saveResult.EventMessages.GetAll().Select(m => m.Message)));
                return null;
            }

            _logger.LogInformation("Products root saved with ID: {Id}", root.Id);

            // Then publish it
            var publishResult = _contentService.Publish(root, Array.Empty<string>());
            if (!publishResult.Success)
            {
                _logger.LogWarning("Failed to publish Products root (content was saved): {Errors}",
                    string.Join(", ", publishResult.EventMessages.GetAll().Select(m => m.Message)));
                // Return the saved but unpublished content - it should still work for creating children
            }

            _logger.LogInformation("Products root container created with ID: {Id}", root.Id);
            return root;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Products root container");
            return null;
        }
    }

    private IContent? CreateProductNode(int parentId, IContentType productDocType, Product product)
    {
        var nodeName = !string.IsNullOrEmpty(product.Name) ? product.Name : product.Sku;
        var content = _contentService.Create(nodeName, parentId, productDocType);

        MapProductToContent(content, product);

        return content;
    }

    private void UpdateProductNode(IContent content, Product product)
    {
        content.Name = !string.IsNullOrEmpty(product.Name) ? product.Name : product.Sku;
        MapProductToContent(content, product);
    }

    private void MapProductToContent(IContent content, Product product)
    {
        // Content tab
        content.SetValue("productName", product.Name ?? "");
        content.SetValue("sku", product.Sku ?? "");
        content.SetValue("slug", product.Slug ?? "");
        content.SetValue("shortDescription", product.ShortDescription ?? "");
        content.SetValue("description", product.Description ?? "");
        content.SetValue("brand", product.Brand ?? "");
        content.SetValue("manufacturer", product.Manufacturer ?? "");
        content.SetValue("mpn", product.Mpn ?? "");
        content.SetValue("gtin", product.Gtin ?? "");

        // Pricing tab
        content.SetValue("basePrice", product.BasePrice);
        content.SetValue("salePrice", product.SalePrice);
        content.SetValue("costPrice", product.CostPrice);
        content.SetValue("compareAtPrice", product.CompareAtPrice);
        content.SetValue("currencyCode", product.CurrencyCode ?? "USD");
        content.SetValue("taxIncluded", product.TaxIncluded);
        content.SetValue("taxClass", product.TaxClass ?? "");

        // Inventory tab
        content.SetValue("trackInventory", product.TrackInventory);
        content.SetValue("stockQuantity", product.StockQuantity);
        content.SetValue("lowStockThreshold", product.LowStockThreshold);
        content.SetValue("allowBackorders", product.AllowBackorders);

        // Shipping tab
        content.SetValue("weight", product.Weight);
        content.SetValue("length", product.Length);
        content.SetValue("width", product.Width);
        content.SetValue("height", product.Height);

        // SEO tab
        content.SetValue("metaTitle", product.MetaTitle ?? "");
        content.SetValue("metaDescription", product.MetaDescription ?? "");
        content.SetValue("metaKeywords", product.MetaKeywords ?? "");

        // Settings tab
        content.SetValue("isVisible", product.IsVisible);
        content.SetValue("isFeatured", product.IsFeatured);
        content.SetValue("status", product.Status.ToString());
    }

    #endregion
}

/// <summary>
/// Result of a product sync operation
/// </summary>
public class SyncResult
{
    public int TotalProducts { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();

    public bool Success => Errors.Count == 0;
}
