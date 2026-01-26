using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for checkout operations.
/// </summary>
public class CheckoutController : EcommerceApiController
{
    private const string AuthScheme = "EcommerceCustomer";
    private readonly ICheckoutService _checkoutService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly EcommerceDbContext _dbContext;

    public CheckoutController(
        ICheckoutService checkoutService,
        IProductService productService,
        ICustomerService customerService,
        EcommerceDbContext dbContext)
    {
        _checkoutService = checkoutService;
        _productService = productService;
        _customerService = customerService;
        _dbContext = dbContext;
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

    /// <summary>
    /// Places an order directly from cart data (for client-side cart).
    /// </summary>
    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken ct = default)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new ApiErrorResponse { Message = "Cart is empty" });
        }

        try
        {
            // Get customer ID if authenticated
            Guid? customerId = null;
            var authResult = await HttpContext.AuthenticateAsync(AuthScheme);
            if (authResult.Succeeded)
            {
                var customerIdClaim = authResult.Principal?.FindFirst("CustomerId")?.Value
                                      ?? authResult.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(customerIdClaim, out var parsedId))
                {
                    customerId = parsedId;
                }
            }

            // Build order lines from cart items
            var orderLines = new List<OrderLine>();
            decimal subtotal = 0;

            foreach (var item in request.Items)
            {
                var product = await _productService.GetByIdAsync(item.ProductId, ct);
                if (product == null)
                {
                    return BadRequest(new ApiErrorResponse { Message = $"Product not found: {item.ProductId}" });
                }

                var unitPrice = product.SalePrice ?? product.BasePrice;
                var lineTotal = unitPrice * item.Quantity;
                subtotal += lineTotal;

                orderLines.Add(new OrderLine
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    ImageUrl = product.PrimaryImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    OriginalPrice = product.BasePrice,
                    LineTotal = lineTotal
                });
            }

            // Calculate totals
            var shippingTotal = request.ShippingCost ?? (subtotal >= 100 ? 0 : 9.99m);
            var taxTotal = Math.Round(subtotal * 0.08m, 2); // 8% tax
            var grandTotal = subtotal + shippingTotal + taxTotal;

            // Generate order number
            var orderNumber = GenerateOrderNumber();

            // Create and save shipping address first
            var shippingAddress = new Address
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                AddressLine1 = request.ShippingAddress1,
                AddressLine2 = request.ShippingAddress2,
                City = request.ShippingCity,
                StateProvince = request.ShippingState,
                PostalCode = request.ShippingPostalCode,
                Country = request.ShippingCountry ?? "US",
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Addresses.Add(shippingAddress);
            await _dbContext.SaveChangesAsync(ct);

            // Create order with address ID
            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = customerId,
                Status = OrderStatus.Confirmed,
                PaymentStatus = PaymentStatus.Captured, // Simulated payment success
                FulfillmentStatus = FulfillmentStatus.Unfulfilled,
                CustomerEmail = request.Email,
                CustomerPhone = request.Phone,
                CustomerName = $"{request.FirstName} {request.LastName}",
                ShippingAddressId = shippingAddress.Id,
                BillingSameAsShipping = true,
                CurrencyCode = "USD",
                Subtotal = subtotal,
                ShippingTotal = shippingTotal,
                TaxTotal = taxTotal,
                GrandTotal = grandTotal,
                PaidAmount = grandTotal,
                PaymentMethod = request.PaymentMethod ?? "card",
                PaymentProvider = "simulated",
                ShippingMethod = request.ShippingMethod ?? "standard",
                ShippingMethodName = shippingTotal == 0 ? "Free Shipping" : "Standard Shipping",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
                Source = "web",
                PlacedAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save order first to get the ID
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(ct);

            // Now add order lines with the order ID
            foreach (var line in orderLines)
            {
                line.OrderId = order.Id;
                line.CreatedAt = DateTime.UtcNow;
                line.UpdatedAt = DateTime.UtcNow;
            }
            _dbContext.OrderLines.AddRange(orderLines);
            await _dbContext.SaveChangesAsync(ct);

            // Update customer's address if logged in and they don't have one
            if (customerId.HasValue)
            {
                var customer = await _customerService.GetByIdAsync(customerId.Value, ct);
                if (customer != null && (customer.Addresses == null || !customer.Addresses.Any()))
                {
                    customer.Addresses ??= new List<Address>();
                    customer.Addresses.Add(new Address
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        AddressLine1 = request.ShippingAddress1,
                        AddressLine2 = request.ShippingAddress2,
                        City = request.ShippingCity,
                        StateProvince = request.ShippingState,
                        PostalCode = request.ShippingPostalCode,
                        Country = request.ShippingCountry ?? "US",
                        Phone = request.Phone,
                        IsDefaultShipping = true,
                        IsDefaultBilling = true
                    });
                    await _customerService.UpdateAsync(customer, ct);
                }
            }

            return ApiSuccess(new
            {
                orderNumber = order.OrderNumber,
                orderId = order.Id,
                grandTotal = order.GrandTotal
            }, "Order placed successfully.");
        }
        catch (Exception ex)
        {
            return ApiError($"Failed to create order: {ex.Message}");
        }
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyMMdd");
        var random = new Random();
        var suffix = new string(Enumerable.Range(0, 4).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[random.Next(36)]).ToArray());
        return $"ALG-{timestamp}{suffix}";
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

public class PlaceOrderRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public string ShippingAddress1 { get; set; } = string.Empty;
    public string? ShippingAddress2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string? ShippingCountry { get; set; }

    public string? ShippingMethod { get; set; }
    public decimal? ShippingCost { get; set; }
    public string? PaymentMethod { get; set; }

    public List<CartItemRequest> Items { get; set; } = new();
}

public class CartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

#endregion
