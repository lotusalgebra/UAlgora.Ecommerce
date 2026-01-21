using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for wishlist management operations.
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly EcommerceDbContext _context;

    public WishlistService(EcommerceDbContext context)
    {
        _context = context;
    }

    #region Wishlists

    public async Task<List<Wishlist>> GetAllWishlistsAsync(bool includeItems = false, CancellationToken ct = default)
    {
        var query = _context.Wishlists
            .Include(w => w.Customer)
            .AsQueryable();

        if (includeItems)
        {
            query = query.Include(w => w.Items).ThenInclude(i => i.Product);
        }

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Wishlist>> GetPagedWishlistsAsync(
        int page = 1,
        int pageSize = 20,
        Guid? customerId = null,
        bool? isPublic = null,
        string? sortBy = null,
        bool descending = true,
        CancellationToken ct = default)
    {
        var query = _context.Wishlists
            .Include(w => w.Customer)
            .Include(w => w.Items)
            .AsQueryable();

        // Apply filters
        if (customerId.HasValue)
        {
            query = query.Where(w => w.CustomerId == customerId.Value);
        }

        if (isPublic.HasValue)
        {
            query = query.Where(w => w.IsPublic == isPublic.Value);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(w => w.Name) : query.OrderBy(w => w.Name),
            "items" => descending ? query.OrderByDescending(w => w.Items.Count) : query.OrderBy(w => w.Items.Count),
            "customer" => descending ? query.OrderByDescending(w => w.Customer!.FirstName) : query.OrderBy(w => w.Customer!.FirstName),
            _ => descending ? query.OrderByDescending(w => w.CreatedAt) : query.OrderBy(w => w.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Wishlist>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Wishlist?> GetWishlistByIdAsync(Guid id, bool includeItems = true, CancellationToken ct = default)
    {
        var query = _context.Wishlists
            .Include(w => w.Customer)
            .AsQueryable();

        if (includeItems)
        {
            query = query
                .Include(w => w.Items).ThenInclude(i => i.Product)
                .Include(w => w.Items).ThenInclude(i => i.Variant);
        }

        return await query.FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<List<Wishlist>> GetWishlistsByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Wishlists
            .Include(w => w.Items)
            .Where(w => w.CustomerId == customerId)
            .OrderByDescending(w => w.IsDefault)
            .ThenBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<Wishlist?> GetDefaultWishlistAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Wishlists
            .Include(w => w.Items).ThenInclude(i => i.Product)
            .Where(w => w.CustomerId == customerId && w.IsDefault)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Wishlist?> GetWishlistByShareTokenAsync(string shareToken, CancellationToken ct = default)
    {
        return await _context.Wishlists
            .Include(w => w.Items).ThenInclude(i => i.Product)
            .Include(w => w.Customer)
            .Where(w => w.ShareToken == shareToken && w.IsPublic)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Wishlist> CreateWishlistAsync(Wishlist wishlist, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults for this customer
        if (wishlist.IsDefault)
        {
            await UnsetDefaultWishlistsAsync(wishlist.CustomerId, ct);
        }

        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync(ct);
        return wishlist;
    }

    public async Task<Wishlist> UpdateWishlistAsync(Wishlist wishlist, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults for this customer
        if (wishlist.IsDefault)
        {
            await UnsetDefaultWishlistsAsync(wishlist.CustomerId, wishlist.Id, ct);
        }

        _context.Wishlists.Update(wishlist);
        await _context.SaveChangesAsync(ct);
        return wishlist;
    }

    public async Task DeleteWishlistAsync(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _context.Wishlists.FindAsync([id], ct);
        if (wishlist != null)
        {
            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync(ct);
        }
    }

    private async Task UnsetDefaultWishlistsAsync(Guid customerId, CancellationToken ct = default)
    {
        var defaults = await _context.Wishlists
            .Where(w => w.CustomerId == customerId && w.IsDefault)
            .ToListAsync(ct);

        foreach (var w in defaults)
        {
            w.IsDefault = false;
        }
    }

    private async Task UnsetDefaultWishlistsAsync(Guid customerId, Guid exceptId, CancellationToken ct = default)
    {
        var defaults = await _context.Wishlists
            .Where(w => w.CustomerId == customerId && w.IsDefault && w.Id != exceptId)
            .ToListAsync(ct);

        foreach (var w in defaults)
        {
            w.IsDefault = false;
        }
    }

    #endregion

    #region Wishlist Actions

    public async Task<Wishlist> SetDefaultWishlistAsync(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _context.Wishlists.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Wishlist {id} not found");

        await UnsetDefaultWishlistsAsync(wishlist.CustomerId, ct);
        wishlist.IsDefault = true;
        await _context.SaveChangesAsync(ct);
        return wishlist;
    }

    public async Task<Wishlist> TogglePublicAsync(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _context.Wishlists.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Wishlist {id} not found");

        wishlist.IsPublic = !wishlist.IsPublic;

        // Generate share token if making public and no token exists
        if (wishlist.IsPublic && string.IsNullOrEmpty(wishlist.ShareToken))
        {
            wishlist.ShareToken = await GenerateUniqueShareTokenAsync(ct);
        }

        await _context.SaveChangesAsync(ct);
        return wishlist;
    }

    public async Task<Wishlist> GenerateShareTokenAsync(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _context.Wishlists.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Wishlist {id} not found");

        wishlist.ShareToken = await GenerateUniqueShareTokenAsync(ct);
        wishlist.IsPublic = true;
        await _context.SaveChangesAsync(ct);
        return wishlist;
    }

    public async Task<Wishlist> RemoveShareTokenAsync(Guid id, CancellationToken ct = default)
    {
        var wishlist = await _context.Wishlists.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Wishlist {id} not found");

        wishlist.ShareToken = null;
        wishlist.IsPublic = false;
        await _context.SaveChangesAsync(ct);
        return wishlist;
    }

    public async Task<Wishlist> RenameWishlistAsync(Guid id, string name, CancellationToken ct = default)
    {
        var wishlist = await _context.Wishlists.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Wishlist {id} not found");

        wishlist.Name = name;
        await _context.SaveChangesAsync(ct);
        return wishlist;
    }

    public async Task<Wishlist> DuplicateWishlistAsync(Guid id, string? newName = null, CancellationToken ct = default)
    {
        var original = await GetWishlistByIdAsync(id, true, ct)
            ?? throw new InvalidOperationException($"Wishlist {id} not found");

        var duplicate = new Wishlist
        {
            CustomerId = original.CustomerId,
            Name = newName ?? $"{original.Name} (Copy)",
            IsDefault = false,
            IsPublic = false,
            ShareToken = null
        };

        _context.Wishlists.Add(duplicate);
        await _context.SaveChangesAsync(ct);

        // Copy items
        foreach (var item in original.Items)
        {
            var newItem = new WishlistItem
            {
                WishlistId = duplicate.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                PriceWhenAdded = item.PriceWhenAdded,
                Notes = item.Notes,
                Quantity = item.Quantity,
                Priority = item.Priority
            };
            _context.WishlistItems.Add(newItem);
        }

        await _context.SaveChangesAsync(ct);
        return duplicate;
    }

    public async Task<Wishlist> ClearWishlistAsync(Guid id, CancellationToken ct = default)
    {
        var items = await _context.WishlistItems
            .Where(i => i.WishlistId == id)
            .ToListAsync(ct);

        _context.WishlistItems.RemoveRange(items);
        await _context.SaveChangesAsync(ct);

        return await GetWishlistByIdAsync(id, true, ct)
            ?? throw new InvalidOperationException($"Wishlist {id} not found");
    }

    public async Task<Wishlist> MergeWishlistsAsync(Guid sourceId, Guid targetId, bool deleteSource = false, CancellationToken ct = default)
    {
        var source = await GetWishlistByIdAsync(sourceId, true, ct)
            ?? throw new InvalidOperationException($"Source wishlist {sourceId} not found");

        var target = await GetWishlistByIdAsync(targetId, true, ct)
            ?? throw new InvalidOperationException($"Target wishlist {targetId} not found");

        // Copy items that don't already exist in target
        foreach (var item in source.Items)
        {
            var exists = target.Items.Any(i =>
                i.ProductId == item.ProductId && i.VariantId == item.VariantId);

            if (!exists)
            {
                var newItem = new WishlistItem
                {
                    WishlistId = targetId,
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    PriceWhenAdded = item.PriceWhenAdded,
                    Notes = item.Notes,
                    Quantity = item.Quantity,
                    Priority = item.Priority
                };
                _context.WishlistItems.Add(newItem);
            }
        }

        if (deleteSource)
        {
            _context.Wishlists.Remove(source);
        }

        await _context.SaveChangesAsync(ct);
        return await GetWishlistByIdAsync(targetId, true, ct)
            ?? throw new InvalidOperationException($"Target wishlist {targetId} not found after merge");
    }

    private async Task<string> GenerateUniqueShareTokenAsync(CancellationToken ct = default)
    {
        string token;
        do
        {
            token = Guid.NewGuid().ToString("N")[..12];
        } while (await _context.Wishlists.AnyAsync(w => w.ShareToken == token, ct));

        return token;
    }

    #endregion

    #region Wishlist Items

    public async Task<WishlistItem?> GetWishlistItemByIdAsync(Guid itemId, CancellationToken ct = default)
    {
        return await _context.WishlistItems
            .Include(i => i.Product)
            .Include(i => i.Variant)
            .Include(i => i.Wishlist)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);
    }

    public async Task<WishlistItem> AddItemAsync(
        Guid wishlistId,
        Guid productId,
        Guid? variantId = null,
        decimal priceWhenAdded = 0,
        string? notes = null,
        int quantity = 1,
        int? priority = null,
        CancellationToken ct = default)
    {
        // Check if item already exists
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(i =>
                i.WishlistId == wishlistId &&
                i.ProductId == productId &&
                i.VariantId == variantId, ct);

        if (existing != null)
        {
            // Update quantity instead of adding duplicate
            existing.Quantity += quantity;
            await _context.SaveChangesAsync(ct);
            return existing;
        }

        var item = new WishlistItem
        {
            WishlistId = wishlistId,
            ProductId = productId,
            VariantId = variantId,
            PriceWhenAdded = priceWhenAdded,
            Notes = notes,
            Quantity = quantity,
            Priority = priority
        };

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<WishlistItem> UpdateItemAsync(WishlistItem item, CancellationToken ct = default)
    {
        _context.WishlistItems.Update(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task RemoveItemAsync(Guid wishlistId, Guid itemId, CancellationToken ct = default)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(i => i.WishlistId == wishlistId && i.Id == itemId, ct);

        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<WishlistItem> UpdateItemNotesAsync(Guid itemId, string? notes, CancellationToken ct = default)
    {
        var item = await _context.WishlistItems.FindAsync([itemId], ct)
            ?? throw new InvalidOperationException($"Wishlist item {itemId} not found");

        item.Notes = notes;
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<WishlistItem> UpdateItemQuantityAsync(Guid itemId, int quantity, CancellationToken ct = default)
    {
        if (quantity < 1)
        {
            throw new ArgumentException("Quantity must be at least 1", nameof(quantity));
        }

        var item = await _context.WishlistItems.FindAsync([itemId], ct)
            ?? throw new InvalidOperationException($"Wishlist item {itemId} not found");

        item.Quantity = quantity;
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<WishlistItem> UpdateItemPriorityAsync(Guid itemId, int? priority, CancellationToken ct = default)
    {
        var item = await _context.WishlistItems.FindAsync([itemId], ct)
            ?? throw new InvalidOperationException($"Wishlist item {itemId} not found");

        item.Priority = priority;
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<WishlistItem> MoveItemAsync(Guid itemId, Guid targetWishlistId, CancellationToken ct = default)
    {
        var item = await _context.WishlistItems.FindAsync([itemId], ct)
            ?? throw new InvalidOperationException($"Wishlist item {itemId} not found");

        // Check if item already exists in target
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(i =>
                i.WishlistId == targetWishlistId &&
                i.ProductId == item.ProductId &&
                i.VariantId == item.VariantId, ct);

        if (existing != null)
        {
            // Item exists in target, just remove from source
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync(ct);
            return existing;
        }

        item.WishlistId = targetWishlistId;
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<bool> IsProductInWishlistAsync(Guid wishlistId, Guid productId, Guid? variantId = null, CancellationToken ct = default)
    {
        var query = _context.WishlistItems
            .Where(i => i.WishlistId == wishlistId && i.ProductId == productId);

        if (variantId.HasValue)
        {
            query = query.Where(i => i.VariantId == variantId.Value);
        }

        return await query.AnyAsync(ct);
    }

    #endregion

    #region Statistics

    public async Task<WishlistStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalWishlists = await _context.Wishlists.CountAsync(ct);
        var totalItems = await _context.WishlistItems.CountAsync(ct);
        var publicWishlists = await _context.Wishlists.CountAsync(w => w.IsPublic, ct);
        var customersWithWishlists = await _context.Wishlists.Select(w => w.CustomerId).Distinct().CountAsync(ct);
        var emptyWishlists = await _context.Wishlists.CountAsync(w => !w.Items.Any(), ct);
        var todayCreated = await _context.Wishlists.CountAsync(w => w.CreatedAt >= todayStart, ct);
        var weekCreated = await _context.Wishlists.CountAsync(w => w.CreatedAt >= weekStart, ct);
        var monthCreated = await _context.Wishlists.CountAsync(w => w.CreatedAt >= monthStart, ct);

        return new WishlistStatistics
        {
            TotalWishlists = totalWishlists,
            TotalItems = totalItems,
            PublicWishlists = publicWishlists,
            PrivateWishlists = totalWishlists - publicWishlists,
            CustomersWithWishlists = customersWithWishlists,
            AverageItemsPerWishlist = totalWishlists > 0 ? (double)totalItems / totalWishlists : 0,
            EmptyWishlists = emptyWishlists,
            TodayCreated = todayCreated,
            ThisWeekCreated = weekCreated,
            ThisMonthCreated = monthCreated
        };
    }

    public async Task<List<ProductWishlistCount>> GetMostWishlistedProductsAsync(int count = 10, CancellationToken ct = default)
    {
        return await _context.WishlistItems
            .GroupBy(i => i.ProductId)
            .Select(g => new ProductWishlistCount
            {
                ProductId = g.Key,
                ProductName = g.First().Product != null ? g.First().Product!.Name : null,
                WishlistCount = g.Select(i => i.WishlistId).Distinct().Count()
            })
            .OrderByDescending(x => x.WishlistCount)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<int> GetCustomerWishlistCountAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Wishlists.CountAsync(w => w.CustomerId == customerId, ct);
    }

    public async Task<int> GetCustomerTotalItemsAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.WishlistItems
            .Where(i => i.Wishlist!.CustomerId == customerId)
            .CountAsync(ct);
    }

    #endregion
}
