using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for payment method and gateway configuration operations.
/// </summary>
public interface IPaymentMethodService
{
    #region Payment Methods

    /// <summary>
    /// Gets a payment method by ID.
    /// </summary>
    Task<PaymentMethodConfig?> GetMethodByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a payment method by code.
    /// </summary>
    Task<PaymentMethodConfig?> GetMethodByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all payment methods.
    /// </summary>
    Task<IReadOnlyList<PaymentMethodConfig>> GetAllMethodsAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets active payment methods.
    /// </summary>
    Task<IReadOnlyList<PaymentMethodConfig>> GetActiveMethodsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the default payment method.
    /// </summary>
    Task<PaymentMethodConfig?> GetDefaultMethodAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets available payment methods for a context.
    /// </summary>
    Task<IReadOnlyList<PaymentMethodConfig>> GetAvailableMethodsAsync(PaymentMethodCheckContext context, CancellationToken ct = default);

    /// <summary>
    /// Creates a new payment method.
    /// </summary>
    Task<PaymentMethodConfig> CreateMethodAsync(PaymentMethodConfig method, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing payment method.
    /// </summary>
    Task<PaymentMethodConfig> UpdateMethodAsync(PaymentMethodConfig method, CancellationToken ct = default);

    /// <summary>
    /// Deletes a payment method.
    /// </summary>
    Task DeleteMethodAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Sets a method as the default.
    /// </summary>
    Task SetDefaultMethodAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates method sort orders.
    /// </summary>
    Task UpdateMethodSortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);

    /// <summary>
    /// Toggles method active status.
    /// </summary>
    Task<PaymentMethodConfig> ToggleMethodStatusAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Payment Gateways

    /// <summary>
    /// Gets a payment gateway by ID.
    /// </summary>
    Task<PaymentGateway?> GetGatewayByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a payment gateway by code.
    /// </summary>
    Task<PaymentGateway?> GetGatewayByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all payment gateways.
    /// </summary>
    Task<IReadOnlyList<PaymentGateway>> GetAllGatewaysAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Gets active payment gateways.
    /// </summary>
    Task<IReadOnlyList<PaymentGateway>> GetActiveGatewaysAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets gateways by provider type.
    /// </summary>
    Task<IReadOnlyList<PaymentGateway>> GetGatewaysByProviderAsync(PaymentProviderType providerType, CancellationToken ct = default);

