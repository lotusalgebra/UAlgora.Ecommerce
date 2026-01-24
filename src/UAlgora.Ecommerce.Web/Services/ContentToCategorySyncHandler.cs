using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Notification handler that syncs Umbraco content tree categories to the category database.
/// When a category is published/updated in the Umbraco content tree, this handler
/// ensures it appears in the Algora Commerce category dashboard.
/// </summary>
public sealed class ContentToCategorySyncHandler :
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly ICategoryService _categoryService;
    private readonly IContentService _contentService;
    private readonly ILogger<ContentToCategorySyncHandler> _logger;

    public ContentToCategorySyncHandler(
        ICategoryService categoryService,
        IContentService contentService,
        ILogger<ContentToCategorySyncHandler> logger)
    {
        _categoryService = categoryService;
        _contentService = contentService;
        _logger = logger;
    }

    /// <summary>
    /// Handle content published - create or update category in database
    /// </summary>
    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.PublishedEntities)
        {
            if (!IsAlgoraCategory(content))
                continue;

            await SyncCategoryToDatabaseAsync(content, cancellationToken);
        }
    }

    /// <summary>
    /// Handle content unpublished - mark category as not visible in database
    /// </summary>
    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.UnpublishedEntities)
        {
            if (!IsAlgoraCategory(content))
                continue;

            await UpdateCategoryVisibilityAsync(content.Id, false, cancellationToken);
        }
    }

    /// <summary>
    /// Handle content deleted - soft delete category in database
    /// </summary>
    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.DeletedEntities)
        {
            if (!IsAlgoraCategory(content))
                continue;

            await DeleteCategoryAsync(content.Id, cancellationToken);
        }
    }

    private bool IsAlgoraCategory(IContent content)
    {
        return content.ContentType.Alias == AlgoraDocumentTypeConstants.CategoryAlias;
    }

    private async Task SyncCategoryToDatabaseAsync(IContent content, CancellationToken ct)
    {
        try
        {
            var name = content.GetValue<string>("categoryName") ?? content.Name ?? "";
            var slug = content.GetValue<string>("slug");

            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Cannot sync category without name. Content ID: {ContentId}", content.Id);
                return;
            }

            // Generate slug if not provided
            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = GenerateSlug(name);
            }

            // Check if category already exists by Umbraco node ID or slug
            var existingCategory = await FindCategoryByNodeIdOrSlugAsync(content.Id, slug, ct);

            if (existingCategory != null)
            {
                // Update existing category
                MapContentToCategory(content, existingCategory);
                await _categoryService.UpdateAsync(existingCategory, ct);
                _logger.LogInformation("Updated category in database: {Name} (Umbraco Node: {NodeId})", name, content.Id);
            }
            else
            {
                // Create new category
                var newCategory = new Category();
                MapContentToCategory(content, newCategory);
                await _categoryService.CreateAsync(newCategory, ct);
                _logger.LogInformation("Created category in database: {Name} (Umbraco Node: {NodeId})", name, content.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing category to database. Content ID: {ContentId}", content.Id);
        }
    }

    private async Task<Category?> FindCategoryByNodeIdOrSlugAsync(int nodeId, string? slug, CancellationToken ct)
    {
        // First try to find by Umbraco node ID
        var allCategories = await _categoryService.GetAllAsync(ct);
        var category = allCategories.FirstOrDefault(c => c.UmbracoNodeId == nodeId);

        if (category != null)
            return category;

        // If not found by node ID and slug is provided, try to find by slug
        if (!string.IsNullOrWhiteSpace(slug))
        {
            return await _categoryService.GetBySlugAsync(slug, ct);
        }

        return null;
    }

    private async Task UpdateCategoryVisibilityAsync(int contentId, bool isVisible, CancellationToken ct)
    {
        try
        {
            var allCategories = await _categoryService.GetAllAsync(ct);
            var category = allCategories.FirstOrDefault(c => c.UmbracoNodeId == contentId);

            if (category != null)
            {
                category.IsVisible = isVisible;
                await _categoryService.UpdateAsync(category, ct);
                _logger.LogInformation("Updated category visibility to {IsVisible}: {Name}", isVisible, category.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category visibility. Content ID: {ContentId}", contentId);
        }
    }

    private async Task DeleteCategoryAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var allCategories = await _categoryService.GetAllAsync(ct);
            var category = allCategories.FirstOrDefault(c => c.UmbracoNodeId == contentId);

            if (category != null)
            {
                await _categoryService.DeleteAsync(category.Id, ct);
                _logger.LogInformation("Deleted category from database: {Name}", category.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category. Content ID: {ContentId}", contentId);
        }
    }

    private void MapContentToCategory(IContent content, Category category)
    {
        // Set Umbraco Node ID for reverse lookup
        category.UmbracoNodeId = content.Id;

        // Content properties
        category.Name = content.GetValue<string>("categoryName") ?? content.Name ?? "";
        category.Slug = content.GetValue<string>("slug") ?? GenerateSlug(category.Name);
        category.Description = content.GetValue<string>("description");

        // Settings properties
        category.IsVisible = content.GetValue<bool>("isVisible");
        category.SortOrder = content.GetValue<int>("sortOrder");

        // SEO properties
        category.MetaTitle = content.GetValue<string>("metaTitle");
        category.MetaDescription = content.GetValue<string>("metaDescription");

        // Media - store as URL string
        var imageValue = content.GetValue<string>("image");
        if (!string.IsNullOrWhiteSpace(imageValue))
        {
            category.ImageUrl = imageValue;
        }

        // Handle parent category relationship
        // Look up parent content to find parent category
        if (content.ParentId > 0)
        {
            var parentContent = _contentService.GetById(content.ParentId);
            if (parentContent != null && parentContent.ContentType.Alias == AlgoraDocumentTypeConstants.CategoryAlias)
            {
                // Find the parent category by Umbraco node ID
                var allCategories = _categoryService.GetAllAsync().GetAwaiter().GetResult();
                var parentCategory = allCategories.FirstOrDefault(c => c.UmbracoNodeId == content.ParentId);

                if (parentCategory != null)
                {
                    category.ParentId = parentCategory.Id;
                }
            }
            else
            {
                // Parent is not a category (could be Catalog), so this is a root category
                category.ParentId = null;
            }
        }

        // Calculate level based on parent
        category.Level = category.ParentId.HasValue ? GetCategoryLevel(category.ParentId.Value) + 1 : 0;

        // Build path
        category.Path = BuildCategoryPath(category);
    }

    private int GetCategoryLevel(Guid parentId)
    {
        var parent = _categoryService.GetByIdAsync(parentId).GetAwaiter().GetResult();
        return parent?.Level ?? 0;
    }

    private string BuildCategoryPath(Category category)
    {
        var path = category.Name;
        var parentId = category.ParentId;

        while (parentId.HasValue)
        {
            var parent = _categoryService.GetByIdAsync(parentId.Value).GetAwaiter().GetResult();
            if (parent == null) break;

            path = $"{parent.Name}/{path}";
            parentId = parent.ParentId;
        }

        return path;
    }

    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        // Convert to lowercase, replace spaces with hyphens, remove special characters
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");

        // Remove any characters that aren't alphanumeric or hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove multiple consecutive hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from start and end
        return slug.Trim('-');
    }
}
