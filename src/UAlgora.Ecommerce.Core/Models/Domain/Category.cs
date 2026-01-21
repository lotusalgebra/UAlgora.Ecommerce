namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a product category for organizing the catalog.
/// </summary>
public class Category : SoftDeleteEntity
{
    /// <summary>
    /// Reference to the Umbraco content node ID.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug for the category.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Category description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Parent category ID for hierarchical structure.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Navigation property to parent category.
    /// </summary>
    public Category? Parent { get; set; }

    /// <summary>
    /// Child categories.
    /// </summary>
    public List<Category> Children { get; set; } = [];

    /// <summary>
    /// Category image ID.
    /// </summary>
    public Guid? ImageId { get; set; }

    /// <summary>
    /// Category image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether this category is visible in navigation.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether this category is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Hierarchical level (0 = root).
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Full path from root (e.g., "Electronics/Computers/Laptops").
    /// </summary>
    public string? Path { get; set; }

    #region SEO

    /// <summary>
    /// Meta title for SEO.
    /// </summary>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Meta description for SEO.
    /// </summary>
    public string? MetaDescription { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Products in this category.
    /// </summary>
    public List<Product> Products { get; set; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether this is a root category.
    /// </summary>
    public bool IsRoot => !ParentId.HasValue;

    /// <summary>
    /// Whether this category has children.
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// Number of products in this category (not including subcategories).
    /// </summary>
    public int ProductCount => Products.Count;

    #endregion
}
