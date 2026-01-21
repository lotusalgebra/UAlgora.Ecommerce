using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for address operations.
/// </summary>
public interface IAddressRepository : IRepository<Address>
{
    /// <summary>
    /// Gets all addresses for a customer.
    /// </summary>
    Task<IReadOnlyList<Address>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets shipping addresses for a customer.
    /// </summary>
    Task<IReadOnlyList<Address>> GetShippingAddressesByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets billing addresses for a customer.
    /// </summary>
    Task<IReadOnlyList<Address>> GetBillingAddressesByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets the default shipping address for a customer.
    /// </summary>
    Task<Address?> GetDefaultShippingAddressAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets the default billing address for a customer.
    /// </summary>
    Task<Address?> GetDefaultBillingAddressAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Sets an address as the default shipping address.
    /// </summary>
    Task SetDefaultShippingAsync(Guid customerId, Guid addressId, CancellationToken ct = default);

    /// <summary>
    /// Sets an address as the default billing address.
    /// </summary>
    Task SetDefaultBillingAsync(Guid customerId, Guid addressId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all addresses for a customer.
    /// </summary>
    Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
}
