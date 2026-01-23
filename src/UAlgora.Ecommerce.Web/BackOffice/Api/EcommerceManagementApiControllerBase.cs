using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Base class for all e-commerce management API controllers.
/// Provides common functionality and authorization for backoffice API endpoints.
/// </summary>
// TODO: Re-enable authorization after implementing proper token-based auth in frontend
// [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = EcommerceConstants.ApiName)]
public abstract class EcommerceManagementApiControllerBase : ManagementApiControllerBase
{
}
