using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for store operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Stores}")]
public class StoreManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IStoreService _storeService;

    public StoreManagementApiController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    /// <summary>
    /// Gets all stores.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Store>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var stores = await _storeService.GetAllAsync();
        return Ok(stores);
    }

    /// <summary>
    /// Gets all active stores.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType<IReadOnlyList<Store>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var stores = await _storeService.GetActiveAsync();
        return Ok(stores);
    }

    /// <summary>
    /// Gets a store by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Store>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var store = await _storeService.GetByIdAsync(id);
        if (store == null)
        {
            return NotFound();
        }
        return Ok(store);
    }

    /// <summary>
    /// Gets a store by code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType<Store>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var store = await _storeService.GetByCodeAsync(code);
        if (store == null)
        {
            return NotFound();
        }
        return Ok(store);
    }

    /// <summary>
    /// Gets a store by domain.
    /// </summary>
    [HttpGet("by-domain/{domain}")]
    [ProducesResponseType<Store>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDomain(string domain)
    {
        var store = await _storeService.GetByDomainAsync(domain);
        if (store == null)
        {
            return NotFound();
        }
        return Ok(store);
    }

    /// <summary>
    /// Gets the current store from request context.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType<Store>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent()
    {
        var store = await _storeService.GetCurrentStoreAsync();
        if (store == null)
        {
            return NotFound(new { error = "No store found for current context" });
        }
        return Ok(store);
    }

    /// <summary>
    /// Creates a new store.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<Store>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateStoreRequest request)
    {
        var store = new Store
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Domain = request.Domain,
            ContactEmail = request.ContactEmail,
            SupportEmail = request.SupportEmail,
            Phone = request.Phone,
            DefaultCurrencyCode = request.DefaultCurrencyCode ?? "USD",
            DefaultLanguage = request.DefaultLanguage ?? "en-US",
            TimeZoneId = request.TimeZoneId ?? "UTC",
            TaxIncludedInPrices = request.TaxIncludedInPrices ?? false,
            AllowGuestCheckout = request.AllowGuestCheckout ?? true,
            OrderNumberPrefix = request.OrderNumberPrefix ?? "ORD",
            Status = request.Status ?? StoreStatus.Active,
            LicenseType = request.LicenseType ?? LicenseType.Trial
        };

        // Set branding if provided
        if (!string.IsNullOrEmpty(request.LogoUrl))
            store.LogoUrl = request.LogoUrl;
        if (!string.IsNullOrEmpty(request.PrimaryColor))
            store.PrimaryColor = request.PrimaryColor;

        // Set address if provided
        if (!string.IsNullOrEmpty(request.AddressLine1))
            store.AddressLine1 = request.AddressLine1;
        if (!string.IsNullOrEmpty(request.City))
            store.City = request.City;
        if (!string.IsNullOrEmpty(request.State))
            store.State = request.State;
        if (!string.IsNullOrEmpty(request.PostalCode))
            store.PostalCode = request.PostalCode;
        if (!string.IsNullOrEmpty(request.CountryCode))
            store.CountryCode = request.CountryCode;

        // Set trial expiration for trial licenses
        if (store.LicenseType == LicenseType.Trial)
        {
            store.TrialExpiresAt = DateTime.UtcNow.AddDays(14);
        }

        var created = await _storeService.CreateAsync(store);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a store.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<Store>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreRequest request)
    {
        var store = await _storeService.GetByIdAsync(id);
        if (store == null)
        {
            return NotFound();
        }

        // Update basic info
        if (!string.IsNullOrEmpty(request.Name))
            store.Name = request.Name;
        if (request.Description != null)
            store.Description = request.Description;
        if (request.Domain != null)
            store.Domain = request.Domain;

        // Update contact info
        if (request.ContactEmail != null)
            store.ContactEmail = request.ContactEmail;
        if (request.SupportEmail != null)
            store.SupportEmail = request.SupportEmail;
        if (request.Phone != null)
            store.Phone = request.Phone;

        // Update branding
        if (request.LogoUrl != null)
            store.LogoUrl = request.LogoUrl;
        if (request.PrimaryColor != null)
            store.PrimaryColor = request.PrimaryColor;

        // Update settings
        if (request.TaxIncludedInPrices.HasValue)
            store.TaxIncludedInPrices = request.TaxIncludedInPrices.Value;
        if (request.AllowGuestCheckout.HasValue)
            store.AllowGuestCheckout = request.AllowGuestCheckout.Value;
        if (request.Status.HasValue)
            store.Status = request.Status.Value;

        var updated = await _storeService.UpdateAsync(store);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a store (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _storeService.DeleteAsync(id);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Validates a store's license.
    /// </summary>
    [HttpPost("{id:guid}/validate-license")]
    [ProducesResponseType<LicenseValidationResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateLicense(Guid id)
    {
        var result = await _storeService.ValidateLicenseAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Checks if a store is in trial mode.
    /// </summary>
    [HttpGet("{id:guid}/is-trial")]
    [ProducesResponseType<StoreTrialResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsTrial(Guid id)
    {
        var isTrial = await _storeService.IsTrialAsync(id);
        var store = await _storeService.GetByIdAsync(id);

        return Ok(new StoreTrialResult
        {
            IsTrial = isTrial,
            TrialExpiresAt = store?.TrialExpiresAt,
            DaysRemaining = store?.TrialDaysRemaining
        });
    }

    /// <summary>
    /// Gets stores with expiring trials.
    /// </summary>
    [HttpGet("expiring-trials")]
    [ProducesResponseType<IReadOnlyList<Store>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringTrials([FromQuery] int days = 7)
    {
        var stores = await _storeService.GetExpiringTrialsAsync(days);
        return Ok(stores);
    }

    /// <summary>
    /// Gets the next order number for a store.
    /// </summary>
    [HttpGet("{id:guid}/next-order-number")]
    [ProducesResponseType<OrderNumberResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNextOrderNumber(Guid id)
    {
        var orderNumber = await _storeService.GetNextOrderNumberAsync(id);
        return Ok(new OrderNumberResult { OrderNumber = orderNumber });
    }
}

#region Request/Response Models

public class CreateStoreRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Domain { get; set; }
    public string? ContactEmail { get; set; }
    public string? SupportEmail { get; set; }
    public string? Phone { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? DefaultCurrencyCode { get; set; }
    public string? DefaultLanguage { get; set; }
    public string? TimeZoneId { get; set; }
    public bool? TaxIncludedInPrices { get; set; }
    public bool? AllowGuestCheckout { get; set; }
    public string? OrderNumberPrefix { get; set; }
    public StoreStatus? Status { get; set; }
    public LicenseType? LicenseType { get; set; }
}

public class UpdateStoreRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Domain { get; set; }
    public string? ContactEmail { get; set; }
    public string? SupportEmail { get; set; }
    public string? Phone { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public bool? TaxIncludedInPrices { get; set; }
    public bool? AllowGuestCheckout { get; set; }
    public StoreStatus? Status { get; set; }
}

public class StoreTrialResult
{
    public bool IsTrial { get; set; }
    public DateTime? TrialExpiresAt { get; set; }
    public int? DaysRemaining { get; set; }
}

public class OrderNumberResult
{
    public required string OrderNumber { get; set; }
}

#endregion
