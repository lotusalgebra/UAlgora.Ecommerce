using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for shopping cart operations.
/// </summary>
public class CartController : EcommerceApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Gets the current cart.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken ct = default)
    {
        try
        {
            var cart = await _cartService.GetCartAsync(ct);
            return ApiSuccess(cart);
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Gets the cart item count.
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetItemCount(CancellationToken ct = default)
    {
        var count = await _cartService.GetItemCountAsync(ct);
        return ApiSuccess(new { count });
    }

    /// <summary>
    /// Adds an item to the cart.
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddToCartApiRequest request,
        CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Quantity must be greater than 0." });
        }

        try
        {
            var addRequest = new AddToCartRequest
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Quantity = request.Quantity
            };

            var cart = await _cartService.AddItemAsync(addRequest, ct);
            return ApiSuccess(cart, "Item added to cart.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Updates item quantity.
    /// </summary>
    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItemQuantity(
        Guid itemId,
        [FromBody] UpdateQuantityRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var cart = await _cartService.UpdateItemQuantityAsync(itemId, request.Quantity, ct);
            return ApiSuccess(cart, "Cart updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct = default)
    {
        try
        {
            var cart = await _cartService.RemoveItemAsync(itemId, ct);
            return ApiSuccess(cart, "Item removed from cart.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    [HttpDelete("items")]
    public async Task<IActionResult> ClearCart(CancellationToken ct = default)
    {
        try
        {
            var cart = await _cartService.ClearCartAsync(ct);
            return ApiSuccess(cart, "Cart cleared.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Applies a coupon code.
    /// </summary>
    [HttpPost("coupon")]
    public async Task<IActionResult> ApplyCoupon(
        [FromBody] ApplyCouponRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CouponCode))
        {
            return BadRequest(new ApiErrorResponse { Message = "Coupon code is required." });
        }

        try
        {
            var cart = await _cartService.ApplyCouponAsync(request.CouponCode, ct);
            return ApiSuccess(cart, "Coupon applied.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Removes the applied coupon.
    /// </summary>
    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct = default)
    {
        try
        {
            var cart = await _cartService.RemoveCouponAsync(ct);
            return ApiSuccess(cart, "Coupon removed.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Sets the shipping address.
    /// </summary>
    [HttpPut("shipping-address")]
    public async Task<IActionResult> SetShippingAddress(
        [FromBody] Address address,
        CancellationToken ct = default)
    {
        try
        {
            var cart = await _cartService.SetShippingAddressAsync(address, ct);
            return ApiSuccess(cart, "Shipping address updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Sets the billing address.
    /// </summary>
    [HttpPut("billing-address")]
    public async Task<IActionResult> SetBillingAddress(
        [FromBody] Address address,
        CancellationToken ct = default)
    {
        try
        {
            var cart = await _cartService.SetBillingAddressAsync(address, ct);
            return ApiSuccess(cart, "Billing address updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Gets available shipping options.
    /// </summary>
    [HttpGet("shipping-options")]
    public async Task<IActionResult> GetShippingOptions(CancellationToken ct = default)
    {
        var options = await _cartService.GetShippingOptionsAsync(ct);
        return ApiSuccess(options);
    }

    /// <summary>
    /// Sets the shipping method.
    /// </summary>
    [HttpPut("shipping-method")]
    public async Task<IActionResult> SetShippingMethod(
        [FromBody] SetShippingMethodRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ShippingMethodId))
        {
            return BadRequest(new ApiErrorResponse { Message = "Shipping method is required." });
        }

        try
        {
            var cart = await _cartService.SetShippingMethodAsync(request.ShippingMethodId, ct);
            return ApiSuccess(cart, "Shipping method updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Validates the cart for checkout.
    /// </summary>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateForCheckout(CancellationToken ct = default)
    {
        var result = await _cartService.ValidateForCheckoutAsync(ct);
        return ApiSuccess(result);
    }
}

#region Request Models

public class AddToCartApiRequest
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}

public class ApplyCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
}

public class SetShippingMethodRequest
{
    public string ShippingMethodId { get; set; } = string.Empty;
}

#endregion
