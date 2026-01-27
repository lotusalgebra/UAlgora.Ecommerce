using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.LicensePortal.Controllers;

/// <summary>
/// Development-only controller for testing purposes.
/// Should be disabled in production.
/// </summary>
public class DevController : Controller
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevController> _logger;

    public DevController(
        ILicenseRepository licenseRepository,
        IWebHostEnvironment environment,
        ILogger<DevController> logger)
    {
        _licenseRepository = licenseRepository;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Seeds a test license for development testing.
    /// Only available in Development environment.
    /// </summary>
    [HttpGet("dev/seed-test-license")]
    public async Task<IActionResult> SeedTestLicense(string email = "test@example.com")
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        // Check if a license already exists for this email
        var existing = await _licenseRepository.GetByCustomerEmailAsync(email);
        if (existing.Count > 0)
        {
            return Ok(new
            {
                message = "Test license already exists",
                email,
                licenseKey = existing.First().Key,
                tier = existing.First().Type.ToString(),
                status = existing.First().Status.ToString(),
                validUntil = existing.First().ValidUntil
            });
        }

        // Generate a unique license key
        var licenseKey = $"ALG-STD-TEST-{DateTime.UtcNow:yyMM}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var license = new License
        {
            Id = Guid.NewGuid(),
            Key = licenseKey,
            Type = LicenseType.Standard,
            Status = LicenseStatus.Active,
            CustomerEmail = email,
            CustomerName = "Test User",
            LicensedDomains = "localhost",
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddYears(1),
            MaxStores = 1,
            MaxProducts = null, // Unlimited
            MaxOrdersPerMonth = null, // Unlimited
            EnabledFeaturesJson = "[\"gift_cards\",\"returns\",\"email_templates\",\"webhooks\",\"api_access\",\"advanced_reporting\",\"multi_currency\",\"import_export\",\"advanced_discounts\"]",
            AutoRenew = true,
            PaymentProcessor = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _licenseRepository.AddAsync(license);

        _logger.LogInformation("Created test license {LicenseKey} for {Email}", licenseKey, email);

        return Ok(new
        {
            message = "Test license created successfully",
            email,
            licenseKey,
            tier = "Standard",
            status = "Active",
            validUntil = license.ValidUntil,
            loginUrl = Url.Action("Login", "Account")
        });
    }

    /// <summary>
    /// Lists all licenses in the database.
    /// Only available in Development environment.
    /// </summary>
    [HttpGet("dev/licenses")]
    public async Task<IActionResult> ListLicenses()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var licenses = await _licenseRepository.GetAllAsync();
        return Ok(licenses.Select(l => new
        {
            LicenseKey = l.Key,
            l.CustomerEmail,
            l.CustomerName,
            Type = l.Type.ToString(),
            Status = l.Status.ToString(),
            l.ValidFrom,
            l.ValidUntil
        }));
    }
}
