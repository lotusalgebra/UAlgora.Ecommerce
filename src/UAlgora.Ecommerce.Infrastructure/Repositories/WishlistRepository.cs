using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for wishlist operations.
/// </summary>
public class WishlistRepository : Repository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Wishlist>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.CustomerId == customerId)
            .Include(w => w.Items)
            .OrderByDescending(w => w.IsDefault)
            .ThenBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<Wishlist?> GetDefaultByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.CustomerId == customerId && w.IsDefault)
            .Include(w => w.Items)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Wishlist> GetOrCreateDefaultAsync(Guid customerId, CancellationToken ct = default)
    {
        var wishlist = await GetDefaultByCustomerIdAsync(customerId, ct);
        if (wishlist != null)
            return wishlist;

        wishlist = new Wishlist
        {
            CustomerId = customerId,
            Name = "My Wishlist",
            IsDefault = true
        };

        await AddAsync(wishlist, ct);
        return wishlist;
    }

    public async Task<Wishlist?> GetByShareTokenAsync(string shareToken, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.ShareToken == shareToken && w.IsPublic)
            .Include(w => w.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Wishlist?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(w => w.Items)
            .ThenInclude(i => i.Product)
            .Include(w => w.Items)
            .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<WishlistItem> AddItemAsync(WishlistItem item, CancellationToken ct = default)
    {
        await Context.WishlistItems.AddAsync(item, ct);
        await Context.SaveChangesAsync(ct);
        return item;
    }

    public async Task RemoveItemAsync(Guid wishlistId, Guid itemId, CancellationToken ct = default)
    {
        var item = await Context.WishlistItems
            .FirstOrDefaultAsync(i => i.WishlistId == wishlistId && i.Id == itemId, ct);

        if (item != null)
        {
            Context.WishlistItems.Remove(item);
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveProductAsync(
        Guid wishlistId,
        Guid productId,
        Guid? variantId = null,
        CancellationToken ct = default)
    {
        var query = Context.WishlistItems
            .Where(i => i.WishlistId == wishlistId && i.ProductId == productId);

        if (variantId.HasValue)
        {
            query = query.Where(i => i.VariantId == variantId.Value);
        }
        else
        {
            query = query.Where(i => i.VariantId == null);
        }

        var items = await query.ToListAsync(ct);
        Context.WishlistItems.RemoveRange(items);
        await Context.SaveChangesAsync(ct);
    }

    public async Task<bool> ContainsProductAsync(
        Guid wishlistId,
        Guid productId,
        Guid? variantId = null,
        CancellationToken ct = default)
    {
        var query = Context.WishlistItems
            .Where(i => i.WishlistId == wishlistId && i.ProductId == productId);

        if (variantId.HasValue)
        {
            query = query.Where(i => i.VariantId == variantId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<bool> IsProductInCustomerWishlistAsync(
        Guid customerId,
        Guid productId,
        Guid? variantId = null,
        CancellationToken ct = default)
    {
        var wishlistIds = await DbSet
            .Where(w => w.CustomerId == customerId)
            .Select(w => w.Id)
            .ToListAsync(ct);

        var query = Context.WishlistItems
            .Where(i => wishlistIds.Contains(i.WishlistId) && i.ProductId == productId);

        if (variantId.HasValue)
        {
            query = query.Where(i => i.VariantId == variantId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task ClearItemsAsync(Guid wishlistId, CancellationToken ct = default)
    {
        var items = await Context.WishlistItems
            .Where(i => i.WishlistId == wishlistId)
            .ToListAsync(ct);

        Context.WishlistItems.RemoveRange(items);
        await Context.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateShareTokenAsync(CancellationToken ct = default)
    {
        string token;
        do
        {
            token = Guid.NewGuid().ToString("N")[..12];
        } while (await DbSet.AnyAsync(w => w.ShareToken == token, ct));

        return token;
    }
}
