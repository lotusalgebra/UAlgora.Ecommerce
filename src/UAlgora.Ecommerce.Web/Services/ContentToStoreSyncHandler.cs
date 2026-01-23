using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Notification handler that syncs Umbraco Store content to the store database.
/// </summary>
public sealed class ContentToStoreSyncHandler :
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly IStoreService _storeService;
    private readonly ILogger<ContentToStoreSyncHandler> _logger;

    public ContentToStoreSyncHandler(
        IStoreService storeService,
        ILogger<ContentToStoreSyncHandler> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.PublishedEntities)
        {
            if (!IsAlgoraStore(content))
                continue;

            await SyncStoreToDatabaseAsync(content, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.UnpublishedEntities)
        {
            if (!IsAlgoraStore(content))
                continue;

            await UpdateStoreStatusAsync(content.Id, StoreStatus.Maintenance, cancellationToken);
        }
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var content in notification.DeletedEntities)
        {
            if (!IsAlgoraStore(content))
                continue;

            await DeleteStoreAsync(content.Id, cancellationToken);
        }
    }

    private bool IsAlgoraStore(IContent content)
    {
        return content.ContentType.Alias == AlgoraDocumentTypeConstants.StoreAlias;
    }

    private async Task SyncStoreToDatabaseAsync(IContent content, CancellationToken ct)
    {
        try
        {
            var code = content.GetValue<string>("storeCode");
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogWarning("Cannot sync store without code. Content ID: {ContentId}", content.Id);
                return;
            }

            var existingStore = await _storeService.GetByCodeAsync(code, ct);

            if (existingStore != null)
            {
                MapContentToStore(content, existingStore);
                await _storeService.UpdateAsync(existingStore, ct);
                _logger.LogInformation("Updated store in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
            else
            {
                var newStore = new Store();
                MapContentToStore(content, newStore);
                await _storeService.CreateAsync(newStore, ct);
                _logger.LogInformation("Created store in database: {Code} (Umbraco Node: {NodeId})", code, content.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing store to database. Content ID: {ContentId}", content.Id);
        }
    }

    private async Task UpdateStoreStatusAsync(int contentId, StoreStatus status, CancellationToken ct)
    {
        try
        {
            var stores = await _storeService.GetAllAsync(ct);
            var store = stores.FirstOrDefault(s => s.UmbracoNodeId == contentId);
            if (store != null)
            {
                store.Status = status;
                await _storeService.UpdateAsync(store, ct);
                _logger.LogInformation("Updated store status to {Status}: {Code}", status, store.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating store status. Content ID: {ContentId}", contentId);
        }
    }

    private async Task DeleteStoreAsync(int contentId, CancellationToken ct)
    {
        try
        {
            var stores = await _storeService.GetAllAsync(ct);
            var store = stores.FirstOrDefault(s => s.UmbracoNodeId == contentId);
            if (store != null)
            {
                await _storeService.DeleteAsync(store.Id, ct);
                _logger.LogInformation("Deleted store from database: {Code}", store.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting store. Content ID: {ContentId}", contentId);
        }
    }

    private void MapContentToStore(IContent content, Store store)
    {
        store.UmbracoNodeId = content.Id;
        store.Code = content.GetValue<string>("storeCode") ?? "";
        store.Name = content.GetValue<string>("storeName") ?? content.Name ?? "";
        store.Description = content.GetValue<string>("description");
        store.Domain = content.GetValue<string>("domain");
        store.UrlSlug = content.GetValue<string>("urlSlug");

        // Branding
        store.LogoUrl = content.GetValue<string>("logo");
        store.FaviconUrl = content.GetValue<string>("favicon");
        store.PrimaryColor = content.GetValue<string>("primaryColor");
        store.SecondaryColor = content.GetValue<string>("secondaryColor");
        store.AccentColor = content.GetValue<string>("accentColor");

        // Contact
        store.ContactEmail = content.GetValue<string>("contactEmail");
        store.SupportEmail = content.GetValue<string>("supportEmail");
        store.Phone = content.GetValue<string>("phone");
        store.AddressLine1 = content.GetValue<string>("addressLine1");
        store.AddressLine2 = content.GetValue<string>("addressLine2");
        store.City = content.GetValue<string>("city");
        store.State = content.GetValue<string>("state");
        store.PostalCode = content.GetValue<string>("postalCode");
        store.CountryCode = content.GetValue<string>("countryCode");

        // Localization
        store.DefaultCurrencyCode = content.GetValue<string>("defaultCurrency") ?? "USD";
        var supportedCurrencies = content.GetValue<string>("supportedCurrencies");
        if (!string.IsNullOrEmpty(supportedCurrencies))
        {
            store.SupportedCurrencies = supportedCurrencies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
        store.DefaultLanguage = content.GetValue<string>("defaultLanguage") ?? "en-US";
        var supportedLanguages = content.GetValue<string>("supportedLanguages");
        if (!string.IsNullOrEmpty(supportedLanguages))
        {
            store.SupportedLanguages = supportedLanguages.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
        store.TimeZoneId = content.GetValue<string>("timezone") ?? "UTC";
        store.TaxIncludedInPrices = content.GetValue<bool>("taxIncludedInPrices");

        // Checkout
        store.AllowGuestCheckout = content.GetValue<bool>("allowGuestCheckout");
        store.MinimumOrderAmount = GetNullableDecimal(content, "minimumOrderAmount");
        store.FreeShippingThreshold = GetNullableDecimal(content, "freeShippingThreshold");
        store.OrderNumberPrefix = content.GetValue<string>("orderNumberPrefix") ?? "ORD";
        store.MaxCartItems = GetNullableInt(content, "maxCartItems");

        // Social
        store.FacebookUrl = content.GetValue<string>("facebook");
        store.TwitterUrl = content.GetValue<string>("twitter");
        store.InstagramUrl = content.GetValue<string>("instagram");
        store.YouTubeUrl = content.GetValue<string>("youtube");
        store.LinkedInUrl = content.GetValue<string>("linkedin");
        store.TikTokUrl = content.GetValue<string>("tiktok");

        // License
        store.LicenseKey = content.GetValue<string>("licenseKey");
        var licenseTypeStr = content.GetValue<string>("licenseType");
        if (!string.IsNullOrEmpty(licenseTypeStr) && Enum.TryParse<LicenseType>(licenseTypeStr, true, out var licenseType))
        {
            store.LicenseType = licenseType;
        }

        var statusStr = content.GetValue<string>("storeStatus");
        if (!string.IsNullOrEmpty(statusStr) && Enum.TryParse<StoreStatus>(statusStr, true, out var status))
        {
            store.Status = status;
        }
        else
        {
            // If isActive is false, set to Maintenance
            var isActive = content.GetValue<bool>("isActive");
            store.Status = isActive ? StoreStatus.Active : StoreStatus.Maintenance;
        }
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
}
