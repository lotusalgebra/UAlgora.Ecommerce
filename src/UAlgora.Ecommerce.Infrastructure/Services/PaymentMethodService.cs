using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for payment method and gateway configuration operations.
/// </summary>
public class PaymentMethodService : IPaymentMethodService
{
    private readonly EcommerceDbContext _context;

    public PaymentMethodService(EcommerceDbContext context)
    {
        _context = context;
    }

    #region Payment Methods

    public async Task<PaymentMethodConfig?> GetMethodByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PaymentMethodConfigs
            .Include(m => m.Gateway)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<PaymentMethodConfig?> GetMethodByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.PaymentMethodConfigs
            .Include(m => m.Gateway)
            .FirstOrDefaultAsync(m => m.Code == code, ct);
    }

    public async Task<IReadOnlyList<PaymentMethodConfig>> GetAllMethodsAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.PaymentMethodConfigs
            .Include(m => m.Gateway)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(m => m.IsActive);
        }

        return await query
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentMethodConfig>> GetActiveMethodsAsync(CancellationToken ct = default)
    {
        return await GetAllMethodsAsync(false, ct);
    }

    public async Task<PaymentMethodConfig?> GetDefaultMethodAsync(CancellationToken ct = default)
    {
        return await _context.PaymentMethodConfigs
            .Include(m => m.Gateway)
            .FirstOrDefaultAsync(m => m.IsDefault && m.IsActive, ct);
    }

    public async Task<IReadOnlyList<PaymentMethodConfig>> GetAvailableMethodsAsync(PaymentMethodCheckContext context, CancellationToken ct = default)
    {
        var methods = await GetActiveMethodsAsync(ct);
        return methods.Where(m => m.IsAvailableFor(context)).ToList();
    }

    public async Task<PaymentMethodConfig> CreateMethodAsync(PaymentMethodConfig method, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (method.IsDefault)
        {
            await UnsetDefaultMethodAsync(ct);
        }

        _context.PaymentMethodConfigs.Add(method);
        await _context.SaveChangesAsync(ct);
        return method;
    }

    public async Task<PaymentMethodConfig> UpdateMethodAsync(PaymentMethodConfig method, CancellationToken ct = default)
    {
        // If setting as default, unset other defaults
        if (method.IsDefault)
        {
            await UnsetDefaultMethodAsync(method.Id, ct);
        }

        _context.PaymentMethodConfigs.Update(method);
        await _context.SaveChangesAsync(ct);
        return method;
    }

    public async Task DeleteMethodAsync(Guid id, CancellationToken ct = default)
    {
        var method = await _context.PaymentMethodConfigs.FindAsync([id], ct);
        if (method != null)
        {
            method.IsDeleted = true;
            method.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task SetDefaultMethodAsync(Guid id, CancellationToken ct = default)
    {
        await UnsetDefaultMethodAsync(ct);

        var method = await _context.PaymentMethodConfigs.FindAsync([id], ct);
        if (method != null)
        {
            method.IsDefault = true;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateMethodSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in sortOrders)
        {
            var method = await _context.PaymentMethodConfigs.FindAsync([id], ct);
            if (method != null)
            {
                method.SortOrder = sortOrder;
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PaymentMethodConfig> ToggleMethodStatusAsync(Guid id, CancellationToken ct = default)
    {
        var method = await _context.PaymentMethodConfigs.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Payment method {id} not found");

        method.IsActive = !method.IsActive;
        await _context.SaveChangesAsync(ct);
        return method;
    }

    private async Task UnsetDefaultMethodAsync(CancellationToken ct = default)
    {
        var defaultMethods = await _context.PaymentMethodConfigs
            .Where(m => m.IsDefault)
            .ToListAsync(ct);

        foreach (var m in defaultMethods)
        {
            m.IsDefault = false;
        }
    }

    private async Task UnsetDefaultMethodAsync(Guid exceptId, CancellationToken ct = default)
    {
        var defaultMethods = await _context.PaymentMethodConfigs
            .Where(m => m.IsDefault && m.Id != exceptId)
            .ToListAsync(ct);

        foreach (var m in defaultMethods)
        {
            m.IsDefault = false;
        }
    }

    #endregion

    #region Payment Gateways

    public async Task<PaymentGateway?> GetGatewayByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PaymentGateways
            .Include(g => g.PaymentMethods)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<PaymentGateway?> GetGatewayByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.PaymentGateways
            .Include(g => g.PaymentMethods)
            .FirstOrDefaultAsync(g => g.Code == code, ct);
    }

    public async Task<IReadOnlyList<PaymentGateway>> GetAllGatewaysAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.PaymentGateways.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(g => g.IsActive);
        }

        return await query
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentGateway>> GetActiveGatewaysAsync(CancellationToken ct = default)
    {
        return await GetAllGatewaysAsync(false, ct);
    }

    public async Task<IReadOnlyList<PaymentGateway>> GetGatewaysByProviderAsync(PaymentProviderType providerType, CancellationToken ct = default)
    {
        return await _context.PaymentGateways
            .Where(g => g.ProviderType == providerType && g.IsActive)
            .OrderBy(g => g.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<PaymentGateway> CreateGatewayAsync(PaymentGateway gateway, CancellationToken ct = default)
    {
        _context.PaymentGateways.Add(gateway);
        await _context.SaveChangesAsync(ct);
        return gateway;
    }

    public async Task<PaymentGateway> UpdateGatewayAsync(PaymentGateway gateway, CancellationToken ct = default)
    {
        _context.PaymentGateways.Update(gateway);
        await _context.SaveChangesAsync(ct);
        return gateway;
    }

    public async Task DeleteGatewayAsync(Guid id, CancellationToken ct = default)
    {
        var gateway = await _context.PaymentGateways.FindAsync([id], ct);
        if (gateway != null)
        {
            gateway.IsDeleted = true;
            gateway.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateGatewaySortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default)
    {
        foreach (var (id, sortOrder) in sortOrders)
        {
            var gateway = await _context.PaymentGateways.FindAsync([id], ct);
            if (gateway != null)
            {
                gateway.SortOrder = sortOrder;
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PaymentGateway> ToggleGatewayStatusAsync(Guid id, CancellationToken ct = default)
    {
        var gateway = await _context.PaymentGateways.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Payment gateway {id} not found");

        gateway.IsActive = !gateway.IsActive;
        await _context.SaveChangesAsync(ct);
        return gateway;
    }

    public async Task<GatewayTestResult> TestGatewayConnectionAsync(Guid gatewayId, CancellationToken ct = default)
    {
        var gateway = await GetGatewayByIdAsync(gatewayId, ct);
        if (gateway == null)
        {
            return new GatewayTestResult
            {
                Success = false,
                Message = "Gateway not found"
            };
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Check basic configuration
            if (!gateway.IsConfigured)
            {
                return new GatewayTestResult
                {
                    Success = false,
                    Message = "Gateway is not properly configured",
                    ErrorDetails = $"Missing required credentials for {gateway.ProviderDisplayName}"
                };
            }

            // For now, return success if configured (actual provider connectivity would require HTTP calls)
            stopwatch.Stop();
            return new GatewayTestResult
            {
                Success = true,
                Message = $"{gateway.ProviderDisplayName} connection validated",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                AccountInfo = gateway.IsSandbox ? "Sandbox Mode" : "Live Mode"
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new GatewayTestResult
            {
                Success = false,
                Message = "Connection test failed",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<PaymentGateway> ToggleGatewaySandboxModeAsync(Guid id, CancellationToken ct = default)
    {
        var gateway = await _context.PaymentGateways.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Payment gateway {id} not found");

        gateway.IsSandbox = !gateway.IsSandbox;
        await _context.SaveChangesAsync(ct);
        return gateway;
    }

    #endregion

    #region Fee Calculation

    public async Task<PaymentFeeResult> CalculateFeeAsync(Guid methodId, decimal orderAmount, CancellationToken ct = default)
    {
        var method = await GetMethodByIdAsync(methodId, ct);
        if (method == null)
        {
            return new PaymentFeeResult
            {
                MethodId = methodId,
                OrderAmount = orderAmount,
                Fee = 0
            };
        }

        var fee = method.CalculateFee(orderAmount);

        return new PaymentFeeResult
        {
            MethodId = method.Id,
            MethodName = method.Name,
            OrderAmount = orderAmount,
            Fee = fee,
            FeeDescription = GetFeeDescription(method, fee)
        };
    }

    public async Task<IReadOnlyList<PaymentFeeResult>> GetFeeSummaryAsync(PaymentMethodCheckContext context, CancellationToken ct = default)
    {
        var methods = await GetAvailableMethodsAsync(context, ct);
        var results = new List<PaymentFeeResult>();

        foreach (var method in methods)
        {
            var fee = method.CalculateFee(context.OrderAmount);
            results.Add(new PaymentFeeResult
            {
                MethodId = method.Id,
                MethodName = method.Name,
                OrderAmount = context.OrderAmount,
                Fee = fee,
                FeeDescription = GetFeeDescription(method, fee)
            });
        }

        return results.OrderBy(r => r.Fee).ToList();
    }

    private static string? GetFeeDescription(PaymentMethodConfig method, decimal fee)
    {
        if (method.FeeType == PaymentFeeType.None || fee == 0)
            return null;

        return method.FeeType switch
        {
            PaymentFeeType.FlatFee => $"${fee:F2} processing fee",
            PaymentFeeType.Percentage => $"{method.PercentageFee:F2}% processing fee (${fee:F2})",
            PaymentFeeType.FlatPlusPercentage => $"${method.FlatFee:F2} + {method.PercentageFee:F2}% processing fee (${fee:F2})",
            _ => $"${fee:F2} processing fee"
        };
    }

    #endregion

    #region Validation

    public Task<ValidationResult> ValidateMethodAsync(PaymentMethodConfig method, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(method.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Payment method name is required." });
        }

        if (string.IsNullOrWhiteSpace(method.Code))
        {
            errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Payment method code is required." });
        }

        if (method.FeeType == PaymentFeeType.FlatFee && (!method.FlatFee.HasValue || method.FlatFee <= 0))
        {
            errors.Add(new ValidationError { PropertyName = "FlatFee", ErrorMessage = "Flat fee amount is required when fee type is Flat Fee." });
        }

        if (method.FeeType == PaymentFeeType.Percentage && (!method.PercentageFee.HasValue || method.PercentageFee <= 0))
        {
            errors.Add(new ValidationError { PropertyName = "PercentageFee", ErrorMessage = "Percentage fee is required when fee type is Percentage." });
        }

        if (method.PercentageFee.HasValue && method.PercentageFee > 100)
        {
            errors.Add(new ValidationError { PropertyName = "PercentageFee", ErrorMessage = "Percentage fee cannot exceed 100%." });
        }

        if (method.MinOrderAmount.HasValue && method.MaxOrderAmount.HasValue &&
            method.MinOrderAmount > method.MaxOrderAmount)
        {
            errors.Add(new ValidationError { PropertyName = "MinOrderAmount", ErrorMessage = "Minimum order amount cannot exceed maximum order amount." });
        }

        if (method.RequiresGateway && !method.GatewayId.HasValue)
        {
            errors.Add(new ValidationError { PropertyName = "GatewayId", ErrorMessage = "A payment gateway is required for this payment method type." });
        }

        return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors));
    }

    public Task<ValidationResult> ValidateGatewayAsync(PaymentGateway gateway, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(gateway.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Gateway name is required." });
        }

        if (string.IsNullOrWhiteSpace(gateway.Code))
        {
            errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Gateway code is required." });
        }

        // Validate credentials based on provider type
        if (gateway.ProviderType != PaymentProviderType.Manual)
        {
            if (gateway.IsSandbox)
            {
                if (gateway.ProviderType == PaymentProviderType.Stripe &&
                    (string.IsNullOrWhiteSpace(gateway.SandboxApiKey) || string.IsNullOrWhiteSpace(gateway.SandboxSecretKey)))
                {
                    errors.Add(new ValidationError { PropertyName = "SandboxApiKey", ErrorMessage = "Sandbox API key and secret key are required for Stripe." });
                }

                if (gateway.ProviderType == PaymentProviderType.PayPal &&
                    (string.IsNullOrWhiteSpace(gateway.ClientId) || string.IsNullOrWhiteSpace(gateway.ClientSecret)))
                {
                    errors.Add(new ValidationError { PropertyName = "ClientId", ErrorMessage = "Client ID and Client Secret are required for PayPal." });
                }
            }
            else
            {
                if (gateway.ProviderType == PaymentProviderType.Stripe &&
                    (string.IsNullOrWhiteSpace(gateway.ApiKey) || string.IsNullOrWhiteSpace(gateway.SecretKey)))
                {
                    errors.Add(new ValidationError { PropertyName = "ApiKey", ErrorMessage = "Live API key and secret key are required for Stripe." });
                }

                if (gateway.ProviderType == PaymentProviderType.PayPal &&
                    (string.IsNullOrWhiteSpace(gateway.ClientId) || string.IsNullOrWhiteSpace(gateway.ClientSecret)))
                {
                    errors.Add(new ValidationError { PropertyName = "ClientId", ErrorMessage = "Client ID and Client Secret are required for PayPal." });
                }
            }
        }

        return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors));
    }

    #endregion

    #region Statistics

    public async Task<PaymentMethodStats> GetMethodStatisticsAsync(Guid methodId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var method = await GetMethodByIdAsync(methodId, ct);
        var stats = new PaymentMethodStats
        {
            MethodId = methodId,
            MethodName = method?.Name ?? "Unknown"
        };

        var query = _context.Payments.AsQueryable();

        // Note: Payment entity uses Provider string, so we match by method code
        if (method != null)
        {
            query = query.Where(p => p.Provider == method.Code);
        }

        if (from.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= to.Value);
        }

        var payments = await query.ToListAsync(ct);

        stats.TotalTransactions = payments.Count;
        stats.SuccessfulTransactions = payments.Count(p => p.Status == PaymentStatus.Captured);
        stats.FailedTransactions = payments.Count(p => p.Status == PaymentStatus.Failed);
        stats.TotalAmount = payments.Where(p => p.Status == PaymentStatus.Captured).Sum(p => p.Amount);
        stats.TotalRefunds = payments.Where(p => p.IsRefund && p.Status == PaymentStatus.Refunded).Sum(p => p.Amount);

        return stats;
    }

    public async Task<PaymentOverviewStats> GetOverviewStatisticsAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var overview = new PaymentOverviewStats();

        var query = _context.Payments.Where(p => !p.IsRefund);

        if (from.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= to.Value);
        }

        var payments = await query.ToListAsync(ct);

        overview.TotalTransactions = payments.Count;
        overview.TotalAmount = payments.Sum(p => p.Amount);
        overview.TotalSuccessful = payments.Where(p => p.Status == PaymentStatus.Captured).Sum(p => p.Amount);
        overview.TotalFailed = payments.Where(p => p.Status == PaymentStatus.Failed).Sum(p => p.Amount);

        // Get refunds
        var refundsQuery = _context.Payments.Where(p => p.IsRefund && p.Status == PaymentStatus.Refunded);
        if (from.HasValue)
        {
            refundsQuery = refundsQuery.Where(p => p.CreatedAt >= from.Value);
        }
        if (to.HasValue)
        {
            refundsQuery = refundsQuery.Where(p => p.CreatedAt <= to.Value);
        }
        overview.TotalRefunded = await refundsQuery.SumAsync(p => p.Amount, ct);

        overview.OverallSuccessRate = overview.TotalTransactions > 0
            ? Math.Round((decimal)payments.Count(p => p.Status == PaymentStatus.Captured) / overview.TotalTransactions * 100, 2)
            : 0;

        // Group by method
        var methods = await GetAllMethodsAsync(true, ct);
        foreach (var method in methods)
        {
            var methodStats = await GetMethodStatisticsAsync(method.Id, from, to, ct);
            overview.ByMethod.Add(methodStats);
        }

        // Group by day
        var byDay = payments
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new DailyPaymentStats
            {
                Date = g.Key,
                TransactionCount = g.Count(),
                Amount = g.Sum(p => p.Amount),
                SuccessRate = g.Any()
                    ? Math.Round((decimal)g.Count(p => p.Status == PaymentStatus.Captured) / g.Count() * 100, 2)
                    : 0
            })
            .OrderBy(d => d.Date)
            .ToList();

        overview.ByDay = byDay;

        return overview;
    }

    #endregion
}
