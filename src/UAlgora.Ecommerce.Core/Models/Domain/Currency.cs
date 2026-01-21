namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a currency configuration.
/// </summary>
public class Currency : SoftDeleteEntity
{
    /// <summary>
    /// ISO 4217 currency code (e.g., USD, EUR, GBP).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Currency name (e.g., US Dollar, Euro).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currency symbol (e.g., $, €, £).
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Native name in local language.
    /// </summary>
    public string? NativeName { get; set; }

    /// <summary>
    /// Number of decimal places for display.
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Decimal separator character.
    /// </summary>
    public string DecimalSeparator { get; set; } = ".";

    /// <summary>
    /// Thousands separator character.
    /// </summary>
    public string ThousandsSeparator { get; set; } = ",";

    /// <summary>
    /// Symbol position relative to amount.
    /// </summary>
    public CurrencySymbolPosition SymbolPosition { get; set; } = CurrencySymbolPosition.Before;

    /// <summary>
    /// Space between symbol and amount.
    /// </summary>
    public bool SpaceBetweenSymbolAndAmount { get; set; } = false;

    /// <summary>
    /// Whether this is the default/base currency.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this currency is active and available.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order in lists.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Rounding precision for calculations.
    /// </summary>
    public CurrencyRounding Rounding { get; set; } = CurrencyRounding.Standard;

    /// <summary>
    /// Rounding increment (e.g., 0.05 for Swiss Franc).
    /// </summary>
    public decimal? RoundingIncrement { get; set; }

    /// <summary>
    /// Countries where this currency is used (ISO country codes).
    /// </summary>
    public List<string> Countries { get; set; } = [];

    /// <summary>
    /// Exchange rates from this currency to others.
    /// </summary>
    public List<ExchangeRate> ExchangeRatesFrom { get; set; } = [];

    /// <summary>
    /// Exchange rates to this currency from others.
    /// </summary>
    public List<ExchangeRate> ExchangeRatesTo { get; set; } = [];

    /// <summary>
    /// Formats a decimal amount according to currency settings.
    /// </summary>
    public string Format(decimal amount)
    {
        var formatted = amount.ToString($"N{DecimalPlaces}")
            .Replace(",", "TEMP")
            .Replace(".", DecimalSeparator)
            .Replace("TEMP", ThousandsSeparator);

        return SymbolPosition switch
        {
            CurrencySymbolPosition.Before => SpaceBetweenSymbolAndAmount
                ? $"{Symbol} {formatted}"
                : $"{Symbol}{formatted}",
            CurrencySymbolPosition.After => SpaceBetweenSymbolAndAmount
                ? $"{formatted} {Symbol}"
                : $"{formatted}{Symbol}",
            _ => $"{Symbol}{formatted}"
        };
    }
}

/// <summary>
/// Represents an exchange rate between two currencies.
/// </summary>
public class ExchangeRate : BaseEntity
{
    /// <summary>
    /// Source currency ID.
    /// </summary>
    public Guid FromCurrencyId { get; set; }

    /// <summary>
    /// Target currency ID.
    /// </summary>
    public Guid ToCurrencyId { get; set; }

    /// <summary>
    /// Exchange rate value (1 FromCurrency = Rate ToCurrency).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Rate source (e.g., manual, API provider name).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// When this rate becomes effective.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// When this rate expires (null = no expiration).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Whether this is the current active rate.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Markup/margin percentage added to the base rate.
    /// </summary>
    public decimal? MarkupPercent { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Currency? FromCurrency { get; set; }

    /// <summary>
    /// Navigation property.
    /// </summary>
    public Currency? ToCurrency { get; set; }

    /// <summary>
    /// Gets the effective rate including markup.
    /// </summary>
    public decimal EffectiveRate => MarkupPercent.HasValue
        ? Rate * (1 + MarkupPercent.Value / 100)
        : Rate;

    /// <summary>
    /// Whether this rate is currently valid.
    /// </summary>
    public bool IsCurrentlyValid => IsActive &&
        DateTime.UtcNow >= EffectiveFrom &&
        (!EffectiveTo.HasValue || DateTime.UtcNow <= EffectiveTo.Value);
}

/// <summary>
/// Currency symbol position.
/// </summary>
public enum CurrencySymbolPosition
{
    /// <summary>
    /// Symbol before amount ($100).
    /// </summary>
    Before = 0,

    /// <summary>
    /// Symbol after amount (100$).
    /// </summary>
    After = 1
}

/// <summary>
/// Currency rounding mode.
/// </summary>
public enum CurrencyRounding
{
    /// <summary>
    /// Standard rounding to decimal places.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Round up (ceiling).
    /// </summary>
    Up = 1,

    /// <summary>
    /// Round down (floor).
    /// </summary>
    Down = 2,

    /// <summary>
    /// Round to nearest increment.
    /// </summary>
    ToIncrement = 3
}
