using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// License generation service implementation.
/// </summary>
public class LicenseGenerationService : ILicenseGenerationService
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly ILicenseSubscriptionRepository _subscriptionRepository;
    private readonly LicensePortalOptions _options;
    private readonly ILogger<LicenseGenerationService> _logger;

    public LicenseGenerationService(
        ILicenseRepository licenseRepository,
        ILicenseSubscriptionRepository subscriptionRepository,
        IOptions<LicensePortalOptions> options,
        ILogger<LicenseGenerationService> logger)
    {
        _licenseRepository = licenseRepository;
        _subscriptionRepository = subscriptionRepository;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<License> GenerateAndActivateLicenseAsync(
        LicenseType tier,
        string customerEmail,
        string customerName,
        string? companyName,
        string? domain,
        string paymentProvider,
        string? subscriptionId = null)
    {
        // Generate unique license key
        var licenseKey = await GenerateUniqueLicenseKeyAsync(tier);

        var now = DateTime.UtcNow;
        var validUntil = now.AddYears(1);

        // Set features and limits based on tier
        var (features, limits) = GetFeaturesAndLimitsForTier(tier);

        var license = new License
        {
            Id = Guid.NewGuid(),
            Key = licenseKey,
            Type = tier,
            Status = LicenseStatus.Active,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            Company = companyName,
            LicensedDomains = domain,
            ValidFrom = now,
            ValidUntil = validUntil,
            IsLifetime = false,
            GracePeriodDays = 7,
            MaxStores = limits.MaxStores,
            MaxProducts = limits.MaxProducts,
            MaxOrdersPerMonth = limits.MaxOrdersPerMonth,
            MaxAdminUsers = limits.MaxAdminUsers,
            EnabledFeaturesJson = JsonSerializer.Serialize(features),
            PaymentProcessor = paymentProvider,
            SubscriptionId = subscriptionId,
            AutoRenew = true,
            NextRenewalDate = validUntil,
            RenewalAmount = GetPriceForTier(tier),
            RenewalCurrency = "USD",
            ActivationCount = 1,
            MaxActivations = tier == LicenseType.Enterprise ? 5 : 1,
            FirstActivatedAt = now,
            LastActivatedAt = now,
            LastValidatedAt = now,
            LastValidationResult = LicenseValidationResult.Valid,
            AllowLocalhost = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Generate signature for tamper detection
        license.Signature = GenerateLicenseSignature(license);

        // Save license
        await _licenseRepository.AddAsync(license);

        _logger.LogInformation(
            "Generated license {LicenseKey} for {Email}, tier {Tier}, valid until {ValidUntil}",
            licenseKey, customerEmail, tier, validUntil);

        return license;
    }

    public async Task<License?> ExtendLicenseAsync(Guid licenseId)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId);
        if (license == null) return null;

        // Extend by 1 year from current expiry or now (whichever is later)
        var baseDate = license.ValidUntil > DateTime.UtcNow
            ? license.ValidUntil.Value
            : DateTime.UtcNow;

        license.ValidUntil = baseDate.AddYears(1);
        license.NextRenewalDate = license.ValidUntil;
        license.Status = LicenseStatus.Active;
        license.UpdatedAt = DateTime.UtcNow;
        license.Signature = GenerateLicenseSignature(license);

        await _licenseRepository.UpdateAsync(license);

        _logger.LogInformation(
            "Extended license {LicenseId} until {ValidUntil}",
            licenseId, license.ValidUntil);

        return license;
    }

    public async Task<License?> SuspendLicenseAsync(Guid licenseId, string reason)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId);
        if (license == null) return null;

        license.Status = LicenseStatus.Suspended;
        license.Notes = $"Suspended: {reason} at {DateTime.UtcNow:u}";
        license.UpdatedAt = DateTime.UtcNow;

        await _licenseRepository.UpdateAsync(license);

        _logger.LogInformation(
            "Suspended license {LicenseId}, reason: {Reason}",
            licenseId, reason);

        return license;
    }

    public async Task<License?> ReactivateLicenseAsync(Guid licenseId)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId);
        if (license == null) return null;

        license.Status = LicenseStatus.Active;
        license.Notes = $"Reactivated at {DateTime.UtcNow:u}";
        license.UpdatedAt = DateTime.UtcNow;
        license.Signature = GenerateLicenseSignature(license);

        await _licenseRepository.UpdateAsync(license);

        _logger.LogInformation("Reactivated license {LicenseId}", licenseId);

        return license;
    }

    public async Task<License?> GetLicenseBySubscriptionIdAsync(string subscriptionId)
    {
        return await _licenseRepository.FirstOrDefaultAsync(
            l => l.SubscriptionId == subscriptionId);
    }

    private async Task<string> GenerateUniqueLicenseKeyAsync(LicenseType tier)
    {
        string key;
        do
        {
            key = GenerateLicenseKey(tier);
        }
        while (await _licenseRepository.KeyExistsAsync(key));

        return key;
    }

    private static string GenerateLicenseKey(LicenseType tier)
    {
        var prefix = tier switch
        {
            LicenseType.Trial => "ALG-TRL",
            LicenseType.Standard => "ALG-STD",
            LicenseType.Enterprise => "ALG-ENT",
            _ => "ALG-XXX"
        };

        var randomPart = GenerateRandomString(4);
        var datePart = DateTime.UtcNow.ToString("yyMM");
        var checksum = GenerateRandomString(4);

        return $"{prefix}-{randomPart}-{datePart}-{checksum}";
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new char[length];

        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[length];
        rng.GetBytes(buffer);

        for (int i = 0; i < length; i++)
        {
            random[i] = chars[buffer[i] % chars.Length];
        }

        return new string(random);
    }

    private static (List<string> Features, LicenseLimits Limits) GetFeaturesAndLimitsForTier(LicenseType tier)
    {
        return tier switch
        {
            LicenseType.Trial => (
                [
                    LicenseFeatures.GiftCards,
                    LicenseFeatures.Returns,
                    LicenseFeatures.EmailTemplates,
                    LicenseFeatures.AuditLogging,
                    LicenseFeatures.Webhooks,
                    LicenseFeatures.ApiAccess
                ],
                new LicenseLimits { MaxStores = 1, MaxProducts = 100, MaxOrdersPerMonth = 50, MaxAdminUsers = 2 }),

            LicenseType.Standard => (
                [
                    LicenseFeatures.GiftCards,
                    LicenseFeatures.Returns,
                    LicenseFeatures.EmailTemplates,
                    LicenseFeatures.AuditLogging,
                    LicenseFeatures.Webhooks,
                    LicenseFeatures.ApiAccess,
                    LicenseFeatures.AdvancedReporting,
                    LicenseFeatures.MultiCurrency,
                    LicenseFeatures.ImportExport,
                    LicenseFeatures.AdvancedDiscounts
                ],
                new LicenseLimits { MaxStores = 1, MaxProducts = null, MaxOrdersPerMonth = null, MaxAdminUsers = 5 }),

            LicenseType.Enterprise => (
                [
                    LicenseFeatures.MultiStore,
                    LicenseFeatures.GiftCards,
                    LicenseFeatures.Returns,
                    LicenseFeatures.EmailTemplates,
                    LicenseFeatures.AuditLogging,
                    LicenseFeatures.Webhooks,
                    LicenseFeatures.ApiAccess,
                    LicenseFeatures.AdvancedReporting,
                    LicenseFeatures.MultiCurrency,
                    LicenseFeatures.ImportExport,
                    LicenseFeatures.AdvancedDiscounts,
                    LicenseFeatures.WhiteLabeling,
                    LicenseFeatures.PrioritySupport,
                    LicenseFeatures.B2BFeatures,
                    LicenseFeatures.Subscriptions,
                    LicenseFeatures.CustomIntegrations
                ],
                new LicenseLimits { MaxStores = null, MaxProducts = null, MaxOrdersPerMonth = null, MaxAdminUsers = null }),

            _ => ([], new LicenseLimits())
        };
    }

    private decimal GetPriceForTier(LicenseType tier)
    {
        return tier switch
        {
            LicenseType.Standard => _options.Pricing.Standard.AnnualPrice,
            LicenseType.Enterprise => _options.Pricing.Enterprise.AnnualPrice,
            _ => 0
        };
    }

    private static string GenerateLicenseSignature(License license)
    {
        var data = $"{license.Key}:{license.CustomerEmail}:{license.Type}:{license.ValidUntil:O}";
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private class LicenseLimits
    {
        public int? MaxStores { get; set; }
        public int? MaxProducts { get; set; }
        public int? MaxOrdersPerMonth { get; set; }
        public int? MaxAdminUsers { get; set; }
    }
}
