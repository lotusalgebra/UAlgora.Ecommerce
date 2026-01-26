using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for customer authentication operations.
/// </summary>
public interface ICustomerAuthService
{
    /// <summary>
    /// Registers a new customer with email and password.
    /// </summary>
    Task<CustomerAuthResult> RegisterAsync(CustomerRegistrationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validates customer credentials for login.
    /// </summary>
    Task<CustomerAuthResult> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);

    /// <summary>
    /// Changes the customer's password.
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid customerId, string currentPassword, string newPassword, CancellationToken ct = default);

    /// <summary>
    /// Initiates password reset process by generating a reset token.
    /// </summary>
    Task<string?> RequestPasswordResetAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Resets password using the reset token.
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);

    /// <summary>
    /// Gets customer by password reset token.
    /// </summary>
    Task<Customer?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default);
}

/// <summary>
/// Result of an authentication operation.
/// </summary>
public class CustomerAuthResult
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The authenticated customer (if successful).
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Error message (if failed).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Whether the account is locked out.
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static CustomerAuthResult Succeeded(Customer customer) =>
        new() { Success = true, Customer = customer };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static CustomerAuthResult Failed(string error) =>
        new() { Success = false, Error = error };

    /// <summary>
    /// Creates a locked out result.
    /// </summary>
    public static CustomerAuthResult LockedOut() =>
        new() { Success = false, IsLockedOut = true, Error = "Account temporarily locked due to too many failed login attempts." };
}

/// <summary>
/// Request for customer registration.
/// </summary>
public class CustomerRegistrationRequest
{
    /// <summary>
    /// Customer email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Customer first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Customer last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Whether the customer accepts marketing emails.
    /// </summary>
    public bool AcceptsMarketing { get; set; }
}
