using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for cart operations.
/// </summary>
public class CartService : ICartService
{
    private readonly EcommerceDbContext _context;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICartContextProvider _contextProvider;
    private readonly IPricingService _pricingService;
    private readonly IDiscountService _discountService;

    public CartService(
        EcommerceDbContext context,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ICartContextProvider contextProvider,
        IPricingService pricingService,
        IDiscountService discountService)
    {
        _context = context;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _contextProvider = contextProvider;
        _pricingService = pricingService;
        _discountService = discountService;
    }

    public async Task<Cart> GetCartAsync(CancellationToken ct = default)
    {
        Cart? cart;

        if (_contextProvider.IsAuthenticated && _contextProvider.GetCustomerId().HasValue)
        {
            cart = await _cartRepository.GetOrCreateByCustomerAsync(_contextProvider.GetCustomerId()!.Value, ct);
        }
        else
        {
            var sessionId = _contextProvider.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new InvalidOperationException("No session or customer context available.");
            }
            cart = await _cartRepository.GetOrCreateBySessionAsync(sessionId, ct);
        }

        return cart;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _cartRepository.GetWithItemsAsync(id, ct);
    }

    public async Task<Cart> AddItemAsync(AddToCartRequest request, CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart = await _cartRepository.GetWithItemsAsync(cart.Id, ct) ?? cart;

        // Get product details
        var product = request.VariantId.HasValue
            ? await _productRepository.GetWithVariantsAsync(request.ProductId, ct)
            : await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product == null)
        {
            throw new InvalidOperationException($"Product {request.ProductId} not found.");
        }

        // Get variant if specified
        ProductVariant? variant = null;
        if (request.VariantId.HasValue)
        {
            variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId.Value);
            if (variant == null)
            {
                throw new InvalidOperationException($"Variant {request.VariantId} not found.");
            }
        }

        // Check if item already exists in cart
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId && i.VariantId == request.VariantId);

        if (existingItem != null)
        {
            // Update quantity
            existingItem.Quantity += request.Quantity;
            existingItem.LineTotal = existingItem.Quantity * existingItem.UnitPrice;
            await _cartRepository.UpdateItemAsync(existingItem, ct);
        }
        else
        {
            // Get pricing
            var pricing = await _pricingService.GetPricingDetailsAsync(request.ProductId, request.VariantId, _contextProvider.GetCustomerId(), ct);

            // Create new cart item
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                ProductName = product.Name,
                Sku = variant?.Sku ?? product.Sku ?? string.Empty,
                VariantName = variant?.Name,
                VariantOptions = variant?.Options,
                ImageId = variant?.ImageId ?? product.ImageIds.FirstOrDefault(),
                Quantity = request.Quantity,
                UnitPrice = pricing.CurrentPrice,
                OriginalPrice = pricing.BasePrice,
                LineTotal = pricing.CurrentPrice * request.Quantity,
                Weight = variant?.Weight ?? product.Weight,
                AddedAt = DateTime.UtcNow
            };

            await _cartRepository.AddItemAsync(cart.Id, cartItem, ct);
        }

        // Recalculate totals
        return await RecalculateAsync(ct);
    }

    public async Task<Cart> UpdateItemQuantityAsync(Guid itemId, int quantity, CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart = await _cartRepository.GetWithItemsAsync(cart.Id, ct) ?? cart;

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Cart item {itemId} not found.");
        }

        if (quantity <= 0)
        {
            return await RemoveItemAsync(itemId, ct);
        }

        item.Quantity = quantity;
        item.LineTotal = item.Quantity * item.UnitPrice;
        await _cartRepository.UpdateItemAsync(item, ct);

        return await RecalculateAsync(ct);
    }

    public async Task<Cart> RemoveItemAsync(Guid itemId, CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        await _cartRepository.RemoveItemAsync(cart.Id, itemId, ct);
        return await RecalculateAsync(ct);
    }

    public async Task<Cart> ClearCartAsync(CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        await _cartRepository.ClearItemsAsync(cart.Id, ct);

        cart.Items.Clear();
        cart.Subtotal = 0;
        cart.DiscountTotal = 0;
        cart.ShippingTotal = 0;
        cart.TaxTotal = 0;
        cart.GrandTotal = 0;
        cart.AppliedDiscounts.Clear();
        cart.CouponCode = null;

        return await _cartRepository.UpdateAsync(cart, ct);
    }

    public async Task<Cart> ApplyCouponAsync(string couponCode, CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart = await _cartRepository.GetWithItemsAsync(cart.Id, ct) ?? cart;

        // Validate the coupon
        var validationResult = await _discountService.ValidateCouponAsync(
            couponCode, cart, _contextProvider.GetCustomerId(), ct);

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage ?? "Invalid coupon code.");
        }

        cart.CouponCode = couponCode;
        await _cartRepository.UpdateAsync(cart, ct);

        return await RecalculateAsync(ct);
    }

    public async Task<Cart> RemoveCouponAsync(CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart.CouponCode = null;

        // Remove coupon discount from applied discounts
        cart.AppliedDiscounts.RemoveAll(d => d.IsCoupon);

        await _cartRepository.UpdateAsync(cart, ct);
        return await RecalculateAsync(ct);
    }

    public async Task<Cart> SetShippingAddressAsync(Address address, CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart.ShippingAddress = address;
        cart.CustomerEmail ??= address.FirstName + "@example.com"; // Placeholder, should be from user input
        await _cartRepository.UpdateAsync(cart, ct);
        return await RecalculateAsync(ct);
    }

    public async Task<Cart> SetBillingAddressAsync(Address address, CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart.BillingAddress = address;
        cart.BillingSameAsShipping = false;
        return await _cartRepository.UpdateAsync(cart, ct);
    }

    public async Task<IReadOnlyList<ShippingOption>> GetShippingOptionsAsync(CancellationToken ct = default)
    {
        // TODO: Integrate with shipping providers
        // For now, return static options
        return new List<ShippingOption>
        {
            new ShippingOption
            {
                Id = "standard",
                Name = "Standard Shipping",
                Description = "5-7 business days",
                Price = 5.99m,
                EstimatedDelivery = "5-7 business days",
                Carrier = "Standard"
            },
            new ShippingOption
            {
                Id = "express",
                Name = "Express Shipping",
                Description = "2-3 business days",
                Price = 12.99m,
                EstimatedDelivery = "2-3 business days",
                Carrier = "Express"
            },
            new ShippingOption
            {
                Id = "overnight",
                Name = "Overnight Shipping",
                Description = "Next business day",
                Price = 24.99m,
                EstimatedDelivery = "1 business day",
                Carrier = "Priority"
            }
        };
    }

    public async Task<Cart> SetShippingMethodAsync(string shippingMethodId, CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        var shippingOptions = await GetShippingOptionsAsync(ct);
        var selectedOption = shippingOptions.FirstOrDefault(o => o.Id == shippingMethodId);

        if (selectedOption == null)
        {
            throw new InvalidOperationException($"Shipping method {shippingMethodId} not found.");
        }

        cart.SelectedShippingMethod = selectedOption.Id;
        cart.SelectedShippingMethodName = selectedOption.Name;
        cart.ShippingTotal = selectedOption.Price;

        await _cartRepository.UpdateAsync(cart, ct);
        return await RecalculateAsync(ct);
    }

    public async Task<Cart> RecalculateAsync(CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart = await _cartRepository.GetWithItemsAsync(cart.Id, ct) ?? cart;

        // Calculate subtotal
        cart.Subtotal = cart.Items.Sum(i => i.LineTotal);

        // Calculate discounts
        var discountCalculation = await _discountService.CalculateCartDiscountsAsync(cart, cart.CouponCode, ct);
        cart.DiscountTotal = discountCalculation.TotalDiscount;

        // Update applied discounts
        cart.AppliedDiscounts = discountCalculation.AppliedDiscounts.Select(d => new AppliedDiscount
        {
            DiscountId = d.DiscountId,
            Code = d.Code,
            Name = d.Name,
            Amount = d.Amount,
            IsCoupon = !string.IsNullOrEmpty(d.Code)
        }).ToList();

        // Apply line item discounts
        foreach (var lineDiscount in discountCalculation.AppliedDiscounts.SelectMany(d => d.LineAllocations))
        {
            var item = cart.Items.FirstOrDefault(i => i.Id == lineDiscount.CartItemId);
            if (item != null)
            {
                item.DiscountAmount = lineDiscount.Amount;
            }
        }

        // Calculate tax (simplified - should use tax service)
        // TODO: Integrate with tax provider
        cart.TaxTotal = (cart.Subtotal - cart.DiscountTotal) * 0.1m; // 10% tax for demo

        // Calculate grand total
        cart.GrandTotal = cart.Subtotal - cart.DiscountTotal + cart.ShippingTotal + cart.TaxTotal;

        return await _cartRepository.UpdateAsync(cart, ct);
    }

    public async Task<Cart> MergeCartsAsync(Guid customerId, CancellationToken ct = default)
    {
        var sessionId = _contextProvider.GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            var customerCart = await _cartRepository.GetByCustomerIdWithItemsAsync(customerId, ct);
            return customerCart ?? await _cartRepository.GetOrCreateByCustomerAsync(customerId, ct);
        }

        return await _cartRepository.MergeCartsAsync(sessionId, customerId, ct);
    }

    public async Task<CartValidationResult> ValidateForCheckoutAsync(CancellationToken ct = default)
    {
        var cart = await GetCartAsync(ct);
        cart = await _cartRepository.GetWithItemsAsync(cart.Id, ct) ?? cart;

        var errors = new List<CartValidationError>();

        // Check if cart is empty
        if (cart.Items.Count == 0)
        {
            errors.Add(new CartValidationError
            {
                ErrorCode = "CART_EMPTY",
                Message = "Your cart is empty."
            });
        }

        // Check stock availability for each item
        foreach (var item in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
            if (product == null)
            {
                errors.Add(new CartValidationError
                {
                    ErrorCode = "PRODUCT_NOT_FOUND",
                    Message = $"Product {item.ProductName} is no longer available.",
                    ItemId = item.Id
                });
                continue;
            }

            if (product.TrackInventory && product.StockQuantity < item.Quantity && !product.AllowBackorders)
            {
                errors.Add(new CartValidationError
                {
                    ErrorCode = "INSUFFICIENT_STOCK",
                    Message = $"Only {product.StockQuantity} units of {item.ProductName} are available.",
                    ItemId = item.Id
                });
            }
        }

        // Check shipping address
        if (cart.ShippingAddress == null)
        {
            errors.Add(new CartValidationError
            {
                ErrorCode = "SHIPPING_ADDRESS_REQUIRED",
                Message = "Shipping address is required."
            });
        }

        // Check shipping method
        if (string.IsNullOrEmpty(cart.SelectedShippingMethod))
        {
            errors.Add(new CartValidationError
            {
                ErrorCode = "SHIPPING_METHOD_REQUIRED",
                Message = "Please select a shipping method."
            });
        }

        return errors.Count == 0
            ? CartValidationResult.Success()
            : new CartValidationResult { Errors = errors };
    }

    public async Task<int> GetItemCountAsync(CancellationToken ct = default)
    {
        try
        {
            var cart = await GetCartAsync(ct);
            cart = await _cartRepository.GetWithItemsAsync(cart.Id, ct) ?? cart;
            return cart.Items.Sum(i => i.Quantity);
        }
        catch
        {
            return 0;
        }
    }

    #region Management Methods (Backoffice)

    public async Task<PagedResult<Cart>> GetPagedCartsAsync(
        int page = 1,
        int pageSize = 20,
        Guid? customerId = null,
        bool? isGuest = null,
        bool? isAbandoned = null,
        string? sortBy = null,
        bool descending = true,
        CancellationToken ct = default)
    {
        var query = _context.Carts
            .Include(c => c.Items)
            .Include(c => c.Customer)
            .AsQueryable();

        // Apply filters
        if (customerId.HasValue)
        {
            query = query.Where(c => c.CustomerId == customerId.Value);
        }

        if (isGuest.HasValue)
        {
            query = isGuest.Value
                ? query.Where(c => c.CustomerId == null)
                : query.Where(c => c.CustomerId != null);
        }

        if (isAbandoned.HasValue && isAbandoned.Value)
        {
            var abandonedDate = DateTime.UtcNow.AddDays(-7);
            query = query.Where(c => c.UpdatedAt < abandonedDate && c.Items.Count > 0);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "createdat" => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "updatedat" => descending ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt),
            "grandtotal" => descending ? query.OrderByDescending(c => c.GrandTotal) : query.OrderBy(c => c.GrandTotal),
            "itemcount" => descending ? query.OrderByDescending(c => c.Items.Count) : query.OrderBy(c => c.Items.Count),
            _ => descending ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt)
        };

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Cart>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalItems
        };
    }

    public async Task<Cart?> GetCartByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<Cart>> GetCartsByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Cart>> GetAbandonedCartsAsync(int daysOld = 7, CancellationToken ct = default)
    {
        var abandonedDate = DateTime.UtcNow.AddDays(-daysOld);
        return await _context.Carts
            .Include(c => c.Items)
            .Include(c => c.Customer)
            .Where(c => c.UpdatedAt < abandonedDate && c.Items.Count > 0)
            .OrderByDescending(c => c.GrandTotal)
            .ToListAsync(ct);
    }

    public async Task<Cart> UpdateCartNotesAsync(Guid cartId, string? notes, CancellationToken ct = default)
    {
        var cart = await _context.Carts.FindAsync([cartId], ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found");

        cart.Notes = notes;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return await GetCartByIdAsync(cartId, ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found after update");
    }

    public async Task DeleteCartAsync(Guid cartId, CancellationToken ct = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found");

        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteExpiredCartsAsync(CancellationToken ct = default)
    {
        var expiredCarts = await _context.Carts
            .Include(c => c.Items)
            .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value < DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var cart in expiredCarts)
        {
            _context.CartItems.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
        }

        await _context.SaveChangesAsync(ct);
        return expiredCarts.Count;
    }

    public async Task<int> DeleteAbandonedCartsAsync(int daysOld = 30, CancellationToken ct = default)
    {
        var abandonedDate = DateTime.UtcNow.AddDays(-daysOld);
        var abandonedCarts = await _context.Carts
            .Include(c => c.Items)
            .Where(c => c.UpdatedAt < abandonedDate)
            .ToListAsync(ct);

        foreach (var cart in abandonedCarts)
        {
            _context.CartItems.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
        }

        await _context.SaveChangesAsync(ct);
        return abandonedCarts.Count;
    }

    public async Task<CartStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);
        var abandonedDate = now.AddDays(-7);

        var carts = await _context.Carts
            .Include(c => c.Items)
            .ToListAsync(ct);

        var activeCarts = carts.Where(c => c.UpdatedAt >= abandonedDate).ToList();
        var abandonedCarts = carts.Where(c => c.UpdatedAt < abandonedDate && c.Items.Count > 0).ToList();
        var cartsWithItems = carts.Where(c => c.Items.Count > 0).ToList();

        return new CartStatistics
        {
            TotalCarts = carts.Count,
            ActiveCarts = activeCarts.Count,
            AbandonedCarts = abandonedCarts.Count,
            ExpiredCarts = carts.Count(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value < now),
            GuestCarts = carts.Count(c => c.CustomerId == null),
            CustomerCarts = carts.Count(c => c.CustomerId != null),
            EmptyCarts = carts.Count(c => c.Items.Count == 0),
            CartsWithItems = cartsWithItems.Count,
            TotalCartValue = cartsWithItems.Sum(c => c.GrandTotal),
            AverageCartValue = cartsWithItems.Count > 0 ? cartsWithItems.Average(c => c.GrandTotal) : 0,
            TotalItems = carts.Sum(c => c.Items.Sum(i => i.Quantity)),
            AverageItemsPerCart = cartsWithItems.Count > 0 ? cartsWithItems.Average(c => c.Items.Sum(i => i.Quantity)) : 0,
            TodayCreated = carts.Count(c => c.CreatedAt >= today),
            ThisWeekCreated = carts.Count(c => c.CreatedAt >= weekAgo),
            ThisMonthCreated = carts.Count(c => c.CreatedAt >= monthAgo),
            AbandonedCartValue = abandonedCarts.Sum(c => c.GrandTotal)
        };
    }

    public async Task<Cart> SetCartExpirationAsync(Guid cartId, DateTime? expiresAt, CancellationToken ct = default)
    {
        var cart = await _context.Carts.FindAsync([cartId], ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found");

        cart.ExpiresAt = expiresAt;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return await GetCartByIdAsync(cartId, ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found after update");
    }

    public async Task<Cart> ClearCartByIdAsync(Guid cartId, CancellationToken ct = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found");

        _context.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        cart.Subtotal = 0;
        cart.DiscountTotal = 0;
        cart.ShippingTotal = 0;
        cart.TaxTotal = 0;
        cart.GrandTotal = 0;
        cart.AppliedDiscounts.Clear();
        cart.CouponCode = null;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return await GetCartByIdAsync(cartId, ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found after clear");
    }

    public async Task<Cart> AssignCartToCustomerAsync(Guid cartId, Guid customerId, CancellationToken ct = default)
    {
        var cart = await _context.Carts.FindAsync([cartId], ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found");

        var customer = await _context.Customers.FindAsync([customerId], ct)
            ?? throw new InvalidOperationException($"Customer {customerId} not found");

        cart.CustomerId = customerId;
        cart.SessionId = null;
        cart.CustomerEmail = customer.Email;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return await GetCartByIdAsync(cartId, ct)
            ?? throw new InvalidOperationException($"Cart {cartId} not found after assignment");
    }

    #endregion
}
