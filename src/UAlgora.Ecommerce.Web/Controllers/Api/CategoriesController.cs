using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using static UAlgora.Ecommerce.Web.ServiceCollectionExtensions;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for category operations.
/// </summary>
public class CategoriesController : EcommerceApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct = default)
    {
        var categories = await _categoryService.GetAllAsync(ct);
        return ApiSuccess(categories);
    }

    /// <summary>
    /// Gets categories as a hierarchical tree.
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree(CancellationToken ct = default)
    {
        var tree = await _categoryService.GetTreeAsync(ct);
        return ApiSuccess(tree);
    }

    /// <summary>
    /// Gets root categories (top-level).
    /// </summary>
    [HttpGet("root")]
    public async Task<IActionResult> GetRootCategories(CancellationToken ct = default)
    {
        var categories = await _categoryService.GetRootCategoriesAsync(ct);
        return ApiSuccess(categories);
    }

    /// <summary>
    /// Gets visible categories for navigation.
    /// </summary>
    [HttpGet("visible")]
    public async Task<IActionResult> GetVisibleCategories(CancellationToken ct = default)
    {
        var categories = await _categoryService.GetVisibleAsync(ct);
        return ApiSuccess(categories);
    }

    /// <summary>
    /// Gets featured categories.
    /// </summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedCategories(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var categories = await _categoryService.GetFeaturedAsync(count, ct);
        return ApiSuccess(categories);
    }

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Category not found." });
        }

        return ApiSuccess(category);
    }

    /// <summary>
    /// Gets a category by slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug, CancellationToken ct = default)
    {
        var category = await _categoryService.GetBySlugAsync(slug, ct);
        if (category == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Category not found." });
        }

        return ApiSuccess(category);
    }

    /// <summary>
    /// Gets child categories.
    /// </summary>
    [HttpGet("{id:guid}/children")]
    public async Task<IActionResult> GetChildren(Guid id, CancellationToken ct = default)
    {
        var children = await _categoryService.GetChildrenAsync(id, ct);
        return ApiSuccess(children);
    }

    /// <summary>
    /// Gets breadcrumb path for a category.
    /// </summary>
    [HttpGet("{id:guid}/breadcrumb")]
    public async Task<IActionResult> GetBreadcrumb(Guid id, CancellationToken ct = default)
    {
        var breadcrumb = await _categoryService.GetBreadcrumbAsync(id, ct);
        return ApiSuccess(breadcrumb);
    }

    /// <summary>
    /// Gets product count for a category.
    /// </summary>
    [HttpGet("{id:guid}/product-count")]
    public async Task<IActionResult> GetProductCount(
        Guid id,
        [FromQuery] bool includeSubcategories = false,
        CancellationToken ct = default)
    {
        var count = await _categoryService.GetProductCountAsync(id, includeSubcategories, ct);
        return ApiSuccess(new { count });
    }
}

#region Admin Endpoints

