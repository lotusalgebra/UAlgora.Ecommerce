using System.Text.RegularExpressions;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for category operations.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _categoryRepository.GetByIdAsync(id, ct);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _categoryRepository.GetBySlugAsync(slug, ct);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await _categoryRepository.GetAllAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default)
    {
        return await _categoryRepository.GetTreeAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        return await _categoryRepository.GetRootCategoriesAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        return await _categoryRepository.GetChildrenAsync(parentId, ct);
    }

    public async Task<IReadOnlyList<Category>> GetFeaturedAsync(int count = 10, CancellationToken ct = default)
    {
        return await _categoryRepository.GetFeaturedAsync(count, ct);
    }

    public async Task<IReadOnlyList<Category>> GetVisibleAsync(CancellationToken ct = default)
    {
        return await _categoryRepository.GetVisibleAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetBreadcrumbAsync(Guid categoryId, CancellationToken ct = default)
    {
        // Get ancestors and include the current category at the end
        var ancestors = await _categoryRepository.GetAncestorsAsync(categoryId, ct);
        var current = await _categoryRepository.GetByIdAsync(categoryId, ct);

        var breadcrumb = ancestors.ToList();
        if (current != null)
        {
            breadcrumb.Add(current);
        }

        return breadcrumb;
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        // Generate slug if not provided
        if (string.IsNullOrWhiteSpace(category.Slug))
        {
            category.Slug = await GenerateSlugAsync(category.Name, ct);
        }

        // Calculate level based on parent
        if (category.ParentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(category.ParentId.Value, ct);
            if (parent != null)
            {
                category.Level = parent.Level + 1;
            }
        }
        else
        {
            category.Level = 0;
        }

        return await _categoryRepository.AddAsync(category, ct);
    }

    public async Task<Category> UpdateAsync(Category category, CancellationToken ct = default)
    {
        return await _categoryRepository.UpdateAsync(category, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // Check for children
        var children = await _categoryRepository.GetChildrenAsync(id, ct);
        if (children.Count > 0)
        {
            // Move children to parent category or make them root
            var category = await _categoryRepository.GetWithParentAsync(id, ct);
            var newParentId = category?.ParentId;

            foreach (var child in children)
            {
                await MoveAsync(child.Id, newParentId, ct);
            }
        }

        await _categoryRepository.SoftDeleteAsync(id, ct);
    }

    public async Task MoveAsync(Guid categoryId, Guid? newParentId, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, ct);
        if (category == null)
        {
            throw new InvalidOperationException($"Category {categoryId} not found.");
        }

        // Prevent circular reference
        if (newParentId.HasValue)
        {
            var descendants = await _categoryRepository.GetDescendantIdsAsync(categoryId, ct);
            if (descendants.Contains(newParentId.Value))
            {
                throw new InvalidOperationException("Cannot move category to its own descendant.");
            }
        }

        category.ParentId = newParentId;

        // Update level
        if (newParentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(newParentId.Value, ct);
            category.Level = (parent?.Level ?? 0) + 1;
        }
        else
        {
            category.Level = 0;
        }

        await _categoryRepository.UpdateAsync(category, ct);

        // Update descendants' levels
        await UpdateDescendantLevelsAsync(categoryId, category.Level, ct);
    }

    public async Task UpdateSortOrdersAsync(
        IEnumerable<(Guid Id, int SortOrder)> sortOrders,
        CancellationToken ct = default)
    {
        await _categoryRepository.UpdateSortOrdersAsync(sortOrders, ct);
    }

    public async Task<string> GenerateSlugAsync(string name, CancellationToken ct = default)
    {
        var slug = GenerateBaseSlug(name);
        var counter = 0;
        var uniqueSlug = slug;

        while (await _categoryRepository.SlugExistsAsync(uniqueSlug, null, ct))
        {
            counter++;
            uniqueSlug = $"{slug}-{counter}";
        }

        return uniqueSlug;
    }

    public async Task<int> GetProductCountAsync(
        Guid categoryId,
        bool includeSubcategories = false,
        CancellationToken ct = default)
    {
        return await _categoryRepository.GetProductCountAsync(categoryId, includeSubcategories, ct);
    }

    public async Task<ValidationResult> ValidateAsync(Category category, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Category name is required." });
        }

        if (!string.IsNullOrWhiteSpace(category.Slug))
        {
            if (await _categoryRepository.SlugExistsAsync(category.Slug, category.Id == Guid.Empty ? null : category.Id, ct))
            {
                errors.Add(new ValidationError { PropertyName = "Slug", ErrorMessage = "Slug already exists." });
            }
        }

        // Validate parent exists if specified
        if (category.ParentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(category.ParentId.Value, ct);
            if (parent == null)
            {
                errors.Add(new ValidationError { PropertyName = "ParentId", ErrorMessage = "Parent category not found." });
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    private async Task UpdateDescendantLevelsAsync(Guid parentId, int parentLevel, CancellationToken ct)
    {
        var children = await _categoryRepository.GetChildrenAsync(parentId, ct);
        foreach (var child in children)
        {
            child.Level = parentLevel + 1;
            await _categoryRepository.UpdateAsync(child, ct);
            await UpdateDescendantLevelsAsync(child.Id, child.Level, ct);
        }
    }

    private static string GenerateBaseSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Guid.NewGuid().ToString()[..8].ToLowerInvariant();

        // Convert to lowercase
        var slug = input.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove special characters except hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove multiple consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 100)
            slug = slug[..100].TrimEnd('-');

        return slug;
    }
}
