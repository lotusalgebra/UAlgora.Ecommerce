namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a shipping zone for geographic-based shipping rates.
/// </summary>
public class ShippingZone : SoftDeleteEntity
{
    /// <summary>
    /// Zone name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Zone description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unique code for this zone.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this zone is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this is the default/fallback zone.
    /// </summary>
    public bool IsDefault { get; set; }

    #region Geographic Criteria

    /// <summary>
    /// Countries included in this zone (ISO 3166-1 alpha-2 codes).
    /// </summary>
    public List<string> Countries { get; set; } = [];

    /// <summary>
    /// States/provinces included (format: "CountryCode-StateCode", e.g., "US-CA").
    /// </summary>
    public List<string> States { get; set; } = [];

    /// <summary>
    /// Postal code patterns (supports wildcards, e.g., "90*", "10001-10099").
    /// </summary>
    public List<string> PostalCodePatterns { get; set; } = [];

    /// <summary>
    /// Cities included in this zone.
    /// </summary>
    public List<string> Cities { get; set; } = [];

    #endregion

    #region Exclusions

    /// <summary>
    /// Countries excluded from this zone.
    /// </summary>
    public List<string> ExcludedCountries { get; set; } = [];

    /// <summary>
    /// States excluded from this zone.
    /// </summary>
    public List<string> ExcludedStates { get; set; } = [];

    /// <summary>
    /// Postal codes excluded from this zone.
    /// </summary>
    public List<string> ExcludedPostalCodes { get; set; } = [];

    #endregion

    /// <summary>
    /// Methods available in this zone.
    /// </summary>
    public List<ShippingMethod> Methods { get; set; } = [];

    /// <summary>
    /// Rates for this zone.
    /// </summary>
    public List<ShippingRate> Rates { get; set; } = [];

    #region Computed Properties

    /// <summary>
    /// Whether this zone has geographic restrictions.
    /// </summary>
    public bool HasRestrictions => Countries.Count > 0 || States.Count > 0 ||
        PostalCodePatterns.Count > 0 || Cities.Count > 0;

    /// <summary>
    /// Gets a summary of included regions.
    /// </summary>
    public string RegionSummary
    {
        get
        {
            var parts = new List<string>();
            if (Countries.Count > 0)
                parts.Add($"{Countries.Count} countries");
            if (States.Count > 0)
                parts.Add($"{States.Count} states");
            if (PostalCodePatterns.Count > 0)
                parts.Add($"{PostalCodePatterns.Count} postal patterns");
            return parts.Count > 0 ? string.Join(", ", parts) : "All regions";
        }
    }

    /// <summary>
    /// Number of active methods in this zone.
    /// </summary>
    public int ActiveMethodCount => Methods.Count(m => m.IsActive);

    #endregion

    /// <summary>
    /// Checks if an address matches this zone.
    /// </summary>
    public bool MatchesAddress(Address address)
    {
        if (address == null) return IsDefault;

        // Check exclusions first
        if (!string.IsNullOrEmpty(address.CountryCode) && ExcludedCountries.Contains(address.CountryCode))
            return false;

        var stateKey = $"{address.CountryCode}-{address.StateProvinceCode}";
        if (!string.IsNullOrEmpty(address.StateProvinceCode) && ExcludedStates.Contains(stateKey))
            return false;

        if (!string.IsNullOrEmpty(address.PostalCode) && ExcludedPostalCodes.Contains(address.PostalCode))
            return false;

        // If no restrictions, match all (unless it's not default)
        if (!HasRestrictions)
            return IsDefault;

        // Check country match
        if (Countries.Count > 0 && !string.IsNullOrEmpty(address.CountryCode))
        {
            if (Countries.Contains(address.CountryCode))
                return true;
        }

        // Check state match
        if (States.Count > 0 && !string.IsNullOrEmpty(address.StateProvinceCode))
        {
            if (States.Contains(stateKey))
                return true;
        }

        // Check postal code match
        if (PostalCodePatterns.Count > 0 && !string.IsNullOrEmpty(address.PostalCode))
        {
            foreach (var pattern in PostalCodePatterns)
            {
                if (MatchesPostalPattern(address.PostalCode, pattern))
                    return true;
            }
        }

        // Check city match
        if (Cities.Count > 0 && !string.IsNullOrEmpty(address.City))
        {
            if (Cities.Contains(address.City, StringComparer.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool MatchesPostalPattern(string postalCode, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return false;

        // Exact match
        if (pattern.Equals(postalCode, StringComparison.OrdinalIgnoreCase))
            return true;

        // Wildcard match (e.g., "90*")
        if (pattern.EndsWith('*'))
        {
            var prefix = pattern[..^1];
            return postalCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        // Range match (e.g., "10001-10099")
        if (pattern.Contains('-'))
        {
            var parts = pattern.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var min) &&
                int.TryParse(parts[1], out var max) &&
                int.TryParse(postalCode.Replace(" ", "").Replace("-", ""), out var code))
            {
                return code >= min && code <= max;
            }
        }

        return false;
    }
}
