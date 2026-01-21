using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for address operations.
/// </summary>
public class AddressRepository : Repository<Address>, IAddressRepository
{
    public AddressRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Address>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefaultShipping)
            .ThenByDescending(a => a.IsDefaultBilling)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Address>> GetShippingAddressesByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.CustomerId == customerId && (a.Type == AddressType.Shipping || a.Type == AddressType.Both))
            .OrderByDescending(a => a.IsDefaultShipping)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Address>> GetBillingAddressesByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.CustomerId == customerId && (a.Type == AddressType.Billing || a.Type == AddressType.Both))
            .OrderByDescending(a => a.IsDefaultBilling)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Address?> GetDefaultShippingAddressAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.CustomerId == customerId && a.IsDefaultShipping)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Address?> GetDefaultBillingAddressAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(a => a.CustomerId == customerId && a.IsDefaultBilling)
            .FirstOrDefaultAsync(ct);
    }

    public async Task SetDefaultShippingAsync(Guid customerId, Guid addressId, CancellationToken ct = default)
    {
        // Clear existing default
        var existingDefaults = await DbSet
            .Where(a => a.CustomerId == customerId && a.IsDefaultShipping)
            .ToListAsync(ct);

        foreach (var addr in existingDefaults)
        {
            addr.IsDefaultShipping = false;
        }

        // Set new default
        var address = await DbSet
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId, ct);

        if (address != null)
        {
            address.IsDefaultShipping = true;
            // Ensure address type allows shipping
            if (address.Type == AddressType.Billing)
            {
                address.Type = AddressType.Both;
            }
        }

        // Update customer's default shipping address ID
        var customer = await Context.Customers.FindAsync(new object[] { customerId }, ct);
        if (customer != null)
        {
            customer.DefaultShippingAddressId = addressId;
        }

        await Context.SaveChangesAsync(ct);
    }

    public async Task SetDefaultBillingAsync(Guid customerId, Guid addressId, CancellationToken ct = default)
    {
        // Clear existing default
        var existingDefaults = await DbSet
            .Where(a => a.CustomerId == customerId && a.IsDefaultBilling)
            .ToListAsync(ct);

        foreach (var addr in existingDefaults)
        {
            addr.IsDefaultBilling = false;
        }

        // Set new default
        var address = await DbSet
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId, ct);

        if (address != null)
        {
            address.IsDefaultBilling = true;
            // Ensure address type allows billing
            if (address.Type == AddressType.Shipping)
            {
                address.Type = AddressType.Both;
            }
        }

        // Update customer's default billing address ID
        var customer = await Context.Customers.FindAsync(new object[] { customerId }, ct);
        if (customer != null)
        {
            customer.DefaultBillingAddressId = addressId;
        }

        await Context.SaveChangesAsync(ct);
    }

    public async Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var addresses = await DbSet
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(ct);

        DbSet.RemoveRange(addresses);
        await Context.SaveChangesAsync(ct);
    }
}
