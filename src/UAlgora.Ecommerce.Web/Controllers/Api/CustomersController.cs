using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using static UAlgora.Ecommerce.Web.ServiceCollectionExtensions;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for customer account operations.
/// </summary>
public class CustomersController : EcommerceApiController
{
    private readonly ICustomerService _customerService;
    private readonly ICartContextProvider _contextProvider;

    public CustomersController(
        ICustomerService customerService,
        ICartContextProvider contextProvider)
    {
        _customerService = customerService;
        _contextProvider = contextProvider;
    }

    /// <summary>
    /// Gets the current customer profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentCustomer(CancellationToken ct = default)
    {
        var customer = await _customerService.GetCurrentAsync(ct);
        if (customer == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Customer not found. Please log in." });
        }

        return ApiSuccess(customer);
    }

    /// <summary>
    /// Updates the current customer profile.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct = default)
    {
        var customer = await _customerService.GetCurrentAsync(ct);
        if (customer == null)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in to update your profile." });
        }

        customer.FirstName = request.FirstName ?? customer.FirstName;
        customer.LastName = request.LastName ?? customer.LastName;
        customer.Phone = request.Phone ?? customer.Phone;
        customer.Company = request.Company ?? customer.Company;
        customer.PreferredCurrency = request.PreferredCurrency ?? customer.PreferredCurrency;
        customer.PreferredLanguage = request.PreferredLanguage ?? customer.PreferredLanguage;

        var validation = await _customerService.ValidateAsync(customer, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var updated = await _customerService.UpdateAsync(customer, ct);
        return ApiSuccess(updated, "Profile updated successfully.");
    }

    /// <summary>
    /// Updates marketing preferences.
    /// </summary>
    [HttpPut("me/marketing")]
    public async Task<IActionResult> UpdateMarketingPreferences(
        [FromBody] MarketingPreferencesRequest request,
        CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        await _customerService.UpdateMarketingPreferencesAsync(customerId.Value, request.AcceptsMarketing, ct);
        return ApiSuccess(new { }, "Marketing preferences updated.");
    }

    #region Addresses

    /// <summary>
    /// Gets customer addresses.
    /// </summary>
    [HttpGet("me/addresses")]
    public async Task<IActionResult> GetAddresses(CancellationToken ct = default)
    {
        var customer = await _customerService.GetCurrentAsync(ct);
        if (customer == null)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        return ApiSuccess(customer.Addresses);
    }

