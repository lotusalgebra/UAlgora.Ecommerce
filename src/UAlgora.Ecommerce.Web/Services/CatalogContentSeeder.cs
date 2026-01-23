using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Seeds sample catalog content for testing and demo purposes.
/// Creates a complete catalog structure with categories and products.
/// </summary>
public class CatalogContentSeeder
{
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<CatalogContentSeeder> _logger;

    public CatalogContentSeeder(
        IContentService contentService,
        IContentTypeService contentTypeService,
        ILogger<CatalogContentSeeder> logger)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Seeds a complete catalog with sample categories and products
    /// </summary>
    public SeedResult SeedSampleCatalog()
    {
        var result = new SeedResult();

        try
        {
            _logger.LogInformation("Algora Commerce: Starting catalog content seeding...");

            // Get document types
            var catalogType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CatalogAlias);
            var categoryType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CategoryAlias);
            var productType = _contentTypeService.Get(AlgoraDocumentTypeConstants.ProductAlias);

            if (catalogType == null || categoryType == null || productType == null)
            {
                result.Errors.Add("Document types not found. Please restart the application.");
                return result;
            }

            // Check if catalog already exists
            var existingCatalog = _contentService.GetRootContent()
                .FirstOrDefault(c => c.ContentType.Alias == AlgoraDocumentTypeConstants.CatalogAlias);

            if (existingCatalog != null)
            {
                _logger.LogInformation("Catalog already exists: {Name}", existingCatalog.Name);
                result.Messages.Add($"Catalog already exists: {existingCatalog.Name}");
                return result;
            }

            // Create Catalog
            var catalog = CreateCatalog(catalogType);
            if (catalog == null)
            {
                result.Errors.Add("Failed to create catalog");
                return result;
            }
            result.Created++;
            result.Messages.Add($"Created catalog: {catalog.Name}");

            // Create Categories with Products
            var categories = new[]
            {
                ("Electronics", "Gadgets, devices and tech accessories", new[]
                {
                    ("Laptop Pro 15", "LP-001", 1299.99m, "High-performance laptop for professionals"),
                    ("Wireless Mouse", "WM-002", 49.99m, "Ergonomic wireless mouse with precision tracking"),
                    ("USB-C Hub", "HB-003", 79.99m, "7-in-1 USB-C hub with HDMI and card reader"),
                    ("Mechanical Keyboard", "KB-004", 149.99m, "RGB mechanical keyboard with Cherry MX switches")
                }),
                ("Clothing", "Fashion and apparel for all occasions", new[]
                {
                    ("Classic T-Shirt", "TS-001", 29.99m, "Premium cotton t-shirt, available in multiple colors"),
                    ("Denim Jeans", "DJ-002", 89.99m, "Slim fit denim jeans with stretch comfort"),
                    ("Winter Jacket", "WJ-003", 199.99m, "Insulated winter jacket with waterproof shell"),
                    ("Running Shoes", "RS-004", 129.99m, "Lightweight running shoes with cushioned sole")
                }),
                ("Home & Garden", "Everything for your home and outdoor spaces", new[]
                {
                    ("Coffee Maker", "CM-001", 89.99m, "Programmable coffee maker with thermal carafe"),
                    ("Plant Pot Set", "PP-002", 34.99m, "Set of 3 ceramic plant pots in modern design"),
                    ("LED Desk Lamp", "DL-003", 59.99m, "Adjustable LED desk lamp with touch controls"),
                    ("Garden Tools Kit", "GT-004", 49.99m, "5-piece garden tool set with carrying bag")
                }),
                ("Sports & Outdoors", "Gear for active lifestyles", new[]
                {
                    ("Yoga Mat", "YM-001", 39.99m, "Non-slip yoga mat with carrying strap"),
                    ("Camping Tent", "CT-002", 249.99m, "4-person waterproof camping tent"),
                    ("Water Bottle", "WB-003", 24.99m, "Insulated stainless steel water bottle"),
                    ("Fitness Tracker", "FT-004", 99.99m, "Smart fitness tracker with heart rate monitor")
                })
            };

