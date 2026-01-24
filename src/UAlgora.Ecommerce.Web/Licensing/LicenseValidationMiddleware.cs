using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Web.Licensing;

/// <summary>
/// Middleware that validates the Algora Commerce license on application startup
/// and periodically during runtime.
/// </summary>
public class LicenseValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LicenseValidationMiddleware> _logger;
    private readonly LicenseContext _licenseContext;
    private readonly LicenseOptions _options;
    private readonly SemaphoreSlim _validationLock = new(1, 1);
    private DateTime _lastValidationTime = DateTime.MinValue;

    public LicenseValidationMiddleware(
        RequestDelegate next,
        ILogger<LicenseValidationMiddleware> logger,
        LicenseContext licenseContext,
        IOptions<LicenseOptions> options)
    {
        _next = next;
        _logger = logger;
        _licenseContext = licenseContext;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if validation is needed
        if (ShouldValidate())
        {
            await ValidateLicenseAsync(context);
        }

        // Add license info to response headers for debugging (only in development)
        if (_options.IncludeLicenseHeaders && context.Response.HasStarted == false)
        {
            context.Response.Headers["X-Algora-License-Tier"] = _licenseContext.TierName;
            context.Response.Headers["X-Algora-License-Valid"] = _licenseContext.IsValid.ToString();

            if (_licenseContext.IsTrial)
            {
                context.Response.Headers["X-Algora-License-Trial"] = "true";
                if (_licenseContext.DaysRemaining.HasValue)
                {
                    context.Response.Headers["X-Algora-Trial-Days-Remaining"] = _licenseContext.DaysRemaining.Value.ToString();
                }
            }
        }

        await _next(context);
    }

    private bool ShouldValidate()
    {
        // Always validate if not yet validated
        if (_licenseContext.State == LicenseValidationState.NotValidated)
        {
            return true;
        }

        // Check if validation interval has passed
        var timeSinceLastValidation = DateTime.UtcNow - _lastValidationTime;
        return timeSinceLastValidation > TimeSpan.FromHours(_options.ValidationIntervalHours);
    }

    private async Task ValidateLicenseAsync(HttpContext context)
    {
        // Use semaphore to prevent multiple concurrent validations
        if (!await _validationLock.WaitAsync(TimeSpan.FromSeconds(1)))
        {
            return; // Another validation is in progress
        }

        try
        {
            // Double-check after acquiring lock
            if (!ShouldValidate())
            {
                return;
            }

            _logger.LogInformation("Validating Algora Commerce license...");

            // Get license service from DI
            var licenseService = context.RequestServices.GetRequiredService<ILicenseService>();

            // Get current domain
            var domain = context.Request.Host.Host;

            // Try to validate with configured license key
            if (!string.IsNullOrEmpty(_options.LicenseKey))
            {
                var result = await licenseService.ValidateAsync(_options.LicenseKey, domain);

                if (result.IsValid && result.License != null)
                {
                    _licenseContext.SetValid(result.License, result.EnabledFeatures);
                    _lastValidationTime = DateTime.UtcNow;

                    _logger.LogInformation(
                        "License validated successfully. Type: {Type}, Customer: {Customer}, Expires: {Expires}",
                        result.License.Type,
                        result.License.CustomerName,
                        result.License.ValidUntil?.ToString("yyyy-MM-dd") ?? "Never");

                    if (result.Warnings.Count > 0)
                    {
                        foreach (var warning in result.Warnings)
                        {
                            _logger.LogWarning("License warning: {Warning}", warning);
                        }
                    }

                    return;
                }

                // Validation failed
                var state = result.Result switch
                {
                    LicenseValidationResult.Expired => LicenseValidationState.Expired,
                    LicenseValidationResult.Revoked => LicenseValidationState.Revoked,
                    LicenseValidationResult.Suspended => LicenseValidationState.Suspended,
                    LicenseValidationResult.InvalidKey => LicenseValidationState.Invalid,
                    LicenseValidationResult.GracePeriod => LicenseValidationState.GracePeriod,
                    _ => LicenseValidationState.ValidationFailed
                };

                _licenseContext.SetInvalid(state, result.ErrorMessage);
                _lastValidationTime = DateTime.UtcNow;

                _logger.LogWarning(
                    "License validation failed: {Result} - {Error}",
                    result.Result,
                    result.ErrorMessage);

                // If in grace period, still allow operation
                if (result.Result == LicenseValidationResult.GracePeriod && result.License != null)
                {
                    _licenseContext.SetValid(result.License, result.EnabledFeatures);
                }

                return;
            }

            // No license key configured - try to find an active license by domain
            var activeLicenses = await licenseService.GetActiveAsync();
            var domainLicense = activeLicenses.FirstOrDefault(l =>
                l.LicensedDomains != null &&
                l.LicensedDomains.Split(',').Any(d =>
                    domain.Equals(d.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    domain.EndsWith($".{d.Trim()}", StringComparison.OrdinalIgnoreCase)));

            if (domainLicense != null)
            {
                var result = await licenseService.ValidateAsync(domainLicense.Key, domain);
                if (result.IsValid && result.License != null)
                {
                    _licenseContext.SetValid(result.License, result.EnabledFeatures);
                    _lastValidationTime = DateTime.UtcNow;

                    _logger.LogInformation(
                        "License found by domain. Type: {Type}, Customer: {Customer}",
                        result.License.Type,
                        result.License.CustomerName);
                    return;
                }
            }

            // No valid license found - set to unlicensed mode
            _licenseContext.SetUnlicensed();
            _lastValidationTime = DateTime.UtcNow;

            _logger.LogWarning(
                "No valid Algora Commerce license found. Running in unlicensed mode with limited features. " +
                "Configure 'Algora:License:Key' in appsettings.json to activate.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during license validation");

            // On error, maintain previous state but mark validation as failed
            if (_licenseContext.State == LicenseValidationState.NotValidated)
            {
                _licenseContext.SetInvalid(LicenseValidationState.ValidationFailed, ex.Message);
            }

            _lastValidationTime = DateTime.UtcNow;
        }
        finally
        {
            _validationLock.Release();
        }
    }
}

/// <summary>
/// License configuration options.
/// </summary>
public class LicenseOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Algora:License";

    /// <summary>
    /// The license key.
    /// </summary>
    public string? LicenseKey { get; set; }

    /// <summary>
    /// Validation interval in hours (default: 24).
    /// </summary>
    public int ValidationIntervalHours { get; set; } = 24;

    /// <summary>
    /// Whether to include license info in response headers (for debugging).
    /// </summary>
    public bool IncludeLicenseHeaders { get; set; }

    /// <summary>
    /// Whether to allow operation in unlicensed mode with limited features.
    /// </summary>
    public bool AllowUnlicensedMode { get; set; } = true;

    /// <summary>
    /// Maximum orders allowed in unlicensed mode.
    /// </summary>
    public int UnlicensedModeMaxOrders { get; set; } = 20;
}
