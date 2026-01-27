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

    #region GST (Goods and Services Tax) Configuration

    /// <summary>
    /// Whether this category uses GST (Indian tax system).
    /// </summary>
    public bool IsGst { get; set; }

    /// <summary>
    /// GST calculation type: "CGST+SGST" (intra-state), "IGST" (inter-state), or "AUTO" (auto-detect).
    /// </summary>
    public string? GstType { get; set; }

    /// <summary>
    /// Central GST rate percentage (for intra-state transactions).
    /// </summary>
    public decimal CgstRate { get; set; }

    /// <summary>
    /// State GST rate percentage (for intra-state transactions).
    /// </summary>
    public decimal SgstRate { get; set; }

    /// <summary>
    /// Integrated GST rate percentage (for inter-state transactions).
    /// Typically equals CGST + SGST.
    /// </summary>
    public decimal IgstRate { get; set; }

    /// <summary>
    /// HSN (Harmonized System of Nomenclature) code for goods classification.
    /// </summary>
    public string? HsnCode { get; set; }

    /// <summary>
    /// SAC (Services Accounting Code) for services classification.
    /// </summary>
    public string? SacCode { get; set; }

    #endregion

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
