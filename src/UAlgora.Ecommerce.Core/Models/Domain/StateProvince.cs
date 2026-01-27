namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a state/province/region within a country.
/// </summary>
public class StateProvince : BaseEntity
{
    /// <summary>
    /// Country this state belongs to.
    /// </summary>
    public Guid CountryId { get; set; }

    /// <summary>
    /// State/province code (e.g., "CA", "NY", "ON").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// State/province name (e.g., "California", "New York", "Ontario").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Abbreviated name if different from code.
    /// </summary>
    public string? Abbreviation { get; set; }

    /// <summary>
    /// Whether this state is active for selection.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Navigation property to country.
    /// </summary>
    public Country? Country { get; set; }

    /// <summary>
    /// Combined key for zone matching (e.g., "US-CA").
    /// </summary>
    public string ZoneKey => Country != null ? $"{Country.Code}-{Code}" : Code;
}
