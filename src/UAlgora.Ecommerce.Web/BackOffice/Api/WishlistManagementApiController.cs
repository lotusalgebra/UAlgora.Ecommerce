using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for wishlist management in the backoffice.
/// </summary>
[ApiController]
[BackOfficeRoute("ecommerce/wishlist")]
[MapToApi("ecommerce-management-api")]
public class WishlistManagementApiController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistManagementApiController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    #region Wishlists

    /// <summary>
    /// Gets all wishlists with optional filtering.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetWishlists(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] bool? isPublic = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken ct = default)
    {
        var result = await _wishlistService.GetPagedWishlistsAsync(page, pageSize, customerId, isPublic, sortBy, descending, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a wishlist by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWishlist(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, true, ct);
        if (wishlist == null)
        {
            return NotFound();
        }
        return Ok(wishlist);
    }

    /// <summary>
    /// Gets wishlists by customer ID.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerWishlists(Guid customerId, CancellationToken ct = default)
    {
        var wishlists = await _wishlistService.GetWishlistsByCustomerIdAsync(customerId, ct);
        return Ok(new WishlistListResponse { Items = wishlists });
    }

    /// <summary>
    /// Gets a wishlist by share token.
    /// </summary>
    [HttpGet("share/{token}")]
    public async Task<IActionResult> GetWishlistByShareToken(string token, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByShareTokenAsync(token, ct);
        if (wishlist == null)
        {
            return NotFound(new { message = "Wishlist not found or is not public" });
        }
        return Ok(wishlist);
    }

    /// <summary>
    /// Creates a new wishlist.
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> CreateWishlist([FromBody] CreateWishlistRequest request, CancellationToken ct = default)
    {
        var wishlist = new Wishlist
        {
            CustomerId = request.CustomerId,
            Name = request.Name,
            IsDefault = request.IsDefault,
            IsPublic = request.IsPublic
        };

        var created = await _wishlistService.CreateWishlistAsync(wishlist, ct);
        return CreatedAtAction(nameof(GetWishlist), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a wishlist.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWishlist(Guid id, [FromBody] UpdateWishlistRequest request, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        wishlist.Name = request.Name;
        wishlist.IsDefault = request.IsDefault;
        wishlist.IsPublic = request.IsPublic;

        var updated = await _wishlistService.UpdateWishlistAsync(wishlist, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a wishlist.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWishlist(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        await _wishlistService.DeleteWishlistAsync(id, ct);
        return NoContent();
    }

    #endregion

    #region Wishlist Actions

    /// <summary>
    /// Sets a wishlist as default.
    /// </summary>
    [HttpPost("{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultWishlist(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        var updated = await _wishlistService.SetDefaultWishlistAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Toggles the public status of a wishlist.
    /// </summary>
    [HttpPost("{id:guid}/toggle-public")]
    public async Task<IActionResult> TogglePublic(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        var updated = await _wishlistService.TogglePublicAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Generates a share token for a wishlist.
    /// </summary>
    [HttpPost("{id:guid}/generate-share-token")]
    public async Task<IActionResult> GenerateShareToken(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        var updated = await _wishlistService.GenerateShareTokenAsync(id, ct);
        return Ok(new { shareToken = updated.ShareToken, shareUrl = $"/wishlist/share/{updated.ShareToken}" });
    }

    /// <summary>
    /// Removes the share token from a wishlist.
    /// </summary>
    [HttpPost("{id:guid}/remove-share-token")]
    public async Task<IActionResult> RemoveShareToken(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        var updated = await _wishlistService.RemoveShareTokenAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Renames a wishlist.
    /// </summary>
    [HttpPost("{id:guid}/rename")]
    public async Task<IActionResult> RenameWishlist(Guid id, [FromBody] RenameWishlistRequest request, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var updated = await _wishlistService.RenameWishlistAsync(id, request.Name, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Duplicates a wishlist.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateWishlist(Guid id, [FromBody] DuplicateWishlistRequest? request, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        var duplicate = await _wishlistService.DuplicateWishlistAsync(id, request?.NewName, ct);
        return Ok(duplicate);
    }

    /// <summary>
    /// Clears all items from a wishlist.
    /// </summary>
    [HttpPost("{id:guid}/clear")]
    public async Task<IActionResult> ClearWishlist(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        var updated = await _wishlistService.ClearWishlistAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Merges items from source wishlist into target wishlist.
    /// </summary>
    [HttpPost("{id:guid}/merge")]
    public async Task<IActionResult> MergeWishlists(Guid id, [FromBody] MergeWishlistRequest request, CancellationToken ct = default)
    {
        var source = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (source == null)
        {
            return NotFound(new { message = "Source wishlist not found" });
        }

        var target = await _wishlistService.GetWishlistByIdAsync(request.TargetWishlistId, false, ct);
        if (target == null)
        {
            return BadRequest(new { message = "Target wishlist not found" });
        }

        var updated = await _wishlistService.MergeWishlistsAsync(id, request.TargetWishlistId, request.DeleteSource, ct);
        return Ok(updated);
    }

    #endregion

    #region Wishlist Items

    /// <summary>
    /// Gets a wishlist item by ID.
    /// </summary>
    [HttpGet("item/{itemId:guid}")]
    public async Task<IActionResult> GetWishlistItem(Guid itemId, CancellationToken ct = default)
    {
        var item = await _wishlistService.GetWishlistItemByIdAsync(itemId, ct);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    /// <summary>
    /// Adds an item to a wishlist.
    /// </summary>
    [HttpPost("{id:guid}/item")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddWishlistItemRequest request, CancellationToken ct = default)
    {
        var wishlist = await _wishlistService.GetWishlistByIdAsync(id, false, ct);
        if (wishlist == null)
        {
            return NotFound();
        }

        var item = await _wishlistService.AddItemAsync(
            id,
            request.ProductId,
            request.VariantId,
            request.PriceWhenAdded,
            request.Notes,
            request.Quantity,
            request.Priority,
            ct);

        return Ok(item);
    }

    /// <summary>
    /// Removes an item from a wishlist.
    /// </summary>
    [HttpDelete("{id:guid}/item/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken ct = default)
    {
        await _wishlistService.RemoveItemAsync(id, itemId, ct);
        return NoContent();
    }

    /// <summary>
    /// Updates item notes.
    /// </summary>
    [HttpPost("item/{itemId:guid}/notes")]
    public async Task<IActionResult> UpdateItemNotes(Guid itemId, [FromBody] UpdateItemNotesRequest request, CancellationToken ct = default)
    {
        var item = await _wishlistService.GetWishlistItemByIdAsync(itemId, ct);
        if (item == null)
        {
            return NotFound();
        }

        var updated = await _wishlistService.UpdateItemNotesAsync(itemId, request.Notes, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates item quantity.
    /// </summary>
    [HttpPost("item/{itemId:guid}/quantity")]
    public async Task<IActionResult> UpdateItemQuantity(Guid itemId, [FromBody] UpdateItemQuantityRequest request, CancellationToken ct = default)
    {
        var item = await _wishlistService.GetWishlistItemByIdAsync(itemId, ct);
        if (item == null)
        {
            return NotFound();
        }

        if (request.Quantity < 1)
        {
            return BadRequest(new { message = "Quantity must be at least 1" });
        }

        var updated = await _wishlistService.UpdateItemQuantityAsync(itemId, request.Quantity, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates item priority.
    /// </summary>
    [HttpPost("item/{itemId:guid}/priority")]
    public async Task<IActionResult> UpdateItemPriority(Guid itemId, [FromBody] UpdateItemPriorityRequest request, CancellationToken ct = default)
    {
        var item = await _wishlistService.GetWishlistItemByIdAsync(itemId, ct);
        if (item == null)
        {
            return NotFound();
        }

        var updated = await _wishlistService.UpdateItemPriorityAsync(itemId, request.Priority, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Moves an item to another wishlist.
    /// </summary>
    [HttpPost("item/{itemId:guid}/move")]
    public async Task<IActionResult> MoveItem(Guid itemId, [FromBody] MoveItemRequest request, CancellationToken ct = default)
    {
        var item = await _wishlistService.GetWishlistItemByIdAsync(itemId, ct);
        if (item == null)
        {
            return NotFound();
        }

        var target = await _wishlistService.GetWishlistByIdAsync(request.TargetWishlistId, false, ct);
        if (target == null)
        {
            return BadRequest(new { message = "Target wishlist not found" });
        }

        var updated = await _wishlistService.MoveItemAsync(itemId, request.TargetWishlistId, ct);
        return Ok(updated);
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets wishlist statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken ct = default)
    {
        var stats = await _wishlistService.GetStatisticsAsync(ct);
        return Ok(stats);
    }

    /// <summary>
    /// Gets the most wishlisted products.
    /// </summary>
    [HttpGet("most-wishlisted")]
    public async Task<IActionResult> GetMostWishlisted([FromQuery] int count = 10, CancellationToken ct = default)
    {
        var products = await _wishlistService.GetMostWishlistedProductsAsync(count, ct);
        return Ok(products);
    }

    #endregion
}

#region Request/Response Models

public class WishlistListResponse
{
    public List<Wishlist> Items { get; set; } = [];
}

public class CreateWishlistRequest
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = "My Wishlist";
    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }
}

public class UpdateWishlistRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }
}

public class RenameWishlistRequest
{
    public string Name { get; set; } = string.Empty;
}

public class DuplicateWishlistRequest
{
    public string? NewName { get; set; }
}

public class MergeWishlistRequest
{
    public Guid TargetWishlistId { get; set; }
    public bool DeleteSource { get; set; }
}

public class AddWishlistItemRequest
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public decimal PriceWhenAdded { get; set; }
    public string? Notes { get; set; }
    public int Quantity { get; set; } = 1;
    public int? Priority { get; set; }
}

public class UpdateItemNotesRequest
{
    public string? Notes { get; set; }
}

public class UpdateItemQuantityRequest
{
    public int Quantity { get; set; }
}

public class UpdateItemPriorityRequest
{
    public int? Priority { get; set; }
}

public class MoveItemRequest
{
    public Guid TargetWishlistId { get; set; }
}

#endregion
