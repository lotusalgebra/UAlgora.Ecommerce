using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for checkout operations.
/// </summary>
public class CheckoutController : EcommerceApiController
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    /// <summary>
    /// Initializes a new checkout session.
    /// </summary>
    [HttpPost("session")]
    public async Task<IActionResult> InitializeCheckout(CancellationToken ct = default)
    {
        try
        {
            var session = await _checkoutService.InitializeAsync(ct);
            return ApiSuccess(session, "Checkout session created.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Gets a checkout session by ID.
    /// </summary>
    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetSession(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _checkoutService.GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Checkout session not found or expired." });
        }

        return ApiSuccess(session);
    }

    /// <summary>
    /// Updates the shipping address for checkout.
    /// </summary>
    [HttpPut("session/{sessionId:guid}/shipping-address")]
    public async Task<IActionResult> UpdateShippingAddress(
        Guid sessionId,
        [FromBody] Address address,
        CancellationToken ct = default)
    {
        try
        {
            var session = await _checkoutService.UpdateShippingAddressAsync(sessionId, address, ct);
            return ApiSuccess(session, "Shipping address updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Updates the billing address for checkout.
    /// </summary>
    [HttpPut("session/{sessionId:guid}/billing-address")]
    public async Task<IActionResult> UpdateBillingAddress(
        Guid sessionId,
        [FromBody] Address address,
        CancellationToken ct = default)
    {
        try
        {
            var session = await _checkoutService.UpdateBillingAddressAsync(sessionId, address, ct);
            return ApiSuccess(session, "Billing address updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Gets available shipping options for checkout.
    /// </summary>
    [HttpGet("session/{sessionId:guid}/shipping-options")]
    public async Task<IActionResult> GetShippingOptions(
        Guid sessionId,
        CancellationToken ct = default)
    {
        try
        {
            var options = await _checkoutService.GetShippingOptionsAsync(sessionId, ct);
            return ApiSuccess(options);
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Updates the shipping method for checkout.
    /// </summary>
    [HttpPut("session/{sessionId:guid}/shipping-method")]
    public async Task<IActionResult> UpdateShippingMethod(
        Guid sessionId,
        [FromBody] UpdateShippingMethodRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ShippingMethodId))
        {
            return BadRequest(new ApiErrorResponse { Message = "Shipping method is required." });
        }

        try
        {
            var session = await _checkoutService.UpdateShippingMethodAsync(sessionId, request.ShippingMethodId, ct);
            return ApiSuccess(session, "Shipping method updated.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Gets available payment methods for checkout.
    /// </summary>
    [HttpGet("session/{sessionId:guid}/payment-methods")]
    public async Task<IActionResult> GetPaymentMethods(
        Guid sessionId,
        CancellationToken ct = default)
    {
        try
        {
            var methods = await _checkoutService.GetPaymentMethodsAsync(sessionId, ct);
            return ApiSuccess(methods);
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Creates a payment intent for the checkout.
    /// </summary>
    [HttpPost("session/{sessionId:guid}/payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent(
        Guid sessionId,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _checkoutService.CreatePaymentIntentAsync(sessionId, ct);
            if (!result.Success)
            {
                return ApiError(result.ErrorMessage ?? "Failed to create payment intent.");
            }

            return ApiSuccess(result);
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Validates the checkout session.
    /// </summary>
    [HttpGet("session/{sessionId:guid}/validate")]
    public async Task<IActionResult> ValidateCheckout(
        Guid sessionId,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _checkoutService.ValidateAsync(sessionId, ct);
            return ApiSuccess(result);
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Completes the checkout and creates an order.
    /// </summary>
    [HttpPost("session/{sessionId:guid}/complete")]
    public async Task<IActionResult> CompleteCheckout(
        Guid sessionId,
        [FromBody] CompleteCheckoutApiRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var completeRequest = new CompleteCheckoutRequest
            {
                PaymentIntentId = request.PaymentIntentId,
                PaymentMethodId = request.PaymentMethodId,
                CustomerNote = request.CustomerNote,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreateAccount = request.CreateAccount,
                Password = request.Password
            };

            var order = await _checkoutService.CompleteAsync(sessionId, completeRequest, ct);
            return ApiSuccess(order, "Order placed successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiError(ex.Message);
        }
    }

    /// <summary>
    /// Cancels a checkout session.
    /// </summary>
    [HttpDelete("session/{sessionId:guid}")]
    public async Task<IActionResult> CancelCheckout(
        Guid sessionId,
        CancellationToken ct = default)
    {
        await _checkoutService.CancelAsync(sessionId, ct);
        return ApiSuccess(new { }, "Checkout session cancelled.");
    }
}

#region Request Models

public class UpdateShippingMethodRequest
{
    public string ShippingMethodId { get; set; } = string.Empty;
}

public class CompleteCheckoutApiRequest
{
    public string? PaymentIntentId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? CustomerNote { get; set; }
    public bool CreateAccount { get; set; }
    public string? Password { get; set; }
}

#endregion
