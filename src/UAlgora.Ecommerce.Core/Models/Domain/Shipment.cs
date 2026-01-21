using UAlgora.Ecommerce.Core.Constants;

namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a shipment for an order.
/// </summary>
public class Shipment : BaseEntity
{
    /// <summary>
    /// Order ID this shipment is for.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Shipment number for reference.
    /// </summary>
    public string? ShipmentNumber { get; set; }

    /// <summary>
    /// Shipment status.
    /// </summary>
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    /// <summary>
    /// Shipping carrier name.
    /// </summary>
    public string? Carrier { get; set; }

    /// <summary>
    /// Carrier code (e.g., "ups", "fedex").
    /// </summary>
    public string? CarrierCode { get; set; }

    /// <summary>
    /// Shipping service/method (e.g., "Ground", "Express").
    /// </summary>
    public string? Service { get; set; }

    /// <summary>
    /// Tracking number.
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Tracking URL.
    /// </summary>
    public string? TrackingUrl { get; set; }

    /// <summary>
    /// Items included in this shipment.
    /// </summary>
    public List<ShipmentItem> Items { get; set; } = [];

    #region Package Details

    /// <summary>
    /// Package weight.
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Weight unit.
    /// </summary>
    public string WeightUnit { get; set; } = "kg";

    /// <summary>
    /// Package length.
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// Package width.
    /// </summary>
    public decimal? Width { get; set; }

    /// <summary>
    /// Package height.
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Dimension unit.
    /// </summary>
    public string DimensionUnit { get; set; } = "cm";

    #endregion

    #region Costs

    /// <summary>
    /// Shipping cost.
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Insurance cost.
    /// </summary>
    public decimal? InsuranceCost { get; set; }

    /// <summary>
    /// Declared value for insurance.
    /// </summary>
    public decimal? DeclaredValue { get; set; }

    #endregion

    #region Shipping Label

    /// <summary>
    /// Label URL (for downloading/printing).
    /// </summary>
    public string? LabelUrl { get; set; }

    /// <summary>
    /// Label format (e.g., "PDF", "ZPL").
    /// </summary>
    public string? LabelFormat { get; set; }

    /// <summary>
    /// Commercial invoice URL (for international).
    /// </summary>
    public string? CommercialInvoiceUrl { get; set; }

    #endregion

    #region Addresses

    /// <summary>
    /// Ship from address ID.
    /// </summary>
    public Guid? ShipFromAddressId { get; set; }

    /// <summary>
    /// Ship from address.
    /// </summary>
    public Address? ShipFromAddress { get; set; }

    /// <summary>
    /// Ship to address ID.
    /// </summary>
    public Guid? ShipToAddressId { get; set; }

    /// <summary>
    /// Ship to address.
    /// </summary>
    public Address? ShipToAddress { get; set; }

    #endregion

    #region Timestamps

    /// <summary>
    /// When the label was created.
    /// </summary>
    public DateTime? LabelCreatedAt { get; set; }

    /// <summary>
    /// When the shipment was picked up.
    /// </summary>
    public DateTime? PickedUpAt { get; set; }

    /// <summary>
    /// When the shipment was shipped.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// When the shipment was delivered.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Estimated delivery date.
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    #endregion

    #region Delivery Details

    /// <summary>
    /// Delivery instructions.
    /// </summary>
    public string? DeliveryInstructions { get; set; }

    /// <summary>
    /// Whether signature is required.
    /// </summary>
    public bool SignatureRequired { get; set; }

    /// <summary>
    /// Recipient name who signed for delivery.
    /// </summary>
    public string? SignedBy { get; set; }

    /// <summary>
    /// Proof of delivery URL.
    /// </summary>
    public string? ProofOfDeliveryUrl { get; set; }

    #endregion

    /// <summary>
    /// Notes about the shipment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to order.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// Tracking events history.
    /// </summary>
    public List<TrackingEvent> TrackingEvents { get; set; } = [];

    #region Computed Properties

    /// <summary>
    /// Total items in this shipment.
    /// </summary>
    public int TotalItems => Items.Sum(i => i.Quantity);

    /// <summary>
    /// Whether the shipment has been delivered.
    /// </summary>
    public bool IsDelivered => Status == ShipmentStatus.Delivered;

    /// <summary>
    /// Whether tracking is available.
    /// </summary>
    public bool HasTracking => !string.IsNullOrWhiteSpace(TrackingNumber);

    #endregion
}

/// <summary>
/// Represents an item in a shipment.
/// </summary>
public class ShipmentItem : BaseEntity
{
    /// <summary>
    /// Shipment ID.
    /// </summary>
    public Guid ShipmentId { get; set; }

    /// <summary>
    /// Order line ID.
    /// </summary>
    public Guid OrderLineId { get; set; }

    /// <summary>
    /// Quantity shipped.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Navigation property to shipment.
    /// </summary>
    public Shipment? Shipment { get; set; }

    /// <summary>
    /// Navigation property to order line.
    /// </summary>
    public OrderLine? OrderLine { get; set; }
}

/// <summary>
/// Represents a tracking event for a shipment.
/// </summary>
public class TrackingEvent
{
    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Event status code.
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// Event description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// City where event occurred.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/province where event occurred.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Country where event occurred.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Postal code where event occurred.
    /// </summary>
    public string? PostalCode { get; set; }
}