    /// <summary>
    /// Adds a new address.
    /// </summary>
    [HttpPost("me/addresses")]
    public async Task<IActionResult> AddAddress(
        [FromBody] Address address,
        CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        try
        {
            var savedAddress = await _customerService.AddAddressAsync(customerId.Value, address, ct);
            return ApiSuccess(savedAddress, "Address added.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Updates an address.
    /// </summary>
    [HttpPut("me/addresses/{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(
        Guid addressId,
        [FromBody] Address address,
        CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        address.Id = addressId;
        address.CustomerId = customerId.Value;

        try
        {
            var updated = await _customerService.UpdateAddressAsync(address, ct);
            return ApiSuccess(updated, "Address updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Deletes an address.
    /// </summary>
    [HttpDelete("me/addresses/{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId, CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        try
        {
            await _customerService.DeleteAddressAsync(customerId.Value, addressId, ct);
            return ApiSuccess(new { }, "Address deleted.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Sets the default shipping address.
    /// </summary>
    [HttpPut("me/addresses/{addressId:guid}/default-shipping")]
    public async Task<IActionResult> SetDefaultShippingAddress(
        Guid addressId,
        CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        await _customerService.SetDefaultShippingAddressAsync(customerId.Value, addressId, ct);
        return ApiSuccess(new { }, "Default shipping address set.");
    }

    /// <summary>
    /// Sets the default billing address.
    /// </summary>
    [HttpPut("me/addresses/{addressId:guid}/default-billing")]
    public async Task<IActionResult> SetDefaultBillingAddress(
        Guid addressId,
        CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        await _customerService.SetDefaultBillingAddressAsync(customerId.Value, addressId, ct);
        return ApiSuccess(new { }, "Default billing address set.");
    }

    #endregion

    #region Orders

    /// <summary>
    /// Gets customer order history.
    /// </summary>
    [HttpGet("me/orders")]
    public async Task<IActionResult> GetOrderHistory(CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        var orders = await _customerService.GetOrderHistoryAsync(customerId.Value, ct);
        return ApiSuccess(orders);
    }

    #endregion

    #region Loyalty

    /// <summary>
    /// Gets loyalty points balance.
    /// </summary>
    [HttpGet("me/loyalty-points")]
    public async Task<IActionResult> GetLoyaltyPoints(CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return Unauthorized(new ApiErrorResponse { Message = "Please log in." });
        }

        var points = await _customerService.GetLoyaltyPointsAsync(customerId.Value, ct);
        return ApiSuccess(new { points });
    }

    #endregion
}

#region Admin Endpoints

/// <summary>
/// Admin API controller for customer management.
/// </summary>
[Route("api/ecommerce/admin/customers")]
[Authorize(Policy = EcommerceAdminPolicy)]
public class CustomersAdminController : EcommerceApiController
{
    private readonly ICustomerService _customerService;

    public CustomersAdminController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Gets paginated customers (admin).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var parameters = new CustomerQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = search
        };

        var result = await _customerService.GetPagedAsync(parameters, ct);
        return ApiPaged(result.Items, result.TotalCount, result.Page, result.PageSize);
    }

    /// <summary>
    /// Searches customers (admin).
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string q,
        [FromQuery] int maxResults = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ApiErrorResponse { Message = "Search query is required." });
        }

        var customers = await _customerService.SearchAsync(q, maxResults, ct);
        return ApiSuccess(customers);
    }

    /// <summary>
    /// Gets a customer by ID (admin).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken ct = default)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        if (customer == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Customer not found." });
        }

        return ApiSuccess(customer);
    }

    /// <summary>
    /// Gets a customer by email (admin).
    /// </summary>
    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetCustomerByEmail(string email, CancellationToken ct = default)
    {
        var customer = await _customerService.GetByEmailAsync(email, ct);
        if (customer == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Customer not found." });
        }

        return ApiSuccess(customer);
    }

    /// <summary>
    /// Gets top customers by spend (admin).
    /// </summary>
    [HttpGet("top-spenders")]
    public async Task<IActionResult> GetTopSpenders(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var customers = await _customerService.GetTopBySpentAsync(count, ct);
        return ApiSuccess(customers);
    }

    /// <summary>
    /// Creates a new customer (admin).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var customer = await _customerService.CreateAsync(request, ct);
            return ApiSuccess(customer, "Customer created.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Adds loyalty points (admin).
    /// </summary>
    [HttpPost("{id:guid}/loyalty-points/add")]
    public async Task<IActionResult> AddLoyaltyPoints(
        Guid id,
        [FromBody] LoyaltyPointsRequest request,
        CancellationToken ct = default)
    {
        if (request.Points <= 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Points must be greater than 0." });
        }

        try
        {
            var newBalance = await _customerService.AddLoyaltyPointsAsync(id, request.Points, request.Reason, ct);
            return ApiSuccess(new { balance = newBalance }, "Loyalty points added.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Deducts loyalty points (admin).
    /// </summary>
    [HttpPost("{id:guid}/loyalty-points/deduct")]
    public async Task<IActionResult> DeductLoyaltyPoints(
        Guid id,
        [FromBody] LoyaltyPointsRequest request,
        CancellationToken ct = default)
    {
        if (request.Points <= 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Points must be greater than 0." });
        }

        try
        {
            var newBalance = await _customerService.DeductLoyaltyPointsAsync(id, request.Points, request.Reason, ct);
            return ApiSuccess(new { balance = newBalance }, "Loyalty points deducted.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Adds store credit (admin).
    /// </summary>
    [HttpPost("{id:guid}/store-credit/add")]
    public async Task<IActionResult> AddStoreCredit(
        Guid id,
        [FromBody] StoreCreditRequest request,
        CancellationToken ct = default)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Amount must be greater than 0." });
        }

        try
        {
            var newBalance = await _customerService.AddStoreCreditAsync(id, request.Amount, request.Reason, ct);
            return ApiSuccess(new { balance = newBalance }, "Store credit added.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Deducts store credit (admin).
    /// </summary>
    [HttpPost("{id:guid}/store-credit/deduct")]
    public async Task<IActionResult> DeductStoreCredit(
        Guid id,
        [FromBody] StoreCreditRequest request,
        CancellationToken ct = default)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Amount must be greater than 0." });
        }

        try
        {
            var newBalance = await _customerService.DeductStoreCreditAsync(id, request.Amount, request.Reason, ct);
            return ApiSuccess(new { balance = newBalance }, "Store credit deducted.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a customer (admin).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken ct = default)
    {
        await _customerService.DeleteAsync(id, ct);
        return ApiSuccess(new { }, "Customer deleted.");
    }
}

#endregion

#region Request Models

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? PreferredCurrency { get; set; }
    public string? PreferredLanguage { get; set; }
}

public class MarketingPreferencesRequest
{
    public bool AcceptsMarketing { get; set; }
}

public class LoyaltyPointsRequest
{
    public int Points { get; set; }
    public string? Reason { get; set; }
}

public class StoreCreditRequest
{
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

#endregion
