using Microsoft.AspNetCore.Authorization;

namespace UAlgora.Ecommerce.Web.Authorization;

/// <summary>
/// Authorization requirement for e-commerce admin access.
/// </summary>
public class EcommerceAdminRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The permission required for admin access.
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Creates a new instance with the specified permission.
    /// </summary>
    public EcommerceAdminRequirement(string permission = "Ecommerce.Admin")
    {
        Permission = permission;
    }
}
