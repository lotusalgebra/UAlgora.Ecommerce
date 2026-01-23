namespace UAlgora.Ecommerce.Web.DocumentTypes.Providers;

/// <summary>
/// Constants for Algora Commerce document types.
/// Centralized location for all aliases and configuration values.
/// </summary>
public static class AlgoraDocumentTypeConstants
{
    /// <summary>
    /// Brand prefix for all Algora document types
    /// </summary>
    public const string BrandPrefix = "algora";

    /// <summary>
    /// Catalog (Shop) document type alias - the root container
    /// </summary>
    public const string CatalogAlias = "algoraCatalog";

    /// <summary>
    /// Product document type alias
    /// </summary>
    public const string ProductAlias = "algoraProduct";

    /// <summary>
    /// Category document type alias
    /// </summary>
    public const string CategoryAlias = "algoraCategory";

    /// <summary>
    /// Order document type alias
    /// </summary>
    public const string OrderAlias = "algoraOrder";

    /// <summary>
    /// Checkout Step document type alias
    /// </summary>
    public const string CheckoutStepAlias = "algoraCheckoutStep";

    // ============ CMS-Driven Storefront Document Types ============

    /// <summary>
    /// Site Settings document type alias - global site configuration
    /// </summary>
    public const string SiteSettingsAlias = "algoraSiteSettings";

    /// <summary>
    /// Home Page document type alias
    /// </summary>
    public const string HomePageAlias = "algoraHomePage";

    /// <summary>
    /// Hero Slide document type alias - for carousel slides
    /// </summary>
    public const string HeroSlideAlias = "algoraHeroSlide";

    /// <summary>
    /// Banner document type alias - promotional banners
    /// </summary>
    public const string BannerAlias = "algoraBanner";

    /// <summary>
    /// Testimonial document type alias
    /// </summary>
    public const string TestimonialAlias = "algoraTestimonial";

    /// <summary>
    /// Feature/USP document type alias
    /// </summary>
    public const string FeatureAlias = "algoraFeature";

    /// <summary>
    /// Content Page document type alias - generic pages
    /// </summary>
    public const string ContentPageAlias = "algoraContentPage";

    // ============ Enterprise E-Commerce Document Types ============

    /// <summary>
    /// Store document type alias - multi-store support
    /// </summary>
    public const string StoreAlias = "algoraStore";

    /// <summary>
    /// Gift Card document type alias
    /// </summary>
    public const string GiftCardAlias = "algoraGiftCard";

    /// <summary>
    /// Email Template document type alias
    /// </summary>
    public const string EmailTemplateAlias = "algoraEmailTemplate";

    /// <summary>
    /// Currency document type alias
    /// </summary>
    public const string CurrencyAlias = "algoraCurrency";

    /// <summary>
    /// Country document type alias
    /// </summary>
    public const string CountryAlias = "algoraCountry";

    /// <summary>
    /// Shipping Zone document type alias
    /// </summary>
    public const string ShippingZoneAlias = "algoraShippingZone";

    /// <summary>
    /// Shipping Method document type alias
    /// </summary>
    public const string ShippingMethodAlias = "algoraShippingMethod";

    /// <summary>
    /// Tax Zone document type alias
    /// </summary>
    public const string TaxZoneAlias = "algoraTaxZone";

    /// <summary>
    /// Tax Rate document type alias
    /// </summary>
    public const string TaxRateAlias = "algoraTaxRate";

    /// <summary>
    /// Payment Method document type alias
    /// </summary>
    public const string PaymentMethodAlias = "algoraPaymentMethod";

    /// <summary>
    /// Discount document type alias
    /// </summary>
    public const string DiscountAlias = "algoraDiscount";

    /// <summary>
    /// Warehouse document type alias
    /// </summary>
    public const string WarehouseAlias = "algoraWarehouse";

    /// <summary>
    /// Webhook document type alias
    /// </summary>
    public const string WebhookAlias = "algoraWebhook";

    /// <summary>
    /// Default icon for commerce-related document types
    /// </summary>
    public const string DefaultIcon = "icon-store";

    /// <summary>
    /// Default icon color for Algora branding (professional blue)
    /// </summary>
    public const string BrandColor = "color-blue";

    // Professional E-Commerce Icons
    public const string ProductIcon = "icon-barcode";
    public const string CategoryIcon = "icon-categories";
    public const string OrderIcon = "icon-invoice";
    public const string CheckoutIcon = "icon-directions-alt";
    public const string CartIcon = "icon-shopping-basket";
    public const string PaymentIcon = "icon-credit-card";
    public const string ShippingIcon = "icon-truck";
    public const string CustomerIcon = "icon-user";
    public const string HomeIcon = "icon-home";
    public const string SliderIcon = "icon-slideshow";
    public const string BannerIcon = "icon-billboard";
    public const string TestimonialIcon = "icon-quote";
    public const string FeatureIcon = "icon-wand";
    public const string SettingsIcon = "icon-settings";
    public const string PageIcon = "icon-document";

    // Enterprise Icons
    public const string StoreIcon = "icon-store";
    public const string GiftCardIcon = "icon-gift";
    public const string EmailIcon = "icon-message";
    public const string CurrencyIcon = "icon-coins-euro";
    public const string CountryIcon = "icon-globe";
    public const string TaxIcon = "icon-receipt-dollar";
    public const string DiscountIcon = "icon-tag";
    public const string WarehouseIcon = "icon-box";
    public const string WebhookIcon = "icon-link";

    /// <summary>
    /// Property group prefix for Algora
    /// </summary>
    public static string GetGroupAlias(string documentType, string groupName)
        => $"{BrandPrefix}{char.ToUpper(documentType[0])}{documentType[1..].ToLower()}{groupName}";
}
