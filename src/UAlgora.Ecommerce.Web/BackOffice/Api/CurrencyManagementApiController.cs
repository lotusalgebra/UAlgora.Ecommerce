using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for currency management in the backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/currency")]
public class CurrencyManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyManagementApiController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    #region Currencies

    /// <summary>
    /// Gets all currencies.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetCurrencies([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var currencies = await _currencyService.GetAllCurrenciesAsync(includeInactive, ct);
        return Ok(new CurrencyListResponse { Items = currencies.ToList() });
    }

    /// <summary>
    /// Gets a currency by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCurrency(Guid id, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }
        return Ok(currency);
    }

    /// <summary>
    /// Gets a currency by code.
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetCurrencyByCode(string code, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByCodeAsync(code, ct);
        if (currency == null)
        {
            return NotFound();
        }
        return Ok(currency);
    }

    /// <summary>
    /// Gets the default currency.
    /// </summary>
    [HttpGet("default")]
    public async Task<IActionResult> GetDefaultCurrency(CancellationToken ct = default)
    {
        var currency = await _currencyService.GetDefaultCurrencyAsync(ct);
        if (currency == null)
        {
            return NotFound(new { message = "No default currency configured" });
        }
        return Ok(currency);
    }

    /// <summary>
    /// Creates a new currency.
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyRequest request, CancellationToken ct = default)
    {
        // Check for duplicate code
        var existing = await _currencyService.GetCurrencyByCodeAsync(request.Code, ct);
        if (existing != null)
        {
            return BadRequest(new { message = $"Currency with code '{request.Code}' already exists" });
        }

        var currency = new Currency
        {
            Code = request.Code,
            Name = request.Name,
            Symbol = request.Symbol,
            NativeName = request.NativeName,
            DecimalPlaces = request.DecimalPlaces,
            DecimalSeparator = request.DecimalSeparator,
            ThousandsSeparator = request.ThousandsSeparator,
            SymbolPosition = Enum.TryParse<CurrencySymbolPosition>(request.SymbolPosition, out var pos) ? pos : CurrencySymbolPosition.Before,
            SpaceBetweenSymbolAndAmount = request.SpaceBetweenSymbolAndAmount,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            Rounding = Enum.TryParse<CurrencyRounding>(request.Rounding, out var rounding) ? rounding : CurrencyRounding.Standard,
            RoundingIncrement = request.RoundingIncrement,
            Countries = request.Countries ?? []
        };

        var created = await _currencyService.CreateCurrencyAsync(currency, ct);
        return CreatedAtAction(nameof(GetCurrency), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a currency.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCurrency(Guid id, [FromBody] UpdateCurrencyRequest request, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        // Check for duplicate code if code is changing
        if (currency.Code != request.Code)
        {
            var existing = await _currencyService.GetCurrencyByCodeAsync(request.Code, ct);
            if (existing != null)
            {
                return BadRequest(new { message = $"Currency with code '{request.Code}' already exists" });
            }
        }

        currency.Code = request.Code;
        currency.Name = request.Name;
        currency.Symbol = request.Symbol;
        currency.NativeName = request.NativeName;
        currency.DecimalPlaces = request.DecimalPlaces;
        currency.DecimalSeparator = request.DecimalSeparator;
        currency.ThousandsSeparator = request.ThousandsSeparator;
        currency.SymbolPosition = Enum.TryParse<CurrencySymbolPosition>(request.SymbolPosition, out var pos) ? pos : CurrencySymbolPosition.Before;
        currency.SpaceBetweenSymbolAndAmount = request.SpaceBetweenSymbolAndAmount;
        currency.IsDefault = request.IsDefault;
        currency.IsActive = request.IsActive;
        currency.SortOrder = request.SortOrder;
        currency.Rounding = Enum.TryParse<CurrencyRounding>(request.Rounding, out var rounding) ? rounding : CurrencyRounding.Standard;
        currency.RoundingIncrement = request.RoundingIncrement;
        currency.Countries = request.Countries ?? [];

        var updated = await _currencyService.UpdateCurrencyAsync(currency, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a currency.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCurrency(Guid id, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        if (currency.IsDefault)
        {
            return BadRequest(new { message = "Cannot delete the default currency" });
        }

        await _currencyService.DeleteCurrencyAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Sets a currency as default.
    /// </summary>
    [HttpPost("{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultCurrency(Guid id, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        await _currencyService.SetDefaultCurrencyAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Toggles the active status of a currency.
    /// </summary>
    [HttpPost("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleCurrencyStatus(Guid id, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        if (currency.IsDefault && currency.IsActive)
        {
            return BadRequest(new { message = "Cannot deactivate the default currency" });
        }

        var updated = await _currencyService.ToggleCurrencyStatusAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the sort order of a currency.
    /// </summary>
    [HttpPost("{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateCurrencySort(Guid id, [FromBody] CurrencySortOrderRequest request, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        currency.SortOrder = request.SortOrder;
        var updated = await _currencyService.UpdateCurrencyAsync(currency, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the decimal places of a currency.
    /// </summary>
    [HttpPost("{id:guid}/update-decimals")]
    public async Task<IActionResult> UpdateCurrencyDecimals(Guid id, [FromBody] CurrencyDecimalRequest request, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        currency.DecimalPlaces = request.DecimalPlaces;
        var updated = await _currencyService.UpdateCurrencyAsync(currency, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the symbol position of a currency.
    /// </summary>
    [HttpPost("{id:guid}/update-symbol-position")]
    public async Task<IActionResult> UpdateCurrencySymbolPosition(Guid id, [FromBody] CurrencySymbolPositionRequest request, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        currency.SymbolPosition = Enum.TryParse<CurrencySymbolPosition>(request.Position, out var pos) ? pos : CurrencySymbolPosition.Before;
        currency.SpaceBetweenSymbolAndAmount = request.SpaceBetween;
        var updated = await _currencyService.UpdateCurrencyAsync(currency, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the rounding settings of a currency.
    /// </summary>
    [HttpPost("{id:guid}/update-rounding")]
    public async Task<IActionResult> UpdateCurrencyRounding(Guid id, [FromBody] CurrencyRoundingRequest request, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        currency.Rounding = Enum.TryParse<CurrencyRounding>(request.Rounding, out var rounding) ? rounding : CurrencyRounding.Standard;
        currency.RoundingIncrement = request.Increment;
        var updated = await _currencyService.UpdateCurrencyAsync(currency, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Duplicates a currency.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateCurrency(Guid id, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        var duplicate = new Currency
        {
            Code = $"{currency.Code}-COPY",
            Name = $"{currency.Name} (Copy)",
            Symbol = currency.Symbol,
            NativeName = currency.NativeName,
            DecimalPlaces = currency.DecimalPlaces,
            DecimalSeparator = currency.DecimalSeparator,
            ThousandsSeparator = currency.ThousandsSeparator,
            SymbolPosition = currency.SymbolPosition,
            SpaceBetweenSymbolAndAmount = currency.SpaceBetweenSymbolAndAmount,
            IsDefault = false, // Don't copy default status
            IsActive = false, // Start inactive
            SortOrder = currency.SortOrder + 1,
            Rounding = currency.Rounding,
            RoundingIncrement = currency.RoundingIncrement,
            Countries = currency.Countries.ToList()
        };

        var created = await _currencyService.CreateCurrencyAsync(duplicate, ct);
        return Ok(created);
    }

    /// <summary>
    /// Formats an amount using the currency's settings.
    /// </summary>
    [HttpGet("{id:guid}/format")]
    public async Task<IActionResult> FormatAmount(Guid id, [FromQuery] decimal amount, CancellationToken ct = default)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id, ct);
        if (currency == null)
        {
            return NotFound();
        }

        var formatted = currency.Format(amount);
        return Ok(new { formatted, amount, currencyCode = currency.Code });
    }

    #endregion

    #region Exchange Rates

    /// <summary>
    /// Gets all exchange rates.
    /// </summary>
    [HttpGet("rate")]
    public async Task<IActionResult> GetExchangeRates([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var rates = await _currencyService.GetAllExchangeRatesAsync(includeInactive, ct);
        return Ok(new ExchangeRateListResponse { Items = rates.ToList() });
    }

    /// <summary>
    /// Gets exchange rates for a specific currency.
    /// </summary>
    [HttpGet("{id:guid}/rates")]
    public async Task<IActionResult> GetCurrencyRates(Guid id, CancellationToken ct = default)
    {
        var rates = await _currencyService.GetExchangeRatesForCurrencyAsync(id, ct);
        return Ok(new ExchangeRateListResponse { Items = rates.ToList() });
    }

    /// <summary>
    /// Gets an exchange rate by ID.
    /// </summary>
    [HttpGet("rate/{id:guid}")]
    public async Task<IActionResult> GetExchangeRate(Guid id, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }
        return Ok(rate);
    }

    /// <summary>
    /// Gets the current exchange rate between two currencies.
    /// </summary>
    [HttpGet("rate/convert")]
    public async Task<IActionResult> GetConversionRate([FromQuery] string from, [FromQuery] string to, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateAsync(from, to, ct);
        if (rate == null)
        {
            return NotFound(new { message = $"No exchange rate found from {from} to {to}" });
        }
        return Ok(rate);
    }

    /// <summary>
    /// Creates a new exchange rate.
    /// </summary>
    [HttpPost("rate")]
    public async Task<IActionResult> CreateExchangeRate([FromBody] CreateExchangeRateRequest request, CancellationToken ct = default)
    {
        // Validate currencies exist
        var fromCurrency = await _currencyService.GetCurrencyByIdAsync(request.FromCurrencyId, ct);
        var toCurrency = await _currencyService.GetCurrencyByIdAsync(request.ToCurrencyId, ct);

        if (fromCurrency == null || toCurrency == null)
        {
            return BadRequest(new { message = "Invalid currency ID(s)" });
        }

        if (request.FromCurrencyId == request.ToCurrencyId)
        {
            return BadRequest(new { message = "Source and target currencies must be different" });
        }

        var rate = new ExchangeRate
        {
            FromCurrencyId = request.FromCurrencyId,
            ToCurrencyId = request.ToCurrencyId,
            Rate = request.Rate,
            Source = request.Source,
            EffectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow,
            EffectiveTo = request.EffectiveTo,
            IsActive = request.IsActive,
            MarkupPercent = request.MarkupPercent
        };

        var created = await _currencyService.CreateExchangeRateAsync(rate, ct);
        return CreatedAtAction(nameof(GetExchangeRate), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an exchange rate.
    /// </summary>
    [HttpPut("rate/{id:guid}")]
    public async Task<IActionResult> UpdateExchangeRate(Guid id, [FromBody] UpdateExchangeRateRequest request, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        rate.Rate = request.Rate;
        rate.Source = request.Source;
        rate.EffectiveFrom = request.EffectiveFrom ?? rate.EffectiveFrom;
        rate.EffectiveTo = request.EffectiveTo;
        rate.IsActive = request.IsActive;
        rate.MarkupPercent = request.MarkupPercent;

        var updated = await _currencyService.UpdateExchangeRateAsync(rate, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an exchange rate.
    /// </summary>
    [HttpDelete("rate/{id:guid}")]
    public async Task<IActionResult> DeleteExchangeRate(Guid id, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        await _currencyService.DeleteExchangeRateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Toggles the active status of an exchange rate.
    /// </summary>
    [HttpPost("rate/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleExchangeRateStatus(Guid id, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        var updated = await _currencyService.ToggleExchangeRateStatusAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the rate value of an exchange rate.
    /// </summary>
    [HttpPost("rate/{id:guid}/update-rate")]
    public async Task<IActionResult> UpdateRateValue(Guid id, [FromBody] ExchangeRateValueRequest request, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        rate.Rate = request.Rate;
        var updated = await _currencyService.UpdateExchangeRateAsync(rate, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the markup percentage of an exchange rate.
    /// </summary>
    [HttpPost("rate/{id:guid}/update-markup")]
    public async Task<IActionResult> UpdateRateMarkup(Guid id, [FromBody] ExchangeRateMarkupRequest request, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        rate.MarkupPercent = request.MarkupPercent;
        var updated = await _currencyService.UpdateExchangeRateAsync(rate, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the effective dates of an exchange rate.
    /// </summary>
    [HttpPost("rate/{id:guid}/update-dates")]
    public async Task<IActionResult> UpdateRateDates(Guid id, [FromBody] ExchangeRateDateRequest request, CancellationToken ct = default)
    {
        var rate = await _currencyService.GetExchangeRateByIdAsync(id, ct);
        if (rate == null)
        {
            return NotFound();
        }

        rate.EffectiveFrom = request.EffectiveFrom ?? rate.EffectiveFrom;
        rate.EffectiveTo = request.EffectiveTo;
        var updated = await _currencyService.UpdateExchangeRateAsync(rate, ct);
        return Ok(updated);
    }

    #endregion

    #region Conversion

    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    [HttpGet("convert")]
    public async Task<IActionResult> ConvertAmount([FromQuery] decimal amount, [FromQuery] string from, [FromQuery] string to, CancellationToken ct = default)
    {
        try
        {
            var converted = await _currencyService.ConvertAsync(amount, from, to, ct);
            var formatted = await _currencyService.FormatAsync(converted, to, ct);
            return Ok(new
            {
                originalAmount = amount,
                fromCurrency = from,
                toCurrency = to,
                convertedAmount = converted,
                formatted
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Rate Updates

    /// <summary>
    /// Gets the last exchange rate update time.
    /// </summary>
    [HttpGet("rate/last-update")]
    public async Task<IActionResult> GetLastRateUpdate(CancellationToken ct = default)
    {
        var lastUpdate = await _currencyService.GetLastRateUpdateTimeAsync(ct);
        return Ok(new { lastUpdate });
    }

    /// <summary>
    /// Updates exchange rates from an external provider.
    /// </summary>
    [HttpPost("rate/update-from-provider")]
    public async Task<IActionResult> UpdateRatesFromProvider([FromBody] UpdateRatesFromProviderRequest request, CancellationToken ct = default)
    {
        var count = await _currencyService.UpdateRatesFromProviderAsync(request.ProviderName, ct);
        return Ok(new { updatedCount = count, provider = request.ProviderName });
    }

    #endregion
}

#region Request/Response Models

public class CurrencyListResponse
{
    public List<Currency> Items { get; set; } = [];
}

public class ExchangeRateListResponse
{
    public List<ExchangeRate> Items { get; set; } = [];
}

public class CreateCurrencyRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? NativeName { get; set; }
    public int DecimalPlaces { get; set; } = 2;
    public string DecimalSeparator { get; set; } = ".";
    public string ThousandsSeparator { get; set; } = ",";
    public string SymbolPosition { get; set; } = "Before";
    public bool SpaceBetweenSymbolAndAmount { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public string Rounding { get; set; } = "Standard";
    public decimal? RoundingIncrement { get; set; }
    public List<string>? Countries { get; set; }
}

public class UpdateCurrencyRequest : CreateCurrencyRequest { }

public class CreateExchangeRateRequest
{
    public Guid FromCurrencyId { get; set; }
    public Guid ToCurrencyId { get; set; }
    public decimal Rate { get; set; }
    public string? Source { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? MarkupPercent { get; set; }
}

public class UpdateExchangeRateRequest
{
    public decimal Rate { get; set; }
    public string? Source { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? MarkupPercent { get; set; }
}

public class CurrencySortOrderRequest
{
    public int SortOrder { get; set; }
}

public class CurrencyDecimalRequest
{
    public int DecimalPlaces { get; set; }
}

public class CurrencySymbolPositionRequest
{
    public string Position { get; set; } = "Before";
    public bool SpaceBetween { get; set; }
}

public class CurrencyRoundingRequest
{
    public string Rounding { get; set; } = "Standard";
    public decimal? Increment { get; set; }
}

public class ExchangeRateValueRequest
{
    public decimal Rate { get; set; }
}

public class ExchangeRateMarkupRequest
{
    public decimal? MarkupPercent { get; set; }
}

public class ExchangeRateDateRequest
{
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class UpdateRatesFromProviderRequest
{
    public string ProviderName { get; set; } = string.Empty;
}

#endregion
