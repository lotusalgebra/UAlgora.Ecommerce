using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a generated invoice for an order.
/// </summary>
public class Invoice : BaseEntity
{
    /// <summary>
    /// Store this invoice belongs to.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Order this invoice is for.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Unique invoice number (e.g., "INV-2024-0001").
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Invoice status.
    /// </summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    /// <summary>
    /// Date the invoice was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Due date for payment.
    /// </summary>
    public DateTime? DueAt { get; set; }

    /// <summary>
    /// Date the invoice was paid.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Date the invoice was voided.
    /// </summary>
    public DateTime? VoidedAt { get; set; }

    #region Company Information

    /// <summary>
    /// Company name on the invoice.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Company address line 1.
    /// </summary>
    public string? CompanyAddress1 { get; set; }

    /// <summary>
    /// Company address line 2.
    /// </summary>
    public string? CompanyAddress2 { get; set; }

    /// <summary>
    /// Company city.
    /// </summary>
    public string? CompanyCity { get; set; }

    /// <summary>
    /// Company state/province.
    /// </summary>
    public string? CompanyState { get; set; }

    /// <summary>
    /// Company postal code.
    /// </summary>
    public string? CompanyPostalCode { get; set; }

    /// <summary>
    /// Company country.
    /// </summary>
    public string? CompanyCountry { get; set; }

    /// <summary>
    /// Company phone number.
    /// </summary>
    public string? CompanyPhone { get; set; }

    /// <summary>
    /// Company email.
    /// </summary>
    public string? CompanyEmail { get; set; }

    /// <summary>
    /// Company website.
    /// </summary>
    public string? CompanyWebsite { get; set; }

    /// <summary>
    /// Tax ID / VAT number.
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Company logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    #endregion

    #region Customer Information

    /// <summary>
    /// Customer name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email.
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Customer phone.
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Billing address (JSON serialized).
    /// </summary>
    public string? BillingAddressJson { get; set; }

    /// <summary>
    /// Shipping address (JSON serialized).
    /// </summary>
    public string? ShippingAddressJson { get; set; }

    #endregion

    #region Amounts

    /// <summary>
    /// Currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Subtotal before tax and shipping.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Discount amount.
    /// </summary>
    public decimal DiscountTotal { get; set; }

