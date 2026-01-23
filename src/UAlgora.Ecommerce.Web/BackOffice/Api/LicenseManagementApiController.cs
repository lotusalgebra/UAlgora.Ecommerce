using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for license operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Licenses}")]
public class LicenseManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ILicenseService _licenseService;

    public LicenseManagementApiController(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    /// <summary>
    /// Gets all active licenses.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType<IReadOnlyList<License>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var licenses = await _licenseService.GetActiveAsync();
        return Ok(licenses);
    }

    /// <summary>
    /// Gets a license by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<License>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var license = await _licenseService.GetByIdAsync(id);
        if (license == null)
        {
            return NotFound();
        }
        return Ok(license);
    }

    /// <summary>
    /// Gets a license by key.
    /// </summary>
    [HttpGet("by-key/{key}")]
    [ProducesResponseType<License>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKey(string key)
    {
        var license = await _licenseService.GetByKeyAsync(key);
        if (license == null)
        {
            return NotFound();
        }
        return Ok(license);
    }

    /// <summary>
    /// Creates a new trial license.
    /// </summary>
    [HttpPost("trial")]
    [ProducesResponseType<License>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTrial([FromBody] CreateTrialLicenseRequest request)
    {
        var license = await _licenseService.CreateTrialAsync(
            request.CustomerName,
            request.CustomerEmail,
            request.TrialDays ?? 14);

        return CreatedAtAction(nameof(GetById), new { id = license.Id }, license);
    }

    /// <summary>
    /// Creates a new license.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<License>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateLicenseRequest request)
    {
        var license = new License
        {
            Type = request.Type,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            ValidFrom = request.ValidFrom ?? DateTime.UtcNow,
            ValidUntil = request.ValidUntil,
            LicensedDomains = request.LicensedDomains,
            MaxActivations = request.MaxActivations ?? 1,
            AllowLocalhost = request.AllowLocalhost ?? true
        };

        var created = await _licenseService.CreateAsync(license);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Validates a license key.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType<LicenseValidationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate([FromBody] ValidateLicenseRequest request)
    {
        var result = await _licenseService.ValidateAsync(request.Key, request.Domain);
        return Ok(result);
    }

    /// <summary>
    /// Activates a license.
    /// </summary>
    [HttpPost("activate")]
    [ProducesResponseType<LicenseActivationResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate([FromBody] ActivateLicenseRequest request)
    {
        var result = await _licenseService.ActivateAsync(
            request.Key,
            request.Domain,
            request.MachineFingerprint);
        return Ok(result);
    }

    /// <summary>
    /// Gets enabled features for a license.
    /// </summary>
    [HttpGet("{id:guid}/features")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatures(Guid id)
    {
        var features = await _licenseService.GetEnabledFeaturesAsync(id);
        return Ok(features);
    }

    /// <summary>
    /// Checks if a feature is enabled for a license.
    /// </summary>
    [HttpGet("{id:guid}/features/{feature}")]
    [ProducesResponseType<FeatureCheckResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckFeature(Guid id, string feature)
    {
        var enabled = await _licenseService.IsFeatureEnabledAsync(id, feature);
        return Ok(new FeatureCheckResult { Feature = feature, IsEnabled = enabled });
    }

    /// <summary>
    /// Gets licenses expiring soon.
    /// </summary>
    [HttpGet("expiring-soon")]
    [ProducesResponseType<IReadOnlyList<License>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringSoon([FromQuery] int days = 30)
    {
        var licenses = await _licenseService.GetExpiringSoonAsync(days);
        return Ok(licenses);
    }

    /// <summary>
    /// Renews a license.
    /// </summary>
    [HttpPost("{id:guid}/renew")]
    [ProducesResponseType<License>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Renew(Guid id, [FromBody] RenewLicenseRequest request)
    {
        try
        {
            var license = await _licenseService.RenewAsync(id, request.NewValidUntil);
            return Ok(license);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Upgrades a license to a new type.
    /// </summary>
    [HttpPost("{id:guid}/upgrade")]
    [ProducesResponseType<License>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upgrade(Guid id, [FromBody] UpgradeLicenseRequest request)
    {
        try
        {
            var license = await _licenseService.UpgradeAsync(id, request.NewType);
            return Ok(license);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivates a license.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _licenseService.DeactivateAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return Ok(new { success = true });
    }
}

#region Request Models

public class CreateTrialLicenseRequest
{
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public int? TrialDays { get; set; }
}

public class CreateLicenseRequest
{
    public LicenseType Type { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? LicensedDomains { get; set; }
    public int? MaxActivations { get; set; }
    public bool? AllowLocalhost { get; set; }
}

public class ValidateLicenseRequest
{
    public required string Key { get; set; }
    public string? Domain { get; set; }
}

public class ActivateLicenseRequest
{
    public required string Key { get; set; }
    public required string Domain { get; set; }
    public string? MachineFingerprint { get; set; }
}

public class RenewLicenseRequest
{
    public DateTime NewValidUntil { get; set; }
}

public class UpgradeLicenseRequest
{
    public LicenseType NewType { get; set; }
}

public class FeatureCheckResult
{
    public required string Feature { get; set; }
    public bool IsEnabled { get; set; }
}

#endregion
