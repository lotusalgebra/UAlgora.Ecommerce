using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Algora Category Content Sync Service
///
/// Syncs categories from the Algora Commerce database to Umbraco content nodes.
/// This allows categories to appear in the Umbraco Content section alongside
/// regular content, enabling full CMS management capabilities.
///
/// Architecture:
/// - Creates category nodes under the Catalog document type
/// - Maintains parent-child hierarchy from database
/// - Maps all category properties to document type properties
/// - Supports incremental sync (only creates new/updates changed)
/// </summary>
public class CategoryContentSyncService
{
    #region Dependencies

    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryContentSyncService> _logger;

    #endregion

    #region Constants

    private const string CatalogRootName = "Catalog";

    #endregion

    #region Constructor

    public CategoryContentSyncService(
        IContentService contentService,
        IContentTypeService contentTypeService,
        ICategoryService categoryService,
        ILogger<CategoryContentSyncService> logger)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _categoryService = categoryService;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Syncs all categories from the database to Umbraco content tree
    /// </summary>
    public async Task<CategorySyncResult> SyncAllCategoriesAsync()
    {
        var result = new CategorySyncResult();

        try
        {
            _logger.LogInformation("Algora Commerce: Starting category content sync...");

            // Verify document types exist
            var categoryDocType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CategoryAlias);
            var catalogDocType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CatalogAlias);

            if (categoryDocType == null)
            {
                _logger.LogError("Algora Category document type not found. Please restart the application.");
                result.Errors.Add("Algora Category document type not found");
                return result;
            }

            if (catalogDocType == null)
            {
                _logger.LogError("Algora Catalog document type not found. Please restart the application.");
                result.Errors.Add("Algora Catalog document type not found");
                return result;
            }

            // Get or create Catalog root container
            var catalogRoot = GetOrCreateCatalogRoot(catalogDocType);
            if (catalogRoot == null)
            {
                result.Errors.Add("Failed to create Catalog root container");
                return result;
            }

            _logger.LogInformation("Using Catalog root container with ID: {Id}", catalogRoot.Id);

            // Get all categories from database
            var allCategories = await _categoryService.GetAllAsync();
            result.TotalCategories = allCategories.Count();

            _logger.LogInformation("Algora Commerce: Found {Count} categories to sync", result.TotalCategories);

            // Build a dictionary of existing category content nodes by UmbracoNodeId
            var existingNodes = GetExistingCategoryNodes(categoryDocType);

            // First sync root categories (no parent), then children
            var rootCategories = allCategories.Where(c => !c.ParentId.HasValue).OrderBy(c => c.SortOrder);
            var childCategories = allCategories.Where(c => c.ParentId.HasValue).OrderBy(c => c.Level).ThenBy(c => c.SortOrder);

            // Sync root categories under Catalog
            foreach (var category in rootCategories)
            {
                await SyncCategoryAsync(category, catalogRoot.Id, categoryDocType, existingNodes, result);
            }

            // Sync child categories - need to find their parent content nodes
            foreach (var category in childCategories)
            {
                // Find parent content node
                var parentCategory = allCategories.FirstOrDefault(c => c.Id == category.ParentId);
                int parentContentId;

                if (parentCategory?.UmbracoNodeId > 0)
                {
                    parentContentId = parentCategory.UmbracoNodeId.Value;
                }
                else if (parentCategory != null && existingNodes.TryGetValue(parentCategory.Slug ?? "", out var parentNode))
                {
                    parentContentId = parentNode.Id;
                }
                else
                {
                    // Parent not found, use Catalog root
                    parentContentId = catalogRoot.Id;
                    _logger.LogWarning("Parent category not found for {Name}, using Catalog root", category.Name);
                }

                await SyncCategoryAsync(category, parentContentId, categoryDocType, existingNodes, result);
            }

            _logger.LogInformation("Algora Commerce: Category sync complete. Created: {Created}, Updated: {Updated}",
                result.Created, result.Updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Algora Commerce: Error during category sync");
            result.Errors.Add($"Sync failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Syncs a single category from database to content tree
    /// </summary>
    public async Task<bool> SyncCategoryToContentAsync(Guid categoryId)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Category not found: {Id}", categoryId);
                return false;
            }

            var categoryDocType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CategoryAlias);
            if (categoryDocType == null)
            {
                _logger.LogError("Algora Category document type not found");
                return false;
            }

            // Find existing content node
            IContent? existingContent = null;
            if (category.UmbracoNodeId.HasValue && category.UmbracoNodeId > 0)
            {
                existingContent = _contentService.GetById(category.UmbracoNodeId.Value);
            }

            if (existingContent != null)
            {
                // Update existing
                MapCategoryToContent(existingContent, category);
                _contentService.Save(existingContent);
                _contentService.Publish(existingContent, Array.Empty<string>());
                _logger.LogInformation("Updated category content: {Name}", category.Name);
            }
            else
            {
                // Need to find parent and create
                var catalogDocType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CatalogAlias);
                var catalogRoot = GetOrCreateCatalogRoot(catalogDocType!);
                if (catalogRoot == null) return false;

                int parentContentId = catalogRoot.Id;

                if (category.ParentId.HasValue)
                {
                    var parentCategory = await _categoryService.GetByIdAsync(category.ParentId.Value);
                    if (parentCategory?.UmbracoNodeId > 0)
                    {
                        parentContentId = parentCategory.UmbracoNodeId.Value;
                    }
                }

                var newContent = _contentService.Create(category.Name ?? "Category", parentContentId, categoryDocType);
                MapCategoryToContent(newContent, category);

