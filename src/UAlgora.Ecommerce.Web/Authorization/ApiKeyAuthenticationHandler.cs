using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UAlgora.Ecommerce.Web.Authorization;

/// <summary>
/// Authentication handler for API key based authentication.
/// Allows external systems to access admin APIs using a configured API key.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string ApiKeyQueryName = "api_key";

    private readonly EcommerceAuthorizationOptions _authOptions;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<EcommerceAuthorizationOptions> authOptions)
        : base(options, logger, encoder)
    {
        _authOptions = authOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key authentication is configured
        if (string.IsNullOrEmpty(_authOptions.ApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Try to get API key from header first, then query string
        string? apiKey = null;

        if (Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue))
        {
            apiKey = headerValue.FirstOrDefault();
        }
        else if (Request.Query.TryGetValue(ApiKeyQueryName, out var queryValue))
        {
            apiKey = queryValue.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Validate API key using constant-time comparison
        if (!CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(apiKey),
            System.Text.Encoding.UTF8.GetBytes(_authOptions.ApiKey)))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
        }

        // Create claims for the API key user
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ApiKeyUser"),
            new Claim(ClaimTypes.NameIdentifier, "api-key-user"),
            new Claim("EcommerceAdmin", "true"),
            new Claim(ClaimTypes.Role, "EcommerceAdmin")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Options for API key authentication.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
}

/// <summary>
/// Constants for authentication schemes.
/// </summary>
public static class EcommerceAuthenticationSchemes
{
    public const string ApiKey = "EcommerceApiKey";
}