    /// <summary>
    /// Shipping amount.
    /// </summary>
    public decimal ShippingTotal { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal TaxTotal { get; set; }

    /// <summary>
    /// Grand total.
    /// </summary>
    public decimal GrandTotal { get; set; }

    /// <summary>
    /// Amount paid.
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Balance due.
    /// </summary>
    public decimal BalanceDue => GrandTotal - PaidAmount;

    #endregion

    #region Tax Configuration

    /// <summary>
    /// Tax system type (GST, VAT, SalesTax, None).
    /// </summary>
    public TaxSystemType TaxSystem { get; set; } = TaxSystemType.None;

    /// <summary>
    /// Tax label to display (e.g., "GST", "VAT", "Tax", "Sales Tax").
    /// </summary>
    public string TaxLabel { get; set; } = "Tax";

    /// <summary>
    /// Whether GST is applicable to this invoice (India).
    /// </summary>
    public bool IsGstApplicable { get; set; }

    /// <summary>
    /// VAT number of the company (EU/UK).
    /// </summary>
    public string? CompanyVatNumber { get; set; }

    /// <summary>
    /// VAT number of the customer (B2B).
    /// </summary>
    public string? CustomerVatNumber { get; set; }

    /// <summary>
    /// VAT rate percentage.
    /// </summary>
    public decimal VatRate { get; set; }

    /// <summary>
    /// VAT amount.
    /// </summary>
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Whether this is a reverse charge invoice (B2B cross-border EU).
    /// </summary>
    public bool IsReverseCharge { get; set; }

    #endregion

    #region GST Breakdown (India)

    /// <summary>
    /// Central GST amount.
    /// </summary>
    public decimal CgstAmount { get; set; }

    /// <summary>
    /// State GST amount.
    /// </summary>
    public decimal SgstAmount { get; set; }

    /// <summary>
    /// Integrated GST amount (for inter-state transactions).
    /// </summary>
    public decimal IgstAmount { get; set; }

    /// <summary>
    /// CGST rate percentage.
    /// </summary>
    public decimal CgstRate { get; set; }

    /// <summary>
    /// SGST rate percentage.
    /// </summary>
    public decimal SgstRate { get; set; }

    /// <summary>
    /// IGST rate percentage.
    /// </summary>
    public decimal IgstRate { get; set; }

    /// <summary>
    /// Company GSTIN (GST Identification Number).
    /// </summary>
    public string? CompanyGstin { get; set; }

    /// <summary>
    /// Customer GSTIN (for B2B invoices).
    /// </summary>
    public string? CustomerGstin { get; set; }

    /// <summary>
    /// Place of supply (state code for GST).
    /// </summary>
    public string? PlaceOfSupply { get; set; }

    /// <summary>
    /// Whether this is an inter-state transaction.
    /// </summary>
    public bool IsInterState { get; set; }

    /// <summary>
    /// GST breakdown by line item (JSON serialized).
    /// Contains HSN/SAC codes and tax amounts per item.
    /// </summary>
    public string? GstBreakdownJson { get; set; }

    /// <summary>
    /// Invoice Reference Number (IRN) for e-invoicing compliance.
    /// </summary>
    public string? Irn { get; set; }

    /// <summary>
    /// Supply type code (e.g., B2B, B2C, SEZWP, SEZWOP).
    /// </summary>
    public string? SupplyTypeCode { get; set; }

    /// <summary>
    /// Document type code (e.g., INV, CRN, DBN).
    /// </summary>
    public string? DocumentTypeCode { get; set; }

    /// <summary>
    /// QR code data for e-invoice verification.
    /// </summary>
    public string? QrCodeData { get; set; }

    /// <summary>
    /// Acknowledgement number from GST portal.
    /// </summary>
    public string? AcknowledgementNumber { get; set; }

    /// <summary>
    /// Acknowledgement date from GST portal.
    /// </summary>
    public DateTime? AcknowledgementDate { get; set; }

    #endregion

    #region Custom Fields

    /// <summary>
    /// Custom field 1 label.
    /// </summary>
    public string? CustomField1Label { get; set; }

    /// <summary>
    /// Custom field 1 value.
    /// </summary>
    public string? CustomField1Value { get; set; }

    /// <summary>
    /// Custom field 2 label.
    /// </summary>
    public string? CustomField2Label { get; set; }

    /// <summary>
    /// Custom field 2 value.
    /// </summary>
    public string? CustomField2Value { get; set; }

    /// <summary>
    /// Custom field 3 label.
    /// </summary>
    public string? CustomField3Label { get; set; }

    /// <summary>
    /// Custom field 3 value.
    /// </summary>
    public string? CustomField3Value { get; set; }

    /// <summary>
    /// Signature image URL.
    /// </summary>
    public string? SignatureImageUrl { get; set; }

    /// <summary>
    /// Amount in words.
    /// </summary>
    public string? AmountInWords { get; set; }

    #endregion

    #region Content

    /// <summary>
    /// Invoice line items (JSON serialized).
    /// </summary>
    public string? LineItemsJson { get; set; }

    /// <summary>
    /// Notes to appear on the invoice.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Terms and conditions.
    /// </summary>
    public string? Terms { get; set; }

    /// <summary>
    /// Footer text.
    /// </summary>
    public string? Footer { get; set; }

    /// <summary>
    /// Payment instructions.
    /// </summary>
    public string? PaymentInstructions { get; set; }

    #endregion

    /// <summary>
    /// Template ID used for this invoice.
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Navigation property to order.
    /// </summary>
    public Order? Order { get; set; }
}

/// <summary>
/// Invoice status enum.
/// </summary>
public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    PartiallyPaid,
    Overdue,
    Voided,
    Refunded
}
