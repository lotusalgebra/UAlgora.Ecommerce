using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for category operations.
/// </summary>
public class CategoryRepository : SoftDeleteRepository<Category>, ICategoryRepository
{
    public CategoryRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }

    public async Task<Category?> GetByUmbracoNodeIdAsync(int nodeId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.UmbracoNodeId == nodeId, ct);
    }

    public async Task<Category?> GetWithChildrenAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Category?> GetWithParentAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default)
    {
        // Get all categories and build tree in memory
        var allCategories = await DbSet
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        // Build tree structure
        var lookup = allCategories.ToLookup(c => c.ParentId);
        foreach (var category in allCategories)
        {
            category.Children = lookup[category.Id].ToList();
        }

        // Return root categories (which now have their children populated)
        return allCategories.Where(c => c.ParentId == null).ToList();
    }

    public async Task<IReadOnlyList<Category>> GetFeaturedAsync(int count = 10, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.IsFeatured && c.IsVisible)
            .OrderBy(c => c.SortOrder)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetVisibleAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.IsVisible)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetAncestorsAsync(Guid categoryId, CancellationToken ct = default)
    {
        var ancestors = new List<Category>();
        var current = await DbSet
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == categoryId, ct);

        while (current?.Parent != null)
        {
            ancestors.Insert(0, current.Parent);
            current = await DbSet
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == current.ParentId, ct);
        }

        return ancestors;
    }

    public async Task<IReadOnlyList<Category>> GetDescendantsAsync(Guid categoryId, CancellationToken ct = default)
    {
        var descendants = new List<Category>();
        await CollectDescendantsAsync(categoryId, descendants, ct);
        return descendants;
    }

    public async Task<IReadOnlyList<Guid>> GetDescendantIdsAsync(Guid categoryId, CancellationToken ct = default)
    {
        var ids = new List<Guid>();
        await CollectDescendantIdsAsync(categoryId, ids, ct);
        return ids;
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(c => c.Slug == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<int> GetProductCountAsync(
        Guid categoryId,
        bool includeSubcategories = false,
        CancellationToken ct = default)
    {
        // Load products with basic server-side filtering, then filter by CategoryIds in memory
        // because CategoryIds is stored as JSON and can't be queried directly by EF Core
        var products = await Context.Products
            .Where(p => p.Status == ProductStatus.Published && !p.IsDeleted)
            .ToListAsync(ct);

        if (includeSubcategories)
        {
            var categoryIds = new List<Guid> { categoryId };
            categoryIds.AddRange(await GetDescendantIdsAsync(categoryId, ct));

            return products.Count(p => p.CategoryIds.Any(cid => categoryIds.Contains(cid)));
        }

        return products.Count(p => p.CategoryIds.Contains(categoryId));
    }

    public async Task UpdateSortOrdersAsync(
        IEnumerable<(Guid Id, int SortOrder)> sortOrders,
        CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in sortOrders)
        {
            var category = await GetByIdAsync(id, ct);
            if (category != null)
            {
                category.SortOrder = sortOrder;
            }
        }
        await Context.SaveChangesAsync(ct);
    }

    private async Task CollectDescendantsAsync(
        Guid parentId,
        List<Category> descendants,
        CancellationToken ct)
    {
        var children = await GetChildrenAsync(parentId, ct);
        foreach (var child in children)
        {
            descendants.Add(child);
            await CollectDescendantsAsync(child.Id, descendants, ct);
        }
    }

    private async Task CollectDescendantIdsAsync(
        Guid parentId,
        List<Guid> ids,
        CancellationToken ct)
    {
        var childIds = await DbSet
            .Where(c => c.ParentId == parentId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        foreach (var childId in childIds)
        {
            ids.Add(childId);
            await CollectDescendantIdsAsync(childId, ids, ct);
        }
    }
}
