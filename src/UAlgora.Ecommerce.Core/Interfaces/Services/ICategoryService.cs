using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for category operations.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a category by slug.
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Gets all categories.
    /// </summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the category tree (hierarchical structure).
    /// </summary>
    Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets root categories.
    /// </summary>
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets child categories of a parent.
    /// </summary>
    Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>
    /// Gets featured categories.
    /// </summary>
    Task<IReadOnlyList<Category>> GetFeaturedAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets visible categories for navigation.
    /// </summary>
    Task<IReadOnlyList<Category>> GetVisibleAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets breadcrumb path for a category.
    /// </summary>
    Task<IReadOnlyList<Category>> GetBreadcrumbAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    Task<Category> CreateAsync(Category category, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    Task<Category> UpdateAsync(Category category, CancellationToken ct = default);

    /// <summary>
    /// Deletes a category.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Moves a category to a new parent.
    /// </summary>
    Task MoveAsync(Guid categoryId, Guid? newParentId, CancellationToken ct = default);

    /// <summary>
    /// Updates category sort orders.
    /// </summary>
    Task UpdateSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);

    /// <summary>
    /// Generates a unique slug.
    /// </summary>
    Task<string> GenerateSlugAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets product count for a category.
    /// </summary>
    Task<int> GetProductCountAsync(Guid categoryId, bool includeSubcategories = false, CancellationToken ct = default);

    /// <summary>
    /// Validates a category.
    /// </summary>
    Task<ValidationResult> ValidateAsync(Category category, CancellationToken ct = default);
}
