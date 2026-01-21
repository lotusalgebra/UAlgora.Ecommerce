using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Authorization;

/// <summary>
/// Authorization handler for e-commerce admin access.
/// Validates that the user is an authenticated Umbraco backoffice user with appropriate permissions.
/// </summary>
public class EcommerceAdminAuthorizationHandler : AuthorizationHandler<EcommerceAdminRequirement>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly EcommerceAuthorizationOptions _options;

    public EcommerceAdminAuthorizationHandler(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IOptions<EcommerceAuthorizationOptions> options)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _options = options.Value;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EcommerceAdminRequirement requirement)
    {
        // Check if user is authenticated via backoffice
        var backOfficeSecurity = _backOfficeSecurityAccessor.BackOfficeSecurity;

        if (backOfficeSecurity?.CurrentUser == null)
        {
            // Not authenticated as backoffice user
            return Task.CompletedTask;
        }

        var currentUser = backOfficeSecurity.CurrentUser;

        // Check if user is admin (always has access)
        if (currentUser.IsAdmin())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user belongs to an allowed user group
        if (_options.AllowedUserGroups.Count > 0)
        {
            var userGroups = currentUser.Groups.Select(g => g.Alias).ToList();
            if (userGroups.Any(g => _options.AllowedUserGroups.Contains(g, StringComparer.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Check for specific permission if configured
        if (!string.IsNullOrEmpty(requirement.Permission))
        {
            // Check if user has the required permission on any content
            // For e-commerce, we typically check for a custom permission or section access
            var hasPermission = currentUser.AllowedSections.Contains("ecommerce", StringComparer.OrdinalIgnoreCase)
                || currentUser.AllowedSections.Contains("content", StringComparer.OrdinalIgnoreCase);

            if (hasPermission)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Configuration options for e-commerce authorization.
/// </summary>
public class EcommerceAuthorizationOptions
{
    /// <summary>
    /// Section name in configuration.
    /// </summary>
    public const string SectionName = "Ecommerce:Authorization";

    /// <summary>
    /// List of Umbraco user group aliases that are allowed admin access.
    /// </summary>
    public List<string> AllowedUserGroups { get; set; } = new() { "admin", "ecommerceAdmin" };

    /// <summary>
    /// Whether to require backoffice authentication (default: true).
    /// Set to false to allow API key authentication.
    /// </summary>
    public bool RequireBackofficeAuth { get; set; } = true;

    /// <summary>
    /// API key for external integrations (optional).
    /// </summary>
    public string? ApiKey { get; set; }
}
