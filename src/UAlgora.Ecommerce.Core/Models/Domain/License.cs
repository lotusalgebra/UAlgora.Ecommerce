namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a software license for Algora Commerce.
/// </summary>
public class License : BaseEntity
{
    /// <summary>
    /// Encrypted license key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// License type/tier.
    /// </summary>
    public LicenseType Type { get; set; } = LicenseType.Trial;

    /// <summary>
    /// License status.
    /// </summary>
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;

    #region Customer Information

    /// <summary>
    /// Licensed customer/company name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email address.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer company/organization.
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Customer phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Customer address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Customer country code.
    /// </summary>
    public string? CountryCode { get; set; }

    #endregion

    #region Validity

    /// <summary>
    /// Date license becomes valid.
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Date license expires.
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Grace period in days after expiration.
    /// </summary>
    public int GracePeriodDays { get; set; } = 7;

    /// <summary>
    /// Whether this is a lifetime license.
    /// </summary>
    public bool IsLifetime { get; set; }

    #endregion

    #region Limits

    /// <summary>
    /// Maximum number of stores allowed.
    /// </summary>
    public int? MaxStores { get; set; }

    /// <summary>
    /// Maximum number of products allowed.
    /// </summary>
    public int? MaxProducts { get; set; }

    /// <summary>
    /// Maximum number of orders per month.
    /// </summary>
    public int? MaxOrdersPerMonth { get; set; }

    /// <summary>
    /// Maximum number of admin users.
    /// </summary>
    public int? MaxAdminUsers { get; set; }

    /// <summary>
    /// Maximum storage in MB.
    /// </summary>
    public int? MaxStorageMb { get; set; }

    #endregion

    #region Features

    /// <summary>
    /// Enabled features as JSON array.
    /// </summary>
    public string? EnabledFeaturesJson { get; set; }

    /// <summary>
    /// Disabled features as JSON array.
    /// </summary>
    public string? DisabledFeaturesJson { get; set; }

    /// <summary>
    /// Custom feature flags as JSON object.
    /// </summary>
    public string? FeatureFlagsJson { get; set; }

    #endregion

    #region Environment

    /// <summary>
    /// Licensed domain(s) - comma separated.
    /// </summary>
    public string? LicensedDomains { get; set; }

    /// <summary>
    /// Licensed IP addresses - comma separated.
    /// </summary>
    public string? LicensedIpAddresses { get; set; }

    /// <summary>
    /// Machine/hardware fingerprint for binding.
    /// </summary>
    public string? MachineFingerprint { get; set; }

    /// <summary>
    /// Whether to allow localhost/development.
    /// </summary>
    public bool AllowLocalhost { get; set; } = true;

    #endregion

    #region Validation

    /// <summary>
    /// Last successful validation timestamp.
    /// </summary>
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>
    /// Last validation result.
    /// </summary>
    public LicenseValidationResult LastValidationResult { get; set; } = LicenseValidationResult.Unknown;

    /// <summary>
    /// Number of consecutive validation failures.
    /// </summary>
    public int ConsecutiveValidationFailures { get; set; }

    /// <summary>
    /// Last validation error message.
    /// </summary>
    public string? LastValidationError { get; set; }

    /// <summary>
    /// Validation interval in hours.
    /// </summary>
    public int ValidationIntervalHours { get; set; } = 24;

    #endregion

    #region Subscription

    /// <summary>
    /// Subscription/payment reference.
    /// </summary>
    public string? SubscriptionId { get; set; }

    /// <summary>
    /// Payment processor (Stripe, PayPal, etc.).
    /// </summary>
    public string? PaymentProcessor { get; set; }

    /// <summary>
    /// Renewal amount.
    /// </summary>
    public decimal? RenewalAmount { get; set; }

    /// <summary>
    /// Renewal currency.
    /// </summary>
    public string? RenewalCurrency { get; set; }

    /// <summary>
    /// Whether auto-renewal is enabled.
    /// </summary>
    public bool AutoRenew { get; set; } = true;

