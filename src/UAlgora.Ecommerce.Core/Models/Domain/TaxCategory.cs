namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a tax category that can be assigned to products.
/// Examples: Standard, Reduced, Zero-Rated, Exempt, Luxury.
/// </summary>
public class TaxCategory : SoftDeleteEntity
{
    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique code for this category.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of what products belong to this category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this category is enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this is the default category for new products.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether products in this category are tax exempt.
    /// </summary>
    public bool IsTaxExempt { get; set; }

    /// <summary>
    /// External tax code for provider integration (e.g., Avalara tax code).
    /// </summary>
    public string? ExternalTaxCode { get; set; }

    /// <summary>
    /// Tax rates configured for this category.
    /// </summary>
    public List<TaxRate> Rates { get; set; } = [];

    #region Computed Properties

    /// <summary>
    /// Number of active rates for this category.
    /// </summary>
    public int ActiveRateCount => Rates.Count(r => r.IsActive);

    #endregion
}
