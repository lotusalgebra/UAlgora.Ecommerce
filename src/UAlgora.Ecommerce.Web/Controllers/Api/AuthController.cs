using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UAlgora.Ecommerce.Core.Interfaces.Services;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for customer authentication operations.
/// </summary>
public class AuthController : EcommerceApiController
{
    private readonly ICustomerAuthService _authService;
    private readonly ICustomerService _customerService;

    /// <summary>
    /// Authentication scheme name for customer cookies.
    /// </summary>
    public const string AuthScheme = "EcommerceCustomer";

    public AuthController(
        ICustomerAuthService authService,
        ICustomerService customerService)
    {
        _authService = authService;
        _customerService = customerService;
    }

    /// <summary>
    /// Registers a new customer account.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiError("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiError("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return ApiError("First name and last name are required.");
        }

        var registrationRequest = new CustomerRegistrationRequest
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            AcceptsMarketing = request.AcceptsMarketing
        };

        var result = await _authService.RegisterAsync(registrationRequest, ct);

        if (!result.Success)
        {
            return ApiError(result.Error ?? "Registration failed.");
        }

        // Auto-login after registration
        await SignInCustomerAsync(result.Customer!);

        return ApiSuccess(new AuthResponse
        {
            IsAuthenticated = true,
            CustomerId = result.Customer!.Id,
            Email = result.Customer.Email,
            FirstName = result.Customer.FirstName,
            LastName = result.Customer.LastName
        }, "Registration successful.");
    }

    /// <summary>
    /// Authenticates a customer with email and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiError("Email and password are required.");
        }

        var result = await _authService.ValidateCredentialsAsync(request.Email, request.Password, ct);

        if (result.IsLockedOut)
        {
            return ApiError("Account temporarily locked due to too many failed login attempts. Please try again in 15 minutes.", 429);
        }

        if (!result.Success)
        {
            return ApiError(result.Error ?? "Invalid credentials.", 401);
        }

        await SignInCustomerAsync(result.Customer!, request.RememberMe);

        return ApiSuccess(new AuthResponse
        {
            IsAuthenticated = true,
            CustomerId = result.Customer!.Id,
            Email = result.Customer.Email,
            FirstName = result.Customer.FirstName,
            LastName = result.Customer.LastName
        }, "Login successful.");
    }

    /// <summary>
    /// Signs out the current customer.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthScheme);
        return ApiSuccess<object?>(null, "Logged out successfully.");
    }

    /// <summary>
    /// Gets the current authentication status.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct = default)
    {
        var customerId = GetCurrentCustomerId();

        if (customerId == null)
        {
            return ApiSuccess(new AuthResponse { IsAuthenticated = false });
        }

        var customer = await _customerService.GetByIdAsync(customerId.Value, ct);

        if (customer == null)
        {
            // Cookie exists but customer not found - sign out
            await HttpContext.SignOutAsync(AuthScheme);
            return ApiSuccess(new AuthResponse { IsAuthenticated = false });
        }

        return ApiSuccess(new AuthResponse
        {
            IsAuthenticated = true,
            CustomerId = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName
        });
    }

    /// <summary>
    /// Changes the password for the authenticated customer.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct = default)
    {
        var customerId = GetCurrentCustomerId();

        if (customerId == null)
        {
            return ApiError("Not authenticated.", 401);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ApiError("Current password and new password are required.");
        }

        if (request.NewPassword.Length < 8)
        {
            return ApiError("New password must be at least 8 characters.");
        }

        var success = await _authService.ChangePasswordAsync(
            customerId.Value,
            request.CurrentPassword,
            request.NewPassword,
            ct);

        if (!success)
        {
            return ApiError("Current password is incorrect.");
        }

        return ApiSuccess<object?>(null, "Password changed successfully.");
    }

    /// <summary>
    /// Requests a password reset email.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiError("Email is required.");
        }

        // Generate reset token (would normally send email here)
        var token = await _authService.RequestPasswordResetAsync(request.Email, ct);

        // Always return success to prevent email enumeration
        return ApiSuccess(new { Message = "If an account exists with this email, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets password using a reset token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ApiError("Token and new password are required.");
        }

        if (request.NewPassword.Length < 8)
        {
            return ApiError("Password must be at least 8 characters.");
        }

        var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword, ct);

        if (!success)
        {
            return ApiError("Invalid or expired reset token.");
        }

        return ApiSuccess<object?>(null, "Password reset successfully. You can now log in with your new password.");
    }

    #region Private Methods

    private async Task SignInCustomerAsync(Core.Models.Domain.Customer customer, bool rememberMe = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new(ClaimTypes.Email, customer.Email),
            new(ClaimTypes.GivenName, customer.FirstName),
            new(ClaimTypes.Surname, customer.LastName),
            new("CustomerId", customer.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, AuthScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddHours(12)
        };

        await HttpContext.SignInAsync(AuthScheme, claimsPrincipal, authProperties);
    }

    private Guid? GetCurrentCustomerId()
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return null;
        }

        return customerId;
    }

    #endregion
}

#region Request/Response Models

/// <summary>
/// Request model for customer registration.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool AcceptsMarketing { get; set; }
}

/// <summary>
/// Request model for customer login.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

/// <summary>
/// Request model for password change.
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request model for forgot password.
/// </summary>
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request model for password reset.
/// </summary>
public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response model for authentication status.
/// </summary>
public class AuthResponse
{
    public bool IsAuthenticated { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

#endregion
