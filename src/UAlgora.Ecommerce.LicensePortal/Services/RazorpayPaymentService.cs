using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Razorpay.Api;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// Razorpay payment service implementation.
/// </summary>
public class RazorpayPaymentService : IRazorpayPaymentService
{
    private readonly LicensePortalOptions _options;
    private readonly ILogger<RazorpayPaymentService> _logger;
    private readonly RazorpayClient _client;

    // Exchange rate USD to INR (should be fetched from API in production)
    private const decimal UsdToInrRate = 83m;

    public RazorpayPaymentService(
        IOptions<LicensePortalOptions> options,
        ILogger<RazorpayPaymentService> logger)
    {
        _options = options.Value;
        _logger = logger;

        _client = new RazorpayClient(
            _options.Razorpay.KeyId,
            _options.Razorpay.KeySecret);
    }

    public async Task<RazorpayOrderResponse> CreateOrderAsync(
        LicenseType tier,
        string customerEmail,
        string customerName,
        string? companyName,
        string? domain,
        string currency = "INR")
    {
        var priceInr = GetPriceForTierInr(tier);
        var tierStr = tier.ToString();

        var result = await Task.Run(() =>
        {
            var orderOptions = new Dictionary<string, object>
            {
                { "amount", (int)(priceInr * 100) }, // Razorpay uses paise
                { "currency", currency },
                { "receipt", $"lic_{tierStr}_{DateTime.UtcNow:yyyyMMddHHmmss}" },
                { "notes", new Dictionary<string, string>
                    {
                        { "tier", tierStr },
                        { "customer_email", customerEmail },
                        { "customer_name", customerName },
                        { "company_name", companyName ?? "" },
                        { "domain", domain ?? "" }
                    }
                }
            };

            var order = _client.Order.Create(orderOptions);
            var orderId = order["id"]?.ToString() ?? "";

            return new RazorpayOrderResponse
            {
                OrderId = orderId,
                Amount = priceInr,
                Currency = currency,
                KeyId = _options.Razorpay.KeyId
            };
        });

        _logger.LogInformation(
            "Created Razorpay order {OrderId} for {Email}, tier {Tier}, amount {Amount} {Currency}",
            result.OrderId, customerEmail, tierStr, priceInr.ToString("N2"), currency);

        return result;
    }

    public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
    {
        try
        {
            var generatedSignature = GenerateSignature($"{orderId}|{paymentId}");
            var isValid = generatedSignature == signature;

            _logger.LogInformation(
                "Verifying Razorpay signature for order {OrderId}, payment {PaymentId}: {IsValid}",
                orderId, paymentId, isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Razorpay signature");
            return false;
        }
    }

    public async Task<Razorpay.Api.Payment?> GetPaymentAsync(string paymentId)
    {
        return await Task.Run(() =>
        {
            try
            {
                return _client.Payment.Fetch(paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Razorpay payment {PaymentId}", paymentId);
                return null;
            }
        });
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            var generatedSignature = GenerateSignature(payload);
            return generatedSignature == signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Razorpay webhook signature");
            return false;
        }
    }

    public decimal GetPriceForTierInr(LicenseType tier)
    {
        var usdPrice = tier switch
        {
            LicenseType.Standard => _options.Pricing.Standard.AnnualPrice,
            LicenseType.Enterprise => _options.Pricing.Enterprise.AnnualPrice,
            _ => 0
        };

        return usdPrice * UsdToInrRate;
    }

    private string GenerateSignature(string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_options.Razorpay.KeySecret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }
}
