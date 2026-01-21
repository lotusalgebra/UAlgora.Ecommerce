using System.Globalization;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for pricing calculations.
/// </summary>
public class PricingService : IPricingService
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly string _defaultCurrency = "USD";

    public PricingService(
        IProductRepository productRepository,
        ICustomerRepository customerRepository)
    {
        _productRepository = productRepository;
        _customerRepository = customerRepository;
    }

    public async Task<decimal> GetProductPriceAsync(
        Guid productId,
        Guid? variantId = null,
        Guid? customerId = null,
        CancellationToken ct = default)
    {
        var details = await GetPricingDetailsAsync(productId, variantId, customerId, ct);
        return details.CurrentPrice;
    }

    public async Task<PricingDetails> GetPricingDetailsAsync(
        Guid productId,
        Guid? variantId = null,
        Guid? customerId = null,
        CancellationToken ct = default)
    {
        var product = variantId.HasValue
            ? await _productRepository.GetWithVariantsAsync(productId, ct)
            : await _productRepository.GetByIdAsync(productId, ct);

        if (product == null)
        {
            throw new InvalidOperationException($"Product {productId} not found.");
        }

        decimal basePrice;
        decimal? salePrice = null;
        decimal? compareAtPrice = null;

        if (variantId.HasValue)
        {
            var variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Value);
            if (variant == null)
            {
                throw new InvalidOperationException($"Variant {variantId} not found.");
            }

            basePrice = variant.Price ?? product.BasePrice;
            salePrice = variant.SalePrice ?? product.SalePrice;
            compareAtPrice = product.CompareAtPrice;
        }
        else
        {
            basePrice = product.BasePrice;
            salePrice = product.SalePrice;
            compareAtPrice = product.CompareAtPrice;
        }

        // Check for customer-specific pricing
        if (customerId.HasValue)
        {
            var customerPrice = await GetCustomerPriceAsync(productId, customerId.Value, variantId, ct);
            if (customerPrice.HasValue && customerPrice.Value < basePrice)
            {
                basePrice = customerPrice.Value;
            }
        }

        // Determine if on sale
        bool isOnSale = salePrice.HasValue && salePrice.Value < basePrice;
        decimal currentPrice = isOnSale ? salePrice!.Value : basePrice;

        // Sale date range checking could be added via custom properties
        // For now, we trust the SalePrice being set means the sale is active

        // Get tiered pricing
        var tierPrices = await GetTieredPricingAsync(productId, variantId, ct);

        // Calculate discount percentage
        decimal? discountPercentage = null;
        if (isOnSale && basePrice > 0)
        {
            discountPercentage = Math.Round((1 - currentPrice / basePrice) * 100, 2);
        }

        return new PricingDetails
        {
            BasePrice = basePrice,
            SalePrice = salePrice,
            CurrentPrice = currentPrice,
            CompareAtPrice = compareAtPrice ?? basePrice,
            IsOnSale = isOnSale,
            DiscountPercentage = discountPercentage,
            CurrencyCode = _defaultCurrency,
            TaxIncluded = product.TaxIncluded,
            TierPrices = tierPrices.Count > 0 ? tierPrices : null
        };
    }

    public async Task<LineItemPricing> CalculateLineItemPriceAsync(
        Guid productId,
        Guid? variantId,
        int quantity,
        Guid? customerId = null,
        CancellationToken ct = default)
    {
        var details = await GetPricingDetailsAsync(productId, variantId, customerId, ct);
        var unitPrice = details.CurrentPrice;
        string? appliedTier = null;

        // Check for tiered pricing
        if (details.TierPrices != null && details.TierPrices.Count > 0)
        {
            var applicableTier = details.TierPrices
                .Where(t => quantity >= t.MinQuantity && (!t.MaxQuantity.HasValue || quantity <= t.MaxQuantity))
                .OrderByDescending(t => t.MinQuantity)
                .FirstOrDefault();

            if (applicableTier != null)
            {
                unitPrice = applicableTier.Price;
                appliedTier = $"{applicableTier.MinQuantity}+ units";
            }
        }

        var lineTotal = unitPrice * quantity;
        decimal? discountAmount = null;

        if (unitPrice < details.BasePrice)
        {
            discountAmount = (details.BasePrice - unitPrice) * quantity;
        }

        return new LineItemPricing
        {
            UnitPrice = unitPrice,
            OriginalPrice = details.BasePrice,
            Quantity = quantity,
            LineTotal = lineTotal,
            DiscountAmount = discountAmount,
            AppliedTier = appliedTier
        };
    }

    public async Task<decimal> CalculateSubtotalAsync(Cart cart, CancellationToken ct = default)
    {
        decimal subtotal = 0;

        foreach (var item in cart.Items)
        {
            var linePrice = await CalculateLineItemPriceAsync(
                item.ProductId,
                item.VariantId,
                item.Quantity,
                cart.CustomerId,
                ct);

            subtotal += linePrice.LineTotal;
        }

        return subtotal;
    }

    public Task<IReadOnlyList<TierPrice>> GetTieredPricingAsync(
        Guid productId,
        Guid? variantId = null,
        CancellationToken ct = default)
    {
        // Tiered pricing could be implemented via a separate TierPrice entity
        // For now, return empty list as Product doesn't have built-in tier prices
        return Task.FromResult<IReadOnlyList<TierPrice>>(Array.Empty<TierPrice>());
    }

    public async Task<decimal?> GetCustomerPriceAsync(
        Guid productId,
        Guid customerId,
        Guid? variantId = null,
        CancellationToken ct = default)
    {
        // Customer-specific pricing could be implemented via custom pricing rules
        // For now, return null as Product doesn't have built-in customer group prices
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        if (customer == null)
        {
            return null;
        }

        // Customer tier-based pricing could be implemented here
        // by checking customer.CustomerTier and applying discounts
        return null;
    }

    public Task<decimal> ConvertCurrencyAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken ct = default)
    {
        // Simple currency conversion - in production, use a real exchange rate service
        // For now, we use static rates for common currencies
        var rates = new Dictionary<string, decimal>
        {
            { "USD", 1.0m },
            { "EUR", 0.85m },
            { "GBP", 0.73m },
            { "CAD", 1.25m },
            { "AUD", 1.35m },
            { "JPY", 110.0m }
        };

        if (!rates.TryGetValue(fromCurrency.ToUpperInvariant(), out var fromRate))
        {
            fromRate = 1.0m;
        }

        if (!rates.TryGetValue(toCurrency.ToUpperInvariant(), out var toRate))
        {
            toRate = 1.0m;
        }

        // Convert to USD first, then to target currency
        var amountInUsd = amount / fromRate;
        var convertedAmount = amountInUsd * toRate;

        return Task.FromResult(Math.Round(convertedAmount, 2));
    }

    public string FormatPrice(decimal amount, string? currencyCode = null)
    {
        var currency = currencyCode ?? _defaultCurrency;

        return currency.ToUpperInvariant() switch
        {
            "USD" => amount.ToString("C", CultureInfo.GetCultureInfo("en-US")),
            "EUR" => amount.ToString("C", CultureInfo.GetCultureInfo("de-DE")),
            "GBP" => amount.ToString("C", CultureInfo.GetCultureInfo("en-GB")),
            "CAD" => amount.ToString("C", CultureInfo.GetCultureInfo("en-CA")),
            "AUD" => amount.ToString("C", CultureInfo.GetCultureInfo("en-AU")),
            "JPY" => amount.ToString("C", CultureInfo.GetCultureInfo("ja-JP")),
            _ => $"{currency} {amount:N2}"
        };
    }
}
