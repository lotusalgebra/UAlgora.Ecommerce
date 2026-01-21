using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for category operations.
/// </summary>
public interface ICategoryRepository : ISoftDeleteRepository<Category>
{
    /// <summary>
    /// Gets a category by slug.
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Gets a category by Umbraco node ID.
    /// </summary>
    Task<Category?> GetByUmbracoNodeIdAsync(int nodeId, CancellationToken ct = default);

    /// <summary>
    /// Gets a category with children loaded.
    /// </summary>
    Task<Category?> GetWithChildrenAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a category with parent loaded.
    /// </summary>
    Task<Category?> GetWithParentAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets root categories (no parent).
    /// </summary>
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets child categories of a parent.
    /// </summary>
    Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>
    /// Gets the full category tree.
    /// </summary>
    Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets featured categories.
    /// </summary>
    Task<IReadOnlyList<Category>> GetFeaturedAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets visible categories for navigation.
    /// </summary>
    Task<IReadOnlyList<Category>> GetVisibleAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets ancestors of a category (path to root).
    /// </summary>
    Task<IReadOnlyList<Category>> GetAncestorsAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Gets all descendants of a category.
    /// </summary>
    Task<IReadOnlyList<Category>> GetDescendantsAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Gets all descendant IDs of a category.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetDescendantIdsAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a slug exists.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets product count for a category.
    /// </summary>
    Task<int> GetProductCountAsync(Guid categoryId, bool includeSubcategories = false, CancellationToken ct = default);

    /// <summary>
    /// Updates category sort orders.
    /// </summary>
    Task UpdateSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);
}