            foreach (var (categoryName, categoryDesc, products) in categories)
            {
                var category = CreateCategory(categoryType, catalog.Id, categoryName, categoryDesc);
                if (category != null)
                {
                    result.Created++;
                    result.Messages.Add($"Created category: {categoryName}");

                    foreach (var (productName, sku, price, desc) in products)
                    {
                        var product = CreateProduct(productType, category.Id, productName, sku, price, desc);
                        if (product != null)
                        {
                            result.Created++;
                        }
                    }
                    result.Messages.Add($"  - Added {products.Length} products to {categoryName}");
                }
            }

            _logger.LogInformation("Algora Commerce: Catalog seeding complete. Created {Count} items.", result.Created);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding catalog content");
            result.Errors.Add(ex.Message);
            return result;
        }
    }

    private IContent? CreateCatalog(IContentType catalogType)
    {
        var catalog = _contentService.Create("Shop", Constants.System.Root, catalogType);

        catalog.SetValue("title", "Welcome to Our Store");
        catalog.SetValue("subtitle", "Discover amazing products at great prices");
        catalog.SetValue("description", "<p>Browse our curated collection of products across multiple categories. We offer quality items with fast shipping and excellent customer service.</p>");
        catalog.SetValue("productsPerPage", 12);
        catalog.SetValue("defaultSortOrder", "newest");
        catalog.SetValue("showFilters", true);
        catalog.SetValue("metaTitle", "Shop - Your One-Stop Store");
        catalog.SetValue("metaDescription", "Browse our wide selection of electronics, clothing, home goods, and more. Quality products at competitive prices.");

        // Save first (to get ID), then publish
        _contentService.Save(catalog);
        _contentService.Publish(catalog, Array.Empty<string>());
        _logger.LogInformation("Created catalog: {Name} (ID: {Id})", catalog.Name, catalog.Id);

        return catalog;
    }

    private IContent? CreateCategory(IContentType categoryType, int parentId, string name, string description)
    {
        var category = _contentService.Create(name, parentId, categoryType);

        category.SetValue("categoryName", name);
        category.SetValue("description", $"<p>{description}</p>");
        category.SetValue("slug", name.ToLower().Replace(" ", "-").Replace("&", "and"));
        category.SetValue("isVisible", true);
        category.SetValue("sortOrder", 0);
        category.SetValue("metaTitle", $"{name} - Shop");
        category.SetValue("metaDescription", description);

        // Save first (to get ID), then publish
        _contentService.Save(category);
        _contentService.Publish(category, Array.Empty<string>());
        _logger.LogDebug("Created category: {Name} (ID: {Id})", category.Name, category.Id);

        return category;
    }

    private IContent? CreateProduct(IContentType productType, int parentId, string name, string sku, decimal price, string description)
    {
        var product = _contentService.Create(name, parentId, productType);

        // Content tab
        product.SetValue("productName", name);
        product.SetValue("sku", sku);
        product.SetValue("shortDescription", description);
        product.SetValue("description", $"<p>{description}</p><p>This is a high-quality product from our catalog. Order now and enjoy fast shipping!</p>");
        product.SetValue("brand", "Algora Brand");

        // Commerce tab
        product.SetValue("basePrice", price);
        product.SetValue("currencyCode", "USD");
        product.SetValue("trackInventory", true);
        product.SetValue("stockQuantity", new Random().Next(10, 100));
        product.SetValue("lowStockThreshold", 5);
        product.SetValue("requiresShipping", true);
        product.SetValue("weight", Math.Round((decimal)(new Random().NextDouble() * 5), 2));

        // Settings tab
        product.SetValue("slug", name.ToLower().Replace(" ", "-"));
        product.SetValue("status", "Published");
        product.SetValue("isVisible", true);
        product.SetValue("isFeatured", new Random().Next(0, 5) == 0); // 20% chance of being featured
        product.SetValue("sortOrder", 0);
        product.SetValue("metaTitle", $"{name} - Buy Online");
        product.SetValue("metaDescription", description);

        // Save first, then publish
        _contentService.Save(product);
        _contentService.Publish(product, Array.Empty<string>());
        _logger.LogDebug("Created product: {Name} (SKU: {Sku})", product.Name, sku);

        return product;
    }
}

public class SeedResult
{
    public int Created { get; set; }
    public List<string> Messages { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool Success => Errors.Count == 0;
}
