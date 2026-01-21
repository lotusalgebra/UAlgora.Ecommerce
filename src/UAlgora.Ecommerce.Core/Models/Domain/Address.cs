using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a customer or order address.
/// </summary>
public class Address : BaseEntity
{
    /// <summary>
    /// Customer ID this address belongs to.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Address type.
    /// </summary>
    public AddressType Type { get; set; } = AddressType.Both;

    /// <summary>
    /// Address label/nickname (e.g., "Home", "Office").
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Company name.
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Address line 1 (street address).
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Address line 2 (apartment, suite, etc.).
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/province/region.
    /// </summary>
    public string? StateProvince { get; set; }

    /// <summary>
    /// State/province code (e.g., "CA", "NY").
    /// </summary>
    public string? StateProvinceCode { get; set; }

    /// <summary>
    /// Postal/ZIP code.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country name.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Whether this is the default shipping address.
    /// </summary>
    public bool IsDefaultShipping { get; set; }

    /// <summary>
    /// Whether this is the default billing address.
    /// </summary>
    public bool IsDefaultBilling { get; set; }

    /// <summary>
    /// Whether this address has been validated.
    /// </summary>
    public bool IsValidated { get; set; }

    /// <summary>
    /// Latitude for geolocation.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude for geolocation.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Navigation property to customer.
    /// </summary>
    public Customer? Customer { get; set; }

    #region Computed Properties

    /// <summary>
    /// Full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Formatted single-line address.
    /// </summary>
    public string FormattedAddress
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(AddressLine1))
                parts.Add(AddressLine1);

            if (!string.IsNullOrWhiteSpace(AddressLine2))
                parts.Add(AddressLine2);

            var cityStateZip = new List<string>();
            if (!string.IsNullOrWhiteSpace(City))
                cityStateZip.Add(City);
            if (!string.IsNullOrWhiteSpace(StateProvince))
                cityStateZip.Add(StateProvince);
            if (!string.IsNullOrWhiteSpace(PostalCode))
                cityStateZip.Add(PostalCode);

            if (cityStateZip.Count > 0)
                parts.Add(string.Join(", ", cityStateZip));

            if (!string.IsNullOrWhiteSpace(Country))
                parts.Add(Country);

            return string.Join(", ", parts);
        }
    }

    /// <summary>
    /// Formatted multi-line address.
    /// </summary>
    public string[] FormattedAddressLines
    {
        get
        {
            var lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(FullName))
                lines.Add(FullName);

            if (!string.IsNullOrWhiteSpace(Company))
                lines.Add(Company);

            if (!string.IsNullOrWhiteSpace(AddressLine1))
                lines.Add(AddressLine1);

            if (!string.IsNullOrWhiteSpace(AddressLine2))
                lines.Add(AddressLine2);

            var cityStateZip = new List<string>();
            if (!string.IsNullOrWhiteSpace(City))
                cityStateZip.Add(City);
            if (!string.IsNullOrWhiteSpace(StateProvinceCode ?? StateProvince))
                cityStateZip.Add(StateProvinceCode ?? StateProvince!);
            if (!string.IsNullOrWhiteSpace(PostalCode))
                cityStateZip.Add(PostalCode);

            if (cityStateZip.Count > 0)
                lines.Add(string.Join(", ", cityStateZip));

            if (!string.IsNullOrWhiteSpace(Country))
                lines.Add(Country);

            return lines.ToArray();
        }
    }

    #endregion

    /// <summary>
    /// Creates a copy of this address.
    /// </summary>
    public Address Clone() => new()
    {
        Type = Type,
        Label = Label,
        FirstName = FirstName,
        LastName = LastName,
        Company = Company,
        AddressLine1 = AddressLine1,
        AddressLine2 = AddressLine2,
        City = City,
        StateProvince = StateProvince,
        StateProvinceCode = StateProvinceCode,
        PostalCode = PostalCode,
        Country = Country,
        CountryCode = CountryCode,
        Phone = Phone,
        Latitude = Latitude,
        Longitude = Longitude
    };
}
