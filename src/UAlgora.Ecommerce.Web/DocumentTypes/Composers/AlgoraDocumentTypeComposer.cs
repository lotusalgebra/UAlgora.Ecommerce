using Microsoft.Extensions.DependencyInjection;
using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using UAlgora.Ecommerce.Web.DocumentTypes.Services;
using UAlgora.Ecommerce.Web.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Composers;

/// <summary>
/// Composer that registers all Algora Commerce document type services.
/// Follows Clean Architecture - centralizes DI registration.
/// </summary>
public sealed class AlgoraDocumentTypeComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register services
        builder.Services.AddSingleton<IDataTypeResolver, DataTypeResolver>();
        builder.Services.AddSingleton<IDocumentTypeInstaller, DocumentTypeInstaller>();

        // Register document type definition providers
        // Using IEnumerable<IDocumentTypeDefinitionProvider> injection pattern
        // Order matters: Settings first, then pages, then components

        // Multi-Store Support (Root level)
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, StoreDocumentTypeProvider>();

        // Site Settings & Pages
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, SiteSettingsDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, HomePageDocumentTypeProvider>();

        // Commerce Document Types
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, CatalogDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, CategoryDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, ProductDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, OrderDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, CheckoutStepDocumentTypeProvider>();

        // CMS Components (children of Home Page)
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, HeroSlideDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, BannerDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, TestimonialDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, FeatureDocumentTypeProvider>();

        // Enterprise Commerce Features
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, GiftCardDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, EmailTemplateDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, DiscountDocumentTypeProvider>();
        builder.Services.AddSingleton<IDocumentTypeDefinitionProvider, WebhookDocumentTypeProvider>();

        // Register startup notification handler
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, AlgoraDocumentTypeStartupHandler>();

        // Register content-to-product sync handlers
        // When products are published/unpublished/deleted in the content tree, sync to product database
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentToProductSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentToProductSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentToProductSyncHandler>();

        // Register content-to-store sync handlers
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentToStoreSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentToStoreSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentToStoreSyncHandler>();

        // Register content-to-gift-card sync handlers
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentToGiftCardSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentToGiftCardSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentToGiftCardSyncHandler>();

        // Register content-to-discount sync handlers
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentToDiscountSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentToDiscountSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentToDiscountSyncHandler>();

        // Register content-to-email-template sync handlers
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentToEmailTemplateSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentToEmailTemplateSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentToEmailTemplateSyncHandler>();

        // Register content-to-webhook sync handlers
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentToWebhookSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentToWebhookSyncHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentToWebhookSyncHandler>();

        // Register content-to-category sync handlers
        // When categories are published/unpublished/deleted in the content tree, sync to category database
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentToCategorySyncHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentToCategorySyncHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, ContentToCategorySyncHandler>();
    }
}

/// <summary>
/// Handles Umbraco startup to install/update document types.
/// Separates startup logic from composition for cleaner architecture.
/// </summary>
public sealed class AlgoraDocumentTypeStartupHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IDocumentTypeInstaller _installer;

    public AlgoraDocumentTypeStartupHandler(IDocumentTypeInstaller installer)
    {
        _installer = installer;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        _installer.InstallAll();
    }
}
