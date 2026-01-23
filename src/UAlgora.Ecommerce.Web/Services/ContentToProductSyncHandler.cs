using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Notification handler that syncs Umbraco content tree products to the product database.
/// When a product is published/updated in the Umbraco content tree, this handler
/// ensures it appears in the Algora Commerce product dashboard.
/// </summary>
public sealed class ContentToProductSyncHandler :
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly IProductService _productService;
    private readonly ILogger<ContentToProductSyncHandler> _logger;

    public ContentToProductSyncHandler(
        IProductService productService,
        ILogger<ContentToProductSyncHandler> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Handle content published - create or update product in database
    /// </summary>
    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.PublishedEntities)
        {
            if (!IsAlgoraProduct(content))
                continue;

            await SyncProductToDatabaseAsync(content, cancellationToken);
        }
    }

    /// <summary>
    /// Handle content unpublished - mark product as draft in database
    /// </summary>
    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.UnpublishedEntities)
        {
            if (!IsAlgoraProduct(content))
                continue;

            await UpdateProductStatusAsync(content.Id, ProductStatus.Draft, cancellationToken);
        }
    }

    /// <summary>
    /// Handle content deleted - soft delete product in database
    /// </summary>
    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.DeletedEntities)
        {
            if (!IsAlgoraProduct(content))
                continue;

            await DeleteProductAsync(content.Id, cancellationToken);
        }
    }

    private bool IsAlgoraProduct(IContent content)
    {
        return content.ContentType.Alias == AlgoraDocumentTypeConstants.ProductAlias;
    }

    private async Task SyncProductToDatabaseAsync(IContent content, CancellationToken ct)
    {
        try
        {
            var sku = content.GetValue<string>("sku");
            if (string.IsNullOrWhiteSpace(sku))
            {
                _logger.LogWarning("Cannot sync product without SKU. Content ID: {ContentId}", content.Id);
                return;
            }

            // Check if product already exists by SKU
            var existingProduct = await _productService.GetBySkuAsync(sku, ct);

            if (existingProduct != null)
            {
                // Update existing product
                MapContentToProduct(content, existingProduct);
                await _productService.UpdateAsync(existingProduct, ct);
                _logger.LogInformation("Updated product in database: {Sku} (Umbraco Node: {NodeId})", sku, content.Id);
            }
            else
            {
                // Create new product
                var newProduct = new Product();
                MapContentToProduct(content, newProduct);
                await _productService.CreateAsync(newProduct, ct);
                _logger.LogInformation("Created product in database: {Sku} (Umbraco Node: {NodeId})", sku, content.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing product to database. Content ID: {ContentId}", content.Id);
        }
    }

    private async Task UpdateProductStatusAsync(int contentId, ProductStatus status, CancellationToken ct)
    {
        try
        {
            // Find product by Umbraco node ID or loop through to find by SKU match
            var products = await _productService.GetPagedAsync(new ProductQueryParameters
            {
                Page = 1,
                PageSize = 10000
            }, ct);

            var product = products.Items.FirstOrDefault(p => p.UmbracoNodeId == contentId);
            if (product != null)
            {
                product.Status = status;
                await _productService.UpdateAsync(product, ct);
                _logger.LogInformation("Updated product status to {Status}: {Sku}", status, product.Sku);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product status. Content ID: {ContentId}", contentId);
        }
    }

    private async Task DeleteProductAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var products = await _productService.GetPagedAsync(new ProductQueryParameters
            {
                Page = 1,
                PageSize = 10000
            }, ct);

            var product = products.Items.FirstOrDefault(p => p.UmbracoNodeId == contentId);
            if (product != null)
            {
                await _productService.DeleteAsync(product.Id, ct);
                _logger.LogInformation("Deleted product from database: {Sku}", product.Sku);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product. Content ID: {ContentId}", contentId);
        }
    }

    private void MapContentToProduct(IContent content, Product product)
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
        product.SalePrice = GetNullableDecimal(content, "salePrice");
        product.CostPrice = GetNullableDecimal(content, "costPrice");
        product.CompareAtPrice = GetNullableDecimal(content, "compareAtPrice");
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
                // InStock covers both normal stock and low stock (IsLowStock computed property handles the distinction)
                product.StockStatus = StockStatus.InStock;
            }
        }
        else
        {
            product.StockStatus = StockStatus.InStock;
        }

        // Commerce tab - Shipping
        product.Weight = GetNullableDecimal(content, "weight");
        product.Length = GetNullableDecimal(content, "length");
        product.Width = GetNullableDecimal(content, "width");
        product.Height = GetNullableDecimal(content, "height");

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
            product.Status = ProductStatus.Published;
        }

        // SEO
        product.MetaTitle = content.GetValue<string>("metaTitle");
        product.MetaDescription = content.GetValue<string>("metaDescription");
        product.MetaKeywords = content.GetValue<string>("metaKeywords");
    }

    private static decimal? GetNullableDecimal(IContent content, string alias)
    {
        var value = content.GetValue<decimal>(alias);
        return value == 0 ? null : value;
    }
}
