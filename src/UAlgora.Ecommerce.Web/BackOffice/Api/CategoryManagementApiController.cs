using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for category operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Categories}")]
public class CategoryManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryManagementApiController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Gets the hierarchical tree structure for categories.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType<CategoryTreeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var rootCategories = await _categoryService.GetRootCategoriesAsync();
        var nodes = new List<CategoryTreeNodeModel>();

        foreach (var category in rootCategories.OrderBy(c => c.SortOrder))
        {
            nodes.Add(await BuildCategoryTreeNode(category));
        }

        return Ok(new CategoryTreeResponse { Nodes = nodes });
    }

    /// <summary>
    /// Gets children nodes for a specific category.
    /// </summary>
    [HttpGet("tree/{id:guid}/children")]
    [ProducesResponseType<CategoryTreeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTreeChildren(Guid id)
    {
        var children = await _categoryService.GetChildrenAsync(id);
        var nodes = new List<CategoryTreeNodeModel>();

        foreach (var category in children.OrderBy(c => c.SortOrder))
        {
            nodes.Add(await BuildCategoryTreeNode(category));
        }

        return Ok(new CategoryTreeResponse { Nodes = nodes });
    }

    /// <summary>
    /// Gets all categories in a flat list.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<CategoryListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        return Ok(new CategoryListResponse
        {
            Items = categories.Select(MapToCategoryItem).ToList(),
            Total = categories.Count()
        });
    }

    /// <summary>
    /// Gets a single category by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var productCount = await _categoryService.GetProductCountAsync(id, false);
        return Ok(MapToCategoryDetail(category, productCount));
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        // Validate request
        var validation = await ValidateCategoryRequest(request);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors });
        }

        // Generate slug if not provided
        var slug = request.Slug;
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = await _categoryService.GenerateSlugAsync(request.Name);
        }

        // Validate parent exists if provided
        if (request.ParentId.HasValue)
        {
            var parent = await _categoryService.GetByIdAsync(request.ParentId.Value);
            if (parent == null)
            {
                return BadRequest(new { errors = new[] { new { propertyName = "parentId", errorMessage = "Parent category not found" } } });
            }
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            ParentId = request.ParentId,
            IsVisible = request.IsVisible,
            IsFeatured = request.IsFeatured,
            SortOrder = request.SortOrder,
            ImageUrl = request.ImageUrl,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription
        };

        var created = await _categoryService.CreateAsync(category);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToCategoryDetail(created, 0));
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Validate request
        var validation = await ValidateCategoryRequest(request, id);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors });
        }

        // Prevent circular reference (category can't be its own parent)
        if (request.ParentId.HasValue && request.ParentId.Value == id)
        {
            return BadRequest(new { errors = new[] { new { propertyName = "parentId", errorMessage = "Category cannot be its own parent" } } });
        }

        // Validate parent exists if provided
        if (request.ParentId.HasValue)
        {
            var parent = await _categoryService.GetByIdAsync(request.ParentId.Value);
            if (parent == null)
            {
                return BadRequest(new { errors = new[] { new { propertyName = "parentId", errorMessage = "Parent category not found" } } });
            }
        }

        // Update properties
        existing.Name = request.Name.Trim();
        existing.Slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await _categoryService.GenerateSlugAsync(request.Name)
            : request.Slug;
        existing.Description = request.Description;
        existing.ParentId = request.ParentId;
        existing.IsVisible = request.IsVisible;
        existing.IsFeatured = request.IsFeatured;
        existing.SortOrder = request.SortOrder;
        existing.ImageUrl = request.ImageUrl;
        existing.MetaTitle = request.MetaTitle;
        existing.MetaDescription = request.MetaDescription;

        var updated = await _categoryService.UpdateAsync(existing);
        var productCount = await _categoryService.GetProductCountAsync(id, false);

        return Ok(MapToCategoryDetail(updated, productCount));
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Check for child categories
        var children = await _categoryService.GetChildrenAsync(id);
        if (children.Any())
        {
            return BadRequest(new { message = "Cannot delete category with subcategories. Delete or move subcategories first." });
        }

        await _categoryService.DeleteAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Moves a category to a new parent.
    /// </summary>
    [HttpPost("{id:guid}/move")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveCategoryRequest request)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Prevent moving to itself
        if (request.NewParentId.HasValue && request.NewParentId.Value == id)
        {
            return BadRequest(new { message = "Category cannot be moved to itself" });
        }

        // Validate new parent exists if provided
        if (request.NewParentId.HasValue)
        {
            var newParent = await _categoryService.GetByIdAsync(request.NewParentId.Value);
            if (newParent == null)
            {
                return BadRequest(new { message = "Target parent category not found" });
            }
        }

        await _categoryService.MoveAsync(id, request.NewParentId);

        return NoContent();
    }

    /// <summary>
    /// Duplicates an existing category.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        var newName = $"{existing.Name} (Copy)";
        var newSlug = await _categoryService.GenerateSlugAsync(newName);

        var duplicate = new Category
        {
            Name = newName,
            Slug = newSlug,
            Description = existing.Description,
            ParentId = existing.ParentId,
            IsVisible = false, // Hidden by default until reviewed
            IsFeatured = false,
            SortOrder = existing.SortOrder + 1,
            ImageUrl = existing.ImageUrl,
            MetaTitle = existing.MetaTitle,
            MetaDescription = existing.MetaDescription
        };

        var created = await _categoryService.CreateAsync(duplicate);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToCategoryDetail(created, 0));
    }

    /// <summary>
    /// Toggles the visibility of a category.
    /// </summary>
    [HttpPost("{id:guid}/toggle-visibility")]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleVisibility(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsVisible = !category.IsVisible;
        var updated = await _categoryService.UpdateAsync(category);
        var productCount = await _categoryService.GetProductCountAsync(id, false);
        return Ok(MapToCategoryDetail(updated, productCount));
    }

    /// <summary>
    /// Toggles the featured status of a category.
    /// </summary>
    [HttpPost("{id:guid}/toggle-featured")]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFeatured(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsFeatured = !category.IsFeatured;
        var updated = await _categoryService.UpdateAsync(category);
        var productCount = await _categoryService.GetProductCountAsync(id, false);
        return Ok(MapToCategoryDetail(updated, productCount));
    }

    /// <summary>
    /// Moves a category to the root level.
    /// </summary>
    [HttpPost("{id:guid}/move-to-root")]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveToRoot(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        if (!category.ParentId.HasValue)
        {
            return BadRequest(new { message = "Category is already at root level" });
        }

        await _categoryService.MoveAsync(id, null);
        var updated = await _categoryService.GetByIdAsync(id);
        var productCount = await _categoryService.GetProductCountAsync(id, false);
        return Ok(MapToCategoryDetail(updated!, productCount));
    }

    /// <summary>
    /// Updates the sort order of a category.
    /// </summary>
    [HttpPost("{id:guid}/update-sort")]
    [ProducesResponseType<CategoryDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSortOrder(Guid id, [FromBody] UpdateSortOrderRequest request)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.SortOrder = request.SortOrder;
        var updated = await _categoryService.UpdateAsync(category);
        var productCount = await _categoryService.GetProductCountAsync(id, false);
        return Ok(MapToCategoryDetail(updated, productCount));
    }

    private async Task<ValidationResult> ValidateCategoryRequest(CategoryRequestBase request, Guid? excludeId = null)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationError { PropertyName = "name", ErrorMessage = "Category name is required" });
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            // Check for duplicate slug
            var existingBySlug = await _categoryService.GetBySlugAsync(request.Slug);
            if (existingBySlug != null && existingBySlug.Id != excludeId)
            {
                errors.Add(new ValidationError { PropertyName = "slug", ErrorMessage = "A category with this slug already exists" });
            }
        }

        return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    private async Task<CategoryTreeNodeModel> BuildCategoryTreeNode(Category category)
    {
        var children = await _categoryService.GetChildrenAsync(category.Id);
        var productCount = await _categoryService.GetProductCountAsync(category.Id, true);

        var icon = category.IsVisible ? EcommerceConstants.Icons.Category : "icon-folder color-grey";
        var cssClasses = new List<string>();
        if (!category.IsVisible) cssClasses.Add("is-hidden");
        if (category.IsFeatured) cssClasses.Add("is-featured");

        return new CategoryTreeNodeModel
        {
            Id = category.Id,
            Name = category.Name,
            Icon = icon,
            HasChildren = children.Any(),
            ProductCount = productCount,
            IsVisible = category.IsVisible,
            IsFeatured = category.IsFeatured,
            SortOrder = category.SortOrder,
            CssClasses = cssClasses
        };
    }

    private static CategoryItemModel MapToCategoryItem(Category category)
    {
        return new CategoryItemModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug ?? string.Empty,
            Description = category.Description,
            ParentId = category.ParentId,
            IsVisible = category.IsVisible,
            IsFeatured = category.IsFeatured,
            SortOrder = category.SortOrder,
            ImageUrl = category.ImageUrl,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private static CategoryDetailModel MapToCategoryDetail(Category category, int productCount)
    {
        return new CategoryDetailModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug ?? string.Empty,
            Description = category.Description,
            ParentId = category.ParentId,
            IsVisible = category.IsVisible,
            IsFeatured = category.IsFeatured,
            SortOrder = category.SortOrder,
            ImageUrl = category.ImageUrl,
            ImageId = category.ImageId,
            Level = category.Level,
            Path = category.Path,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            ProductCount = productCount,
            IsRoot = category.IsRoot,
            HasChildren = category.HasChildren,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}

#region Response Models

public class CategoryTreeResponse
{
    public List<CategoryTreeNodeModel> Nodes { get; set; } = [];
}

public class CategoryTreeNodeModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public bool HasChildren { get; set; }
    public int ProductCount { get; set; }
    public bool IsVisible { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public List<string> CssClasses { get; set; } = [];
}

public class CategoryListResponse
{
    public List<CategoryItemModel> Items { get; set; } = [];
    public int Total { get; set; }
}

public class CategoryItemModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsVisible { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public string? ImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CategoryDetailModel : CategoryItemModel
{
    public Guid? ImageId { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }
    public int ProductCount { get; set; }
    public bool IsRoot { get; set; }
    public bool HasChildren { get; set; }
}

#endregion

#region Request Models

public abstract class CategoryRequestBase
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public string? ImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

public class CreateCategoryRequest : CategoryRequestBase
{
}

public class UpdateCategoryRequest : CategoryRequestBase
{
}

public class MoveCategoryRequest
{
    public Guid? NewParentId { get; set; }
}

public class UpdateSortOrderRequest
{
    public int SortOrder { get; set; }
}

#endregion
