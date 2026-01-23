using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Notification handler that syncs Umbraco Discount content to the database.
/// </summary>
public sealed class ContentToDiscountSyncHandler :
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<ContentToDiscountSyncHandler> _logger;

    public ContentToDiscountSyncHandler(
        IDiscountService discountService,
        ILogger<ContentToDiscountSyncHandler> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.PublishedEntities)
        {
            if (!IsAlgoraDiscount(content))
                continue;

            await SyncDiscountToDatabaseAsync(content, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.UnpublishedEntities)
        {
            if (!IsAlgoraDiscount(content))
                continue;

            await DeactivateDiscountAsync(content.Id, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.DeletedEntities)
        {
            if (!IsAlgoraDiscount(content))
                continue;

            await DeleteDiscountAsync(content.Id, cancellationToken);
        }
    }

    private bool IsAlgoraDiscount(IContent content)
    {
        return content.ContentType.Alias == AlgoraDocumentTypeConstants.DiscountAlias;
    }

    private async Task SyncDiscountToDatabaseAsync(IContent content, CancellationToken ct)
    {
        try
        {
            var code = content.GetValue<string>("code");
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogWarning("Cannot sync discount without code. Content ID: {ContentId}", content.Id);
                return;
            }

            var existingDiscount = await _discountService.GetByCodeAsync(code, ct);

            if (existingDiscount != null)
            {
                MapContentToDiscount(content, existingDiscount);
                await _discountService.UpdateAsync(existingDiscount, ct);
                _logger.LogInformation("Updated discount in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
            else
            {
                var newDiscount = new Discount();
                MapContentToDiscount(content, newDiscount);
                await _discountService.CreateAsync(newDiscount, ct);
                _logger.LogInformation("Created discount in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing discount to database. Content ID: {ContentId}", content.Id);
        }
    }

    private async Task DeactivateDiscountAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var discounts = await _discountService.GetActiveAsync(ct);
            var discount = discounts.FirstOrDefault(d => d.UmbracoNodeId == contentId);
            if (discount != null)
            {
                discount.IsActive = false;
                await _discountService.UpdateAsync(discount, ct);
                _logger.LogInformation("Deactivated discount: {Code}", discount.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating discount. Content ID: {ContentId}", contentId);
        }
    }

    private async Task DeleteDiscountAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var discounts = await _discountService.GetActiveAsync(ct);
            var discount = discounts.FirstOrDefault(d => d.UmbracoNodeId == contentId);
            if (discount != null)
            {
                await _discountService.DeleteAsync(discount.Id, ct);
                _logger.LogInformation("Deleted discount from database: {Code}", discount.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discount. Content ID: {ContentId}", contentId);
        }
    }

    private void MapContentToDiscount(IContent content, Discount discount)
    {
        discount.UmbracoNodeId = content.Id;
        discount.Name = content.GetValue<string>("discountName") ?? "";
        discount.Description = content.GetValue<string>("description");
        discount.Code = content.GetValue<string>("code") ?? "";
        discount.IsActive = content.GetValue<bool>("isActive");

        // Value
        var typeStr = content.GetValue<string>("discountType");
        if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<DiscountType>(typeStr, true, out var discountType))
        {
            discount.Type = discountType;
        }

        var scopeStr = content.GetValue<string>("discountScope");
        if (!string.IsNullOrEmpty(scopeStr) && Enum.TryParse<DiscountScope>(scopeStr, true, out var scope))
        {
            discount.Scope = scope;
        }

        discount.Value = content.GetValue<decimal>("discountValue");
        discount.MaxDiscountAmount = GetNullableDecimal(content, "maxDiscountAmount");

        // Conditions
        discount.MinimumOrderAmount = GetNullableDecimal(content, "minimumOrderAmount");
        discount.MinimumQuantity = GetNullableInt(content, "minimumQuantity");
        discount.FirstTimeCustomerOnly = content.GetValue<bool>("firstTimeCustomerOnly");
        discount.ExcludeSaleItems = content.GetValue<bool>("excludeSaleItems");

        var tiersStr = content.GetValue<string>("eligibleCustomerTiers");
        if (!string.IsNullOrEmpty(tiersStr))
        {
            discount.EligibleCustomerTiers = tiersStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        // Limits
        discount.TotalUsageLimit = GetNullableInt(content, "totalUsageLimit");
        discount.PerCustomerLimit = GetNullableInt(content, "perCustomerLimit");
        discount.UsageCount = content.GetValue<int>("usageCount");
        discount.CanCombine = content.GetValue<bool>("canCombine");
        discount.Priority = content.GetValue<int>("priority");

        // Validity
        discount.StartDate = GetNullableDateTime(content, "startDate");
        discount.EndDate = GetNullableDateTime(content, "endDate");
    }

    private static decimal? GetNullableDecimal(IContent content, string alias)
    {
        var value = content.GetValue<decimal>(alias);
        return value == 0 ? null : value;
    }

    private static int? GetNullableInt(IContent content, string alias)
    {
        var value = content.GetValue<int>(alias);
        return value == 0 ? null : value;
    }

    private static DateTime? GetNullableDateTime(IContent content, string alias)
    {
        var value = content.GetValue<DateTime>(alias);
        return value == default ? null : value;
    }
}
