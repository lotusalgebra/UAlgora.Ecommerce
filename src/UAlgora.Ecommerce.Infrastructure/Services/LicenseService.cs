using System.Security.Cryptography;
using System.Text.Json;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for License operations.
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly ILicenseRepository _licenseRepository;

    // Standard features per license type
    private static readonly Dictionary<LicenseType, List<string>> LicenseFeatures = new()
    {
        [LicenseType.Trial] = new List<string>
        {
            Core.Models.Domain.LicenseFeatures.GiftCards,
            Core.Models.Domain.LicenseFeatures.Returns,
            Core.Models.Domain.LicenseFeatures.EmailTemplates,
            Core.Models.Domain.LicenseFeatures.AuditLogging,
            Core.Models.Domain.LicenseFeatures.Webhooks,
            Core.Models.Domain.LicenseFeatures.ApiAccess
        },
        [LicenseType.Standard] = new List<string>
        {
            Core.Models.Domain.LicenseFeatures.GiftCards,
            Core.Models.Domain.LicenseFeatures.Returns,
            Core.Models.Domain.LicenseFeatures.EmailTemplates,
            Core.Models.Domain.LicenseFeatures.AdvancedReporting,
            Core.Models.Domain.LicenseFeatures.AuditLogging,
            Core.Models.Domain.LicenseFeatures.Webhooks,
            Core.Models.Domain.LicenseFeatures.ApiAccess,
            Core.Models.Domain.LicenseFeatures.AdvancedDiscounts,
            Core.Models.Domain.LicenseFeatures.MultiCurrency,
            Core.Models.Domain.LicenseFeatures.ImportExport
        },
        [LicenseType.Enterprise] = new List<string>
        {
            Core.Models.Domain.LicenseFeatures.MultiStore,
            Core.Models.Domain.LicenseFeatures.GiftCards,
            Core.Models.Domain.LicenseFeatures.Returns,
            Core.Models.Domain.LicenseFeatures.EmailTemplates,
            Core.Models.Domain.LicenseFeatures.AdvancedReporting,
            Core.Models.Domain.LicenseFeatures.AuditLogging,
            Core.Models.Domain.LicenseFeatures.Webhooks,
            Core.Models.Domain.LicenseFeatures.ApiAccess,
            Core.Models.Domain.LicenseFeatures.CustomIntegrations,
            Core.Models.Domain.LicenseFeatures.WhiteLabeling,
            Core.Models.Domain.LicenseFeatures.PrioritySupport,
            Core.Models.Domain.LicenseFeatures.AdvancedDiscounts,
            Core.Models.Domain.LicenseFeatures.B2BFeatures,
            Core.Models.Domain.LicenseFeatures.Subscriptions,
            Core.Models.Domain.LicenseFeatures.MultiCurrency,
            Core.Models.Domain.LicenseFeatures.MultiLanguage,
            Core.Models.Domain.LicenseFeatures.AdvancedInventory,
            Core.Models.Domain.LicenseFeatures.AdvancedShipping,
            Core.Models.Domain.LicenseFeatures.AdvancedTax,
            Core.Models.Domain.LicenseFeatures.ImportExport
        }
    };

    public LicenseService(ILicenseRepository licenseRepository)
    {
        _licenseRepository = licenseRepository;
    }

    public async Task<License?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _licenseRepository.GetByIdAsync(id, ct);
    }

    public async Task<License?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        return await _licenseRepository.GetByKeyAsync(key, ct);
    }

    public async Task<IReadOnlyList<License>> GetByCustomerEmailAsync(string email, CancellationToken ct = default)
    {
        return await _licenseRepository.GetByCustomerEmailAsync(email, ct);
    }

    public async Task<IReadOnlyList<License>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _licenseRepository.GetActiveAsync(ct);
    }

    public async Task<License> CreateAsync(License license, CancellationToken ct = default)
    {
        // Generate key if not provided
        if (string.IsNullOrEmpty(license.Key))
        {
            license.Key = await GenerateKeyAsync(license.Type, ct);
        }

        // Validate unique key
        if (await _licenseRepository.KeyExistsAsync(license.Key, ct: ct))
        {
            throw new InvalidOperationException($"License key already exists.");
        }

        // Set default features based on type
        if (string.IsNullOrEmpty(license.EnabledFeaturesJson) && LicenseFeatures.TryGetValue(license.Type, out var features))
        {
            license.EnabledFeaturesJson = JsonSerializer.Serialize(features);
        }

        // Set default limits based on type
        SetDefaultLimits(license);

        // Generate signature
        license.Signature = GenerateSignature(license);

        return await _licenseRepository.AddAsync(license, ct);
    }

    public async Task<string> GenerateKeyAsync(LicenseType type, CancellationToken ct = default)
    {
        string key;
        do
        {
            key = GenerateUniqueKey(type);
        } while (await _licenseRepository.KeyExistsAsync(key, ct: ct));

        return key;
    }

    private static string GenerateUniqueKey(LicenseType type)
    {
        var prefix = type switch
        {
            LicenseType.Trial => "TRL",
            LicenseType.Standard => "STD",
            LicenseType.Enterprise => "ENT",
            _ => "ALG"
        };

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[16];
        rng.GetBytes(bytes);

        // Format: PREFIX-XXXX-XXXX-XXXX-XXXX
        var hex = Convert.ToHexString(bytes);
        return $"{prefix}-{hex[..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..16]}";
    }

    public async Task<LicenseValidationResponse> ValidateAsync(string key, string? domain = null, CancellationToken ct = default)
    {
        var license = await _licenseRepository.GetByKeyAsync(key, ct);

        if (license == null)
        {
            return new LicenseValidationResponse
            {
                IsValid = false,
                Result = LicenseValidationResult.InvalidKey,
                ErrorMessage = "License key not found."
            };
        }

        // Check status
        if (license.Status == LicenseStatus.Revoked)
        {
            return new LicenseValidationResponse
            {
                IsValid = false,
                Result = LicenseValidationResult.Revoked,
                License = license,
                ErrorMessage = "License has been revoked."
            };
        }

        if (license.Status == LicenseStatus.Suspended)
        {
            return new LicenseValidationResponse
            {
                IsValid = false,
                Result = LicenseValidationResult.Suspended,
                License = license,
                ErrorMessage = "License has been suspended."
            };
        }

        // Check validity dates
        if (license.ValidFrom > DateTime.UtcNow)
        {
            return new LicenseValidationResponse
            {
                IsValid = false,
                Result = LicenseValidationResult.InvalidKey,
                License = license,
                ErrorMessage = "License is not yet valid."
            };
        }

        if (license.IsExpired)
        {
            var result = license.IsInGracePeriod
                ? LicenseValidationResult.GracePeriod
                : LicenseValidationResult.Expired;

            return new LicenseValidationResponse
            {
                IsValid = license.IsInGracePeriod,
                Result = result,
                License = license,
                ErrorMessage = license.IsInGracePeriod
                    ? $"License expired but in grace period. {license.DaysUntilExpiration} days remaining."
                    : "License has expired.",
                DaysRemaining = license.DaysUntilExpiration,
                Warnings = license.IsInGracePeriod
                    ? new List<string> { "License is in grace period. Please renew to avoid service interruption." }
                    : new List<string>()
            };
        }

        // Check domain if provided and license has domain restrictions
        if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(license.LicensedDomains))
        {
            var licensedDomains = license.LicensedDomains.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Allow localhost if enabled
            if (license.AllowLocalhost && (domain == "localhost" || domain.StartsWith("127.") || domain == "::1"))
            {
                // Allowed
            }
            else if (!licensedDomains.Any(d => domain.Equals(d, StringComparison.OrdinalIgnoreCase) ||
                                                domain.EndsWith($".{d}", StringComparison.OrdinalIgnoreCase)))
            {
                return new LicenseValidationResponse
                {
                    IsValid = false,
                    Result = LicenseValidationResult.DomainNotLicensed,
                    License = license,
                    ErrorMessage = $"Domain '{domain}' is not licensed."
                };
            }
        }

        // Verify signature
        var expectedSignature = GenerateSignature(license);
        if (!string.IsNullOrEmpty(license.Signature) && license.Signature != expectedSignature)
        {
            return new LicenseValidationResponse
            {
                IsValid = false,
                Result = LicenseValidationResult.Tampered,
                License = license,
                ErrorMessage = "License integrity check failed."
            };
        }

        // Update validation timestamp
        license.LastValidatedAt = DateTime.UtcNow;
        license.LastValidationResult = LicenseValidationResult.Valid;
        license.ConsecutiveValidationFailures = 0;
        await _licenseRepository.UpdateAsync(license, ct);

        // Get enabled features
        var enabledFeatures = await GetEnabledFeaturesAsync(license.Id, ct);

        // Build warnings
        var warnings = new List<string>();
        if (license.DaysUntilExpiration.HasValue && license.DaysUntilExpiration <= 30)
        {
            warnings.Add($"License expires in {license.DaysUntilExpiration} days. Consider renewing.");
        }

        return new LicenseValidationResponse
        {
            IsValid = true,
            Result = LicenseValidationResult.Valid,
            License = license,
            DaysRemaining = license.DaysUntilExpiration,
            EnabledFeatures = enabledFeatures,
            Warnings = warnings
        };
    }

    public async Task<LicenseActivationResult> ActivateAsync(string key, string domain, string? machineFingerprint = null, CancellationToken ct = default)
    {
        var license = await _licenseRepository.GetByKeyAsync(key, ct);

        if (license == null)
        {
            return new LicenseActivationResult
            {
                Success = false,
                ErrorMessage = "License key not found."
            };
        }

        // Check activation limit
        if (license.ActivationCount >= license.MaxActivations)
        {
            return new LicenseActivationResult
            {
                Success = false,
                ErrorMessage = $"Maximum activations ({license.MaxActivations}) reached.",
                RemainingActivations = 0
            };
        }

        // Activate
        license.ActivationCount++;
        license.LastActivatedAt = DateTime.UtcNow;
        if (!license.FirstActivatedAt.HasValue)
        {
            license.FirstActivatedAt = DateTime.UtcNow;
        }

        // Store domain and fingerprint
        if (!string.IsNullOrEmpty(domain))
        {
            var domains = string.IsNullOrEmpty(license.LicensedDomains)
                ? new List<string>()
                : license.LicensedDomains.Split(',').ToList();

            if (!domains.Contains(domain, StringComparer.OrdinalIgnoreCase))
            {
                domains.Add(domain);
                license.LicensedDomains = string.Join(",", domains);
            }
        }

        if (!string.IsNullOrEmpty(machineFingerprint))
        {
            license.MachineFingerprint = machineFingerprint;
        }

        license.Status = LicenseStatus.Active;
        await _licenseRepository.UpdateAsync(license, ct);

        return new LicenseActivationResult
        {
            Success = true,
            License = license,
            RemainingActivations = license.MaxActivations - license.ActivationCount
        };
    }

    public async Task<bool> DeactivateAsync(Guid licenseId, CancellationToken ct = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, ct);
        if (license == null)
        {
            return false;
        }

        license.Status = LicenseStatus.Suspended;
        await _licenseRepository.UpdateAsync(license, ct);
        return true;
    }

    public async Task<License> RenewAsync(Guid licenseId, DateTime newValidUntil, CancellationToken ct = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, ct);
        if (license == null)
        {
            throw new InvalidOperationException("License not found.");
        }

        license.ValidUntil = newValidUntil;
        license.Status = LicenseStatus.Active;
        license.NextRenewalDate = newValidUntil.AddYears(1);

        // Regenerate signature
        license.Signature = GenerateSignature(license);

        return await _licenseRepository.UpdateAsync(license, ct);
    }

    public async Task<License> UpgradeAsync(Guid licenseId, LicenseType newType, CancellationToken ct = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, ct);
        if (license == null)
        {
            throw new InvalidOperationException("License not found.");
        }

        license.Type = newType;

        // Update features based on new type
        if (LicenseFeatures.TryGetValue(newType, out var features))
        {
            license.EnabledFeaturesJson = JsonSerializer.Serialize(features);
        }

        // Update limits based on new type
        SetDefaultLimits(license);

        // Regenerate signature
        license.Signature = GenerateSignature(license);

        return await _licenseRepository.UpdateAsync(license, ct);
    }

    public async Task<bool> IsFeatureEnabledAsync(Guid licenseId, string feature, CancellationToken ct = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, ct);
        if (license == null || !license.IsValid)
        {
            return false;
        }

        var features = await GetEnabledFeaturesAsync(licenseId, ct);
        return features.Contains(feature);
    }

    public async Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(Guid licenseId, CancellationToken ct = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, ct);
        if (license == null)
        {
            return Array.Empty<string>();
        }

        if (!string.IsNullOrEmpty(license.EnabledFeaturesJson))
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(license.EnabledFeaturesJson) ?? new List<string>();
            }
            catch
            {
                // Fall back to type-based features
            }
        }

        // Return default features for the license type
        return LicenseFeatures.TryGetValue(license.Type, out var features)
            ? features
            : new List<string>();
    }

    public async Task<IReadOnlyList<License>> GetExpiringSoonAsync(int days = 30, CancellationToken ct = default)
    {
        return await _licenseRepository.GetExpiringSoonAsync(days, ct);
    }

    public async Task<License> UpdateAsync(License license, CancellationToken ct = default)
    {
        // Regenerate signature on update
        license.Signature = GenerateSignature(license);
        return await _licenseRepository.UpdateAsync(license, ct);
    }

    public async Task<License> CreateTrialAsync(string customerName, string customerEmail, int trialDays = 14, CancellationToken ct = default)
    {
        var license = new License
        {
            Type = LicenseType.Trial,
            Status = LicenseStatus.Active,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(trialDays),
            MaxStores = 1,
            MaxProducts = 100,
            MaxOrdersPerMonth = 50,
            MaxActivations = 1,
            AllowLocalhost = true,
            GracePeriodDays = 3
        };

        return await CreateAsync(license, ct);
    }

    private static void SetDefaultLimits(License license)
    {
        switch (license.Type)
        {
            case LicenseType.Trial:
                license.MaxStores ??= 1;
                license.MaxProducts ??= 100;
                license.MaxOrdersPerMonth ??= 50;
                license.MaxAdminUsers ??= 1;
                break;

            case LicenseType.Standard:
                license.MaxStores ??= 1;
                license.MaxProducts ??= null; // Unlimited
                license.MaxOrdersPerMonth ??= null; // Unlimited
                license.MaxAdminUsers ??= 5;
                break;

            case LicenseType.Enterprise:
                license.MaxStores ??= null; // Unlimited
                license.MaxProducts ??= null; // Unlimited
                license.MaxOrdersPerMonth ??= null; // Unlimited
                license.MaxAdminUsers ??= null; // Unlimited
                break;
        }
    }

    private static string GenerateSignature(License license)
    {
        // Create a signature from key license properties
        var data = $"{license.Key}|{license.Type}|{license.CustomerEmail}|{license.ValidFrom:O}|{license.ValidUntil:O}|{license.MaxStores}|{license.MaxActivations}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