/// <summary>
/// Admin API controller for category management.
/// </summary>
[Route("api/ecommerce/admin/categories")]
[Authorize(Policy = EcommerceAdminPolicy)]
public class CategoriesAdminController : EcommerceApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesAdminController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Gets all categories (admin).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories(
        [FromQuery] bool? isVisible = null,
        [FromQuery] bool? isFeatured = null,
        CancellationToken ct = default)
    {
        var categories = await _categoryService.GetAllAsync(ct);

        // Apply filters if specified
        if (isVisible.HasValue)
        {
            categories = categories.Where(c => c.IsVisible == isVisible.Value).ToList();
        }
        if (isFeatured.HasValue)
        {
            categories = categories.Where(c => c.IsFeatured == isFeatured.Value).ToList();
        }

        return ApiSuccess(categories);
    }

    /// <summary>
    /// Gets the category tree (admin).
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree(CancellationToken ct = default)
    {
        var tree = await _categoryService.GetTreeAsync(ct);
        return ApiSuccess(tree);
    }

    /// <summary>
    /// Gets a category by ID (admin).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Category not found." });
        }

        return ApiSuccess(category);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug ?? await _categoryService.GenerateSlugAsync(request.Name, ct),
            Description = request.Description,
            ParentId = request.ParentId,
            ImageId = request.ImageId,
            ImageUrl = request.ImageUrl,
            IsVisible = request.IsVisible,
            IsFeatured = request.IsFeatured,
            SortOrder = request.SortOrder,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            UmbracoNodeId = request.UmbracoNodeId
        };

        // Set level based on parent
        if (request.ParentId.HasValue)
        {
            var parent = await _categoryService.GetByIdAsync(request.ParentId.Value, ct);
            if (parent != null)
            {
                category.Level = parent.Level + 1;
                category.Path = string.IsNullOrEmpty(parent.Path)
                    ? $"{parent.Name}/{request.Name}"
                    : $"{parent.Path}/{request.Name}";
            }
        }
        else
        {
            category.Level = 0;
            category.Path = request.Name;
        }

        var validation = await _categoryService.ValidateAsync(category, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var created = await _categoryService.CreateAsync(category, ct);
        return ApiSuccess(created, "Category created.");
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Category not found." });
        }

        // Update properties
        category.Name = request.Name ?? category.Name;
        category.Slug = request.Slug ?? category.Slug;
        category.Description = request.Description ?? category.Description;
        category.ImageId = request.ImageId ?? category.ImageId;
        category.ImageUrl = request.ImageUrl ?? category.ImageUrl;

        if (request.IsVisible.HasValue)
            category.IsVisible = request.IsVisible.Value;
        if (request.IsFeatured.HasValue)
            category.IsFeatured = request.IsFeatured.Value;
        if (request.SortOrder.HasValue)
            category.SortOrder = request.SortOrder.Value;

        category.MetaTitle = request.MetaTitle ?? category.MetaTitle;
        category.MetaDescription = request.MetaDescription ?? category.MetaDescription;

        // Update path if name changed
        if (request.Name != null && request.Name != category.Name)
        {
            // Rebuild path
            var breadcrumb = await _categoryService.GetBreadcrumbAsync(id, ct);
            var pathParts = breadcrumb.Select(c => c.Id == id ? request.Name : c.Name);
            category.Path = string.Join("/", pathParts);
        }

        var validation = await _categoryService.ValidateAsync(category, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var updated = await _categoryService.UpdateAsync(category, ct);
        return ApiSuccess(updated, "Category updated.");
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        [FromQuery] bool deleteChildren = false,
        CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Category not found." });
        }

        // Check if category has children
        var children = await _categoryService.GetChildrenAsync(id, ct);
        if (children.Count > 0 && !deleteChildren)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Category has child categories. Set deleteChildren=true to delete them, or move/delete them first."
            });
        }

        // Check if category has products
        var productCount = await _categoryService.GetProductCountAsync(id, false, ct);
        if (productCount > 0)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = $"Category has {productCount} products. Please reassign or delete the products first."
            });
        }

        // Delete children if requested
        if (deleteChildren)
        {
            foreach (var child in children)
            {
                await _categoryService.DeleteAsync(child.Id, ct);
            }
        }

        await _categoryService.DeleteAsync(id, ct);
        return ApiSuccess(new { }, "Category deleted.");
    }

    /// <summary>
    /// Moves a category to a new parent.
    /// </summary>
    [HttpPost("{id:guid}/move")]
    public async Task<IActionResult> MoveCategory(
        Guid id,
        [FromBody] MoveCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Category not found." });
        }

        // Prevent moving to itself or its own descendants
        if (request.NewParentId.HasValue)
        {
            if (request.NewParentId.Value == id)
            {
                return BadRequest(new ApiErrorResponse { Message = "Cannot move category to itself." });
            }

            var newParent = await _categoryService.GetByIdAsync(request.NewParentId.Value, ct);
            if (newParent == null)
            {
                return NotFound(new ApiErrorResponse { Message = "Target parent category not found." });
            }

            // Check if new parent is a descendant of this category
            var breadcrumb = await _categoryService.GetBreadcrumbAsync(request.NewParentId.Value, ct);
            if (breadcrumb.Any(c => c.Id == id))
            {
                return BadRequest(new ApiErrorResponse { Message = "Cannot move category to its own descendant." });
            }
        }

        try
        {
            await _categoryService.MoveAsync(id, request.NewParentId, ct);

            // Reload the category to get updated data
            var updated = await _categoryService.GetByIdAsync(id, ct);
            return ApiSuccess(updated, "Category moved.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Updates sort orders for multiple categories.
    /// </summary>
    [HttpPost("sort-orders")]
    public async Task<IActionResult> UpdateSortOrders(
        [FromBody] UpdateSortOrdersRequest request,
        CancellationToken ct = default)
    {
        if (request.SortOrders == null || request.SortOrders.Count == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "At least one sort order is required." });
        }

        var sortOrders = request.SortOrders.Select(s => (s.Id, s.SortOrder));

        try
        {
            await _categoryService.UpdateSortOrdersAsync(sortOrders, ct);
            return ApiSuccess(new { }, "Sort orders updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Generates a unique slug.
    /// </summary>
    [HttpPost("generate-slug")]
    public async Task<IActionResult> GenerateSlug(
        [FromBody] GenerateCategorySlugRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiErrorResponse { Message = "Name is required." });
        }

        var slug = await _categoryService.GenerateSlugAsync(request.Name, ct);
        return ApiSuccess(new { slug });
    }

    /// <summary>
    /// Updates category visibility.
    /// </summary>
    [HttpPut("{id:guid}/visibility")]
    public async Task<IActionResult> UpdateVisibility(
        Guid id,
        [FromBody] UpdateCategoryVisibilityRequest request,
        CancellationToken ct = default)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Category not found." });
        }

        category.IsVisible = request.IsVisible;
        if (request.IsFeatured.HasValue)
            category.IsFeatured = request.IsFeatured.Value;

        var updated = await _categoryService.UpdateAsync(category, ct);
        return ApiSuccess(updated, "Category visibility updated.");
    }

    #region Bulk Operations

    /// <summary>
    /// Updates visibility for multiple categories.
    /// </summary>
    [HttpPost("bulk/visibility")]
    public async Task<IActionResult> BulkUpdateVisibility(
        [FromBody] BulkUpdateCategoryVisibilityRequest request,
        CancellationToken ct = default)
    {
        if (request.CategoryIds == null || request.CategoryIds.Count == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "At least one category ID is required." });
        }

        var updatedCount = 0;
        var errors = new List<string>();

        foreach (var categoryId in request.CategoryIds)
        {
            var category = await _categoryService.GetByIdAsync(categoryId, ct);
            if (category == null)
            {
                errors.Add($"Category {categoryId} not found.");
                continue;
            }

            category.IsVisible = request.IsVisible;
            if (request.IsFeatured.HasValue)
                category.IsFeatured = request.IsFeatured.Value;

            await _categoryService.UpdateAsync(category, ct);
            updatedCount++;
        }

        return ApiSuccess(new { updatedCount, errors }, $"{updatedCount} categories updated.");
    }

    /// <summary>
    /// Deletes multiple categories.
    /// </summary>
    [HttpPost("bulk/delete")]
    public async Task<IActionResult> BulkDelete(
        [FromBody] BulkDeleteCategoriesRequest request,
        CancellationToken ct = default)
    {
        if (request.CategoryIds == null || request.CategoryIds.Count == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "At least one category ID is required." });
        }

        var deletedCount = 0;
        var errors = new List<string>();

        foreach (var categoryId in request.CategoryIds)
        {
            var category = await _categoryService.GetByIdAsync(categoryId, ct);
            if (category == null)
            {
                errors.Add($"Category {categoryId} not found.");
                continue;
            }

            // Check for children
            var children = await _categoryService.GetChildrenAsync(categoryId, ct);
            if (children.Count > 0)
            {
                errors.Add($"Category {categoryId} has child categories.");
                continue;
            }

            // Check for products
            var productCount = await _categoryService.GetProductCountAsync(categoryId, false, ct);
            if (productCount > 0)
            {
                errors.Add($"Category {categoryId} has {productCount} products.");
                continue;
            }

            try
            {
                await _categoryService.DeleteAsync(categoryId, ct);
                deletedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to delete category {categoryId}: {ex.Message}");
            }
        }

        return ApiSuccess(new { deletedCount, errors }, $"{deletedCount} categories deleted.");
    }

    #endregion
}

#endregion

#region Request Models

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int? UmbracoNodeId { get; set; }
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public Guid? ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsVisible { get; set; }
    public bool? IsFeatured { get; set; }
    public int? SortOrder { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

public class MoveCategoryRequest
{
    public Guid? NewParentId { get; set; }
}

public class UpdateSortOrdersRequest
{
    public List<CategorySortOrderItem> SortOrders { get; set; } = [];
}

public class CategorySortOrderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}

public class GenerateCategorySlugRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryVisibilityRequest
{
    public bool IsVisible { get; set; }
    public bool? IsFeatured { get; set; }
}

public class BulkUpdateCategoryVisibilityRequest
{
    public List<Guid> CategoryIds { get; set; } = [];
    public bool IsVisible { get; set; }
    public bool? IsFeatured { get; set; }
}

public class BulkDeleteCategoriesRequest
{
    public List<Guid> CategoryIds { get; set; } = [];
}

#endregion
