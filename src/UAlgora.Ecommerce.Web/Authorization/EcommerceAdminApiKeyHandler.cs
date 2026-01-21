using Microsoft.AspNetCore.Authorization;

namespace UAlgora.Ecommerce.Web.Authorization;

/// <summary>
/// Authorization handler that allows access via API key authentication.
/// </summary>
public class EcommerceAdminApiKeyHandler : AuthorizationHandler<EcommerceAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EcommerceAdminRequirement requirement)
    {
        // Check if user was authenticated via API key
        if (context.User.HasClaim(c => c.Type == "EcommerceAdmin" && c.Value == "true"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
