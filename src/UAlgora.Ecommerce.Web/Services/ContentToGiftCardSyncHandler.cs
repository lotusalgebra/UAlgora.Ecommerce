using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Notification handler that syncs Umbraco Gift Card content to the database.
/// </summary>
public sealed class ContentToGiftCardSyncHandler :
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly IGiftCardService _giftCardService;
    private readonly ILogger<ContentToGiftCardSyncHandler> _logger;

    public ContentToGiftCardSyncHandler(
        IGiftCardService giftCardService,
        ILogger<ContentToGiftCardSyncHandler> logger)
    {
        _giftCardService = giftCardService;
        _logger = logger;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.PublishedEntities)
        {
            if (!IsAlgoraGiftCard(content))
                continue;

            await SyncGiftCardToDatabaseAsync(content, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.UnpublishedEntities)
        {
            if (!IsAlgoraGiftCard(content))
                continue;

            await UpdateGiftCardStatusAsync(content.Id, GiftCardStatus.Disabled, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.DeletedEntities)
        {
            if (!IsAlgoraGiftCard(content))
                continue;

            await DeleteGiftCardAsync(content.Id, cancellationToken);
        }
    }

    private bool IsAlgoraGiftCard(IContent content)
    {
        return content.ContentType.Alias == AlgoraDocumentTypeConstants.GiftCardAlias;
    }

    private async Task SyncGiftCardToDatabaseAsync(IContent content, CancellationToken ct)
    {
        try
        {
            var code = content.GetValue<string>("code");
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogWarning("Cannot sync gift card without code. Content ID: {ContentId}", content.Id);
                return;
            }

            var existingGiftCard = await _giftCardService.GetByCodeAsync(code, ct);

            if (existingGiftCard != null)
            {
                MapContentToGiftCard(content, existingGiftCard);
                await _giftCardService.UpdateAsync(existingGiftCard, ct);
                _logger.LogInformation("Updated gift card in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
            else
            {
                var newGiftCard = new GiftCard();
                MapContentToGiftCard(content, newGiftCard);
                await _giftCardService.CreateAsync(newGiftCard, ct);
                _logger.LogInformation("Created gift card in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing gift card to database. Content ID: {ContentId}", content.Id);
        }
    }

    private async Task UpdateGiftCardStatusAsync(int contentId, GiftCardStatus status, CancellationToken ct)
    {
        try
        {
            // Find gift card by Umbraco node ID
            var giftCards = await GetAllGiftCardsAsync(ct);
            var giftCard = giftCards.FirstOrDefault(gc => gc.UmbracoNodeId == contentId);
            if (giftCard != null)
            {
                giftCard.Status = status;
                giftCard.IsActive = false;
                await _giftCardService.UpdateAsync(giftCard, ct);
                _logger.LogInformation("Updated gift card status to {Status}: {Code}", status, giftCard.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating gift card status. Content ID: {ContentId}", contentId);
        }
    }

    private async Task DeleteGiftCardAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var giftCards = await GetAllGiftCardsAsync(ct);
            var giftCard = giftCards.FirstOrDefault(gc => gc.UmbracoNodeId == contentId);
            if (giftCard != null)
            {
                await _giftCardService.DeleteAsync(giftCard.Id, ct);
                _logger.LogInformation("Deleted gift card from database: {Code}", giftCard.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting gift card. Content ID: {ContentId}", contentId);
        }
    }

    private async Task<IReadOnlyList<GiftCard>> GetAllGiftCardsAsync(CancellationToken ct)
    {
        // Get gift cards from all stores - we need to find by UmbracoNodeId
        // This is a simplified approach; in production, you might want to cache or index
        return await _giftCardService.GetByStoreAsync(Guid.Empty, ct);
    }

    private void MapContentToGiftCard(IContent content, GiftCard giftCard)
    {
        giftCard.UmbracoNodeId = content.Id;
        giftCard.Code = content.GetValue<string>("code") ?? "";
        giftCard.Name = content.GetValue<string>("name");

        var typeStr = content.GetValue<string>("giftCardType");
        if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<GiftCardType>(typeStr, true, out var cardType))
        {
            giftCard.Type = cardType;
        }

        var statusStr = content.GetValue<string>("status");
        if (!string.IsNullOrEmpty(statusStr) && Enum.TryParse<GiftCardStatus>(statusStr, true, out var status))
        {
            giftCard.Status = status;
        }
        else
        {
            giftCard.Status = GiftCardStatus.Active;
        }

        giftCard.IsActive = content.GetValue<bool>("isActive");

        // Value
        giftCard.InitialValue = content.GetValue<decimal>("initialValue");
        var balance = content.GetValue<decimal>("balance");
        if (balance > 0)
        {
            giftCard.Balance = balance;
        }
        else
        {
            giftCard.Balance = giftCard.InitialValue;
        }
        giftCard.CurrencyCode = content.GetValue<string>("currencyCode") ?? "USD";

        // Recipient
        giftCard.RecipientName = content.GetValue<string>("recipientName");
        giftCard.RecipientEmail = content.GetValue<string>("recipientEmail");
        giftCard.SenderName = content.GetValue<string>("senderName");
        giftCard.Message = content.GetValue<string>("message");

        // Validity
        giftCard.IssuedAt = content.GetValue<DateTime>("issuedAt");
        if (giftCard.IssuedAt == default)
        {
            giftCard.IssuedAt = DateTime.UtcNow;
        }
        giftCard.ValidFrom = GetNullableDateTime(content, "validFrom");
        giftCard.ExpiresAt = GetNullableDateTime(content, "expiresAt");
        giftCard.UsageCount = content.GetValue<int>("usageCount");
        giftCard.LastUsedAt = GetNullableDateTime(content, "lastUsedAt");

        // Restrictions
        giftCard.MinimumOrderAmount = GetNullableDecimal(content, "minimumOrderAmount");
        giftCard.MaxRedemptionPerOrder = GetNullableDecimal(content, "maxRedemptionPerOrder");
        giftCard.CanCombineWithDiscounts = content.GetValue<bool>("canCombineWithDiscounts");
        giftCard.Notes = content.GetValue<string>("notes");
    }

    private static decimal? GetNullableDecimal(IContent content, string alias)
    {
        var value = content.GetValue<decimal>(alias);
        return value == 0 ? null : value;
    }

    private static DateTime? GetNullableDateTime(IContent content, string alias)
    {
        var value = content.GetValue<DateTime>(alias);
        return value == default ? null : value;
    }
}
