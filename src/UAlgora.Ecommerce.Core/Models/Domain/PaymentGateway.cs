namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a payment gateway/provider configuration.
/// </summary>
public class PaymentGateway : SoftDeleteEntity
{
    /// <summary>
    /// Gateway display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique code for this gateway.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Provider type identifier.
    /// </summary>
    public PaymentProviderType ProviderType { get; set; }

    /// <summary>
    /// Description of the gateway.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this gateway is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether to use sandbox/test mode.
    /// </summary>
    public bool IsSandbox { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    #region API Credentials

    /// <summary>
    /// API key or publishable key.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Secret key or private key (encrypted).
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Merchant ID or account ID.
    /// </summary>
    public string? MerchantId { get; set; }

    /// <summary>
    /// Client ID (for OAuth providers).
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Client secret (for OAuth providers).
    /// </summary>
    public string? ClientSecret { get; set; }

    #endregion

    #region Sandbox Credentials

    /// <summary>
    /// Sandbox API key.
    /// </summary>
    public string? SandboxApiKey { get; set; }

    /// <summary>
    /// Sandbox secret key (encrypted).
    /// </summary>
    public string? SandboxSecretKey { get; set; }

    /// <summary>
    /// Sandbox merchant ID.
    /// </summary>
    public string? SandboxMerchantId { get; set; }

    #endregion

    #region Webhook Settings

    /// <summary>
    /// Webhook endpoint URL.
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Webhook signing secret.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Sandbox webhook secret.
    /// </summary>
    public string? SandboxWebhookSecret { get; set; }

    /// <summary>
    /// Whether webhooks are enabled.
    /// </summary>
    public bool WebhooksEnabled { get; set; } = true;

    #endregion

    #region Configuration

    /// <summary>
    /// Supported currencies.
    /// </summary>
    public List<string> SupportedCurrencies { get; set; } = [];

    /// <summary>
    /// Supported countries.
    /// </summary>
    public List<string> SupportedCountries { get; set; } = [];

    /// <summary>
    /// Supported payment methods.
    /// </summary>
    public List<string> SupportedPaymentMethods { get; set; } = [];

    /// <summary>
    /// Additional provider-specific settings (JSON).
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = [];

    #endregion

    #region Stripe-Specific Settings

    /// <summary>
    /// Stripe statement descriptor.
    /// </summary>
    public string? StatementDescriptor { get; set; }

    /// <summary>
    /// Stripe statement descriptor suffix.
    /// </summary>
    public string? StatementDescriptorSuffix { get; set; }

    #endregion

    #region PayPal-Specific Settings

    /// <summary>
    /// PayPal brand name shown in checkout.
    /// </summary>
    public string? BrandName { get; set; }

    /// <summary>
    /// PayPal landing page type.
    /// </summary>
    public string? LandingPage { get; set; }

    /// <summary>
    /// PayPal user action preference.
    /// </summary>
    public string? UserAction { get; set; }

    #endregion

    /// <summary>
    /// Payment methods using this gateway.
    /// </summary>
    public List<PaymentMethodConfig> PaymentMethods { get; set; } = [];

    #region Computed Properties

    /// <summary>
    /// Gets the active API key based on mode.
    /// </summary>
    public string? ActiveApiKey => IsSandbox ? SandboxApiKey : ApiKey;

    /// <summary>
    /// Gets the active secret key based on mode.
    /// </summary>
    public string? ActiveSecretKey => IsSandbox ? SandboxSecretKey : SecretKey;

    /// <summary>
    /// Gets the active merchant ID based on mode.
    /// </summary>
    public string? ActiveMerchantId => IsSandbox ? SandboxMerchantId : MerchantId;

    /// <summary>
    /// Gets the active webhook secret based on mode.
    /// </summary>
    public string? ActiveWebhookSecret => IsSandbox ? SandboxWebhookSecret : WebhookSecret;

    /// <summary>
    /// Whether the gateway is properly configured.
    /// </summary>
    public bool IsConfigured => ProviderType switch
    {
        PaymentProviderType.Stripe => !string.IsNullOrEmpty(ActiveApiKey) && !string.IsNullOrEmpty(ActiveSecretKey),
        PaymentProviderType.PayPal => !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret),
        PaymentProviderType.Square => !string.IsNullOrEmpty(ActiveApiKey),
        PaymentProviderType.Braintree => !string.IsNullOrEmpty(ActiveMerchantId) && !string.IsNullOrEmpty(ActiveApiKey),
        PaymentProviderType.AuthorizeNet => !string.IsNullOrEmpty(ActiveApiKey) && !string.IsNullOrEmpty(ActiveSecretKey),
        PaymentProviderType.Manual => true,
        _ => false
    };

    /// <summary>
    /// Display name for the provider type.
    /// </summary>
    public string ProviderDisplayName => ProviderType switch
    {
        PaymentProviderType.Stripe => "Stripe",
        PaymentProviderType.PayPal => "PayPal",
        PaymentProviderType.Square => "Square",
        PaymentProviderType.Braintree => "Braintree",
        PaymentProviderType.AuthorizeNet => "Authorize.Net",
        PaymentProviderType.Adyen => "Adyen",
        PaymentProviderType.Mollie => "Mollie",
        PaymentProviderType.Klarna => "Klarna",
        PaymentProviderType.Manual => "Manual/Offline",
        _ => "Unknown"
    };

    #endregion
}

/// <summary>
/// Payment provider type.
/// </summary>
public enum PaymentProviderType
{
    /// <summary>
    /// Stripe payment gateway.
    /// </summary>
    Stripe = 0,

    /// <summary>
    /// PayPal payment gateway.
    /// </summary>
    PayPal = 1,

    /// <summary>
    /// Square payment gateway.
    /// </summary>
    Square = 2,

    /// <summary>
    /// Braintree payment gateway.
    /// </summary>
    Braintree = 3,

    /// <summary>
    /// Authorize.Net payment gateway.
    /// </summary>
    AuthorizeNet = 4,

    /// <summary>
    /// Adyen payment gateway.
    /// </summary>
    Adyen = 5,

    /// <summary>
    /// Mollie payment gateway.
    /// </summary>
    Mollie = 6,

    /// <summary>
    /// Klarna payment gateway.
    /// </summary>
    Klarna = 7,

    /// <summary>
    /// Manual/offline payment processing.
    /// </summary>
    Manual = 99
}
