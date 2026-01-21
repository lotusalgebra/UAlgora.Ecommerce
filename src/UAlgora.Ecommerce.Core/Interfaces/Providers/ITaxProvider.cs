using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Providers;

/// <summary>
/// Provider interface for tax calculation integrations.
/// </summary>
public interface ITaxProvider
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Checks if the provider is available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Calculates tax for an order.
    /// </summary>
    Task<TaxCalculationResponse> CalculateTaxAsync(TaxCalculationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Creates a tax transaction (commits the tax).
    /// </summary>
    Task<TaxTransactionResponse> CreateTransactionAsync(TaxTransactionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Voids a tax transaction.
    /// </summary>
    Task<TaxVoidResponse> VoidTransactionAsync(string transactionId, CancellationToken ct = default);

    /// <summary>
    /// Refunds a tax transaction.
    /// </summary>
    Task<TaxRefundResponse> RefundTransactionAsync(TaxRefundRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets tax rates for a location.
    /// </summary>
    Task<TaxRateResponse> GetTaxRatesAsync(Address address, CancellationToken ct = default);

    /// <summary>
    /// Validates a tax exemption certificate.
    /// </summary>
    Task<TaxExemptionValidationResponse> ValidateExemptionAsync(string exemptionNumber, string? state = null, CancellationToken ct = default);

    /// <summary>
    /// Gets a tax transaction by ID.
    /// </summary>
    Task<TaxTransactionResponse> GetTransactionAsync(string transactionId, CancellationToken ct = default);

    /// <summary>
    /// Checks if tax should be collected for a destination.
    /// </summary>
    Task<bool> HasNexusAsync(Address address, CancellationToken ct = default);
}

/// <summary>
/// Tax calculation request.
/// </summary>
public class TaxCalculationRequest
{
    public Guid? OrderId { get; set; }
    public string? CustomerCode { get; set; }
    public Address FromAddress { get; set; } = new();
    public Address ToAddress { get; set; } = new();
    public List<TaxLineItem> LineItems { get; set; } = [];
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? ExemptionNumber { get; set; }
    public bool IsCommit { get; set; }
    public DateTime? TransactionDate { get; set; }
}

/// <summary>
/// Tax line item.
/// </summary>
public class TaxLineItem
{
    public string LineId { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? TaxCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool IsTaxExempt { get; set; }
}

/// <summary>
/// Tax calculation response.
/// </summary>
public class TaxCalculationResponse
{
    public bool Success { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalTaxable { get; set; }
    public decimal TotalExempt { get; set; }
    public List<TaxLineResult> LineResults { get; set; } = [];
    public List<TaxJurisdiction> Jurisdictions { get; set; } = [];
    public string? TransactionId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tax calculation result per line.
/// </summary>
public class TaxLineResult
{
    public string LineId { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal EffectiveRate { get; set; }
    public bool IsExempt { get; set; }
    public List<TaxJurisdiction> Jurisdictions { get; set; } = [];
}

/// <summary>
/// Tax jurisdiction breakdown.
/// </summary>
public class TaxJurisdiction
{
    public string JurisdictionType { get; set; } = string.Empty;
    public string JurisdictionName { get; set; } = string.Empty;
    public string? JurisdictionCode { get; set; }
    public decimal Rate { get; set; }
    public decimal TaxAmount { get; set; }
}

/// <summary>
/// Tax transaction request.
/// </summary>
public class TaxTransactionRequest
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public Address FromAddress { get; set; } = new();
    public Address ToAddress { get; set; } = new();
    public List<TaxLineItem> LineItems { get; set; } = [];
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? ExemptionNumber { get; set; }
    public DateTime TransactionDate { get; set; }
}

/// <summary>
/// Tax transaction response.
/// </summary>
public class TaxTransactionResponse
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? TransactionCode { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? TransactionDate { get; set; }
    public List<TaxLineResult> LineResults { get; set; } = [];
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tax void response.
/// </summary>
public class TaxVoidResponse
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tax refund request.
/// </summary>
public class TaxRefundRequest
{
    public string OriginalTransactionId { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public string? RefundReason { get; set; }
    public List<TaxLineItem>? LineItems { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime RefundDate { get; set; }
}

/// <summary>
/// Tax refund response.
/// </summary>
public class TaxRefundResponse
{
    public bool Success { get; set; }
    public string? RefundTransactionId { get; set; }
    public decimal TaxRefunded { get; set; }
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tax rate response.
/// </summary>
public class TaxRateResponse
{
    public bool Success { get; set; }
    public decimal TotalRate { get; set; }
    public List<TaxJurisdiction> Jurisdictions { get; set; } = [];
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tax exemption validation response.
/// </summary>
public class TaxExemptionValidationResponse
{
    public bool Success { get; set; }
    public bool IsValid { get; set; }
    public string? ExemptionNumber { get; set; }
    public string? ExemptionType { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public List<string>? ValidStates { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
