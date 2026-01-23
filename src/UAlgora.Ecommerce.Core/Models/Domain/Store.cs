namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a store in a multi-store e-commerce system.
/// Each store can have its own branding, settings, products, and configuration.
/// </summary>
public class Store : SoftDeleteEntity
{
    /// <summary>
    /// Reference to the Umbraco content node ID.
    /// </summary>
    public int? UmbracoNodeId { get; set; }

    /// <summary>
    /// Unique store code for identification.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Store display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Store description.
    /// </summary>
    public string? Description { get; set; }

    #region Domain & URLs

    /// <summary>
    /// Primary domain for the store.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Additional domains that should resolve to this store.
    /// </summary>
    public List<string> AlternateDomains { get; set; } = [];

    /// <summary>
    /// URL slug if running multiple stores on same domain.
    /// </summary>
    public string? UrlSlug { get; set; }

    #endregion

    #region Branding

    /// <summary>
    /// Store logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Store favicon URL.
    /// </summary>
    public string? FaviconUrl { get; set; }

    /// <summary>
    /// Primary brand color (hex).
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Secondary brand color (hex).
    /// </summary>
    public string? SecondaryColor { get; set; }

    /// <summary>
    /// Accent color (hex).
    /// </summary>
    public string? AccentColor { get; set; }

    #endregion

    #region Contact Information

    /// <summary>
    /// Store contact email.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Support email.
    /// </summary>
    public string? SupportEmail { get; set; }

    /// <summary>
    /// Store phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Physical address line 1.
    /// </summary>
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Physical address line 2.
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Postal/ZIP code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string? CountryCode { get; set; }

    #endregion

    #region Localization

    /// <summary>
    /// Default currency code (ISO 4217).
    /// </summary>
    public string DefaultCurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Supported currency codes.
    /// </summary>
    public List<string> SupportedCurrencies { get; set; } = ["USD"];

    /// <summary>
    /// Default language/culture code.
    /// </summary>
    public string DefaultLanguage { get; set; } = "en-US";

    /// <summary>
    /// Supported language codes.
    /// </summary>
    public List<string> SupportedLanguages { get; set; } = ["en-US"];

    /// <summary>
    /// Time zone ID for the store.
    /// </summary>
    public string TimeZoneId { get; set; } = "UTC";

    #endregion

    #region Settings

    /// <summary>
    /// Whether prices include tax.
    /// </summary>
    public bool TaxIncludedInPrices { get; set; }

    /// <summary>
    /// Default tax zone ID.
    /// </summary>
    public Guid? DefaultTaxZoneId { get; set; }

    /// <summary>
    /// Default shipping zone ID.
    /// </summary>
    public Guid? DefaultShippingZoneId { get; set; }

    /// <summary>
    /// Default warehouse ID.
    /// </summary>
    public Guid? DefaultWarehouseId { get; set; }

    /// <summary>
    /// Minimum order amount for checkout.
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Free shipping threshold.
    /// </summary>
    public decimal? FreeShippingThreshold { get; set; }

    /// <summary>
    /// Allow guest checkout without account.
    /// </summary>
    public bool AllowGuestCheckout { get; set; } = true;

    /// <summary>
    /// Require account for checkout.
    /// </summary>
    public bool RequireEmailVerification { get; set; }

    /// <summary>
    /// Maximum items per cart.
    /// </summary>
    public int? MaxCartItems { get; set; }

    /// <summary>
    /// Days to hold abandoned carts.
    /// </summary>
    public int AbandonedCartRetentionDays { get; set; } = 30;

    /// <summary>
    /// Order number prefix.
    /// </summary>
    public string OrderNumberPrefix { get; set; } = "ORD";

    /// <summary>
    /// Next order number sequence.
    /// </summary>
    public int OrderNumberSequence { get; set; } = 1000;

    #endregion

    #region Social Media

    /// <summary>
    /// Facebook page URL.
    /// </summary>
    public string? FacebookUrl { get; set; }

    /// <summary>
    /// Twitter/X profile URL.
    /// </summary>
    public string? TwitterUrl { get; set; }

    /// <summary>
    /// Instagram profile URL.
    /// </summary>
    public string? InstagramUrl { get; set; }

    /// <summary>
    /// LinkedIn page URL.
    /// </summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// YouTube channel URL.
    /// </summary>
    public string? YouTubeUrl { get; set; }

    /// <summary>
    /// TikTok profile URL.
    /// </summary>
    public string? TikTokUrl { get; set; }

    #endregion

    #region License & Status

    /// <summary>
    /// Store status.
    /// </summary>
    public StoreStatus Status { get; set; } = StoreStatus.Active;

    /// <summary>
    /// License key for this store.
    /// </summary>
    public string? LicenseKey { get; set; }

    /// <summary>
    /// License type.
    /// </summary>
    public LicenseType LicenseType { get; set; } = LicenseType.Trial;

    /// <summary>
    /// Trial expiration date.
    /// </summary>
    public DateTime? TrialExpiresAt { get; set; }

    /// <summary>
    /// License expiration date.
    /// </summary>
    public DateTime? LicenseExpiresAt { get; set; }

    /// <summary>
    /// Last license validation timestamp.
    /// </summary>
    public DateTime? LastLicenseValidation { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Custom metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Store-specific settings as JSON.
    /// </summary>
    public string? SettingsJson { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Products in this store.
    /// </summary>
    public List<Product> Products { get; set; } = [];

    /// <summary>
    /// Categories in this store.
    /// </summary>
    public List<Category> Categories { get; set; } = [];

    /// <summary>
    /// Orders placed in this store.
    /// </summary>
    public List<Order> Orders { get; set; } = [];

    /// <summary>
    /// Customers registered in this store.
    /// </summary>
    public List<Customer> Customers { get; set; } = [];

    /// <summary>
    /// Discounts for this store.
    /// </summary>
    public List<Discount> Discounts { get; set; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether the store is in trial mode.
    /// </summary>
    public bool IsTrial => LicenseType == LicenseType.Trial;

    /// <summary>
    /// Whether the trial has expired.
    /// </summary>
    public bool IsTrialExpired => IsTrial && TrialExpiresAt.HasValue && TrialExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Whether the license is valid.
    /// </summary>
    public bool IsLicenseValid => !string.IsNullOrEmpty(LicenseKey) &&
                                   (!LicenseExpiresAt.HasValue || LicenseExpiresAt > DateTime.UtcNow);

    /// <summary>
    /// Days remaining in trial.
    /// </summary>
    public int? TrialDaysRemaining => IsTrial && TrialExpiresAt.HasValue
        ? Math.Max(0, (TrialExpiresAt.Value - DateTime.UtcNow).Days)
        : null;

    #endregion
}

/// <summary>
/// Store operational status.
/// </summary>
public enum StoreStatus
{
    /// <summary>
    /// Store is active and operational.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Store is in maintenance mode.
    /// </summary>
    Maintenance = 1,

    /// <summary>
    /// Store is suspended (e.g., payment issue).
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Store is closed/disabled.
    /// </summary>
    Closed = 3
}

/// <summary>
/// License type for the store.
/// </summary>
public enum LicenseType
{
    /// <summary>
    /// Trial license (14 days).
    /// </summary>
    Trial = 0,

    /// <summary>
    /// Standard license ($1,500/year).
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Enterprise license ($3,000/year).
    /// </summary>
    Enterprise = 2
}
