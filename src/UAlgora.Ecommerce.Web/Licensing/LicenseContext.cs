using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Web.Licensing;

/// <summary>
/// Holds the current license state for the application.
/// This is a singleton that gets updated by the license validation middleware.
/// </summary>
public class LicenseContext
{
    private readonly object _lock = new();
    private License? _currentLicense;
    private LicenseValidationState _state = LicenseValidationState.NotValidated;
    private DateTime _lastValidated = DateTime.MinValue;
    private IReadOnlyList<string> _enabledFeatures = [];
    private string? _validationError;

    /// <summary>
    /// Gets the current license.
    /// </summary>
    public License? CurrentLicense
    {
        get { lock (_lock) return _currentLicense; }
    }

    /// <summary>
    /// Gets the current validation state.
    /// </summary>
    public LicenseValidationState State
    {
        get { lock (_lock) return _state; }
    }

    /// <summary>
    /// Gets when the license was last validated.
    /// </summary>
    public DateTime LastValidated
    {
        get { lock (_lock) return _lastValidated; }
    }

    /// <summary>
    /// Gets the enabled features for the current license.
    /// </summary>
    public IReadOnlyList<string> EnabledFeatures
    {
        get { lock (_lock) return _enabledFeatures; }
    }

    /// <summary>
    /// Gets the last validation error message.
    /// </summary>
    public string? ValidationError
    {
        get { lock (_lock) return _validationError; }
    }

    /// <summary>
    /// Whether the current license is valid.
    /// </summary>
    public bool IsValid
    {
        get
        {
            lock (_lock)
            {
                return _state == LicenseValidationState.Valid ||
                       _state == LicenseValidationState.GracePeriod ||
                       _state == LicenseValidationState.Trial;
            }
        }
    }

    /// <summary>
    /// Whether the license is in trial mode.
    /// </summary>
    public bool IsTrial
    {
        get
        {
            lock (_lock)
            {
                return _state == LicenseValidationState.Trial ||
                       (_currentLicense?.Type == LicenseType.Trial);
            }
        }
    }

    /// <summary>
    /// Whether the license is in grace period.
    /// </summary>
    public bool IsInGracePeriod
    {
        get { lock (_lock) return _state == LicenseValidationState.GracePeriod; }
    }

    /// <summary>
    /// Days remaining until expiration.
    /// </summary>
    public int? DaysRemaining
    {
        get
        {
            lock (_lock)
            {
                return _currentLicense?.DaysUntilExpiration;
            }
        }
    }

    /// <summary>
    /// Updates the license context with a valid license.
    /// </summary>
    public void SetValid(License license, IReadOnlyList<string> features)
    {
        lock (_lock)
        {
            _currentLicense = license;
            _state = license.Type == LicenseType.Trial
                ? LicenseValidationState.Trial
                : (license.IsInGracePeriod ? LicenseValidationState.GracePeriod : LicenseValidationState.Valid);
            _lastValidated = DateTime.UtcNow;
            _enabledFeatures = features;
            _validationError = null;
        }
    }

    /// <summary>
    /// Updates the license context with an invalid state.
    /// </summary>
    public void SetInvalid(LicenseValidationState state, string? error = null)
    {
        lock (_lock)
        {
            _state = state;
            _lastValidated = DateTime.UtcNow;
            _validationError = error;
        }
    }

    /// <summary>
    /// Sets the context to unlicensed/trial mode.
    /// </summary>
    public void SetUnlicensed()
    {
        lock (_lock)
        {
            _currentLicense = null;
            _state = LicenseValidationState.Unlicensed;
            _lastValidated = DateTime.UtcNow;
            _enabledFeatures = [];
            _validationError = "No valid license found.";
        }
    }

    /// <summary>
    /// Checks if a specific feature is enabled.
    /// </summary>
    public bool IsFeatureEnabled(string feature)
    {
        lock (_lock)
        {
            // In unlicensed mode, allow basic features
            if (_state == LicenseValidationState.Unlicensed)
            {
                return false;
            }

            return _enabledFeatures.Contains(feature);
        }
    }

    /// <summary>
    /// Gets the license tier name.
    /// </summary>
    public string TierName
    {
        get
        {
            lock (_lock)
            {
                if (_currentLicense == null)
                {
                    return "Unlicensed";
                }

                return _currentLicense.Type switch
                {
                    LicenseType.Trial => "Trial",
                    LicenseType.Standard => "Standard",
                    LicenseType.Enterprise => "Enterprise",
                    _ => "Unknown"
                };
            }
        }
    }
}

/// <summary>
/// License validation states.
/// </summary>
public enum LicenseValidationState
{
    /// <summary>
    /// Not yet validated.
    /// </summary>
    NotValidated,

    /// <summary>
    /// License is valid and active.
    /// </summary>
    Valid,

    /// <summary>
    /// License is a valid trial.
    /// </summary>
    Trial,

    /// <summary>
    /// License is in grace period (expired but still functional).
    /// </summary>
    GracePeriod,

    /// <summary>
    /// License is expired.
    /// </summary>
    Expired,

    /// <summary>
    /// License key is invalid.
    /// </summary>
    Invalid,

    /// <summary>
    /// No license configured (unlicensed mode).
    /// </summary>
    Unlicensed,

    /// <summary>
    /// License has been revoked.
    /// </summary>
    Revoked,

    /// <summary>
    /// License has been suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// Validation failed due to network error.
    /// </summary>
    ValidationFailed
}
