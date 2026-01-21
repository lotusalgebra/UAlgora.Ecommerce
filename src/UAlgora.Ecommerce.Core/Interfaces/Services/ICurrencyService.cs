using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for currency management.
/// </summary>
public interface ICurrencyService
{
    #region Currencies

    /// <summary>
    /// Gets all currencies.
    /// </summary>
    Task<List<Currency>> GetAllCurrenciesAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets a currency by ID.
    /// </summary>
    Task<Currency?> GetCurrencyByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a currency by code.
    /// </summary>
    Task<Currency?> GetCurrencyByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets the default currency.
    /// </summary>
    Task<Currency?> GetDefaultCurrencyAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new currency.
    /// </summary>
    Task<Currency> CreateCurrencyAsync(Currency currency, CancellationToken ct = default);

    /// <summary>
    /// Updates a currency.
    /// </summary>
    Task<Currency> UpdateCurrencyAsync(Currency currency, CancellationToken ct = default);

    /// <summary>
    /// Deletes a currency.
    /// </summary>
    Task DeleteCurrencyAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Sets a currency as default.
    /// </summary>
    Task SetDefaultCurrencyAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Toggles currency active status.
    /// </summary>
    Task<Currency> ToggleCurrencyStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Exchange Rates

    /// <summary>
    /// Gets all exchange rates.
    /// </summary>
    Task<List<ExchangeRate>> GetAllExchangeRatesAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets exchange rates for a specific currency.
    /// </summary>
    Task<List<ExchangeRate>> GetExchangeRatesForCurrencyAsync(Guid currencyId, CancellationToken ct = default);

    /// <summary>
    /// Gets an exchange rate by ID.
    /// </summary>
    Task<ExchangeRate?> GetExchangeRateByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the current exchange rate between two currencies.
    /// </summary>
    Task<ExchangeRate?> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode, CancellationToken ct = default);

    /// <summary>
    /// Creates a new exchange rate.
    /// </summary>
    Task<ExchangeRate> CreateExchangeRateAsync(ExchangeRate rate, CancellationToken ct = default);

    /// <summary>
    /// Updates an exchange rate.
    /// </summary>
    Task<ExchangeRate> UpdateExchangeRateAsync(ExchangeRate rate, CancellationToken ct = default);

    /// <summary>
    /// Deletes an exchange rate.
    /// </summary>
    Task DeleteExchangeRateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Toggles exchange rate active status.
    /// </summary>
    Task<ExchangeRate> ToggleExchangeRateStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Conversion

    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    Task<decimal> ConvertAsync(decimal amount, string fromCurrencyCode, string toCurrencyCode, CancellationToken ct = default);

    /// <summary>
    /// Converts a Money object to another currency.
    /// </summary>
    Task<Money> ConvertAsync(Money money, string toCurrencyCode, CancellationToken ct = default);

    /// <summary>
    /// Formats an amount using the specified currency's formatting rules.
    /// </summary>
    Task<string> FormatAsync(decimal amount, string currencyCode, CancellationToken ct = default);

    #endregion

    #region Rate Updates

    /// <summary>
    /// Updates exchange rates from an external provider.
    /// </summary>
    Task<int> UpdateRatesFromProviderAsync(string providerName, CancellationToken ct = default);

    /// <summary>
    /// Gets the last rate update time.
    /// </summary>
    Task<DateTime?> GetLastRateUpdateTimeAsync(CancellationToken ct = default);

    #endregion
}