    /// <summary>
    /// Creates a new payment gateway.
    /// </summary>
    Task<PaymentGateway> CreateGatewayAsync(PaymentGateway gateway, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing payment gateway.
    /// </summary>
    Task<PaymentGateway> UpdateGatewayAsync(PaymentGateway gateway, CancellationToken ct = default);

    /// <summary>
    /// Deletes a payment gateway.
    /// </summary>
    Task DeleteGatewayAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates gateway sort orders.
    /// </summary>
    Task UpdateGatewaySortOrdersAsync(IEnumerable<(Guid Id, int SortOrder)> sortOrders, CancellationToken ct = default);

    /// <summary>
    /// Toggles gateway active status.
    /// </summary>
    Task<PaymentGateway> ToggleGatewayStatusAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Tests gateway connection.
    /// </summary>
    Task<GatewayTestResult> TestGatewayConnectionAsync(Guid gatewayId, CancellationToken ct = default);

    /// <summary>
    /// Toggles gateway sandbox mode.
    /// </summary>
    Task<PaymentGateway> ToggleGatewaySandboxModeAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Fee Calculation

    /// <summary>
    /// Calculates processing fee for a payment method.
    /// </summary>
    Task<PaymentFeeResult> CalculateFeeAsync(Guid methodId, decimal orderAmount, CancellationToken ct = default);

    /// <summary>
    /// Gets fee summary for all available methods.
    /// </summary>
    Task<IReadOnlyList<PaymentFeeResult>> GetFeeSummaryAsync(PaymentMethodCheckContext context, CancellationToken ct = default);

    #endregion

    #region Validation

    /// <summary>
    /// Validates a payment method.
    /// </summary>
    Task<ValidationResult> ValidateMethodAsync(PaymentMethodConfig method, CancellationToken ct = default);

    /// <summary>
    /// Validates a payment gateway.
    /// </summary>
    Task<ValidationResult> ValidateGatewayAsync(PaymentGateway gateway, CancellationToken ct = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets payment method usage statistics.
    /// </summary>
    Task<PaymentMethodStats> GetMethodStatisticsAsync(Guid methodId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);

    /// <summary>
    /// Gets overall payment statistics.
    /// </summary>
    Task<PaymentOverviewStats> GetOverviewStatisticsAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Result of processing fee calculation.
/// </summary>
public class PaymentFeeResult
{
    /// <summary>
    /// Payment method ID.
    /// </summary>
    public Guid MethodId { get; set; }

    /// <summary>
    /// Payment method name.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Order amount.
    /// </summary>
    public decimal OrderAmount { get; set; }

    /// <summary>
    /// Processing fee.
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// Total amount including fee.
    /// </summary>
    public decimal TotalAmount => OrderAmount + Fee;

    /// <summary>
    /// Fee description for display.
    /// </summary>
    public string? FeeDescription { get; set; }
}

/// <summary>
/// Result of gateway connection test.
/// </summary>
public class GatewayTestResult
{
    /// <summary>
    /// Whether test was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Provider account info if available.
    /// </summary>
    public string? AccountInfo { get; set; }

    /// <summary>
    /// Error details if test failed.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Timestamp of test.
    /// </summary>
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Payment method usage statistics.
/// </summary>
public class PaymentMethodStats
{
    /// <summary>
    /// Payment method ID.
    /// </summary>
    public Guid MethodId { get; set; }

    /// <summary>
    /// Payment method name.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of transactions.
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Successful transactions.
    /// </summary>
    public int SuccessfulTransactions { get; set; }

    /// <summary>
    /// Failed transactions.
    /// </summary>
    public int FailedTransactions { get; set; }

    /// <summary>
    /// Success rate percentage.
    /// </summary>
    public decimal SuccessRate => TotalTransactions > 0
        ? Math.Round((decimal)SuccessfulTransactions / TotalTransactions * 100, 2)
        : 0;

    /// <summary>
    /// Total amount processed.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Total fees collected.
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Total refunds.
    /// </summary>
    public decimal TotalRefunds { get; set; }

    /// <summary>
    /// Average transaction amount.
    /// </summary>
    public decimal AverageTransactionAmount => TotalTransactions > 0
        ? Math.Round(TotalAmount / TotalTransactions, 2)
        : 0;

    /// <summary>
    /// Refund rate percentage.
    /// </summary>
    public decimal RefundRate => TotalAmount > 0
        ? Math.Round(TotalRefunds / TotalAmount * 100, 2)
        : 0;
}

/// <summary>
/// Overall payment statistics.
/// </summary>
public class PaymentOverviewStats
{
    /// <summary>
    /// Total transactions across all methods.
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Total amount processed.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Total successful amount.
    /// </summary>
    public decimal TotalSuccessful { get; set; }

    /// <summary>
    /// Total failed amount.
    /// </summary>
    public decimal TotalFailed { get; set; }

    /// <summary>
    /// Total refunded amount.
    /// </summary>
    public decimal TotalRefunded { get; set; }

    /// <summary>
    /// Overall success rate.
    /// </summary>
    public decimal OverallSuccessRate { get; set; }

    /// <summary>
    /// Breakdown by payment method.
    /// </summary>
    public List<PaymentMethodStats> ByMethod { get; set; } = [];

    /// <summary>
    /// Breakdown by day.
    /// </summary>
    public List<DailyPaymentStats> ByDay { get; set; } = [];
}

/// <summary>
/// Daily payment statistics.
/// </summary>
public class DailyPaymentStats
{
    /// <summary>
    /// Date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Number of transactions.
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Total amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Success rate for the day.
    /// </summary>
    public decimal SuccessRate { get; set; }
}
