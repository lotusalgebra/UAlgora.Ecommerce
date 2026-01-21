using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for shipping operations.
/// </summary>
public interface IShippingService
{
    #region Shipping Methods

    /// <summary>
    /// Gets a shipping method by ID.
    /// </summary>
    Task<ShippingMethod?> GetMethodByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a shipping method by code.
    /// </summary>
    Task<ShippingMethod?> GetMethodByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all shipping methods.
    /// </summary>
    Task<IReadOnlyList<ShippingMethod>> GetAllMethodsAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets active shipping methods.
    /// </summary>
    Task<IReadOnlyList<ShippingMethod>> GetActiveMethodsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets methods available for a zone.
    /// </summary>
    Task<IReadOnlyList<ShippingMethod>> GetMethodsForZoneAsync(Guid zoneId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new shipping method.
    /// </summary>
    Task<ShippingMethod> CreateMethodAsync(ShippingMethod method, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing shipping method.
    /// </summary>
    Task<ShippingMethod> UpdateMethodAsync(ShippingMethod method, CancellationToken ct = default);

    /// <summary>
    /// Deletes a shipping method.
    /// </summary>
    Task DeleteMethodAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates method sort orders.
    /// </summary>
    Task UpdateMethodSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);

    /// <summary>
    /// Toggles method active status.
    /// </summary>
    Task<ShippingMethod> ToggleMethodStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Shipping Zones

    /// <summary>
    /// Gets a shipping zone by ID.
    /// </summary>
    Task<ShippingZone?> GetZoneByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a shipping zone by code.
    /// </summary>
    Task<ShippingZone?> GetZoneByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all shipping zones.
    /// </summary>
    Task<IReadOnlyList<ShippingZone>> GetAllZonesAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets active shipping zones.
    /// </summary>
    Task<IReadOnlyList<ShippingZone>> GetActiveZonesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the default zone.
    /// </summary>
    Task<ShippingZone?> GetDefaultZoneAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets zone matching an address.
    /// </summary>
    Task<ShippingZone?> GetZoneForAddressAsync(Address address, CancellationToken ct = default);

    /// <summary>
    /// Creates a new shipping zone.
    /// </summary>
    Task<ShippingZone> CreateZoneAsync(ShippingZone zone, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing shipping zone.
    /// </summary>
    Task<ShippingZone> UpdateZoneAsync(ShippingZone zone, CancellationToken ct = default);

    /// <summary>
    /// Deletes a shipping zone.
    /// </summary>
    Task DeleteZoneAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates zone sort orders.
    /// </summary>
    Task UpdateZoneSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);

    /// <summary>
    /// Sets a zone as the default.
    /// </summary>
    Task SetDefaultZoneAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Toggles zone active status.
    /// </summary>
    Task<ShippingZone> ToggleZoneStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Shipping Rates

    /// <summary>
    /// Gets a shipping rate by ID.
    /// </summary>
    Task<ShippingRate?> GetRateByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets rates for a zone.
    /// </summary>
    Task<IReadOnlyList<ShippingRate>> GetRatesForZoneAsync(Guid zoneId, CancellationToken ct = default);

    /// <summary>
    /// Gets rates for a method.
    /// </summary>
    Task<IReadOnlyList<ShippingRate>> GetRatesForMethodAsync(Guid methodId, CancellationToken ct = default);

    /// <summary>
    /// Gets rate for a specific zone and method.
    /// </summary>
    Task<ShippingRate?> GetRateAsync(Guid zoneId, Guid methodId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new shipping rate.
    /// </summary>
    Task<ShippingRate> CreateRateAsync(ShippingRate rate, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing shipping rate.
    /// </summary>
    Task<ShippingRate> UpdateRateAsync(ShippingRate rate, CancellationToken ct = default);

    /// <summary>
    /// Deletes a shipping rate.
    /// </summary>
    Task DeleteRateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Bulk creates rates for a zone across all methods.
    /// </summary>
    Task<IReadOnlyList<ShippingRate>> CreateRatesForZoneAsync(Guid zoneId, decimal defaultRate, CancellationToken ct = default);

    #endregion

    #region Rate Calculation

    /// <summary>
    /// Gets available shipping options for an address and order.
    /// </summary>
    Task<IReadOnlyList<AvailableShippingMethod>> GetShippingOptionsAsync(
        Address shippingAddress,
        decimal orderTotal,
        decimal orderWeight,
        int itemCount,
        CancellationToken ct = default);

    /// <summary>
    /// Calculates shipping cost for a specific method.
    /// </summary>
    Task<ShippingCostResult> CalculateShippingCostAsync(
        Guid methodId,
        Address shippingAddress,
        decimal orderTotal,
        decimal orderWeight,
        int itemCount,
        CancellationToken ct = default);

    /// <summary>
    /// Validates if shipping is available to an address.
    /// </summary>
    Task<bool> CanShipToAsync(Address address, CancellationToken ct = default);

    #endregion

    #region Validation

    /// <summary>
    /// Validates a shipping method.
    /// </summary>
    Task<ValidationResult> ValidateMethodAsync(ShippingMethod method, CancellationToken ct = default);

    /// <summary>
    /// Validates a shipping zone.
    /// </summary>
    Task<ValidationResult> ValidateZoneAsync(ShippingZone zone, CancellationToken ct = default);

    /// <summary>
    /// Validates a shipping rate.
    /// </summary>
    Task<ValidationResult> ValidateRateAsync(ShippingRate rate, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Represents an available shipping method with calculated rate.
/// </summary>
public class AvailableShippingMethod
{
    /// <summary>
    /// Shipping method ID.
    /// </summary>
    public Guid MethodId { get; set; }

    /// <summary>
    /// Method code.
    /// </summary>
    public string MethodCode { get; set; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Calculated cost.
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Whether this is free shipping.
    /// </summary>
    public bool IsFree => Cost == 0;

    /// <summary>
    /// Minimum estimated delivery days.
    /// </summary>
    public int? EstimatedDaysMin { get; set; }

    /// <summary>
    /// Maximum estimated delivery days.
    /// </summary>
    public int? EstimatedDaysMax { get; set; }

    /// <summary>
    /// Delivery estimate text.
    /// </summary>
    public string? DeliveryEstimateText { get; set; }

    /// <summary>
    /// Carrier name if applicable.
    /// </summary>
    public string? CarrierName { get; set; }

    /// <summary>
    /// Icon name for display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Sort order.
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Result of shipping cost calculation.
/// </summary>
public class ShippingCostResult
{
    /// <summary>
    /// Whether calculation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Calculated cost.
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Whether this is free shipping.
    /// </summary>
    public bool IsFree { get; set; }

    /// <summary>
    /// Reason for free shipping if applicable.
    /// </summary>
    public string? FreeShippingReason { get; set; }

    /// <summary>
    /// Error message if calculation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Delivery estimate text.
    /// </summary>
    public string? DeliveryEstimateText { get; set; }
}
