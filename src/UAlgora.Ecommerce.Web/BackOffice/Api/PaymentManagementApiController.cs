using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for payment management in the backoffice.
/// </summary>
[ApiController]
[BackOfficeRoute("ecommerce/payment")]
[MapToApi("ecommerce-management-api")]
public class PaymentManagementApiController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentManagementApiController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    #region Payment Methods

    /// <summary>
    /// Gets all payment methods.
    /// </summary>
    [HttpGet("methods")]
    public async Task<IActionResult> GetMethods([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var methods = await _paymentMethodService.GetAllMethodsAsync(includeInactive, ct);
        return Ok(methods.ToList());
    }

    /// <summary>
    /// Gets a payment method by ID.
    /// </summary>
    [HttpGet("method/{id:guid}")]
    public async Task<IActionResult> GetMethod(Guid id, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }
        return Ok(method);
    }

    /// <summary>
    /// Gets a payment method by code.
    /// </summary>
    [HttpGet("method/code/{code}")]
    public async Task<IActionResult> GetMethodByCode(string code, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByCodeAsync(code, ct);
        if (method == null)
        {
            return NotFound();
        }
        return Ok(method);
    }

    /// <summary>
    /// Creates a new payment method.
    /// </summary>
    [HttpPost("method")]
    public async Task<IActionResult> CreateMethod([FromBody] CreatePaymentMethodRequest request, CancellationToken ct = default)
    {
        var method = new PaymentMethodConfig
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            CheckoutInstructions = request.CheckoutInstructions,
            Type = Enum.TryParse<PaymentMethodType>(request.Type, out var type) ? type : PaymentMethodType.CreditCard,
            GatewayId = request.GatewayId,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder,

            // Fee settings
            FeeType = Enum.TryParse<PaymentFeeType>(request.FeeType, out var feeType) ? feeType : PaymentFeeType.None,
            FlatFee = request.FlatFee,
            PercentageFee = request.PercentageFee,
            MaxFee = request.MaxFee,
            ShowFeeAtCheckout = request.ShowFeeAtCheckout,

            // Restrictions
            MinOrderAmount = request.MinOrderAmount,
            MaxOrderAmount = request.MaxOrderAmount,
            AllowedCountries = request.AllowedCountries ?? [],
            ExcludedCountries = request.ExcludedCountries ?? [],
            AllowedCurrencies = request.AllowedCurrencies ?? [],
            AllowedCustomerGroups = request.AllowedCustomerGroups ?? [],

            // Display settings
            IconName = request.IconName,
            ImageUrl = request.ImageUrl,
            ShowCardLogos = request.ShowCardLogos,
            CssClass = request.CssClass,

            // Capture settings
            CaptureMode = Enum.TryParse<PaymentCaptureMode>(request.CaptureMode, out var captureMode) ? captureMode : PaymentCaptureMode.Immediate,
            AutoCaptureDays = request.AutoCaptureDays,

            // Security settings
            Require3DSecure = request.Require3DSecure,
            RequireCvv = request.RequireCvv,
            RequireBillingAddress = request.RequireBillingAddress,
            AllowSavePaymentMethod = request.AllowSavePaymentMethod,

            // Refund settings
            AllowRefunds = request.AllowRefunds,
            AllowPartialRefunds = request.AllowPartialRefunds,
            RefundTimeLimitDays = request.RefundTimeLimitDays
        };

        var validation = await _paymentMethodService.ValidateMethodAsync(method, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var created = await _paymentMethodService.CreateMethodAsync(method, ct);
        return CreatedAtAction(nameof(GetMethod), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a payment method.
    /// </summary>
    [HttpPut("method/{id:guid}")]
    public async Task<IActionResult> UpdateMethod(Guid id, [FromBody] UpdatePaymentMethodRequest request, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.Name = request.Name;
        method.Code = request.Code;
        method.Description = request.Description;
        method.CheckoutInstructions = request.CheckoutInstructions;
        method.Type = Enum.TryParse<PaymentMethodType>(request.Type, out var type) ? type : PaymentMethodType.CreditCard;
        method.GatewayId = request.GatewayId;
        method.IsActive = request.IsActive;
        method.IsDefault = request.IsDefault;
        method.SortOrder = request.SortOrder;

        // Fee settings
        method.FeeType = Enum.TryParse<PaymentFeeType>(request.FeeType, out var feeType) ? feeType : PaymentFeeType.None;
        method.FlatFee = request.FlatFee;
        method.PercentageFee = request.PercentageFee;
        method.MaxFee = request.MaxFee;
        method.ShowFeeAtCheckout = request.ShowFeeAtCheckout;

        // Restrictions
        method.MinOrderAmount = request.MinOrderAmount;
        method.MaxOrderAmount = request.MaxOrderAmount;
        method.AllowedCountries = request.AllowedCountries ?? [];
        method.ExcludedCountries = request.ExcludedCountries ?? [];
        method.AllowedCurrencies = request.AllowedCurrencies ?? [];
        method.AllowedCustomerGroups = request.AllowedCustomerGroups ?? [];

        // Display settings
        method.IconName = request.IconName;
        method.ImageUrl = request.ImageUrl;
        method.ShowCardLogos = request.ShowCardLogos;
        method.CssClass = request.CssClass;

        // Capture settings
        method.CaptureMode = Enum.TryParse<PaymentCaptureMode>(request.CaptureMode, out var captureMode) ? captureMode : PaymentCaptureMode.Immediate;
        method.AutoCaptureDays = request.AutoCaptureDays;

        // Security settings
        method.Require3DSecure = request.Require3DSecure;
        method.RequireCvv = request.RequireCvv;
        method.RequireBillingAddress = request.RequireBillingAddress;
        method.AllowSavePaymentMethod = request.AllowSavePaymentMethod;

        // Refund settings
        method.AllowRefunds = request.AllowRefunds;
        method.AllowPartialRefunds = request.AllowPartialRefunds;
        method.RefundTimeLimitDays = request.RefundTimeLimitDays;

        var validation = await _paymentMethodService.ValidateMethodAsync(method, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var updated = await _paymentMethodService.UpdateMethodAsync(method, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a payment method.
    /// </summary>
    [HttpDelete("method/{id:guid}")]
    public async Task<IActionResult> DeleteMethod(Guid id, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        await _paymentMethodService.DeleteMethodAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Sets a payment method as default.
    /// </summary>
    [HttpPost("method/{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultMethod(Guid id, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        await _paymentMethodService.SetDefaultMethodAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Toggles payment method status.
    /// </summary>
    [HttpPost("method/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleMethodStatus(Guid id, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        var updated = await _paymentMethodService.ToggleMethodStatusAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the sort order of a payment method.
    /// </summary>
    [HttpPost("method/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateMethodSort(Guid id, [FromBody] PaymentSortOrderRequest request, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.SortOrder = request.SortOrder;
        var updated = await _paymentMethodService.UpdateMethodAsync(method, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Toggles 3D Secure requirement for a payment method.
    /// </summary>
    [HttpPost("method/{id:guid}/toggle-3dsecure")]
    public async Task<IActionResult> ToggleMethod3DSecure(Guid id, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.Require3DSecure = !method.Require3DSecure;
        var updated = await _paymentMethodService.UpdateMethodAsync(method, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Toggles refunds for a payment method.
    /// </summary>
    [HttpPost("method/{id:guid}/toggle-refunds")]
    public async Task<IActionResult> ToggleMethodRefunds(Guid id, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.AllowRefunds = !method.AllowRefunds;
        var updated = await _paymentMethodService.UpdateMethodAsync(method, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the fee settings for a payment method.
    /// </summary>
    [HttpPost("method/{id:guid}/update-fee")]
    public async Task<IActionResult> UpdateMethodFee(Guid id, [FromBody] PaymentFeeUpdateRequest request, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        method.FeeType = Enum.TryParse<PaymentFeeType>(request.FeeType, out var feeType) ? feeType : PaymentFeeType.None;
        method.FlatFee = request.FlatFee;
        method.PercentageFee = request.PercentageFee;
        method.MaxFee = request.MaxFee;

        var updated = await _paymentMethodService.UpdateMethodAsync(method, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Duplicates a payment method.
    /// </summary>
    [HttpPost("method/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateMethod(Guid id, CancellationToken ct = default)
    {
        var method = await _paymentMethodService.GetMethodByIdAsync(id, ct);
        if (method == null)
        {
            return NotFound();
        }

        var duplicate = new PaymentMethodConfig
        {
            Name = $"{method.Name} (Copy)",
            Code = $"{method.Code}-copy-{DateTime.UtcNow.Ticks}",
            Description = method.Description,
            CheckoutInstructions = method.CheckoutInstructions,
            Type = method.Type,
            GatewayId = method.GatewayId,
            IsActive = false, // Start inactive
            IsDefault = false, // Don't copy default status
            SortOrder = method.SortOrder + 1,

            // Fee settings
            FeeType = method.FeeType,
            FlatFee = method.FlatFee,
            PercentageFee = method.PercentageFee,
            MaxFee = method.MaxFee,
            ShowFeeAtCheckout = method.ShowFeeAtCheckout,

            // Restrictions
            MinOrderAmount = method.MinOrderAmount,
            MaxOrderAmount = method.MaxOrderAmount,
            AllowedCountries = method.AllowedCountries.ToList(),
            ExcludedCountries = method.ExcludedCountries.ToList(),
            AllowedCurrencies = method.AllowedCurrencies.ToList(),
            AllowedCustomerGroups = method.AllowedCustomerGroups.ToList(),

            // Display settings
            IconName = method.IconName,
            ImageUrl = method.ImageUrl,
            ShowCardLogos = method.ShowCardLogos,
            CssClass = method.CssClass,

            // Capture settings
            CaptureMode = method.CaptureMode,
            AutoCaptureDays = method.AutoCaptureDays,

            // Security settings
            Require3DSecure = method.Require3DSecure,
            RequireCvv = method.RequireCvv,
            RequireBillingAddress = method.RequireBillingAddress,
            AllowSavePaymentMethod = method.AllowSavePaymentMethod,

            // Refund settings
            AllowRefunds = method.AllowRefunds,
            AllowPartialRefunds = method.AllowPartialRefunds,
            RefundTimeLimitDays = method.RefundTimeLimitDays
        };

        var created = await _paymentMethodService.CreateMethodAsync(duplicate, ct);
        return Ok(created);
    }

    /// <summary>
    /// Calculates the fee for a payment method.
    /// </summary>
    [HttpPost("method/{id:guid}/calculate-fee")]
    public async Task<IActionResult> CalculateFee(Guid id, [FromBody] CalculateFeeRequest request, CancellationToken ct = default)
    {
        var result = await _paymentMethodService.CalculateFeeAsync(id, request.OrderAmount, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets payment method statistics.
    /// </summary>
    [HttpGet("method/{id:guid}/statistics")]
    public async Task<IActionResult> GetMethodStatistics(Guid id, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
    {
        var stats = await _paymentMethodService.GetMethodStatisticsAsync(id, from, to, ct);
        return Ok(stats);
    }

    /// <summary>
    /// Gets available payment methods for checkout context.
    /// </summary>
    [HttpPost("methods/available")]
    public async Task<IActionResult> GetAvailableMethods([FromBody] PaymentMethodCheckContext context, CancellationToken ct = default)
    {
        var methods = await _paymentMethodService.GetAvailableMethodsAsync(context, ct);
        return Ok(methods.ToList());
    }

    /// <summary>
    /// Gets fee summary for all available methods.
    /// </summary>
    [HttpPost("methods/fee-summary")]
    public async Task<IActionResult> GetFeeSummary([FromBody] PaymentMethodCheckContext context, CancellationToken ct = default)
    {
        var fees = await _paymentMethodService.GetFeeSummaryAsync(context, ct);
        return Ok(fees.ToList());
    }

    #endregion

    #region Payment Gateways

    /// <summary>
    /// Gets all payment gateways.
    /// </summary>
    [HttpGet("gateways")]
    public async Task<IActionResult> GetGateways([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var gateways = await _paymentMethodService.GetAllGatewaysAsync(includeInactive, ct);
        return Ok(gateways.ToList());
    }

    /// <summary>
    /// Gets a payment gateway by ID.
    /// </summary>
    [HttpGet("gateway/{id:guid}")]
    public async Task<IActionResult> GetGateway(Guid id, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }
        return Ok(gateway);
    }

    /// <summary>
    /// Gets a payment gateway by code.
    /// </summary>
    [HttpGet("gateway/code/{code}")]
    public async Task<IActionResult> GetGatewayByCode(string code, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByCodeAsync(code, ct);
        if (gateway == null)
        {
            return NotFound();
        }
        return Ok(gateway);
    }

    /// <summary>
    /// Gets gateways by provider type.
    /// </summary>
    [HttpGet("gateways/provider/{providerType}")]
    public async Task<IActionResult> GetGatewaysByProvider(string providerType, CancellationToken ct = default)
    {
        if (!Enum.TryParse<PaymentProviderType>(providerType, out var provider))
        {
            return BadRequest(new { message = "Invalid provider type" });
        }

        var gateways = await _paymentMethodService.GetGatewaysByProviderAsync(provider, ct);
        return Ok(gateways.ToList());
    }

    /// <summary>
    /// Creates a new payment gateway.
    /// </summary>
    [HttpPost("gateway")]
    public async Task<IActionResult> CreateGateway([FromBody] CreatePaymentGatewayRequest request, CancellationToken ct = default)
    {
        var gateway = new PaymentGateway
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ProviderType = Enum.TryParse<PaymentProviderType>(request.ProviderType, out var providerType) ? providerType : PaymentProviderType.Stripe,
            IsActive = request.IsActive,
            IsSandbox = request.IsSandbox,
            SortOrder = request.SortOrder,

            // Live credentials
            ApiKey = request.ApiKey,
            SecretKey = request.SecretKey,
            MerchantId = request.MerchantId,
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,

            // Sandbox credentials
            SandboxApiKey = request.SandboxApiKey,
            SandboxSecretKey = request.SandboxSecretKey,
            SandboxMerchantId = request.SandboxMerchantId,

            // Webhook settings
            WebhookUrl = request.WebhookUrl,
            WebhookSecret = request.WebhookSecret,
            SandboxWebhookSecret = request.SandboxWebhookSecret,
            WebhooksEnabled = request.WebhooksEnabled,

            // Supported options
            SupportedCurrencies = request.SupportedCurrencies ?? [],
            SupportedCountries = request.SupportedCountries ?? [],
            SupportedPaymentMethods = request.SupportedPaymentMethods ?? [],

            // Provider-specific settings
            Settings = request.Settings ?? new Dictionary<string, string>(),
            StatementDescriptor = request.StatementDescriptor,
            StatementDescriptorSuffix = request.StatementDescriptorSuffix,
            BrandName = request.BrandName,
            LandingPage = request.LandingPage,
            UserAction = request.UserAction
        };

        var validation = await _paymentMethodService.ValidateGatewayAsync(gateway, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var created = await _paymentMethodService.CreateGatewayAsync(gateway, ct);
        return CreatedAtAction(nameof(GetGateway), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a payment gateway.
    /// </summary>
    [HttpPut("gateway/{id:guid}")]
    public async Task<IActionResult> UpdateGateway(Guid id, [FromBody] UpdatePaymentGatewayRequest request, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        gateway.Name = request.Name;
        gateway.Code = request.Code;
        gateway.Description = request.Description;
        gateway.ProviderType = Enum.TryParse<PaymentProviderType>(request.ProviderType, out var providerType) ? providerType : PaymentProviderType.Stripe;
        gateway.IsActive = request.IsActive;
        gateway.IsSandbox = request.IsSandbox;
        gateway.SortOrder = request.SortOrder;

        // Live credentials
        gateway.ApiKey = request.ApiKey;
        gateway.SecretKey = request.SecretKey;
        gateway.MerchantId = request.MerchantId;
        gateway.ClientId = request.ClientId;
        gateway.ClientSecret = request.ClientSecret;

        // Sandbox credentials
        gateway.SandboxApiKey = request.SandboxApiKey;
        gateway.SandboxSecretKey = request.SandboxSecretKey;
        gateway.SandboxMerchantId = request.SandboxMerchantId;

        // Webhook settings
        gateway.WebhookUrl = request.WebhookUrl;
        gateway.WebhookSecret = request.WebhookSecret;
        gateway.SandboxWebhookSecret = request.SandboxWebhookSecret;
        gateway.WebhooksEnabled = request.WebhooksEnabled;

        // Supported options
        gateway.SupportedCurrencies = request.SupportedCurrencies ?? [];
        gateway.SupportedCountries = request.SupportedCountries ?? [];
        gateway.SupportedPaymentMethods = request.SupportedPaymentMethods ?? [];

        // Provider-specific settings
        gateway.Settings = request.Settings ?? new Dictionary<string, string>();
        gateway.StatementDescriptor = request.StatementDescriptor;
        gateway.StatementDescriptorSuffix = request.StatementDescriptorSuffix;
        gateway.BrandName = request.BrandName;
        gateway.LandingPage = request.LandingPage;
        gateway.UserAction = request.UserAction;

        var validation = await _paymentMethodService.ValidateGatewayAsync(gateway, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)) });
        }

        var updated = await _paymentMethodService.UpdateGatewayAsync(gateway, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a payment gateway.
    /// </summary>
    [HttpDelete("gateway/{id:guid}")]
    public async Task<IActionResult> DeleteGateway(Guid id, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        await _paymentMethodService.DeleteGatewayAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Tests a payment gateway connection.
    /// </summary>
    [HttpPost("gateway/{id:guid}/test")]
    public async Task<IActionResult> TestGatewayConnection(Guid id, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        var result = await _paymentMethodService.TestGatewayConnectionAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Toggles gateway status.
    /// </summary>
    [HttpPost("gateway/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleGatewayStatus(Guid id, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        var updated = await _paymentMethodService.ToggleGatewayStatusAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Toggles gateway sandbox mode.
    /// </summary>
    [HttpPost("gateway/{id:guid}/toggle-sandbox")]
    public async Task<IActionResult> ToggleGatewaySandbox(Guid id, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        var updated = await _paymentMethodService.ToggleGatewaySandboxModeAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the sort order of a payment gateway.
    /// </summary>
    [HttpPost("gateway/{id:guid}/update-sort")]
    public async Task<IActionResult> UpdateGatewaySort(Guid id, [FromBody] PaymentSortOrderRequest request, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        gateway.SortOrder = request.SortOrder;
        var updated = await _paymentMethodService.UpdateGatewayAsync(gateway, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Toggles webhooks for a payment gateway.
    /// </summary>
    [HttpPost("gateway/{id:guid}/toggle-webhooks")]
    public async Task<IActionResult> ToggleGatewayWebhooks(Guid id, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        gateway.WebhooksEnabled = !gateway.WebhooksEnabled;
        var updated = await _paymentMethodService.UpdateGatewayAsync(gateway, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Duplicates a payment gateway.
    /// </summary>
    [HttpPost("gateway/{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateGateway(Guid id, CancellationToken ct = default)
    {
        var gateway = await _paymentMethodService.GetGatewayByIdAsync(id, ct);
        if (gateway == null)
        {
            return NotFound();
        }

        var duplicate = new PaymentGateway
        {
            Name = $"{gateway.Name} (Copy)",
            Code = $"{gateway.Code}-copy-{DateTime.UtcNow.Ticks}",
            Description = gateway.Description,
            ProviderType = gateway.ProviderType,
            IsActive = false, // Start inactive
            IsSandbox = true, // Start in sandbox mode for safety
            SortOrder = gateway.SortOrder + 1,

            // Don't copy credentials for security - user should enter new ones
            // Copy sandbox credentials only
            SandboxApiKey = gateway.SandboxApiKey,
            SandboxSecretKey = gateway.SandboxSecretKey,
            SandboxMerchantId = gateway.SandboxMerchantId,

            // Webhook settings
            WebhooksEnabled = gateway.WebhooksEnabled,

            // Supported options
            SupportedCurrencies = gateway.SupportedCurrencies.ToList(),
            SupportedCountries = gateway.SupportedCountries.ToList(),
            SupportedPaymentMethods = gateway.SupportedPaymentMethods.ToList(),

            // Provider-specific settings
            Settings = new Dictionary<string, string>(gateway.Settings),
            StatementDescriptor = gateway.StatementDescriptor,
            StatementDescriptorSuffix = gateway.StatementDescriptorSuffix,
            BrandName = gateway.BrandName,
            LandingPage = gateway.LandingPage,
            UserAction = gateway.UserAction
        };

        var created = await _paymentMethodService.CreateGatewayAsync(duplicate, ct);
        return Ok(created);
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets overall payment statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetOverviewStatistics([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
    {
        var stats = await _paymentMethodService.GetOverviewStatisticsAsync(from, to, ct);
        return Ok(stats);
    }

    #endregion
}

#region Request/Response Models

public class CreatePaymentMethodRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CheckoutInstructions { get; set; }
    public string Type { get; set; } = "CreditCard";
    public Guid? GatewayId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }

    // Fee settings
    public string FeeType { get; set; } = "None";
    public decimal? FlatFee { get; set; }
    public decimal? PercentageFee { get; set; }
    public decimal? MaxFee { get; set; }
    public bool ShowFeeAtCheckout { get; set; } = true;

    // Restrictions
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxOrderAmount { get; set; }
    public List<string>? AllowedCountries { get; set; }
    public List<string>? ExcludedCountries { get; set; }
    public List<string>? AllowedCurrencies { get; set; }
    public List<string>? AllowedCustomerGroups { get; set; }

    // Display settings
    public string? IconName { get; set; }
    public string? ImageUrl { get; set; }
    public bool ShowCardLogos { get; set; } = true;
    public string? CssClass { get; set; }

    // Capture settings
    public string CaptureMode { get; set; } = "Immediate";
    public int? AutoCaptureDays { get; set; }

    // Security settings
    public bool Require3DSecure { get; set; }
    public bool RequireCvv { get; set; } = true;
    public bool RequireBillingAddress { get; set; } = true;
    public bool AllowSavePaymentMethod { get; set; } = true;

    // Refund settings
    public bool AllowRefunds { get; set; } = true;
    public bool AllowPartialRefunds { get; set; } = true;
    public int RefundTimeLimitDays { get; set; }
}

public class UpdatePaymentMethodRequest : CreatePaymentMethodRequest { }

public class CreatePaymentGatewayRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProviderType { get; set; } = "Stripe";
    public bool IsActive { get; set; } = true;
    public bool IsSandbox { get; set; } = true;
    public int SortOrder { get; set; }

    // Live credentials
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public string? MerchantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    // Sandbox credentials
    public string? SandboxApiKey { get; set; }
    public string? SandboxSecretKey { get; set; }
    public string? SandboxMerchantId { get; set; }

    // Webhook settings
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    public string? SandboxWebhookSecret { get; set; }
    public bool WebhooksEnabled { get; set; } = true;

    // Supported options
    public List<string>? SupportedCurrencies { get; set; }
    public List<string>? SupportedCountries { get; set; }
    public List<string>? SupportedPaymentMethods { get; set; }

    // Provider-specific settings
    public Dictionary<string, string>? Settings { get; set; }
    public string? StatementDescriptor { get; set; }
    public string? StatementDescriptorSuffix { get; set; }
    public string? BrandName { get; set; }
    public string? LandingPage { get; set; }
    public string? UserAction { get; set; }
}

public class UpdatePaymentGatewayRequest : CreatePaymentGatewayRequest { }

public class CalculateFeeRequest
{
    public decimal OrderAmount { get; set; }
}

public class PaymentSortOrderRequest
{
    public int SortOrder { get; set; }
}

public class PaymentFeeUpdateRequest
{
    public string FeeType { get; set; } = "None";
    public decimal? FlatFee { get; set; }
    public decimal? PercentageFee { get; set; }
    public decimal? MaxFee { get; set; }
}

#endregion
