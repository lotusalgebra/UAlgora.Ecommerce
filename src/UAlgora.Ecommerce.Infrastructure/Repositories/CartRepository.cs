using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for cart operations.
/// </summary>
public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.SessionId == sessionId, ct);
    }

    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
    }

    public async Task<Cart?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Cart?> GetBySessionIdWithItemsAsync(string sessionId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId, ct);
    }

    public async Task<Cart?> GetByCustomerIdWithItemsAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
    }

    public async Task<Cart> GetOrCreateBySessionAsync(string sessionId, CancellationToken ct = default)
    {
        var cart = await GetBySessionIdWithItemsAsync(sessionId, ct);
        if (cart != null)
            return cart;

        cart = new Cart
        {
            SessionId = sessionId,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        await AddAsync(cart, ct);
        return cart;
    }

    public async Task<Cart> GetOrCreateByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        var cart = await GetByCustomerIdWithItemsAsync(customerId, ct);
        if (cart != null)
            return cart;

        cart = new Cart
        {
            CustomerId = customerId
        };

        await AddAsync(cart, ct);
        return cart;
    }

    public async Task<Cart> MergeCartsAsync(string sessionId, Guid customerId, CancellationToken ct = default)
    {
        var guestCart = await GetBySessionIdWithItemsAsync(sessionId, ct);
        var customerCart = await GetByCustomerIdWithItemsAsync(customerId, ct);

        // If no guest cart, just return or create customer cart
        if (guestCart == null)
        {
            return customerCart ?? await GetOrCreateByCustomerAsync(customerId, ct);
        }

        // If no customer cart, convert guest cart to customer cart
        if (customerCart == null)
        {
            guestCart.CustomerId = customerId;
            guestCart.SessionId = null;
            await Context.SaveChangesAsync(ct);
            return guestCart;
        }

        // Merge items from guest cart into customer cart
        foreach (var guestItem in guestCart.Items)
        {
            var existingItem = customerCart.Items
                .FirstOrDefault(i => i.ProductId == guestItem.ProductId && i.VariantId == guestItem.VariantId);

            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += guestItem.Quantity;
                existingItem.LineTotal = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                // Add new item
                guestItem.CartId = customerCart.Id;
                customerCart.Items.Add(guestItem);
            }
        }

        // Delete guest cart
        Context.Carts.Remove(guestCart);
        await Context.SaveChangesAsync(ct);

        return customerCart;
    }

    public async Task<CartItem> AddItemAsync(Guid cartId, CartItem item, CancellationToken ct = default)
    {
        item.CartId = cartId;
        item.AddedAt = DateTime.UtcNow;
        await Context.CartItems.AddAsync(item, ct);
        await Context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<CartItem> UpdateItemAsync(CartItem item, CancellationToken ct = default)
    {
        Context.CartItems.Update(item);
        await Context.SaveChangesAsync(ct);
        return item;
    }

    public async Task RemoveItemAsync(Guid cartId, Guid itemId, CancellationToken ct = default)
    {
        var item = await Context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.Id == itemId, ct);

        if (item != null)
        {
            Context.CartItems.Remove(item);
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task ClearItemsAsync(Guid cartId, CancellationToken ct = default)
    {
        var items = await Context.CartItems
            .Where(i => i.CartId == cartId)
            .ToListAsync(ct);

        Context.CartItems.RemoveRange(items);
        await Context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Cart>> GetExpiredCartsAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value < DateTime.UtcNow)
            .ToListAsync(ct);
    }

    public async Task DeleteExpiredCartsAsync(CancellationToken ct = default)
    {
        var expiredCarts = await DbSet
            .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value < DateTime.UtcNow)
            .Include(c => c.Items)
            .ToListAsync(ct);

        foreach (var cart in expiredCarts)
        {
            Context.CartItems.RemoveRange(cart.Items);
        }
        Context.Carts.RemoveRange(expiredCarts);

        await Context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Cart>> GetAbandonedCartsAsync(int hoursAgo = 24, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-hoursAgo);
        return await DbSet
            .Where(c => c.UpdatedAt < cutoff)
            .Where(c => c.Items.Any()) // Only carts with items
            .Include(c => c.Items)
            .OrderBy(c => c.UpdatedAt)
            .ToListAsync(ct);
    }
}