                var saveResult = _contentService.Save(newContent);
                if (saveResult.Success)
                {
                    _contentService.Publish(newContent, Array.Empty<string>());

                    // Update category with Umbraco node ID
                    category.UmbracoNodeId = newContent.Id;
                    await _categoryService.UpdateAsync(category);

                    _logger.LogInformation("Created category content: {Name}", category.Name);
                }
                else
                {
                    _logger.LogError("Failed to save category content: {Name}", category.Name);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing category to content: {Id}", categoryId);
            return false;
        }
    }

    #endregion

    #region Private Methods

    private IContent? GetOrCreateCatalogRoot(IContentType catalogDocType)
    {
        // Try to find existing Catalog root
        var rootNodes = _contentService.GetRootContent();
        var existingRoot = rootNodes.FirstOrDefault(c =>
            c.ContentType.Alias == AlgoraDocumentTypeConstants.CatalogAlias ||
            c.Name == CatalogRootName);

        if (existingRoot != null)
        {
            _logger.LogInformation("Found existing Catalog root container with ID: {Id}", existingRoot.Id);
            return existingRoot;
        }

        // Create new Catalog container
        _logger.LogInformation("Creating Catalog root container...");

        try
        {
            var root = _contentService.Create(CatalogRootName, Constants.System.Root, catalogDocType);
            root.SetValue("catalogName", CatalogRootName);
            root.SetValue("description", "Algora Commerce Product Catalog");

            var saveResult = _contentService.Save(root);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save Catalog root: {Errors}",
                    string.Join(", ", saveResult.EventMessages.GetAll().Select(m => m.Message)));
                return null;
            }

            var publishResult = _contentService.Publish(root, Array.Empty<string>());
            if (!publishResult.Success)
            {
                _logger.LogWarning("Failed to publish Catalog root (content was saved)");
            }

            _logger.LogInformation("Catalog root container created with ID: {Id}", root.Id);
            return root;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Catalog root container");
            return null;
        }
    }

    private Dictionary<string, IContent> GetExistingCategoryNodes(IContentType categoryDocType)
    {
        var result = new Dictionary<string, IContent>();

        try
        {
            var categoryContents = _contentService.GetPagedOfType(
                categoryDocType.Id,
                0,
                int.MaxValue,
                out _,
                null);

            foreach (var content in categoryContents)
            {
                var slug = content.GetValue<string>("slug");
                if (!string.IsNullOrEmpty(slug) && !result.ContainsKey(slug))
                {
                    result[slug] = content;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting existing category nodes");
        }

        return result;
    }

    private async Task SyncCategoryAsync(
        Category category,
        int parentContentId,
        IContentType categoryDocType,
        Dictionary<string, IContent> existingNodes,
        CategorySyncResult result)
    {
        try
        {
            var slug = category.Slug ?? "";
            IContent? existingNode = null;

            // First check by UmbracoNodeId
            if (category.UmbracoNodeId.HasValue && category.UmbracoNodeId > 0)
            {
                existingNode = _contentService.GetById(category.UmbracoNodeId.Value);
            }

            // Then check by slug
            if (existingNode == null && existingNodes.TryGetValue(slug, out var nodeBySlug))
            {
                existingNode = nodeBySlug;
            }

            if (existingNode != null)
            {
                // Update existing node
                MapCategoryToContent(existingNode, category);
                _contentService.Save(existingNode);
                result.Updated++;

                // Update UmbracoNodeId if not set
                if (!category.UmbracoNodeId.HasValue || category.UmbracoNodeId != existingNode.Id)
                {
                    category.UmbracoNodeId = existingNode.Id;
                    await _categoryService.UpdateAsync(category);
                }
            }
            else
            {
                // Create new node
                var newNode = CreateCategoryNode(parentContentId, categoryDocType, category);
                if (newNode != null)
                {
                    var saveResult = _contentService.Save(newNode);
                    if (saveResult.Success)
                    {
                        _contentService.Publish(newNode, Array.Empty<string>());
                        result.Created++;

                        // Update category with Umbraco node ID
                        category.UmbracoNodeId = newNode.Id;
                        await _categoryService.UpdateAsync(category);

                        // Add to existing nodes for child lookups
                        if (!string.IsNullOrEmpty(slug))
                        {
                            existingNodes[slug] = newNode;
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to save category {Name}: {Errors}",
                            category.Name, string.Join(", ", saveResult.EventMessages.GetAll().Select(m => m.Message)));
                        result.Errors.Add($"Failed to save category {category.Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing category {Name}", category.Name);
            result.Errors.Add($"Failed to sync category {category.Name}: {ex.Message}");
        }
    }

    private IContent CreateCategoryNode(int parentId, IContentType categoryDocType, Category category)
    {
        var nodeName = !string.IsNullOrEmpty(category.Name) ? category.Name : "Category";
        var content = _contentService.Create(nodeName, parentId, categoryDocType);

        MapCategoryToContent(content, category);

        return content;
    }

    private void MapCategoryToContent(IContent content, Category category)
    {
        content.Name = category.Name ?? "Category";

        // Content properties
        content.SetValue("categoryName", category.Name ?? "");
        content.SetValue("slug", category.Slug ?? "");
        content.SetValue("description", category.Description ?? "");

        // Settings properties
        content.SetValue("isVisible", category.IsVisible);
        content.SetValue("sortOrder", category.SortOrder);

        // SEO properties
        content.SetValue("metaTitle", category.MetaTitle ?? "");
        content.SetValue("metaDescription", category.MetaDescription ?? "");

        // Media - set image URL if available
        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            content.SetValue("image", category.ImageUrl);
        }
    }

    #endregion
}

/// <summary>
/// Result of a category sync operation
/// </summary>
public class CategorySyncResult
{
    public int TotalCategories { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();

    public bool Success => Errors.Count == 0;
}
