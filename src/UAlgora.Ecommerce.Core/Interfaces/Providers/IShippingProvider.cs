using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Providers;

/// <summary>
/// Provider interface for shipping carrier integrations.
/// </summary>
public interface IShippingProvider
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
    /// Gets supported shipping methods.
    /// </summary>
    IReadOnlyList<string> SupportedMethods { get; }

    /// <summary>
    /// Checks if the provider is available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets available shipping rates.
    /// </summary>
    Task<ShippingRateResponse> GetRatesAsync(ShippingRateRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validates a shipping address.
    /// </summary>
    Task<AddressValidationResponse> ValidateAddressAsync(Address address, CancellationToken ct = default);

    /// <summary>
    /// Creates a shipment.
    /// </summary>
    Task<CreateShipmentResponse> CreateShipmentAsync(CreateShipmentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Purchases a shipping label.
    /// </summary>
    Task<ShippingLabelResponse> PurchaseLabelAsync(PurchaseLabelRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets tracking information.
    /// </summary>
    Task<TrackingResponse> GetTrackingAsync(string trackingNumber, CancellationToken ct = default);

    /// <summary>
    /// Cancels a shipment.
    /// </summary>
    Task<CancelShipmentResponse> CancelShipmentAsync(string shipmentId, CancellationToken ct = default);

    /// <summary>
    /// Gets estimated delivery date.
    /// </summary>
    Task<DeliveryEstimateResponse> GetDeliveryEstimateAsync(DeliveryEstimateRequest request, CancellationToken ct = default);

    /// <summary>
    /// Creates a return label.
    /// </summary>
    Task<ShippingLabelResponse> CreateReturnLabelAsync(CreateReturnLabelRequest request, CancellationToken ct = default);

    /// <summary>
    /// Checks if shipping is available to a destination.
    /// </summary>
    Task<bool> CanShipToAsync(Address destination, CancellationToken ct = default);
}

/// <summary>
/// Shipping rate request.
/// </summary>
public class ShippingRateRequest
{
    public Address FromAddress { get; set; } = new();
    public Address ToAddress { get; set; } = new();
    public List<ShippingPackage> Packages { get; set; } = [];
    public DateTime? ShipDate { get; set; }
    public bool IncludeInsurance { get; set; }
    public decimal? InsuredValue { get; set; }
    public bool RequireSignature { get; set; }
}

/// <summary>
/// Shipping package dimensions.
/// </summary>
public class ShippingPackage
{
    public decimal Weight { get; set; }
    public string WeightUnit { get; set; } = "lb";
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string DimensionUnit { get; set; } = "in";
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Shipping rate response.
/// </summary>
public class ShippingRateResponse
{
    public bool Success { get; set; }
    public List<ShippingRate> Rates { get; set; } = [];
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual shipping rate.
/// </summary>
public class ShippingRate
{
    public string RateId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public bool IsGuaranteed { get; set; }
    public decimal? InsuranceCost { get; set; }
    public bool IncludesTracking { get; set; } = true;
}

/// <summary>
/// Address validation response.
/// </summary>
public class AddressValidationResponse
{
    public bool IsValid { get; set; }
    public bool IsResidential { get; set; }
    public Address? SuggestedAddress { get; set; }
    public List<string> ValidationMessages { get; set; } = [];
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request to create a shipment.
/// </summary>
public class CreateShipmentRequest
{
    public Guid OrderId { get; set; }
    public Address FromAddress { get; set; } = new();
    public Address ToAddress { get; set; } = new();
    public List<ShippingPackage> Packages { get; set; } = [];
    public string ServiceCode { get; set; } = string.Empty;
    public bool IncludeInsurance { get; set; }
    public decimal? InsuredValue { get; set; }
    public bool RequireSignature { get; set; }
    public string? Reference { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}

/// <summary>
/// Create shipment response.
/// </summary>
public class CreateShipmentResponse
{
    public bool Success { get; set; }
    public string? ShipmentId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request to purchase a shipping label.
/// </summary>
public class PurchaseLabelRequest
{
    public string ShipmentId { get; set; } = string.Empty;
    public string? RateId { get; set; }
    public string LabelFormat { get; set; } = "PDF";
    public string LabelSize { get; set; } = "4x6";
}

/// <summary>
/// Shipping label response.
/// </summary>
public class ShippingLabelResponse
{
    public bool Success { get; set; }
    public string? LabelId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? LabelUrl { get; set; }
    public byte[]? LabelData { get; set; }
    public string? LabelFormat { get; set; }
    public decimal Cost { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tracking response.
/// </summary>
public class TrackingResponse
{
    public bool Success { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public string? Status { get; set; }
    public string? StatusDescription { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? SignedBy { get; set; }
    public List<TrackingEventInfo> Events { get; set; } = [];
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tracking event information.
/// </summary>
public class TrackingEventInfo
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
}

/// <summary>
/// Cancel shipment response.
/// </summary>
public class CancelShipmentResponse
{
    public bool Success { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Delivery estimate request.
/// </summary>
public class DeliveryEstimateRequest
{
    public Address FromAddress { get; set; } = new();
    public Address ToAddress { get; set; } = new();
    public string ServiceCode { get; set; } = string.Empty;
    public DateTime? ShipDate { get; set; }
}

/// <summary>
/// Delivery estimate response.
/// </summary>
public class DeliveryEstimateResponse
{
    public bool Success { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public bool IsGuaranteed { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request to create a return label.
/// </summary>
public class CreateReturnLabelRequest
{
    public Guid OrderId { get; set; }
    public string OriginalTrackingNumber { get; set; } = string.Empty;
    public Address FromAddress { get; set; } = new();
    public Address ToAddress { get; set; } = new();
    public List<ShippingPackage> Packages { get; set; } = [];
    public string? ServiceCode { get; set; }
    public string LabelFormat { get; set; } = "PDF";
}
