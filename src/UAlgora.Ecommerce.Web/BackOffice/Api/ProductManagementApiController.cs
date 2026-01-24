using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.Services;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for product operations in the Umbraco backoffice.
/// Automatically syncs products to Umbraco content tree for bidirectional management.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Products}")]
public class ProductManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ProductContentSyncService _syncService;
    private readonly ILogger<ProductManagementApiController> _logger;

    public ProductManagementApiController(
        IProductService productService,
        ICategoryService categoryService,
        ProductContentSyncService syncService,
        ILogger<ProductManagementApiController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the tree structure for products organized by categories.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType<ProductTreeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var nodes = new List<TreeNodeModel>();

        // Add "All Products" node
        var allProductsParams = new ProductQueryParameters { Page = 1, PageSize = 1 };
        var allProductsResult = await _productService.GetPagedAsync(allProductsParams);
        nodes.Add(new TreeNodeModel
        {
            Id = "all-products",
            Name = "All Products",
            Icon = EcommerceConstants.Icons.Products,
            HasChildren = allProductsResult.TotalCount > 0,
            NodeType = "folder",
            Badge = allProductsResult.TotalCount.ToString()
        });

        // Add root categories
        var rootCategories = await _categoryService.GetRootCategoriesAsync();
        foreach (var category in rootCategories.Where(c => c.IsVisible).OrderBy(c => c.SortOrder))
        {
            var children = await _categoryService.GetChildrenAsync(category.Id);
            var productCount = await _categoryService.GetProductCountAsync(category.Id, false);

            nodes.Add(new TreeNodeModel
            {
                Id = $"category-{category.Id}",
                Name = category.Name,
                Icon = EcommerceConstants.Icons.Category,
                HasChildren = children.Any() || productCount > 0,
                NodeType = "category",
                EntityId = category.Id,
                Badge = productCount.ToString()
            });
        }

        // Add "Uncategorized" node
        nodes.Add(new TreeNodeModel
        {
            Id = "uncategorized",
            Name = "Uncategorized",
            Icon = EcommerceConstants.Icons.Category,
            HasChildren = true,
            NodeType = "folder"
        });

        return Ok(new ProductTreeResponse { Nodes = nodes });
    }

    /// <summary>
    /// Gets children nodes for a specific tree node.
    /// </summary>
    [HttpGet("tree/{nodeId}/children")]
    [ProducesResponseType<ProductTreeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTreeChildren(string nodeId)
    {
        var nodes = new List<TreeNodeModel>();

        if (nodeId == "all-products")
        {
            var parameters = new ProductQueryParameters { Page = 1, PageSize = 100 };
            var result = await _productService.GetPagedAsync(parameters);

            foreach (var product in result.Items)
            {
                nodes.Add(CreateProductNode(product));
            }
        }
        else if (nodeId == "uncategorized")
        {
            var parameters = new ProductQueryParameters { Page = 1, PageSize = 100 };
            var result = await _productService.GetPagedAsync(parameters);

            foreach (var product in result.Items.Where(p => p.CategoryIds.Count == 0))
            {
                nodes.Add(CreateProductNode(product));
            }
        }
        else if (nodeId.StartsWith("category-") && Guid.TryParse(nodeId.Replace("category-", ""), out var categoryId))
        {
            // Add child categories
            var children = await _categoryService.GetChildrenAsync(categoryId);
            foreach (var category in children.Where(c => c.IsVisible).OrderBy(c => c.SortOrder))
            {
                var grandChildren = await _categoryService.GetChildrenAsync(category.Id);
                var productCount = await _categoryService.GetProductCountAsync(category.Id, false);

                nodes.Add(new TreeNodeModel
                {
                    Id = $"category-{category.Id}",
                    Name = category.Name,
                    Icon = EcommerceConstants.Icons.Category,
                    HasChildren = grandChildren.Any() || productCount > 0,
                    NodeType = "category",
                    EntityId = category.Id,
                    Badge = productCount.ToString()
                });
            }

            // Add products in this category
            var products = await _productService.GetByCategoryAsync(categoryId, false);
            foreach (var product in products)
            {
                nodes.Add(CreateProductNode(product));
            }
        }

        return Ok(new ProductTreeResponse { Nodes = nodes });
    }

    /// <summary>
    /// Gets a paged list of products for the management view.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<ProductListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] ProductStatus? status = null,
        [FromQuery] string? search = null)
    {
        var parameters = new ProductQueryParameters
        {
            Page = (skip / take) + 1,
            PageSize = take,
            CategoryId = categoryId,
            Status = status,
            SearchTerm = search
        };

        var result = await _productService.GetPagedAsync(parameters);

        return Ok(new ProductListResponse
        {
            Items = result.Items.Select(MapToProductItem).ToList(),
            Total = result.TotalCount,
            Skip = skip,
            Take = take
        });
    }

    /// <summary>
    /// Gets a single product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(MapToProductDetail(product));
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        // Validate request
        var validation = await ValidateProductRequest(request);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors });
        }

        // Generate slug if not provided
        var slug = request.Slug;
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = await _productService.GenerateSlugAsync(request.Name);
        }

        // Create product entity
        var product = new Core.Models.Domain.Product
        {
            Name = request.Name.Trim(),
            Sku = request.Sku.Trim(),
            Slug = slug,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            BasePrice = request.BasePrice,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            CompareAtPrice = request.CompareAtPrice,
            CurrencyCode = request.CurrencyCode ?? "USD",
            TaxIncluded = request.TaxIncluded,
            TaxClass = request.TaxClass,
            TrackInventory = request.TrackInventory,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            AllowBackorders = request.AllowBackorders,
            Weight = request.Weight,
            WeightUnit = request.WeightUnit ?? "kg",
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            DimensionUnit = request.DimensionUnit ?? "cm",
            CategoryIds = request.CategoryIds ?? [],
            Tags = request.Tags ?? [],
            Brand = request.Brand,
            Manufacturer = request.Manufacturer,
            Mpn = request.Mpn,
            Gtin = request.Gtin,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            Status = request.Status ?? ProductStatus.Draft,
            IsFeatured = request.IsFeatured,
            IsVisible = request.IsVisible,
            SortOrder = request.SortOrder,
            PrimaryImageUrl = request.PrimaryImageUrl,
            ImageIds = request.ImageIds ?? []
        };

        var created = await _productService.CreateAsync(product);

        // Sync to Umbraco content tree for bidirectional management
        try
        {
            await _syncService.SyncProductToContentAsync(created.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync product to content tree: {Id}", created.Id);
            // Don't fail the create - content sync is secondary
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToProductDetail(created));
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var existing = await _productService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Validate request
        var validation = await ValidateProductRequest(request, id);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors });
        }

        // Update properties
        existing.Name = request.Name.Trim();
        existing.Sku = request.Sku.Trim();
        existing.Slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await _productService.GenerateSlugAsync(request.Name)
            : request.Slug;
        existing.Description = request.Description;
        existing.ShortDescription = request.ShortDescription;
        existing.BasePrice = request.BasePrice;
        existing.SalePrice = request.SalePrice;
        existing.CostPrice = request.CostPrice;
        existing.CompareAtPrice = request.CompareAtPrice;
        existing.CurrencyCode = request.CurrencyCode ?? "USD";
        existing.TaxIncluded = request.TaxIncluded;
        existing.TaxClass = request.TaxClass;
        existing.TrackInventory = request.TrackInventory;
        existing.StockQuantity = request.StockQuantity;
        existing.LowStockThreshold = request.LowStockThreshold;
        existing.AllowBackorders = request.AllowBackorders;
        existing.Weight = request.Weight;
        existing.WeightUnit = request.WeightUnit ?? "kg";
        existing.Length = request.Length;
        existing.Width = request.Width;
        existing.Height = request.Height;
        existing.DimensionUnit = request.DimensionUnit ?? "cm";
        existing.CategoryIds = request.CategoryIds ?? [];
        existing.Tags = request.Tags ?? [];
        existing.Brand = request.Brand;
        existing.Manufacturer = request.Manufacturer;
        existing.Mpn = request.Mpn;
        existing.Gtin = request.Gtin;
        existing.MetaTitle = request.MetaTitle;
        existing.MetaDescription = request.MetaDescription;
        existing.MetaKeywords = request.MetaKeywords;
        existing.Status = request.Status ?? existing.Status;
        existing.IsFeatured = request.IsFeatured;
        existing.IsVisible = request.IsVisible;
        existing.SortOrder = request.SortOrder;
        existing.PrimaryImageUrl = request.PrimaryImageUrl;
        existing.ImageIds = request.ImageIds ?? [];

        var updated = await _productService.UpdateAsync(existing);

        // Sync to Umbraco content tree for bidirectional management
        try
        {
            await _syncService.SyncProductToContentAsync(updated.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync product to content tree: {Id}", updated.Id);
        }

        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _productService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await _productService.DeleteAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Duplicates an existing product.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var existing = await _productService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Generate unique SKU and slug
        var newSku = $"{existing.Sku}-COPY-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var newName = $"{existing.Name} (Copy)";
        var newSlug = await _productService.GenerateSlugAsync(newName);

        // Create duplicate product
        var duplicate = new Core.Models.Domain.Product
        {
            Name = newName,
            Sku = newSku,
            Slug = newSlug,
            Description = existing.Description,
            ShortDescription = existing.ShortDescription,
            BasePrice = existing.BasePrice,
            SalePrice = existing.SalePrice,
            CostPrice = existing.CostPrice,
            CompareAtPrice = existing.CompareAtPrice,
            CurrencyCode = existing.CurrencyCode,
            TaxIncluded = existing.TaxIncluded,
            TaxClass = existing.TaxClass,
            TrackInventory = existing.TrackInventory,
            StockQuantity = existing.StockQuantity,
            LowStockThreshold = existing.LowStockThreshold,
            AllowBackorders = existing.AllowBackorders,
            Weight = existing.Weight,
            WeightUnit = existing.WeightUnit,
            Length = existing.Length,
            Width = existing.Width,
            Height = existing.Height,
            DimensionUnit = existing.DimensionUnit,
            CategoryIds = existing.CategoryIds.ToList(),
            Tags = existing.Tags.ToList(),
            Brand = existing.Brand,
            Manufacturer = existing.Manufacturer,
            Mpn = existing.Mpn,
            Gtin = null, // GTIN should be unique, don't copy it
            MetaTitle = existing.MetaTitle,
            MetaDescription = existing.MetaDescription,
            MetaKeywords = existing.MetaKeywords,
            Status = ProductStatus.Draft, // New copies start as draft
            IsFeatured = false,
            IsVisible = false, // Hidden by default until reviewed
            SortOrder = existing.SortOrder,
            PrimaryImageUrl = existing.PrimaryImageUrl,
            ImageIds = existing.ImageIds.ToList()
        };

        // Copy variants if any
        foreach (var variant in existing.Variants)
        {
            duplicate.Variants.Add(new ProductVariant
            {
                Sku = $"{variant.Sku}-COPY",
                Name = variant.Name,
                Options = new Dictionary<string, string>(variant.Options),
                Price = variant.Price,
                SalePrice = variant.SalePrice,
                CostPrice = variant.CostPrice,
                StockQuantity = variant.StockQuantity,
                ImageUrl = variant.ImageUrl,
                Weight = variant.Weight,
                IsDefault = variant.IsDefault,
                IsAvailable = variant.IsAvailable,
                SortOrder = variant.SortOrder
            });
        }

        var created = await _productService.CreateAsync(duplicate);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToProductDetail(created));
    }

    /// <summary>
    /// Publishes a product.
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Status = ProductStatus.Published;
        product.IsVisible = true;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Unpublishes a product (sets to draft).
    /// </summary>
    [HttpPost("{id:guid}/unpublish")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Status = ProductStatus.Draft;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Archives a product.
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Status = ProductStatus.Archived;
        product.IsVisible = false;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Restores an archived product.
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Status = ProductStatus.Draft;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Toggles the featured status of a product.
    /// </summary>
    [HttpPost("{id:guid}/toggle-featured")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFeatured(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.IsFeatured = !product.IsFeatured;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Toggles the visibility of a product.
    /// </summary>
    [HttpPost("{id:guid}/toggle-visibility")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleVisibility(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.IsVisible = !product.IsVisible;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Adjusts the stock quantity of a product.
    /// </summary>
    [HttpPost("{id:guid}/adjust-stock")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustStockRequest request)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var newQuantity = product.StockQuantity + request.Adjustment;
        if (newQuantity < 0)
        {
            return BadRequest(new { message = "Stock quantity cannot be negative" });
        }

        product.StockQuantity = newQuantity;

        // Update stock status
        if (newQuantity == 0)
        {
            product.StockStatus = StockStatus.OutOfStock;
        }
        else
        {
            product.StockStatus = StockStatus.InStock;
        }

        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Sets a product on sale.
    /// </summary>
    [HttpPost("{id:guid}/set-sale")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetSale(Guid id, [FromBody] SetSaleRequest request)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (request.SalePrice.HasValue && request.SalePrice >= product.BasePrice)
        {
            return BadRequest(new { message = "Sale price must be less than base price" });
        }

        product.SalePrice = request.SalePrice;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    /// <summary>
    /// Removes sale price from a product.
    /// </summary>
    [HttpPost("{id:guid}/remove-sale")]
    [ProducesResponseType<ProductDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSale(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.SalePrice = null;
        var updated = await _productService.UpdateAsync(product);
        return Ok(MapToProductDetail(updated));
    }

    private async Task<ValidationResult> ValidateProductRequest(ProductRequestBase request, Guid? excludeId = null)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationError { PropertyName = "name", ErrorMessage = "Product name is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            errors.Add(new ValidationError { PropertyName = "sku", ErrorMessage = "Product SKU is required" });
        }
        else
        {
            // Check for duplicate SKU
            var existingBySku = await _productService.GetBySkuAsync(request.Sku);
            if (existingBySku != null && existingBySku.Id != excludeId)
            {
                errors.Add(new ValidationError { PropertyName = "sku", ErrorMessage = "A product with this SKU already exists" });
            }
        }

        if (request.BasePrice < 0)
        {
            errors.Add(new ValidationError { PropertyName = "basePrice", ErrorMessage = "Base price cannot be negative" });
        }

        return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    private static TreeNodeModel CreateProductNode(Core.Models.Domain.Product product)
    {
        var icon = product.Status == ProductStatus.Published
            ? EcommerceConstants.Icons.Product
            : "icon-box color-grey";

        var cssClasses = new List<string>();
        if (product.Status == ProductStatus.Draft) cssClasses.Add("is-draft");
        if (product.Status == ProductStatus.Archived) cssClasses.Add("is-archived");
        if (!product.IsVisible) cssClasses.Add("is-hidden");

        return new TreeNodeModel
        {
            Id = $"product-{product.Id}",
            Name = product.Name,
            Icon = icon,
            HasChildren = false,
            NodeType = "product",
            EntityId = product.Id,
            CssClasses = cssClasses
        };
    }

    private static ProductItemModel MapToProductItem(Core.Models.Domain.Product product)
    {
        return new ProductItemModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Slug = product.Slug ?? string.Empty,
            Price = product.CurrentPrice,
            BasePrice = product.BasePrice,
            SalePrice = product.SalePrice,
            CompareAtPrice = product.CompareAtPrice,
            Status = product.Status.ToString(),
            IsVisible = product.IsVisible,
            IsFeatured = product.IsFeatured,
            StockQuantity = product.StockQuantity,
            TrackInventory = product.TrackInventory,
            CategoryIds = product.CategoryIds,
            ImageUrl = product.PrimaryImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private static ProductDetailModel MapToProductDetail(Core.Models.Domain.Product product)
    {
        return new ProductDetailModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Slug = product.Slug ?? string.Empty,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            BasePrice = product.BasePrice,
            SalePrice = product.SalePrice,
            CostPrice = product.CostPrice,
            CompareAtPrice = product.CompareAtPrice,
            CurrentPrice = product.CurrentPrice,
            CurrencyCode = product.CurrencyCode,
            TaxIncluded = product.TaxIncluded,
            TaxClass = product.TaxClass,
            TrackInventory = product.TrackInventory,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            AllowBackorders = product.AllowBackorders,
            StockStatus = product.StockStatus.ToString(),
            Weight = product.Weight,
            WeightUnit = product.WeightUnit,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            DimensionUnit = product.DimensionUnit,
            PrimaryImageUrl = product.PrimaryImageUrl,
            ImageIds = product.ImageIds,
            CategoryIds = product.CategoryIds,
            Tags = product.Tags,
            Brand = product.Brand,
            Manufacturer = product.Manufacturer,
            Mpn = product.Mpn,
            Gtin = product.Gtin,
            HasVariants = product.HasVariants,
            Variants = product.Variants.Select(v => new ProductVariantModel
            {
                Id = v.Id,
                Sku = v.Sku,
                Name = v.Name,
                Options = v.Options,
                Price = v.Price,
                SalePrice = v.SalePrice,
                CostPrice = v.CostPrice,
                StockQuantity = v.StockQuantity,
                ImageUrl = v.ImageUrl,
                Gtin = v.Gtin,
                Weight = v.Weight,
                IsDefault = v.IsDefault,
                IsAvailable = v.IsAvailable,
                SortOrder = v.SortOrder
            }).ToList(),
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            MetaKeywords = product.MetaKeywords,
            Status = product.Status.ToString(),
            IsFeatured = product.IsFeatured,
            IsVisible = product.IsVisible,
            SortOrder = product.SortOrder,
            IsOnSale = product.IsOnSale,
            IsInStock = product.IsInStock,
            IsLowStock = product.IsLowStock,
            DiscountPercentage = product.DiscountPercentage,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}

#region Response Models

public class ProductTreeResponse
{
    public List<TreeNodeModel> Nodes { get; set; } = [];
}

public class TreeNodeModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public bool HasChildren { get; set; }
    public required string NodeType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Badge { get; set; }
    public List<string> CssClasses { get; set; } = [];
}

public class ProductListResponse
{
    public List<ProductItemModel> Items { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class ProductItemModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public required string Slug { get; set; }
    public decimal Price { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public required string Status { get; set; }
    public bool IsVisible { get; set; }
    public bool IsFeatured { get; set; }
    public int StockQuantity { get; set; }
    public bool TrackInventory { get; set; }
    public List<Guid> CategoryIds { get; set; } = [];
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductDetailModel : ProductItemModel
{
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public required string CurrencyCode { get; set; }
    public bool TaxIncluded { get; set; }
    public string? TaxClass { get; set; }
    public int LowStockThreshold { get; set; }
    public bool AllowBackorders { get; set; }
    public required string StockStatus { get; set; }
    public decimal? Weight { get; set; }
    public required string WeightUnit { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public required string DimensionUnit { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<Guid> ImageIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Mpn { get; set; }
    public string? Gtin { get; set; }
    public bool HasVariants { get; set; }
    public List<ProductVariantModel> Variants { get; set; } = [];
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public int SortOrder { get; set; }
    public bool IsOnSale { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

public class ProductVariantModel
{
    public Guid Id { get; set; }
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public Dictionary<string, string> Options { get; set; } = [];
    public decimal? Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Gtin { get; set; }
    public decimal? Weight { get; set; }
    public bool IsDefault { get; set; }
    public bool IsAvailable { get; set; }
    public int SortOrder { get; set; }
}

#endregion

#region Request Models

public abstract class ProductRequestBase
{
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? CurrencyCode { get; set; }
    public bool TaxIncluded { get; set; }
    public string? TaxClass { get; set; }
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public bool AllowBackorders { get; set; }
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<Guid>? ImageIds { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public List<string>? Tags { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Mpn { get; set; }
    public string? Gtin { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public ProductStatus? Status { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
}

public class CreateProductRequest : ProductRequestBase
{
}

public class UpdateProductRequest : ProductRequestBase
{
}

public class AdjustStockRequest
{
    public int Adjustment { get; set; }
}

public class SetSaleRequest
{
    public decimal? SalePrice { get; set; }
}

#endregion
