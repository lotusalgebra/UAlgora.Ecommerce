namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a customizable invoice template.
/// </summary>
public class InvoiceTemplate : BaseEntity
{
    /// <summary>
    /// Store this template belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template code for lookup.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Template description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this is the default template.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this template is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Template type (Invoice, PackagingSlip, Receipt).
    /// </summary>
    public InvoiceTemplateType TemplateType { get; set; } = InvoiceTemplateType.Invoice;

    #region Company Defaults

    /// <summary>
    /// Default company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Default company address.
    /// </summary>
    public string? CompanyAddress { get; set; }

    /// <summary>
    /// Default company phone.
    /// </summary>
    public string? CompanyPhone { get; set; }

    /// <summary>
    /// Default company email.
    /// </summary>
    public string? CompanyEmail { get; set; }

    /// <summary>
    /// Default company website.
    /// </summary>
    public string? CompanyWebsite { get; set; }

    /// <summary>
    /// Default tax ID.
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Default logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    #endregion

    #region Styling

    /// <summary>
    /// Primary color (hex) - used for header bar.
    /// </summary>
    public string PrimaryColor { get; set; } = "#1e3a5f";

    /// <summary>
    /// Secondary color (hex) - used for text and borders.
    /// </summary>
    public string SecondaryColor { get; set; } = "#424242";

    /// <summary>
    /// Accent color (hex) - used for highlights.
    /// </summary>
    public string AccentColor { get; set; } = "#2563eb";

    /// <summary>
    /// Header background color (hex).
    /// </summary>
    public string HeaderColor { get; set; } = "#1e3a5f";

    /// <summary>
    /// Header text color (hex).
    /// </summary>
    public string HeaderTextColor { get; set; } = "#ffffff";

    /// <summary>
    /// Font family.
    /// </summary>
    public string FontFamily { get; set; } = "Arial, sans-serif";

    /// <summary>
    /// Custom CSS.
    /// </summary>
    public string? CustomCss { get; set; }

    #endregion

    #region GST Compliance (India)

    /// <summary>
    /// Company GSTIN (GST Identification Number).
    /// </summary>
    public string? CompanyGstin { get; set; }

    /// <summary>
    /// Default place of supply.
    /// </summary>
    public string? DefaultPlaceOfSupply { get; set; }

    /// <summary>
    /// Supply type code (e.g., B2B, B2C, SEZWP, SEZWOP).
    /// </summary>
    public string? SupplyTypeCode { get; set; }

    /// <summary>
    /// Document type code (e.g., INV, CRN, DBN).
    /// </summary>
    public string? DocumentTypeCode { get; set; }

    /// <summary>
    /// Whether to generate IRN (Invoice Reference Number) for e-invoicing.
    /// </summary>
    public bool GenerateIrn { get; set; }

    /// <summary>
    /// Whether to show amount in words.
    /// </summary>
    public bool ShowAmountInWords { get; set; } = true;

    /// <summary>
    /// Whether to show HSN/SAC codes.
    /// </summary>
    public bool ShowHsnSacCodes { get; set; } = true;

    #endregion

    #region Signature & Custom Fields

    /// <summary>
    /// Signature image URL.
    /// </summary>
    public string? SignatureImageUrl { get; set; }

    /// <summary>
    /// Signature label text.
    /// </summary>
    public string SignatureLabel { get; set; } = "Authorized Signatory";

    /// <summary>
    /// Custom field 1 label.
    /// </summary>
    public string? CustomField1Label { get; set; }

    /// <summary>
    /// Custom field 1 default value.
    /// </summary>
    public string? CustomField1Value { get; set; }

    /// <summary>
    /// Custom field 2 label.
    /// </summary>
    public string? CustomField2Label { get; set; }

    /// <summary>
    /// Custom field 2 default value.
    /// </summary>
    public string? CustomField2Value { get; set; }

    /// <summary>
    /// Custom field 3 label.
    /// </summary>
    public string? CustomField3Label { get; set; }

    /// <summary>
    /// Custom field 3 default value.
    /// </summary>
    public string? CustomField3Value { get; set; }

    /// <summary>
    /// Thank you message.
    /// </summary>
    public string ThankYouMessage { get; set; } = "Thank you for your business!";

    #endregion

    #region Content Options

    /// <summary>
    /// Show company logo.
    /// </summary>
    public bool ShowLogo { get; set; } = true;

    /// <summary>
    /// Show shipping address.
    /// </summary>
    public bool ShowShippingAddress { get; set; } = true;

    /// <summary>
    /// Show product images.
    /// </summary>
    public bool ShowProductImages { get; set; } = false;

    /// <summary>
    /// Show SKU.
    /// </summary>
    public bool ShowSku { get; set; } = true;

    /// <summary>
    /// Show tax breakdown.
    /// </summary>
    public bool ShowTaxBreakdown { get; set; } = true;

    /// <summary>
    /// Show payment instructions.
    /// </summary>
    public bool ShowPaymentInstructions { get; set; } = true;

    /// <summary>
    /// Show barcode.
    /// </summary>
    public bool ShowBarcode { get; set; } = false;

    /// <summary>
    /// Show QR code.
    /// </summary>
    public bool ShowQrCode { get; set; } = false;

    #endregion

    #region Default Text

    /// <summary>
    /// Default notes.
    /// </summary>
    public string? DefaultNotes { get; set; }

    /// <summary>
    /// Default terms and conditions.
    /// </summary>
    public string? DefaultTerms { get; set; }

    /// <summary>
    /// Default footer text.
    /// </summary>
    public string? DefaultFooter { get; set; }

    /// <summary>
    /// Default payment instructions.
    /// </summary>
    public string? DefaultPaymentInstructions { get; set; }

    /// <summary>
    /// Invoice title (e.g., "INVOICE", "TAX INVOICE").
    /// </summary>
    public string InvoiceTitle { get; set; } = "INVOICE";

    /// <summary>
    /// Packaging slip title.
    /// </summary>
    public string PackingSlipTitle { get; set; } = "PACKING SLIP";

    #endregion

    #region Numbering

    /// <summary>
    /// Invoice number prefix.
    /// </summary>
    public string InvoiceNumberPrefix { get; set; } = "INV-";

    /// <summary>
    /// Include year in invoice number.
    /// </summary>
    public bool IncludeYearInNumber { get; set; } = true;

    /// <summary>
    /// Minimum digits in invoice number.
    /// </summary>
    public int NumberPadding { get; set; } = 6;

    #endregion

    /// <summary>
    /// Custom HTML template (advanced).
    /// </summary>
    public string? HtmlTemplate { get; set; }

    /// <summary>
    /// Date format.
    /// </summary>
    public string DateFormat { get; set; } = "MMM dd, yyyy";

    /// <summary>
    /// Payment terms in days.
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;
}

/// <summary>
/// Invoice template type.
/// </summary>
public enum InvoiceTemplateType
{
    Invoice,
    PackagingSlip,
    Receipt,
    CreditNote,
    Quotation
}
