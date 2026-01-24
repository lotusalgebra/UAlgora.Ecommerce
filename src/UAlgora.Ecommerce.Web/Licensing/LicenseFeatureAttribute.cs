using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace UAlgora.Ecommerce.Web.Licensing;

/// <summary>
/// Attribute to require a specific license feature to access an action or controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class LicenseFeatureAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Creates a new instance of the LicenseFeatureAttribute.
    /// </summary>
    /// <param name="feature">The required feature (use LicenseFeatures constants).</param>
    public LicenseFeatureAttribute(string feature) : base(typeof(LicenseFeatureFilter))
    {
        Arguments = [feature];
    }
}

/// <summary>
/// Filter that checks if a license feature is enabled.
/// </summary>
public class LicenseFeatureFilter : IAsyncActionFilter
{
    private readonly string _requiredFeature;
    private readonly LicenseContext _licenseContext;

    public LicenseFeatureFilter(string requiredFeature, LicenseContext licenseContext)
    {
        _requiredFeature = requiredFeature;
        _licenseContext = licenseContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check if the license is valid
        if (!_licenseContext.IsValid)
        {
            context.Result = new ObjectResult(new
            {
                error = "License Required",
                message = "A valid Algora Commerce license is required to access this feature.",
                feature = _requiredFeature,
                licenseState = _licenseContext.State.ToString()
            })
            {
                StatusCode = 402 // Payment Required
            };
            return;
        }

        // Check if the specific feature is enabled
        if (!_licenseContext.IsFeatureEnabled(_requiredFeature))
        {
            context.Result = new ObjectResult(new
            {
                error = "Feature Not Licensed",
                message = $"The '{_requiredFeature}' feature is not included in your license tier ({_licenseContext.TierName}).",
                feature = _requiredFeature,
                tier = _licenseContext.TierName,
                upgradeRequired = true
            })
            {
                StatusCode = 402 // Payment Required
            };
            return;
        }

        await next();
    }
}

/// <summary>
/// Attribute to require any valid license to access an action or controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireLicenseAttribute : TypeFilterAttribute
{
    public RequireLicenseAttribute() : base(typeof(RequireLicenseFilter))
    {
    }
}

/// <summary>
/// Filter that checks if any valid license exists.
/// </summary>
public class RequireLicenseFilter : IAsyncActionFilter
{
    private readonly LicenseContext _licenseContext;

    public RequireLicenseFilter(LicenseContext licenseContext)
    {
        _licenseContext = licenseContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!_licenseContext.IsValid)
        {
            context.Result = new ObjectResult(new
            {
                error = "License Required",
                message = "A valid Algora Commerce license is required to access this resource.",
                licenseState = _licenseContext.State.ToString(),
                trialAvailable = true
            })
            {
                StatusCode = 402 // Payment Required
            };
            return;
        }

        await next();
    }
}
