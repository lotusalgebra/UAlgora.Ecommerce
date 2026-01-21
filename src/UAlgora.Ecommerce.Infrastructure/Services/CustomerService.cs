using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for customer operations.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICartContextProvider _contextProvider;

    public CustomerService(
        ICustomerRepository customerRepository,
        IAddressRepository addressRepository,
        IOrderRepository orderRepository,
        ICartContextProvider contextProvider)
    {
        _customerRepository = customerRepository;
        _addressRepository = addressRepository;
        _orderRepository = orderRepository;
        _contextProvider = contextProvider;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _customerRepository.GetByIdAsync(id, ct);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _customerRepository.GetByEmailAsync(email, ct);
    }

    public async Task<Customer?> GetCurrentAsync(CancellationToken ct = default)
    {
        var customerId = _contextProvider.GetCustomerId();
        if (!customerId.HasValue)
        {
            return null;
        }

        return await _customerRepository.GetWithAddressesAsync(customerId.Value, ct);
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(
        CustomerQueryParameters parameters,
        CancellationToken ct = default)
    {
        return await _customerRepository.GetPagedAsync(parameters, ct);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken ct = default)
    {
        return await _customerRepository.SearchAsync(searchTerm, maxResults, ct);
    }

    public async Task<IReadOnlyList<Customer>> GetTopBySpentAsync(int count = 10, CancellationToken ct = default)
    {
        return await _customerRepository.GetTopBySpentAsync(count, ct);
    }

    public async Task<Customer> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        // Check if email is available
        if (await _customerRepository.EmailExistsAsync(request.Email, null, ct))
        {
            throw new InvalidOperationException($"Email {request.Email} is already registered.");
        }

        var customer = new Customer
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Company = request.Company,
            AcceptsMarketing = request.AcceptsMarketing,
            MarketingConsentAt = request.AcceptsMarketing ? DateTime.UtcNow : null,
            Source = request.Source,
            Tags = request.Tags ?? [],
            Status = CustomerStatus.Active
        };

        return await _customerRepository.AddAsync(customer, ct);
    }

    public async Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        return await _customerRepository.UpdateAsync(customer, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _customerRepository.SoftDeleteAsync(id, ct);
    }

    public async Task<Address> AddAddressAsync(Guid customerId, Address address, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetWithAddressesAsync(customerId, ct);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {customerId} not found.");
        }

        address.CustomerId = customerId;

        // If this is the first address, make it default
        if (customer.Addresses.Count == 0)
        {
            address.IsDefaultShipping = true;
            address.IsDefaultBilling = true;
        }

        var savedAddress = await _addressRepository.AddAsync(address, ct);

        // Update customer default address IDs if needed
        if (address.IsDefaultShipping)
        {
            customer.DefaultShippingAddressId = savedAddress.Id;
        }
        if (address.IsDefaultBilling)
        {
            customer.DefaultBillingAddressId = savedAddress.Id;
        }

        await _customerRepository.UpdateAsync(customer, ct);
        return savedAddress;
    }

    public async Task<Address> UpdateAddressAsync(Address address, CancellationToken ct = default)
    {
        return await _addressRepository.UpdateAsync(address, ct);
    }

    public async Task DeleteAddressAsync(Guid customerId, Guid addressId, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {customerId} not found.");
        }

        // Clear default references if deleting default address
        if (customer.DefaultShippingAddressId == addressId)
        {
            customer.DefaultShippingAddressId = null;
        }
        if (customer.DefaultBillingAddressId == addressId)
        {
            customer.DefaultBillingAddressId = null;
        }

        await _addressRepository.DeleteAsync(addressId, ct);
        await _customerRepository.UpdateAsync(customer, ct);
    }

    public async Task SetDefaultShippingAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken ct = default)
    {
        await _addressRepository.SetDefaultShippingAsync(customerId, addressId, ct);
    }

    public async Task SetDefaultBillingAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken ct = default)
    {
        await _addressRepository.SetDefaultBillingAsync(customerId, addressId, ct);
    }

    public async Task<IReadOnlyList<Order>> GetOrderHistoryAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _orderRepository.GetByCustomerIdAsync(customerId, ct);
    }

    public async Task UpdateStatisticsAsync(Guid customerId, CancellationToken ct = default)
    {
        await _customerRepository.UpdateStatisticsAsync(customerId, ct);
    }

    public async Task UpdateLastLoginAsync(Guid customerId, CancellationToken ct = default)
    {
        await _customerRepository.UpdateLastLoginAsync(customerId, ct);
    }

    public async Task<int> AddLoyaltyPointsAsync(
        Guid customerId,
        int points,
        string? reason = null,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {customerId} not found.");
        }

        customer.LoyaltyPoints += points;
        customer.TotalLoyaltyPointsEarned += points;

        await _customerRepository.UpdateAsync(customer, ct);
        return customer.LoyaltyPoints;
    }

    public async Task<int> DeductLoyaltyPointsAsync(
        Guid customerId,
        int points,
        string? reason = null,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {customerId} not found.");
        }

        if (customer.LoyaltyPoints < points)
        {
            throw new InvalidOperationException("Insufficient loyalty points.");
        }

        customer.LoyaltyPoints -= points;

        await _customerRepository.UpdateAsync(customer, ct);
        return customer.LoyaltyPoints;
    }

    public async Task<int> GetLoyaltyPointsAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        return customer?.LoyaltyPoints ?? 0;
    }

    public async Task<decimal> AddStoreCreditAsync(
        Guid customerId,
        decimal amount,
        string? reason = null,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {customerId} not found.");
        }

        customer.StoreCreditBalance += amount;

        await _customerRepository.UpdateAsync(customer, ct);
        return customer.StoreCreditBalance;
    }

    public async Task<decimal> DeductStoreCreditAsync(
        Guid customerId,
        decimal amount,
        string? reason = null,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {customerId} not found.");
        }

        if (customer.StoreCreditBalance < amount)
        {
            throw new InvalidOperationException("Insufficient store credit.");
        }

        customer.StoreCreditBalance -= amount;

        await _customerRepository.UpdateAsync(customer, ct);
        return customer.StoreCreditBalance;
    }

    public async Task UpdateMarketingPreferencesAsync(
        Guid customerId,
        bool acceptsMarketing,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {customerId} not found.");
        }

        customer.AcceptsMarketing = acceptsMarketing;
        customer.MarketingConsentAt = acceptsMarketing ? DateTime.UtcNow : null;

        await _customerRepository.UpdateAsync(customer, ct);
    }

    public async Task<ValidationResult> ValidateAsync(Customer customer, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            errors.Add(new ValidationError { PropertyName = "Email", ErrorMessage = "Email is required." });
        }
        else if (!IsValidEmail(customer.Email))
        {
            errors.Add(new ValidationError { PropertyName = "Email", ErrorMessage = "Invalid email format." });
        }

        if (string.IsNullOrWhiteSpace(customer.FirstName))
        {
            errors.Add(new ValidationError { PropertyName = "FirstName", ErrorMessage = "First name is required." });
        }

        if (string.IsNullOrWhiteSpace(customer.LastName))
        {
            errors.Add(new ValidationError { PropertyName = "LastName", ErrorMessage = "Last name is required." });
        }

        // Check email uniqueness
        if (!string.IsNullOrWhiteSpace(customer.Email))
        {
            var excludeId = customer.Id == Guid.Empty ? null : (Guid?)customer.Id;
            if (await _customerRepository.EmailExistsAsync(customer.Email, excludeId, ct))
            {
                errors.Add(new ValidationError { PropertyName = "Email", ErrorMessage = "Email is already registered." });
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    public async Task<bool> IsEmailAvailableAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken ct = default)
    {
        return !await _customerRepository.EmailExistsAsync(email, excludeId, ct);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
