using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for product operations.
/// </summary>
public class ProductRepository : SoftDeleteRepository<Product>, IProductRepository
{
    public ProductRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.Sku == sku, ct);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);
    }

    public async Task<Product?> GetByUmbracoNodeIdAsync(int nodeId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.UmbracoNodeId == nodeId, ct);
    }

    public async Task<Product?> GetWithVariantsAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        ProductQueryParameters parameters,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Sku != null && p.Sku.ToLower().Contains(term)) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        // Category and Tag filtering is done client-side because they are stored as JSON
        // These filters will be applied after loading the data
        var categoryId = parameters.CategoryId;
        var tags = parameters.Tags;

        if (!string.IsNullOrWhiteSpace(parameters.Brand))
        {
            query = query.Where(p => p.Brand == parameters.Brand);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice <= parameters.MaxPrice.Value);
        }

        if (parameters.InStock.HasValue)
        {
            if (parameters.InStock.Value)
            {
                query = query.Where(p => !p.TrackInventory || p.StockQuantity > 0 || p.AllowBackorders);
            }
            else
            {
                query = query.Where(p => p.TrackInventory && p.StockQuantity <= 0 && !p.AllowBackorders);
            }
        }

        if (parameters.OnSale.HasValue && parameters.OnSale.Value)
        {
            query = query.Where(p => p.SalePrice.HasValue && p.SalePrice < p.BasePrice);
        }

        if (parameters.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == parameters.IsFeatured.Value);
        }

        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }
        else
        {
            // Default to published products
            query = query.Where(p => p.Status == ProductStatus.Published && p.IsVisible);
        }

        // Apply sorting
        query = parameters.SortBy switch
        {
            ProductSortBy.Newest => query.OrderByDescending(p => p.CreatedAt),
            ProductSortBy.Oldest => query.OrderBy(p => p.CreatedAt),
            ProductSortBy.PriceLowToHigh => query.OrderBy(p => p.SalePrice ?? p.BasePrice),
            ProductSortBy.PriceHighToLow => query.OrderByDescending(p => p.SalePrice ?? p.BasePrice),
            ProductSortBy.NameAscending => query.OrderBy(p => p.Name),
            ProductSortBy.NameDescending => query.OrderByDescending(p => p.Name),
            ProductSortBy.BestSelling => query.OrderByDescending(p => p.SortOrder), // TODO: Implement actual sales count
            ProductSortBy.TopRated => query.OrderByDescending(p => p.SortOrder), // TODO: Implement actual rating
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // Load all products matching server-side filters
        var allItems = await query.ToListAsync(ct);

        // Apply client-side filters for JSON-stored collections
        IEnumerable<Product> filteredItems = allItems;

        if (categoryId.HasValue)
        {
            filteredItems = filteredItems.Where(p => p.CategoryIds.Contains(categoryId.Value));
        }

        if (tags?.Any() == true)
        {
            filteredItems = filteredItems.Where(p => p.Tags.Any(t => tags.Contains(t)));
        }

        var filteredList = filteredItems.ToList();

        // Get total count after all filters
        var totalCount = filteredList.Count;

        // Apply pagination on filtered results
        var items = filteredList
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        if (parameters.IncludeVariants && items.Any())
        {
            var productIds = items.Select(p => p.Id).ToList();
            var variants = await Context.ProductVariants
                .Where(v => productIds.Contains(v.ProductId))
                .ToListAsync(ct);

            foreach (var product in items)
            {
                product.Variants = variants.Where(v => v.ProductId == product.Id).ToList();
            }
        }

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        bool includeSubcategories = false,
        CancellationToken ct = default)
    {
        // Load products and filter client-side for JSON-stored CategoryIds
        var products = await DbSet
            .Where(p => p.Status == ProductStatus.Published && p.IsVisible)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        return products.Where(p => p.CategoryIds.Contains(categoryId)).ToList();
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 10, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.IsFeatured && p.Status == ProductStatus.Published && p.IsVisible)
            .OrderBy(p => p.SortOrder)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetOnSaleAsync(int count = 10, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.SalePrice.HasValue && p.SalePrice < p.BasePrice)
            .Where(p => p.Status == ProductStatus.Published && p.IsVisible)
            .OrderByDescending(p => (p.BasePrice - p.SalePrice!.Value) / p.BasePrice)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetNewArrivalsAsync(
        int count = 10,
        int daysBack = 30,
        CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
        return await DbSet
            .Where(p => p.CreatedAt >= cutoffDate)
            .Where(p => p.Status == ProductStatus.Published && p.IsVisible)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetBestSellersAsync(int count = 10, CancellationToken ct = default)
    {
        // TODO: Implement actual sales tracking
        // For now, return products ordered by sort order
        return await DbSet
            .Where(p => p.Status == ProductStatus.Published && p.IsVisible)
            .OrderBy(p => p.SortOrder)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(
        Guid productId,
        int count = 10,
        CancellationToken ct = default)
    {
        var product = await GetByIdAsync(productId, ct);
        if (product == null)
            return Array.Empty<Product>();

        // Load products and filter client-side for JSON-stored collections
        var products = await DbSet
            .Where(p => p.Id != productId)
            .Where(p => p.Status == ProductStatus.Published && p.IsVisible)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        // Score and filter related products
        var scored = products.Select(p => new
        {
            Product = p,
            Score = (p.CategoryIds.Any(cid => product.CategoryIds.Contains(cid)) ? 3 : 0) +
                    (p.Tags.Any(t => product.Tags.Contains(t)) ? 2 : 0) +
                    (p.Brand == product.Brand ? 1 : 0)
        })
        .Where(x => x.Score > 0)
        .OrderByDescending(x => x.Score)
        .Take(count)
        .Select(x => x.Product)
        .ToList();

        return scored;
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await DbSet
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(p => p.Sku == sku);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(p => p.Slug == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.TrackInventory && p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetOutOfStockAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.TrackInventory && p.StockQuantity <= 0)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken ct = default)
    {
        var term = searchTerm.ToLower();
        return await DbSet
            .Where(p => p.Status == ProductStatus.Published && p.IsVisible)
            .Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Sku != null && p.Sku.ToLower().Contains(term)) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(term)))
            .OrderByDescending(p => p.Name.ToLower().StartsWith(term))
            .ThenBy(p => p.Name)
            .Take(maxResults)
            .ToListAsync(ct);
    }

    public async Task UpdateStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var product = await GetByIdAsync(productId, ct);
        if (product != null)
        {
            product.StockQuantity = quantity;
            product.StockStatus = quantity > 0 ? StockStatus.InStock :
                                  product.AllowBackorders ? StockStatus.OnBackorder : StockStatus.OutOfStock;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateVariantStockAsync(Guid variantId, int quantity, CancellationToken ct = default)
    {
        var variant = await Context.ProductVariants.FindAsync(new object[] { variantId }, ct);
        if (variant != null)
        {
            variant.StockQuantity = quantity;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<string>> GetAllBrandsAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Brand != null && p.Status == ProductStatus.Published)
            .Select(p => p.Brand!)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default)
    {
        var products = await DbSet
            .Where(p => p.Status == ProductStatus.Published)
            .Select(p => p.Tags)
            .ToListAsync(ct);

        return products
            .SelectMany(t => t)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    private async Task<List<Guid>> GetCategoryWithDescendantIds(Guid categoryId, CancellationToken ct)
    {
        var result = new List<Guid> { categoryId };
        var children = await Context.Categories
            .Where(c => c.ParentId == categoryId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        foreach (var childId in children)
        {
            result.AddRange(await GetCategoryWithDescendantIds(childId, ct));
        }

        return result;
    }
}
