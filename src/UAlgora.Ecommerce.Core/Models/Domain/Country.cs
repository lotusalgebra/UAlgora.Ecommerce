namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a country for address selection and geographic operations.
/// </summary>
public class Country : BaseEntity
{
    /// <summary>
    /// ISO 3166-1 alpha-2 code (e.g., "US", "GB", "CA").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-3 code (e.g., "USA", "GBR", "CAN").
    /// </summary>
    public string? Alpha3Code { get; set; }

    /// <summary>
    /// ISO 3166-1 numeric code.
    /// </summary>
    public int? NumericCode { get; set; }

    /// <summary>
    /// Country name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Official country name.
    /// </summary>
    public string? OfficialName { get; set; }

    /// <summary>
    /// Country calling code (e.g., "+1", "+44").
    /// </summary>
    public string? CallingCode { get; set; }

    /// <summary>
    /// Currency code used in this country (e.g., "USD", "GBP").
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Whether this country is active for selection.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether shipping is available to this country.
    /// </summary>
    public bool AllowShipping { get; set; } = true;

    /// <summary>
    /// Whether billing from this country is allowed.
    /// </summary>
    public bool AllowBilling { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether to display this country at the top of lists.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// States/provinces in this country.
    /// </summary>
    public List<StateProvince> States { get; set; } = [];

    /// <summary>
    /// Whether this country has states/provinces.
    /// </summary>
    public bool HasStates => States.Count > 0;

    /// <summary>
    /// Label for state field (e.g., "State", "Province", "County", "Region").
    /// </summary>
    public string StateLabel { get; set; } = "State/Province";

    /// <summary>
    /// Label for postal code field (e.g., "ZIP Code", "Postal Code", "Postcode").
    /// </summary>
    public string PostalCodeLabel { get; set; } = "Postal Code";

    /// <summary>
    /// Whether postal code is required for addresses in this country.
    /// </summary>
    public bool RequiresPostalCode { get; set; } = true;

    /// <summary>
    /// Whether state is required for addresses in this country.
    /// </summary>
    public bool RequiresState { get; set; } = true;

    /// <summary>
    /// Regex pattern for validating postal codes.
    /// </summary>
    public string? PostalCodePattern { get; set; }
}
