using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for customer operations.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a customer by email.
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets the current logged-in customer.
    /// </summary>
    Task<Customer?> GetCurrentAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets paginated customers.
    /// </summary>
    Task<PagedResult<Customer>> GetPagedAsync(CustomerQueryParameters parameters, CancellationToken ct = default);

    /// <summary>
    /// Searches customers.
    /// </summary>
    Task<IReadOnlyList<Customer>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken ct = default);

    /// <summary>
    /// Gets top customers by spend.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetTopBySpentAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    Task<Customer> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a customer.
    /// </summary>
    Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default);

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds an address to a customer.
    /// </summary>
    Task<Address> AddAddressAsync(Guid customerId, Address address, CancellationToken ct = default);

    /// <summary>
    /// Updates a customer address.
    /// </summary>
    Task<Address> UpdateAddressAsync(Address address, CancellationToken ct = default);

    /// <summary>
    /// Deletes a customer address.
    /// </summary>
    Task DeleteAddressAsync(Guid customerId, Guid addressId, CancellationToken ct = default);

    /// <summary>
    /// Sets the default shipping address.
    /// </summary>
    Task SetDefaultShippingAddressAsync(Guid customerId, Guid addressId, CancellationToken ct = default);

    /// <summary>
    /// Sets the default billing address.
    /// </summary>
    Task SetDefaultBillingAddressAsync(Guid customerId, Guid addressId, CancellationToken ct = default);

    /// <summary>
    /// Gets customer's order history.
    /// </summary>
    Task<IReadOnlyList<Order>> GetOrderHistoryAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Updates customer statistics.
    /// </summary>
    Task UpdateStatisticsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Updates last login timestamp.
    /// </summary>
    Task UpdateLastLoginAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Adds loyalty points to a customer.
    /// </summary>
    Task<int> AddLoyaltyPointsAsync(Guid customerId, int points, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Deducts loyalty points from a customer.
    /// </summary>
    Task<int> DeductLoyaltyPointsAsync(Guid customerId, int points, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Gets loyalty points balance.
    /// </summary>
    Task<int> GetLoyaltyPointsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Adds store credit to a customer.
    /// </summary>
    Task<decimal> AddStoreCreditAsync(Guid customerId, decimal amount, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Deducts store credit from a customer.
    /// </summary>
    Task<decimal> DeductStoreCreditAsync(Guid customerId, decimal amount, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Updates customer marketing preferences.
    /// </summary>
    Task UpdateMarketingPreferencesAsync(Guid customerId, bool acceptsMarketing, CancellationToken ct = default);

    /// <summary>
    /// Validates a customer.
    /// </summary>
    Task<ValidationResult> ValidateAsync(Customer customer, CancellationToken ct = default);

    /// <summary>
    /// Checks if an email is available.
    /// </summary>
    Task<bool> IsEmailAvailableAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
}

/// <summary>
/// Request to create a customer.
/// </summary>
public class CreateCustomerRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public bool AcceptsMarketing { get; set; }
    public string? Source { get; set; }
    public List<string>? Tags { get; set; }
}
