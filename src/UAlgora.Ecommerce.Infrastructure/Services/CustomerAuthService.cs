using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Customer authentication service implementation.
/// </summary>
public class CustomerAuthService : ICustomerAuthService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher _passwordHasher;

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const int MinPasswordLength = 8;

    public CustomerAuthService(
        ICustomerRepository customerRepository,
        IPasswordHasher passwordHasher)
    {
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public async Task<CustomerAuthResult> RegisterAsync(
        CustomerRegistrationRequest request,
        CancellationToken ct = default)
    {
        // Validate email
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return CustomerAuthResult.Failed("Email is required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check email availability
        if (await _customerRepository.EmailExistsAsync(normalizedEmail, null, ct))
        {
            return CustomerAuthResult.Failed("An account with this email already exists.");
        }

        // Validate password strength
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < MinPasswordLength)
        {
            return CustomerAuthResult.Failed($"Password must be at least {MinPasswordLength} characters.");
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return CustomerAuthResult.Failed("First name and last name are required.");
        }

        // Create customer
        var customer = new Customer
        {
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            AcceptsMarketing = request.AcceptsMarketing,
            MarketingConsentAt = request.AcceptsMarketing ? DateTime.UtcNow : null,
            Status = CustomerStatus.Active,
            Source = "Website Registration",
            EmailVerified = false,
            FailedLoginAttempts = 0
        };

        var created = await _customerRepository.AddAsync(customer, ct);
        return CustomerAuthResult.Succeeded(created);
    }

    /// <inheritdoc />
    public async Task<CustomerAuthResult> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return CustomerAuthResult.Failed("Email and password are required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var customer = await _customerRepository.GetByEmailAsync(normalizedEmail, ct);

        if (customer == null)
        {
            // Don't reveal whether the email exists
            return CustomerAuthResult.Failed("Invalid email or password.");
        }

        // Check if account has a password (might be Umbraco member only)
        if (string.IsNullOrEmpty(customer.PasswordHash))
        {
            return CustomerAuthResult.Failed("This account requires password setup. Please use the forgot password feature.");
        }

        // Check lockout
        if (customer.LockedUntil.HasValue && customer.LockedUntil > DateTime.UtcNow)
        {
            return CustomerAuthResult.LockedOut();
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(password, customer.PasswordHash))
        {
            // Increment failed attempts
            customer.FailedLoginAttempts++;

            if (customer.FailedLoginAttempts >= MaxFailedAttempts)
            {
                customer.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
            }

            await _customerRepository.UpdateAsync(customer, ct);
            return CustomerAuthResult.Failed("Invalid email or password.");
        }

        // Check account status
        if (customer.Status != CustomerStatus.Active)
        {
            return CustomerAuthResult.Failed("Your account is not active. Please contact support.");
        }

        // Successful login - reset failed attempts and update last login
        customer.FailedLoginAttempts = 0;
        customer.LockedUntil = null;
        customer.LastLoginAt = DateTime.UtcNow;
        await _customerRepository.UpdateAsync(customer, ct);

        return CustomerAuthResult.Succeeded(customer);
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(
        Guid customerId,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);

        if (customer == null)
        {
            return false;
        }

        // If no current password hash, can't verify current password
        if (string.IsNullOrEmpty(customer.PasswordHash))
        {
            return false;
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(currentPassword, customer.PasswordHash))
        {
            return false;
        }

        // Validate new password
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < MinPasswordLength)
        {
            return false;
        }

        // Update password
        customer.PasswordHash = _passwordHasher.HashPassword(newPassword);
        customer.PasswordResetToken = null;
        customer.PasswordResetTokenExpiry = null;
        await _customerRepository.UpdateAsync(customer, ct);

        return true;
    }

    /// <inheritdoc />
    public async Task<string?> RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var customer = await _customerRepository.GetByEmailAsync(normalizedEmail, ct);

        if (customer == null)
        {
            // Don't reveal whether the email exists - return null but don't indicate error
            return null;
        }

        // Generate secure token
        var tokenBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        customer.PasswordResetToken = token;
        customer.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _customerRepository.UpdateAsync(customer, ct);

        return token;
    }

    /// <inheritdoc />
    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        if (newPassword.Length < MinPasswordLength)
        {
            return false;
        }

        var customer = await GetByPasswordResetTokenAsync(token, ct);

        if (customer == null)
        {
            return false;
        }

        // Update password and clear reset token
        customer.PasswordHash = _passwordHasher.HashPassword(newPassword);
        customer.PasswordResetToken = null;
        customer.PasswordResetTokenExpiry = null;
        customer.FailedLoginAttempts = 0;
        customer.LockedUntil = null;
        await _customerRepository.UpdateAsync(customer, ct);

        return true;
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        // This would ideally be a repository method, but we can search by token
        // For now, we'll need to add this to the repository interface
        // Returning null as placeholder - needs repository support
        return null;
    }
}
