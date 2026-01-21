using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for checkout operations.
/// </summary>
public class CheckoutService : ICheckoutService
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly ICartContextProvider _contextProvider;
    private readonly IInventoryService _inventoryService;

    // In-memory session store (in production, use distributed cache like Redis)
    private readonly Dictionary<Guid, CheckoutSession> _sessions = new();
    private readonly object _lock = new();

    public CheckoutService(
        ICartService cartService,
        IOrderService orderService,
        ICustomerService customerService,
        ICartContextProvider contextProvider,
        IInventoryService inventoryService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _customerService = customerService;
        _contextProvider = contextProvider;
        _inventoryService = inventoryService;
    }

    public async Task<CheckoutSession> InitializeAsync(CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartAsync(ct);

        // Validate cart for checkout
        var validation = await _cartService.ValidateForCheckoutAsync(ct);
        if (!validation.IsValid && validation.Errors.Any(e => e.ErrorCode == "CART_EMPTY"))
        {
            throw new InvalidOperationException("Cannot start checkout with an empty cart.");
        }

        var session = new CheckoutSession
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            CustomerId = _contextProvider.GetCustomerId(),
            CustomerEmail = cart.CustomerEmail,
            CustomerPhone = cart.CustomerPhone,
            ShippingAddress = cart.ShippingAddress,
            BillingAddress = cart.BillingAddress,
            BillingSameAsShipping = cart.BillingSameAsShipping,
            SelectedShippingMethod = cart.SelectedShippingMethod,
            Subtotal = cart.Subtotal,
            DiscountTotal = cart.DiscountTotal,
            ShippingTotal = cart.ShippingTotal,
            TaxTotal = cart.TaxTotal,
            GrandTotal = cart.GrandTotal,
            CurrentStep = CheckoutStep.Information,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        // Pre-populate from customer profile if authenticated
        if (session.CustomerId.HasValue)
        {
            var customer = await _customerService.GetCurrentAsync(ct);
            if (customer != null)
            {
                session.CustomerEmail ??= customer.Email;
                session.CustomerPhone ??= customer.Phone;

                // Get default shipping address
                if (session.ShippingAddress == null && customer.DefaultShippingAddressId.HasValue)
                {
                    session.ShippingAddress = customer.Addresses
                        .FirstOrDefault(a => a.Id == customer.DefaultShippingAddressId.Value);
                }

                // Get default billing address
                if (session.BillingAddress == null && customer.DefaultBillingAddressId.HasValue)
                {
                    session.BillingAddress = customer.Addresses
                        .FirstOrDefault(a => a.Id == customer.DefaultBillingAddressId.Value);
                }
            }
        }

        lock (_lock)
        {
            _sessions[session.Id] = session;
        }

        return session;
    }

    public Task<CheckoutSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                // Check if session is expired
                if (session.ExpiresAt.HasValue && session.ExpiresAt < DateTime.UtcNow)
                {
                    _sessions.Remove(sessionId);
                    return Task.FromResult<CheckoutSession?>(null);
                }

                return Task.FromResult<CheckoutSession?>(session);
            }
        }

        return Task.FromResult<CheckoutSession?>(null);
    }

    public async Task<CheckoutSession> UpdateShippingAddressAsync(
        Guid sessionId,
        Address address,
        CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            throw new InvalidOperationException($"Checkout session {sessionId} not found.");
        }

        session.ShippingAddress = address;

        // Update cart as well
        await _cartService.SetShippingAddressAsync(address, ct);

        // Advance to next step if appropriate
        if (session.CurrentStep == CheckoutStep.Information)
        {
            session.CurrentStep = CheckoutStep.Shipping;
        }

        lock (_lock)
        {
            _sessions[sessionId] = session;
        }

        return session;
    }

    public async Task<CheckoutSession> UpdateBillingAddressAsync(
        Guid sessionId,
        Address address,
        CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            throw new InvalidOperationException($"Checkout session {sessionId} not found.");
        }

        session.BillingAddress = address;
        session.BillingSameAsShipping = false;

        // Update cart as well
        await _cartService.SetBillingAddressAsync(address, ct);

        lock (_lock)
        {
            _sessions[sessionId] = session;
        }

        return session;
    }

    public async Task<CheckoutSession> UpdateShippingMethodAsync(
        Guid sessionId,
        string shippingMethodId,
        CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            throw new InvalidOperationException($"Checkout session {sessionId} not found.");
        }

        // Validate shipping method exists
        var shippingOptions = await GetShippingOptionsAsync(sessionId, ct);
        var selectedOption = shippingOptions.FirstOrDefault(o => o.Id == shippingMethodId);

        if (selectedOption == null)
        {
            throw new InvalidOperationException($"Shipping method {shippingMethodId} not found.");
        }

        session.SelectedShippingMethod = selectedOption.Id;
        session.ShippingTotal = selectedOption.Price;

        // Update cart as well
        var cart = await _cartService.SetShippingMethodAsync(shippingMethodId, ct);

        // Recalculate totals
        session.Subtotal = cart.Subtotal;
        session.DiscountTotal = cart.DiscountTotal;
        session.ShippingTotal = cart.ShippingTotal;
        session.TaxTotal = cart.TaxTotal;
        session.GrandTotal = cart.GrandTotal;

        // Advance to next step if appropriate
        if (session.CurrentStep == CheckoutStep.Shipping)
        {
            session.CurrentStep = CheckoutStep.Payment;
        }

        lock (_lock)
        {
            _sessions[sessionId] = session;
        }

        return session;
    }

    public async Task<IReadOnlyList<ShippingOption>> GetShippingOptionsAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            throw new InvalidOperationException($"Checkout session {sessionId} not found.");
        }

        // Get shipping options from cart service
        return await _cartService.GetShippingOptionsAsync(ct);
    }

    public Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        // Return static payment methods
        // TODO: Integrate with payment gateway to get available methods
        IReadOnlyList<PaymentMethod> methods = new List<PaymentMethod>
        {
            new PaymentMethod
            {
                Id = "card",
                Name = "Credit/Debit Card",
                Type = "card",
                Icon = "credit-card",
                IsDefault = true
            },
            new PaymentMethod
            {
                Id = "paypal",
                Name = "PayPal",
                Type = "paypal",
                Icon = "paypal"
            },
            new PaymentMethod
            {
                Id = "bank_transfer",
                Name = "Bank Transfer",
                Type = "bank_transfer",
                Icon = "bank"
            }
        };

        return Task.FromResult(methods);
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = "Checkout session not found."
            };
        }

        // Validate session is ready for payment
        var validation = await ValidateAsync(sessionId, ct);
        if (!validation.IsValid && validation.FailedAtStep != CheckoutStep.Payment)
        {
            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = string.Join(", ", validation.Errors)
            };
        }

        // TODO: Integrate with payment gateway (Stripe, etc.)
        // For now, generate a mock payment intent
        var paymentIntentId = $"pi_{Guid.NewGuid():N}";
        var clientSecret = $"secret_{Guid.NewGuid():N}";

        session.PaymentIntentId = paymentIntentId;
        session.PaymentClientSecret = clientSecret;

        lock (_lock)
        {
            _sessions[sessionId] = session;
        }

        return new PaymentIntentResult
        {
            Success = true,
            PaymentIntentId = paymentIntentId,
            ClientSecret = clientSecret
        };
    }

    public async Task<Order> CompleteAsync(
        Guid sessionId,
        CompleteCheckoutRequest request,
        CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            throw new InvalidOperationException($"Checkout session {sessionId} not found.");
        }

        // Final validation
        var validation = await ValidateAsync(sessionId, ct);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Checkout validation failed: {string.Join(", ", validation.Errors)}");
        }

        // Get the cart
        var cart = await _cartService.GetCartAsync(ct);

        // Create order request
        var orderRequest = new CreateOrderRequest
        {
            CustomerEmail = session.CustomerEmail ?? string.Empty,
            CustomerPhone = session.CustomerPhone,
            CustomerName = session.ShippingAddress != null
                ? $"{session.ShippingAddress.FirstName} {session.ShippingAddress.LastName}"
                : null,
            CustomerId = session.CustomerId,
            ShippingAddress = session.ShippingAddress!,
            BillingAddress = session.BillingSameAsShipping
                ? session.ShippingAddress
                : session.BillingAddress,
            BillingSameAsShipping = session.BillingSameAsShipping,
            ShippingMethod = session.SelectedShippingMethod,
            PaymentMethod = session.SelectedPaymentMethod ?? request.PaymentMethodId,
            PaymentIntentId = request.PaymentIntentId ?? session.PaymentIntentId,
            CustomerNote = request.CustomerNote,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Source = "checkout"
        };

        // Create the order
        var order = await _orderService.CreateFromCartAsync(cart, orderRequest, ct);

        // Clear the cart
        await _cartService.ClearCartAsync(ct);

        // Create customer account if requested
        if (request.CreateAccount && !session.CustomerId.HasValue && !string.IsNullOrEmpty(request.Password))
        {
            try
            {
                var createCustomerRequest = new CreateCustomerRequest
                {
                    Email = session.CustomerEmail!,
                    FirstName = session.ShippingAddress?.FirstName ?? "",
                    LastName = session.ShippingAddress?.LastName ?? "",
                    Phone = session.CustomerPhone,
                    Source = "checkout"
                };

                await _customerService.CreateAsync(createCustomerRequest, ct);
            }
            catch
            {
                // Don't fail the order if customer creation fails
            }
        }

        // Remove the checkout session
        lock (_lock)
        {
            _sessions.Remove(sessionId);
        }

        return order;
    }

    public async Task<CheckoutValidationResult> ValidateAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
        {
            return CheckoutValidationResult.Failure("Checkout session not found or expired.");
        }

        var errors = new List<string>();

        // Validate Information step
        if (string.IsNullOrWhiteSpace(session.CustomerEmail))
        {
            errors.Add("Email address is required.");
            return new CheckoutValidationResult
            {
                Errors = errors,
                FailedAtStep = CheckoutStep.Information
            };
        }

        if (session.ShippingAddress == null)
        {
            errors.Add("Shipping address is required.");
            return new CheckoutValidationResult
            {
                Errors = errors,
                FailedAtStep = CheckoutStep.Information
            };
        }

        // Validate Shipping step
        if (string.IsNullOrWhiteSpace(session.SelectedShippingMethod))
        {
            errors.Add("Please select a shipping method.");
            return new CheckoutValidationResult
            {
                Errors = errors,
                FailedAtStep = CheckoutStep.Shipping
            };
        }

        // Validate Payment step
        if (string.IsNullOrWhiteSpace(session.SelectedPaymentMethod) &&
            string.IsNullOrWhiteSpace(session.PaymentIntentId))
        {
            errors.Add("Please complete payment information.");
            return new CheckoutValidationResult
            {
                Errors = errors,
                FailedAtStep = CheckoutStep.Payment
            };
        }

        // Validate cart items are still available
        var cart = await _cartService.GetCartAsync(ct);
        var cartValidation = await _cartService.ValidateForCheckoutAsync(ct);

        if (!cartValidation.IsValid)
        {
            foreach (var error in cartValidation.Errors)
            {
                errors.Add(error.Message);
            }
        }

        return errors.Count == 0
            ? CheckoutValidationResult.Success()
            : new CheckoutValidationResult { Errors = errors };
    }

    public Task CancelAsync(Guid sessionId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _sessions.Remove(sessionId);
        }

        return Task.CompletedTask;
    }
}
