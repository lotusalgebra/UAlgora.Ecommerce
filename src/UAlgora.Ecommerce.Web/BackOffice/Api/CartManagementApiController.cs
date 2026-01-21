using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for cart management in the backoffice.
/// </summary>
[ApiController]
[BackOfficeRoute("ecommerce/cart")]
[MapToApi("ecommerce-management-api")]
public class CartManagementApiController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartManagementApiController(ICartService cartService)
    {
        _cartService = cartService;
    }

    #region Carts

    /// <summary>
    /// Gets all carts with optional filtering.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetCarts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] bool? isGuest = null,
        [FromQuery] bool? isAbandoned = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken ct = default)
    {
        var result = await _cartService.GetPagedCartsAsync(page, pageSize, customerId, isGuest, isAbandoned, sortBy, descending, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a cart by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCart(Guid id, CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartByIdAsync(id, ct);
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    /// <summary>
    /// Gets carts by customer ID.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerCarts(Guid customerId, CancellationToken ct = default)
    {
        var carts = await _cartService.GetCartsByCustomerIdAsync(customerId, ct);
        return Ok(new CartListResponse { Items = carts });
    }

    /// <summary>
    /// Gets abandoned carts.
    /// </summary>
    [HttpGet("abandoned")]
    public async Task<IActionResult> GetAbandonedCarts([FromQuery] int daysOld = 7, CancellationToken ct = default)
    {
        var carts = await _cartService.GetAbandonedCartsAsync(daysOld, ct);
        return Ok(new CartListResponse { Items = carts });
    }

    /// <summary>
    /// Deletes a cart.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCart(Guid id, CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartByIdAsync(id, ct);
        if (cart == null)
        {
            return NotFound();
        }

        await _cartService.DeleteCartAsync(id, ct);
        return NoContent();
    }

    #endregion

    #region Cart Actions

    /// <summary>
    /// Updates cart notes.
    /// </summary>
    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> UpdateNotes(Guid id, [FromBody] UpdateCartNotesRequest request, CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartByIdAsync(id, ct);
        if (cart == null)
        {
            return NotFound();
        }

        var updated = await _cartService.UpdateCartNotesAsync(id, request.Notes, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Clears all items from a cart.
    /// </summary>
    [HttpPost("{id:guid}/clear")]
    public async Task<IActionResult> ClearCart(Guid id, CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartByIdAsync(id, ct);
        if (cart == null)
        {
            return NotFound();
        }

        var updated = await _cartService.ClearCartByIdAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Sets cart expiration.
    /// </summary>
    [HttpPost("{id:guid}/set-expiration")]
    public async Task<IActionResult> SetExpiration(Guid id, [FromBody] SetCartExpirationRequest request, CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartByIdAsync(id, ct);
        if (cart == null)
        {
            return NotFound();
        }

        var updated = await _cartService.SetCartExpirationAsync(id, request.ExpiresAt, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Removes cart expiration.
    /// </summary>
    [HttpPost("{id:guid}/remove-expiration")]
    public async Task<IActionResult> RemoveExpiration(Guid id, CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartByIdAsync(id, ct);
        if (cart == null)
        {
            return NotFound();
        }

        var updated = await _cartService.SetCartExpirationAsync(id, null, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Assigns a cart to a customer.
    /// </summary>
    [HttpPost("{id:guid}/assign-customer")]
    public async Task<IActionResult> AssignCustomer(Guid id, [FromBody] AssignCartCustomerRequest request, CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartByIdAsync(id, ct);
        if (cart == null)
        {
            return NotFound();
        }

        try
        {
            var updated = await _cartService.AssignCartToCustomerAsync(id, request.CustomerId, ct);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Deletes expired carts.
    /// </summary>
    [HttpPost("bulk/delete-expired")]
    public async Task<IActionResult> DeleteExpiredCarts(CancellationToken ct = default)
    {
        var count = await _cartService.DeleteExpiredCartsAsync(ct);
        return Ok(new { deletedCount = count, message = $"Deleted {count} expired cart(s)" });
    }

    /// <summary>
    /// Deletes abandoned carts.
    /// </summary>
    [HttpPost("bulk/delete-abandoned")]
    public async Task<IActionResult> DeleteAbandonedCarts([FromBody] DeleteAbandonedCartsRequest? request, CancellationToken ct = default)
    {
        var daysOld = request?.DaysOld ?? 30;
        var count = await _cartService.DeleteAbandonedCartsAsync(daysOld, ct);
        return Ok(new { deletedCount = count, message = $"Deleted {count} abandoned cart(s) older than {daysOld} days" });
    }

    /// <summary>
    /// Deletes multiple carts.
    /// </summary>
    [HttpPost("bulk/delete")]
    public async Task<IActionResult> BulkDeleteCarts([FromBody] BulkDeleteCartsRequest request, CancellationToken ct = default)
    {
        var deletedCount = 0;
        var errors = new List<string>();

        foreach (var id in request.CartIds)
        {
            try
            {
                await _cartService.DeleteCartAsync(id, ct);
                deletedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Cart {id}: {ex.Message}");
            }
        }

        return Ok(new
        {
            deletedCount,
            totalRequested = request.CartIds.Count,
            errors = errors.Count > 0 ? errors : null
        });
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets cart statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken ct = default)
    {
        var stats = await _cartService.GetStatisticsAsync(ct);
        return Ok(stats);
    }

    #endregion
}

#region Request/Response Models

public class CartListResponse
{
    public List<Cart> Items { get; set; } = [];
}

public class UpdateCartNotesRequest
{
    public string? Notes { get; set; }
}

public class SetCartExpirationRequest
{
    public DateTime? ExpiresAt { get; set; }
}

public class AssignCartCustomerRequest
{
    public Guid CustomerId { get; set; }
}

public class DeleteAbandonedCartsRequest
{
    public int DaysOld { get; set; } = 30;
}

public class BulkDeleteCartsRequest
{
    public List<Guid> CartIds { get; set; } = [];
}

#endregion
