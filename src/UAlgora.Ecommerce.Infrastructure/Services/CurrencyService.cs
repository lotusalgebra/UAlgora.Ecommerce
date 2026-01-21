using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for currency management operations.
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly EcommerceDbContext _context;

    public CurrencyService(EcommerceDbContext context)
    {
        _context = context;
    }

    #region Currencies

    public async Task<List<Currency>> GetAllCurrenciesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Currencies.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Currency?> GetCurrencyByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Currencies
            .Include(c => c.ExchangeRatesFrom)
            .Include(c => c.ExchangeRatesTo)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Currency?> GetCurrencyByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<Currency?> GetDefaultCurrencyAsync(CancellationToken ct = default)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive, ct);
    }

    public async Task<Currency> CreateCurrencyAsync(Currency currency, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (currency.IsDefault)
        {
            await UnsetDefaultCurrencyAsync(ct);
        }

        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync(ct);
        return currency;
    }

    public async Task<Currency> UpdateCurrencyAsync(Currency currency, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (currency.IsDefault)
        {
            await UnsetDefaultCurrencyAsync(currency.Id, ct);
        }

        _context.Currencies.Update(currency);
        await _context.SaveChangesAsync(ct);
        return currency;
    }

    public async Task DeleteCurrencyAsync(Guid id, CancellationToken ct = default)
    {
        var currency = await _context.Currencies.FindAsync([id], ct);
        if (currency != null)
        {
            currency.IsDeleted = true;
            currency.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task SetDefaultCurrencyAsync(Guid id, CancellationToken ct = default)
    {
        await UnsetDefaultCurrencyAsync(ct);

        var currency = await _context.Currencies.FindAsync([id], ct);
        if (currency != null)
        {
            currency.IsDefault = true;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<Currency> ToggleCurrencyStatusAsync(Guid id, CancellationToken ct = default)
    {
        var currency = await _context.Currencies.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Currency {id} not found");

        currency.IsActive = !currency.IsActive;
        await _context.SaveChangesAsync(ct);
        return currency;
    }

    private async Task UnsetDefaultCurrencyAsync(CancellationToken ct = default)
    {
        var defaultCurrencies = await _context.Currencies
            .Where(c => c.IsDefault)
            .ToListAsync(ct);

        foreach (var c in defaultCurrencies)
        {
            c.IsDefault = false;
        }
    }

    private async Task UnsetDefaultCurrencyAsync(Guid exceptId, CancellationToken ct = default)
    {
        var defaultCurrencies = await _context.Currencies
            .Where(c => c.IsDefault && c.Id != exceptId)
            .ToListAsync(ct);

        foreach (var c in defaultCurrencies)
        {
            c.IsDefault = false;
        }
    }

    #endregion

    #region Exchange Rates

    public async Task<List<ExchangeRate>> GetAllExchangeRatesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.ExchangeRates
            .Include(r => r.FromCurrency)
            .Include(r => r.ToCurrency)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        return await query
            .OrderBy(r => r.FromCurrency!.Code)
            .ThenBy(r => r.ToCurrency!.Code)
            .ToListAsync(ct);
    }

    public async Task<List<ExchangeRate>> GetExchangeRatesForCurrencyAsync(Guid currencyId, CancellationToken ct = default)
    {
        return await _context.ExchangeRates
            .Include(r => r.FromCurrency)
            .Include(r => r.ToCurrency)
            .Where(r => r.FromCurrencyId == currencyId || r.ToCurrencyId == currencyId)
            .Where(r => r.IsActive)
            .ToListAsync(ct);
    }

    public async Task<ExchangeRate?> GetExchangeRateByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ExchangeRates
            .Include(r => r.FromCurrency)
            .Include(r => r.ToCurrency)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<ExchangeRate?> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await _context.ExchangeRates
            .Include(r => r.FromCurrency)
            .Include(r => r.ToCurrency)
            .Where(r => r.FromCurrency!.Code == fromCurrencyCode)
            .Where(r => r.ToCurrency!.Code == toCurrencyCode)
            .Where(r => r.IsActive)
            .Where(r => r.EffectiveFrom <= now)
            .Where(r => !r.EffectiveTo.HasValue || r.EffectiveTo >= now)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ExchangeRate> CreateExchangeRateAsync(ExchangeRate rate, CancellationToken ct = default)
    {
        _context.ExchangeRates.Add(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task<ExchangeRate> UpdateExchangeRateAsync(ExchangeRate rate, CancellationToken ct = default)
    {
        _context.ExchangeRates.Update(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task DeleteExchangeRateAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await _context.ExchangeRates.FindAsync([id], ct);
        if (rate != null)
        {
            _context.ExchangeRates.Remove(rate);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<ExchangeRate> ToggleExchangeRateStatusAsync(Guid id, CancellationToken ct = default)
    {
        var rate = await _context.ExchangeRates.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Exchange rate {id} not found");

        rate.IsActive = !rate.IsActive;
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    #endregion

    #region Conversion

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrencyCode, string toCurrencyCode, CancellationToken ct = default)
    {
        if (fromCurrencyCode == toCurrencyCode)
            return amount;

        var rate = await GetExchangeRateAsync(fromCurrencyCode, toCurrencyCode, ct);

        if (rate != null)
        {
            return amount * rate.EffectiveRate;
        }

        // Try reverse rate
        var reverseRate = await GetExchangeRateAsync(toCurrencyCode, fromCurrencyCode, ct);
        if (reverseRate != null && reverseRate.EffectiveRate != 0)
        {
            return amount / reverseRate.EffectiveRate;
        }

        throw new InvalidOperationException($"No exchange rate found between {fromCurrencyCode} and {toCurrencyCode}");
    }

    public async Task<Money> ConvertAsync(Money money, string toCurrencyCode, CancellationToken ct = default)
    {
        var convertedAmount = await ConvertAsync(money.Amount, money.CurrencyCode, toCurrencyCode, ct);
        return new Money(convertedAmount, toCurrencyCode);
    }

    public async Task<string> FormatAsync(decimal amount, string currencyCode, CancellationToken ct = default)
    {
        var currency = await GetCurrencyByCodeAsync(currencyCode, ct);
        if (currency == null)
        {
            return $"{amount:F2} {currencyCode}";
        }

        return currency.Format(amount);
    }

    #endregion

    #region Rate Updates

    public async Task<int> UpdateRatesFromProviderAsync(string providerName, CancellationToken ct = default)
    {
        // This would integrate with external exchange rate providers
        // For now, return 0 as no provider is implemented
        await Task.CompletedTask;
        return 0;
    }

    public async Task<DateTime?> GetLastRateUpdateTimeAsync(CancellationToken ct = default)
    {
        var lastRate = await _context.ExchangeRates
            .OrderByDescending(r => r.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return lastRate?.UpdatedAt;
    }

    #endregion
}
