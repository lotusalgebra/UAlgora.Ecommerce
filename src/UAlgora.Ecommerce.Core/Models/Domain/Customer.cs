using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a customer account.
/// </summary>
public class Customer : SoftDeleteEntity
{
    /// <summary>
    /// Store this customer belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Reference to Umbraco member ID.
    /// </summary>
    public int? UmbracoMemberId { get; set; }

    /// <summary>
    /// Customer email address (unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Customer last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Company name (for B2B).
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Tax/VAT number (for B2B).
    /// </summary>
    public string? TaxNumber { get; set; }

    /// <summary>
    /// Account status.
    /// </summary>
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    /// <summary>
    /// Whether the customer has accepted marketing.
    /// </summary>
    public bool AcceptsMarketing { get; set; }

    /// <summary>
    /// When marketing consent was given.
    /// </summary>
    public DateTime? MarketingConsentAt { get; set; }

    /// <summary>
    /// Customer's preferred currency.
    /// </summary>
    public string? PreferredCurrency { get; set; }

    /// <summary>
    /// Customer's preferred language.
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Customer's timezone.
    /// </summary>
    public string? Timezone { get; set; }

    #region Addresses

    /// <summary>
    /// Customer addresses.
    /// </summary>
    public List<Address> Addresses { get; set; } = [];

    /// <summary>
    /// Default shipping address ID.
    /// </summary>
    public Guid? DefaultShippingAddressId { get; set; }

    /// <summary>
    /// Default billing address ID.
    /// </summary>
    public Guid? DefaultBillingAddressId { get; set; }

    #endregion

    #region Orders

    /// <summary>
    /// Customer orders.
    /// </summary>
    public List<Order> Orders { get; set; } = [];

    /// <summary>
    /// Total number of orders.
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total amount spent (lifetime).
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// Average order value.
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Last order date.
    /// </summary>
    public DateTime? LastOrderAt { get; set; }

    #endregion

    #region Loyalty

    /// <summary>
    /// Loyalty points balance.
    /// </summary>
    public int LoyaltyPoints { get; set; }

    /// <summary>
    /// Total loyalty points earned (lifetime).
    /// </summary>
    public int TotalLoyaltyPointsEarned { get; set; }

    /// <summary>
    /// Customer tier/segment.
    /// </summary>
    public string? CustomerTier { get; set; }

    /// <summary>
    /// Store credit balance.
    /// </summary>
    public decimal StoreCreditBalance { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// Tags for segmentation.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Custom notes about the customer.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Acquisition source.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Last login date.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Whether email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// When email was verified.
    /// </summary>
    public DateTime? EmailVerifiedAt { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Customer full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Whether the customer is active.
    /// </summary>
    public bool IsActive => Status == CustomerStatus.Active;

    /// <summary>
    /// Default shipping address.
    /// </summary>
    public Address? DefaultShippingAddress =>
        Addresses.FirstOrDefault(a => a.Id == DefaultShippingAddressId)
        ?? Addresses.FirstOrDefault(a => a.IsDefaultShipping);

    /// <summary>
    /// Default billing address.
    /// </summary>
    public Address? DefaultBillingAddress =>
        Addresses.FirstOrDefault(a => a.Id == DefaultBillingAddressId)
        ?? Addresses.FirstOrDefault(a => a.IsDefaultBilling);

    #endregion
}