    /// <summary>
    /// Next renewal date.
    /// </summary>
    public DateTime? NextRenewalDate { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Notes about the license.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Custom metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// License signature for tamper detection.
    /// </summary>
    public string? Signature { get; set; }

    #endregion

    #region Activation

    /// <summary>
    /// Number of activations.
    /// </summary>
    public int ActivationCount { get; set; }

    /// <summary>
    /// Maximum allowed activations.
    /// </summary>
    public int MaxActivations { get; set; } = 1;

    /// <summary>
    /// First activation date.
    /// </summary>
    public DateTime? FirstActivatedAt { get; set; }

    /// <summary>
    /// Last activation date.
    /// </summary>
    public DateTime? LastActivatedAt { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether the license is valid.
    /// </summary>
    public bool IsValid => Status == LicenseStatus.Active &&
                           ValidFrom <= DateTime.UtcNow &&
                           (!ValidUntil.HasValue || ValidUntil.Value.AddDays(GracePeriodDays) >= DateTime.UtcNow);

    /// <summary>
    /// Whether the license is expired.
    /// </summary>
    public bool IsExpired => ValidUntil.HasValue && ValidUntil < DateTime.UtcNow;

    /// <summary>
    /// Whether the license is in grace period.
    /// </summary>
    public bool IsInGracePeriod => IsExpired && ValidUntil.HasValue &&
                                    ValidUntil.Value.AddDays(GracePeriodDays) >= DateTime.UtcNow;

    /// <summary>
    /// Days until expiration (null if lifetime).
    /// </summary>
    public int? DaysUntilExpiration => ValidUntil.HasValue
        ? Math.Max(0, (ValidUntil.Value - DateTime.UtcNow).Days)
        : null;

    /// <summary>
    /// Whether validation is required.
    /// </summary>
    public bool NeedsValidation => !LastValidatedAt.HasValue ||
                                    LastValidatedAt.Value.AddHours(ValidationIntervalHours) < DateTime.UtcNow;

    #endregion
}

/// <summary>
/// License status.
/// </summary>
public enum LicenseStatus
{
    /// <summary>
    /// License is active and valid.
    /// </summary>
    Active = 0,

    /// <summary>
    /// License is expired.
    /// </summary>
    Expired = 1,

    /// <summary>
    /// License is suspended (e.g., payment issue).
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// License has been revoked.
    /// </summary>
    Revoked = 3,

    /// <summary>
    /// License is in grace period.
    /// </summary>
    GracePeriod = 4,

    /// <summary>
    /// License is pending activation.
    /// </summary>
    PendingActivation = 5
}

/// <summary>
/// License validation result.
/// </summary>
public enum LicenseValidationResult
{
    /// <summary>
    /// Unknown/not validated.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// License is valid.
    /// </summary>
    Valid = 1,

    /// <summary>
    /// License is expired.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// License key is invalid.
    /// </summary>
    InvalidKey = 3,

    /// <summary>
    /// Domain not licensed.
    /// </summary>
    DomainNotLicensed = 4,

    /// <summary>
    /// License has been revoked.
    /// </summary>
    Revoked = 5,

    /// <summary>
    /// License is suspended.
    /// </summary>
    Suspended = 6,

    /// <summary>
    /// Validation server error.
    /// </summary>
    ServerError = 7,

    /// <summary>
    /// Network error during validation.
    /// </summary>
    NetworkError = 8,

    /// <summary>
    /// Signature verification failed.
    /// </summary>
    SignatureInvalid = 9,

    /// <summary>
    /// License has been tampered with.
    /// </summary>
    Tampered = 10,

    /// <summary>
    /// Maximum activations reached.
    /// </summary>
    MaxActivationsReached = 11,

    /// <summary>
    /// License is in grace period.
    /// </summary>
    GracePeriod = 12
}

/// <summary>
/// License features that can be enabled/disabled.
/// </summary>
public static class LicenseFeatures
{
    public const string MultiStore = "multi_store";
    public const string GiftCards = "gift_cards";
    public const string Returns = "returns";
    public const string EmailTemplates = "email_templates";
    public const string AdvancedReporting = "advanced_reporting";
    public const string AuditLogging = "audit_logging";
    public const string Webhooks = "webhooks";
    public const string ApiAccess = "api_access";
    public const string CustomIntegrations = "custom_integrations";
    public const string WhiteLabeling = "white_labeling";
    public const string PrioritySupport = "priority_support";
    public const string AdvancedDiscounts = "advanced_discounts";
    public const string B2BFeatures = "b2b_features";
    public const string Subscriptions = "subscriptions";
    public const string MultiCurrency = "multi_currency";
    public const string MultiLanguage = "multi_language";
    public const string AdvancedInventory = "advanced_inventory";
    public const string AdvancedShipping = "advanced_shipping";
    public const string AdvancedTax = "advanced_tax";
    public const string ImportExport = "import_export";
}
