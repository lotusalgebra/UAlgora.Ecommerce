using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for country and state/province operations.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/country")]
public class CountryManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ICountryRepository _countryRepository;

    public CountryManagementApiController(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    /// <summary>
    /// Gets all countries.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<CountryListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeStates = true, CancellationToken ct = default)
    {
        var countries = includeStates
            ? await _countryRepository.GetAllWithStatesAsync(ct)
            : await _countryRepository.GetAllAsync(ct);

        return Ok(new CountryListResponse
        {
            Items = countries.Select(MapToModel).ToList(),
            Total = countries.Count
        });
    }

    /// <summary>
    /// Gets all countries available for shipping.
    /// </summary>
    [HttpGet("shipping")]
    [ProducesResponseType<CountryListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShippingCountries(CancellationToken ct = default)
    {
        var countries = await _countryRepository.GetShippingCountriesAsync(ct);

        return Ok(new CountryListResponse
        {
            Items = countries.Select(MapToModel).ToList(),
            Total = countries.Count
        });
    }

    /// <summary>
    /// Gets all countries available for billing.
    /// </summary>
    [HttpGet("billing")]
    [ProducesResponseType<CountryListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingCountries(CancellationToken ct = default)
    {
        var countries = await _countryRepository.GetBillingCountriesAsync(ct);

        return Ok(new CountryListResponse
        {
            Items = countries.Select(MapToModel).ToList(),
            Total = countries.Count
        });
    }

    /// <summary>
    /// Gets a country by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CountryModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var country = await _countryRepository.GetWithStatesAsync(id, ct);
        if (country == null)
            return NotFound();

        return Ok(MapToModel(country));
    }

    /// <summary>
    /// Gets a country by code.
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType<CountryModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct = default)
    {
        var country = await _countryRepository.GetWithStatesByCodeAsync(code.ToUpperInvariant(), ct);
        if (country == null)
            return NotFound();

        return Ok(MapToModel(country));
    }

    /// <summary>
    /// Gets states for a country by country code.
    /// </summary>
    [HttpGet("code/{countryCode}/states")]
    [ProducesResponseType<StateListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatesByCountryCode(string countryCode, CancellationToken ct = default)
    {
        var states = await _countryRepository.GetStatesByCountryCodeAsync(countryCode.ToUpperInvariant(), ct);

        return Ok(new StateListResponse
        {
            Items = states.Select(MapStateToModel).ToList(),
            Total = states.Count
        });
    }

    /// <summary>
    /// Gets states for a country by ID.
    /// </summary>
    [HttpGet("{id:guid}/states")]
    [ProducesResponseType<StateListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates(Guid id, CancellationToken ct = default)
    {
        var states = await _countryRepository.GetStatesAsync(id, ct);

        return Ok(new StateListResponse
        {
            Items = states.Select(MapStateToModel).ToList(),
            Total = states.Count
        });
    }

    /// <summary>
    /// Creates a new country.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CountryModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCountryRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Code and Name are required" });

        var code = request.Code.ToUpperInvariant();
        if (await _countryRepository.CodeExistsAsync(code, ct: ct))
            return BadRequest(new { message = "Country code already exists" });

        var country = new Country
        {
            Code = code,
            Alpha3Code = request.Alpha3Code?.ToUpperInvariant(),
            NumericCode = request.NumericCode,
            Name = request.Name,
            OfficialName = request.OfficialName,
            CallingCode = request.CallingCode,
            CurrencyCode = request.CurrencyCode?.ToUpperInvariant(),
            IsActive = request.IsActive,
            AllowShipping = request.AllowShipping,
            AllowBilling = request.AllowBilling,
            SortOrder = request.SortOrder,
            IsFeatured = request.IsFeatured,
            StateLabel = request.StateLabel ?? "State/Province",
            PostalCodeLabel = request.PostalCodeLabel ?? "Postal Code",
            RequiresPostalCode = request.RequiresPostalCode,
            RequiresState = request.RequiresState,
            PostalCodePattern = request.PostalCodePattern
        };

        var created = await _countryRepository.AddAsync(country, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToModel(created));
    }

    /// <summary>
    /// Updates a country.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CountryModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCountryRequest request, CancellationToken ct = default)
    {
        var country = await _countryRepository.GetByIdAsync(id, ct);
        if (country == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.ToUpperInvariant();
            if (code != country.Code && await _countryRepository.CodeExistsAsync(code, id, ct))
                return BadRequest(new { message = "Country code already exists" });
            country.Code = code;
        }

        if (request.Name != null) country.Name = request.Name;
        if (request.Alpha3Code != null) country.Alpha3Code = request.Alpha3Code.ToUpperInvariant();
        if (request.NumericCode.HasValue) country.NumericCode = request.NumericCode;
        if (request.OfficialName != null) country.OfficialName = request.OfficialName;
        if (request.CallingCode != null) country.CallingCode = request.CallingCode;
        if (request.CurrencyCode != null) country.CurrencyCode = request.CurrencyCode.ToUpperInvariant();
        if (request.IsActive.HasValue) country.IsActive = request.IsActive.Value;
        if (request.AllowShipping.HasValue) country.AllowShipping = request.AllowShipping.Value;
        if (request.AllowBilling.HasValue) country.AllowBilling = request.AllowBilling.Value;
        if (request.SortOrder.HasValue) country.SortOrder = request.SortOrder.Value;
        if (request.IsFeatured.HasValue) country.IsFeatured = request.IsFeatured.Value;
        if (request.StateLabel != null) country.StateLabel = request.StateLabel;
        if (request.PostalCodeLabel != null) country.PostalCodeLabel = request.PostalCodeLabel;
        if (request.RequiresPostalCode.HasValue) country.RequiresPostalCode = request.RequiresPostalCode.Value;
        if (request.RequiresState.HasValue) country.RequiresState = request.RequiresState.Value;
        if (request.PostalCodePattern != null) country.PostalCodePattern = request.PostalCodePattern;

        var updated = await _countryRepository.UpdateAsync(country, ct);
        return Ok(MapToModel(updated));
    }

    /// <summary>
    /// Deletes a country.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var country = await _countryRepository.GetByIdAsync(id, ct);
        if (country == null)
            return NotFound();

        await _countryRepository.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Toggles the active status of a country.
    /// </summary>
    [HttpPost("{id:guid}/toggle")]
    [ProducesResponseType<CountryModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct = default)
    {
        var country = await _countryRepository.GetByIdAsync(id, ct);
        if (country == null)
            return NotFound();

        country.IsActive = !country.IsActive;
        var updated = await _countryRepository.UpdateAsync(country, ct);
        return Ok(MapToModel(updated));
    }

    /// <summary>
    /// Adds a state/province to a country.
    /// </summary>
    [HttpPost("{countryId:guid}/states")]
    [ProducesResponseType<StateModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddState(Guid countryId, [FromBody] CreateStateRequest request, CancellationToken ct = default)
    {
        var country = await _countryRepository.GetByIdAsync(countryId, ct);
        if (country == null)
            return NotFound(new { message = "Country not found" });

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Code and Name are required" });

        var code = request.Code.ToUpperInvariant();
        if (await _countryRepository.StateCodeExistsAsync(countryId, code, ct: ct))
            return BadRequest(new { message = "State code already exists in this country" });

        var state = new StateProvince
        {
            CountryId = countryId,
            Code = code,
            Name = request.Name,
            Abbreviation = request.Abbreviation,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        };

        var created = await _countryRepository.AddStateAsync(state, ct);
        return CreatedAtAction(nameof(GetStates), new { id = countryId }, MapStateToModel(created));
    }

    /// <summary>
    /// Updates a state/province.
    /// </summary>
    [HttpPut("states/{stateId:guid}")]
    [ProducesResponseType<StateModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateState(Guid stateId, [FromBody] UpdateStateRequest request, CancellationToken ct = default)
    {
        var state = await _countryRepository.GetStateByIdAsync(stateId, ct);
        if (state == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.ToUpperInvariant();
            if (code != state.Code && await _countryRepository.StateCodeExistsAsync(state.CountryId, code, stateId, ct))
                return BadRequest(new { message = "State code already exists in this country" });
            state.Code = code;
        }

        if (request.Name != null) state.Name = request.Name;
        if (request.Abbreviation != null) state.Abbreviation = request.Abbreviation;
        if (request.IsActive.HasValue) state.IsActive = request.IsActive.Value;
        if (request.SortOrder.HasValue) state.SortOrder = request.SortOrder.Value;

        var updated = await _countryRepository.UpdateStateAsync(state, ct);
        return Ok(MapStateToModel(updated));
    }

    /// <summary>
    /// Deletes a state/province.
    /// </summary>
    [HttpDelete("states/{stateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteState(Guid stateId, CancellationToken ct = default)
    {
        var state = await _countryRepository.GetStateByIdAsync(stateId, ct);
        if (state == null)
            return NotFound();

        await _countryRepository.DeleteStateAsync(stateId, ct);
        return NoContent();
    }

    /// <summary>
    /// Seeds default countries and states.
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedDefaults(CancellationToken ct = default)
    {
        var existingCount = await _countryRepository.CountAsync(ct);
        if (existingCount > 0)
            return Ok(new { message = "Countries already exist", count = existingCount });

        var countries = GetDefaultCountries();
        foreach (var country in countries)
        {
            await _countryRepository.AddAsync(country, ct);
        }

        return Ok(new { message = "Default countries seeded successfully", count = countries.Count });
    }

    #region Mapping

    private static CountryModel MapToModel(Country country) => new()
    {
        Id = country.Id,
        Code = country.Code,
        Alpha3Code = country.Alpha3Code,
        NumericCode = country.NumericCode,
        Name = country.Name,
        OfficialName = country.OfficialName,
        CallingCode = country.CallingCode,
        CurrencyCode = country.CurrencyCode,
        IsActive = country.IsActive,
        AllowShipping = country.AllowShipping,
        AllowBilling = country.AllowBilling,
        SortOrder = country.SortOrder,
        IsFeatured = country.IsFeatured,
        StateLabel = country.StateLabel,
        PostalCodeLabel = country.PostalCodeLabel,
        RequiresPostalCode = country.RequiresPostalCode,
        RequiresState = country.RequiresState,
        PostalCodePattern = country.PostalCodePattern,
        HasStates = country.HasStates,
        States = country.States.Select(MapStateToModel).ToList()
    };

    private static StateModel MapStateToModel(StateProvince state) => new()
    {
        Id = state.Id,
        CountryId = state.CountryId,
        Code = state.Code,
        Name = state.Name,
        Abbreviation = state.Abbreviation,
        IsActive = state.IsActive,
        SortOrder = state.SortOrder
    };

    #endregion

    #region Default Data

    private static List<Country> GetDefaultCountries() =>
    [
        new Country
        {
            Code = "US",
            Alpha3Code = "USA",
            NumericCode = 840,
            Name = "United States",
            OfficialName = "United States of America",
            CallingCode = "+1",
            CurrencyCode = "USD",
            IsFeatured = true,
            StateLabel = "State",
            PostalCodeLabel = "ZIP Code",
            PostalCodePattern = @"^\d{5}(-\d{4})?$",
            States = GetUSStates()
        },
        new Country
        {
            Code = "CA",
            Alpha3Code = "CAN",
            NumericCode = 124,
            Name = "Canada",
            CallingCode = "+1",
            CurrencyCode = "CAD",
            IsFeatured = true,
            StateLabel = "Province",
            PostalCodeLabel = "Postal Code",
            PostalCodePattern = @"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$",
            States = GetCanadaProvinces()
        },
        new Country
        {
            Code = "GB",
            Alpha3Code = "GBR",
            NumericCode = 826,
            Name = "United Kingdom",
            OfficialName = "United Kingdom of Great Britain and Northern Ireland",
            CallingCode = "+44",
            CurrencyCode = "GBP",
            IsFeatured = true,
            StateLabel = "County",
            PostalCodeLabel = "Postcode",
            RequiresState = false,
            PostalCodePattern = @"^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$"
        },
        new Country
        {
            Code = "AU",
            Alpha3Code = "AUS",
            NumericCode = 36,
            Name = "Australia",
            CallingCode = "+61",
            CurrencyCode = "AUD",
            StateLabel = "State/Territory",
            PostalCodeLabel = "Postcode",
            PostalCodePattern = @"^\d{4}$",
            States = GetAustraliaStates()
        },
        new Country
        {
            Code = "DE",
            Alpha3Code = "DEU",
            NumericCode = 276,
            Name = "Germany",
            OfficialName = "Federal Republic of Germany",
            CallingCode = "+49",
            CurrencyCode = "EUR",
            StateLabel = "State",
            PostalCodeLabel = "Postal Code",
            RequiresState = false,
            PostalCodePattern = @"^\d{5}$"
        },
        new Country
        {
            Code = "FR",
            Alpha3Code = "FRA",
            NumericCode = 250,
            Name = "France",
            OfficialName = "French Republic",
            CallingCode = "+33",
            CurrencyCode = "EUR",
            StateLabel = "Region",
            PostalCodeLabel = "Postal Code",
            RequiresState = false,
            PostalCodePattern = @"^\d{5}$"
        },
        new Country
        {
            Code = "IN",
            Alpha3Code = "IND",
            NumericCode = 356,
            Name = "India",
            OfficialName = "Republic of India",
            CallingCode = "+91",
            CurrencyCode = "INR",
            StateLabel = "State",
            PostalCodeLabel = "PIN Code",
            PostalCodePattern = @"^\d{6}$",
            States = GetIndiaStates()
        },
        new Country
        {
            Code = "JP",
            Alpha3Code = "JPN",
            NumericCode = 392,
            Name = "Japan",
            CallingCode = "+81",
            CurrencyCode = "JPY",
            StateLabel = "Prefecture",
            PostalCodeLabel = "Postal Code",
            PostalCodePattern = @"^\d{3}-?\d{4}$"
        },
        new Country
        {
            Code = "CN",
            Alpha3Code = "CHN",
            NumericCode = 156,
            Name = "China",
            OfficialName = "People's Republic of China",
            CallingCode = "+86",
            CurrencyCode = "CNY",
            StateLabel = "Province",
            PostalCodeLabel = "Postal Code",
            PostalCodePattern = @"^\d{6}$"
        },
        new Country
        {
            Code = "MX",
            Alpha3Code = "MEX",
            NumericCode = 484,
            Name = "Mexico",
            OfficialName = "United Mexican States",
            CallingCode = "+52",
            CurrencyCode = "MXN",
            StateLabel = "State",
            PostalCodeLabel = "Postal Code",
            PostalCodePattern = @"^\d{5}$"
        }
    ];

    private static List<StateProvince> GetUSStates() =>
    [
        new StateProvince { Code = "AL", Name = "Alabama" },
        new StateProvince { Code = "AK", Name = "Alaska" },
        new StateProvince { Code = "AZ", Name = "Arizona" },
        new StateProvince { Code = "AR", Name = "Arkansas" },
        new StateProvince { Code = "CA", Name = "California" },
        new StateProvince { Code = "CO", Name = "Colorado" },
        new StateProvince { Code = "CT", Name = "Connecticut" },
        new StateProvince { Code = "DE", Name = "Delaware" },
        new StateProvince { Code = "FL", Name = "Florida" },
        new StateProvince { Code = "GA", Name = "Georgia" },
        new StateProvince { Code = "HI", Name = "Hawaii" },
        new StateProvince { Code = "ID", Name = "Idaho" },
        new StateProvince { Code = "IL", Name = "Illinois" },
        new StateProvince { Code = "IN", Name = "Indiana" },
        new StateProvince { Code = "IA", Name = "Iowa" },
        new StateProvince { Code = "KS", Name = "Kansas" },
        new StateProvince { Code = "KY", Name = "Kentucky" },
        new StateProvince { Code = "LA", Name = "Louisiana" },
        new StateProvince { Code = "ME", Name = "Maine" },
        new StateProvince { Code = "MD", Name = "Maryland" },
        new StateProvince { Code = "MA", Name = "Massachusetts" },
        new StateProvince { Code = "MI", Name = "Michigan" },
        new StateProvince { Code = "MN", Name = "Minnesota" },
        new StateProvince { Code = "MS", Name = "Mississippi" },
        new StateProvince { Code = "MO", Name = "Missouri" },
        new StateProvince { Code = "MT", Name = "Montana" },
        new StateProvince { Code = "NE", Name = "Nebraska" },
        new StateProvince { Code = "NV", Name = "Nevada" },
        new StateProvince { Code = "NH", Name = "New Hampshire" },
        new StateProvince { Code = "NJ", Name = "New Jersey" },
        new StateProvince { Code = "NM", Name = "New Mexico" },
        new StateProvince { Code = "NY", Name = "New York" },
        new StateProvince { Code = "NC", Name = "North Carolina" },
        new StateProvince { Code = "ND", Name = "North Dakota" },
        new StateProvince { Code = "OH", Name = "Ohio" },
        new StateProvince { Code = "OK", Name = "Oklahoma" },
        new StateProvince { Code = "OR", Name = "Oregon" },
        new StateProvince { Code = "PA", Name = "Pennsylvania" },
        new StateProvince { Code = "RI", Name = "Rhode Island" },
        new StateProvince { Code = "SC", Name = "South Carolina" },
        new StateProvince { Code = "SD", Name = "South Dakota" },
        new StateProvince { Code = "TN", Name = "Tennessee" },
        new StateProvince { Code = "TX", Name = "Texas" },
        new StateProvince { Code = "UT", Name = "Utah" },
        new StateProvince { Code = "VT", Name = "Vermont" },
        new StateProvince { Code = "VA", Name = "Virginia" },
        new StateProvince { Code = "WA", Name = "Washington" },
        new StateProvince { Code = "WV", Name = "West Virginia" },
        new StateProvince { Code = "WI", Name = "Wisconsin" },
        new StateProvince { Code = "WY", Name = "Wyoming" },
        new StateProvince { Code = "DC", Name = "District of Columbia" }
    ];

    private static List<StateProvince> GetCanadaProvinces() =>
    [
        new StateProvince { Code = "AB", Name = "Alberta" },
        new StateProvince { Code = "BC", Name = "British Columbia" },
        new StateProvince { Code = "MB", Name = "Manitoba" },
        new StateProvince { Code = "NB", Name = "New Brunswick" },
        new StateProvince { Code = "NL", Name = "Newfoundland and Labrador" },
        new StateProvince { Code = "NS", Name = "Nova Scotia" },
        new StateProvince { Code = "NT", Name = "Northwest Territories" },
        new StateProvince { Code = "NU", Name = "Nunavut" },
        new StateProvince { Code = "ON", Name = "Ontario" },
        new StateProvince { Code = "PE", Name = "Prince Edward Island" },
        new StateProvince { Code = "QC", Name = "Quebec" },
        new StateProvince { Code = "SK", Name = "Saskatchewan" },
        new StateProvince { Code = "YT", Name = "Yukon" }
    ];

    private static List<StateProvince> GetAustraliaStates() =>
    [
        new StateProvince { Code = "ACT", Name = "Australian Capital Territory" },
        new StateProvince { Code = "NSW", Name = "New South Wales" },
        new StateProvince { Code = "NT", Name = "Northern Territory" },
        new StateProvince { Code = "QLD", Name = "Queensland" },
        new StateProvince { Code = "SA", Name = "South Australia" },
        new StateProvince { Code = "TAS", Name = "Tasmania" },
        new StateProvince { Code = "VIC", Name = "Victoria" },
        new StateProvince { Code = "WA", Name = "Western Australia" }
    ];

    private static List<StateProvince> GetIndiaStates() =>
    [
        new StateProvince { Code = "AN", Name = "Andaman and Nicobar Islands" },
        new StateProvince { Code = "AP", Name = "Andhra Pradesh" },
        new StateProvince { Code = "AR", Name = "Arunachal Pradesh" },
        new StateProvince { Code = "AS", Name = "Assam" },
        new StateProvince { Code = "BR", Name = "Bihar" },
        new StateProvince { Code = "CH", Name = "Chandigarh" },
        new StateProvince { Code = "CT", Name = "Chhattisgarh" },
        new StateProvince { Code = "DD", Name = "Daman and Diu" },
        new StateProvince { Code = "DL", Name = "Delhi" },
        new StateProvince { Code = "GA", Name = "Goa" },
        new StateProvince { Code = "GJ", Name = "Gujarat" },
        new StateProvince { Code = "HR", Name = "Haryana" },
        new StateProvince { Code = "HP", Name = "Himachal Pradesh" },
        new StateProvince { Code = "JK", Name = "Jammu and Kashmir" },
        new StateProvince { Code = "JH", Name = "Jharkhand" },
        new StateProvince { Code = "KA", Name = "Karnataka" },
        new StateProvince { Code = "KL", Name = "Kerala" },
        new StateProvince { Code = "LA", Name = "Ladakh" },
        new StateProvince { Code = "LD", Name = "Lakshadweep" },
        new StateProvince { Code = "MP", Name = "Madhya Pradesh" },
        new StateProvince { Code = "MH", Name = "Maharashtra" },
        new StateProvince { Code = "MN", Name = "Manipur" },
        new StateProvince { Code = "ML", Name = "Meghalaya" },
        new StateProvince { Code = "MZ", Name = "Mizoram" },
        new StateProvince { Code = "NL", Name = "Nagaland" },
        new StateProvince { Code = "OR", Name = "Odisha" },
        new StateProvince { Code = "PY", Name = "Puducherry" },
        new StateProvince { Code = "PB", Name = "Punjab" },
        new StateProvince { Code = "RJ", Name = "Rajasthan" },
        new StateProvince { Code = "SK", Name = "Sikkim" },
        new StateProvince { Code = "TN", Name = "Tamil Nadu" },
        new StateProvince { Code = "TG", Name = "Telangana" },
        new StateProvince { Code = "TR", Name = "Tripura" },
        new StateProvince { Code = "UP", Name = "Uttar Pradesh" },
        new StateProvince { Code = "UK", Name = "Uttarakhand" },
        new StateProvince { Code = "WB", Name = "West Bengal" }
    ];

    #endregion
}

#region Models

public class CountryListResponse
{
    public List<CountryModel> Items { get; set; } = [];
    public int Total { get; set; }
}

public class StateListResponse
{
    public List<StateModel> Items { get; set; } = [];
    public int Total { get; set; }
}

public class CountryModel
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public string? Alpha3Code { get; set; }
    public int? NumericCode { get; set; }
    public required string Name { get; set; }
    public string? OfficialName { get; set; }
    public string? CallingCode { get; set; }
    public string? CurrencyCode { get; set; }
    public bool IsActive { get; set; }
    public bool AllowShipping { get; set; }
    public bool AllowBilling { get; set; }
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
    public required string StateLabel { get; set; }
    public required string PostalCodeLabel { get; set; }
    public bool RequiresPostalCode { get; set; }
    public bool RequiresState { get; set; }
    public string? PostalCodePattern { get; set; }
    public bool HasStates { get; set; }
    public List<StateModel> States { get; set; } = [];
}

public class StateModel
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Abbreviation { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class CreateCountryRequest
{
    public required string Code { get; set; }
    public string? Alpha3Code { get; set; }
    public int? NumericCode { get; set; }
    public required string Name { get; set; }
    public string? OfficialName { get; set; }
    public string? CallingCode { get; set; }
    public string? CurrencyCode { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowShipping { get; set; } = true;
    public bool AllowBilling { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
    public string? StateLabel { get; set; }
    public string? PostalCodeLabel { get; set; }
    public bool RequiresPostalCode { get; set; } = true;
    public bool RequiresState { get; set; } = true;
    public string? PostalCodePattern { get; set; }
}

public class UpdateCountryRequest
{
    public string? Code { get; set; }
    public string? Alpha3Code { get; set; }
    public int? NumericCode { get; set; }
    public string? Name { get; set; }
    public string? OfficialName { get; set; }
    public string? CallingCode { get; set; }
    public string? CurrencyCode { get; set; }
    public bool? IsActive { get; set; }
    public bool? AllowShipping { get; set; }
    public bool? AllowBilling { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsFeatured { get; set; }
    public string? StateLabel { get; set; }
    public string? PostalCodeLabel { get; set; }
    public bool? RequiresPostalCode { get; set; }
    public bool? RequiresState { get; set; }
    public string? PostalCodePattern { get; set; }
}

public class CreateStateRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Abbreviation { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateStateRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Abbreviation { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}

#endregion
