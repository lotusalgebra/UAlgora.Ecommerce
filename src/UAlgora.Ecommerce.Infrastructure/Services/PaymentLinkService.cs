using System.Security.Cryptography;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for Payment Link operations.
/// </summary>
public class PaymentLinkService : IPaymentLinkService
{
    private readonly IPaymentLinkRepository _paymentLinkRepository;

    public PaymentLinkService(IPaymentLinkRepository paymentLinkRepository)
    {
        _paymentLinkRepository = paymentLinkRepository;
    }

    public async Task<PaymentLink?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetByIdAsync(id, ct);
    }

    public async Task<PaymentLink?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetByCodeAsync(code, ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetAllAsync(CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetAllAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetByStoreAsync(storeId, ct);
    }

    public async Task<IReadOnlyList<PaymentLink>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetActiveAsync(ct);
    }

    public async Task<PaymentLink> CreateAsync(PaymentLink paymentLink, CancellationToken ct = default)
    {
        // Generate code if not provided
        if (string.IsNullOrEmpty(paymentLink.Code))
        {
            paymentLink.Code = await GenerateCodeAsync(ct: ct);
        }

        // Validate unique code
        if (await _paymentLinkRepository.CodeExistsAsync(paymentLink.Code, ct: ct))
        {
            throw new InvalidOperationException($"Payment link with code '{paymentLink.Code}' already exists.");
        }

        // Set defaults
        paymentLink.Status = PaymentLinkStatus.Active;
        paymentLink.IsActive = true;
        paymentLink.UsageCount = 0;
        paymentLink.TotalCollected = 0;

        return await _paymentLinkRepository.AddAsync(paymentLink, ct);
    }

    public async Task<PaymentLink> UpdateAsync(PaymentLink paymentLink, CancellationToken ct = default)
    {
        // Validate unique code if changed
        var existing = await _paymentLinkRepository.GetByIdAsync(paymentLink.Id, ct);
        if (existing != null && existing.Code != paymentLink.Code)
        {
            if (await _paymentLinkRepository.CodeExistsAsync(paymentLink.Code, paymentLink.Id, ct))
            {
                throw new InvalidOperationException($"Payment link with code '{paymentLink.Code}' already exists.");
            }
        }

        return await _paymentLinkRepository.UpdateAsync(paymentLink, ct);
    }

    public async Task<string> GenerateCodeAsync(string? prefix = null, CancellationToken ct = default)
    {
        string code;
        do
        {
            code = GenerateUniqueCode(prefix);
        } while (await _paymentLinkRepository.CodeExistsAsync(code, ct: ct));

        return code;
    }

    private static string GenerateUniqueCode(string? prefix = null)
    {
        const string chars = "abcdefghjkmnpqrstuvwxyz23456789"; // Lowercase for URL-friendly codes
        var code = new char[8];
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[8];
        rng.GetBytes(bytes);

        for (int i = 0; i < code.Length; i++)
        {
            code[i] = chars[bytes[i] % chars.Length];
        }

        return string.IsNullOrEmpty(prefix)
            ? new string(code)
            : $"{prefix}-{new string(code)}";
    }

    public async Task<PaymentLinkValidationResult> ValidateAsync(string code, CancellationToken ct = default)
    {
        var paymentLink = await _paymentLinkRepository.GetByCodeAsync(code, ct);

        if (paymentLink == null)
        {
            return PaymentLinkValidationResult.Failure("NOT_FOUND", "Payment link not found.");
        }

        if (!paymentLink.IsActive)
        {
            return PaymentLinkValidationResult.Failure("INACTIVE", "Payment link is inactive.");
        }

        if (paymentLink.Status != PaymentLinkStatus.Active)
        {
            return PaymentLinkValidationResult.Failure("STATUS_INVALID", $"Payment link status is {paymentLink.Status}.");
        }

        if (paymentLink.IsExpired)
        {
            return PaymentLinkValidationResult.Failure("EXPIRED", "Payment link has expired.");
        }

        if (paymentLink.ValidFrom.HasValue && paymentLink.ValidFrom.Value > DateTime.UtcNow)
        {
            return PaymentLinkValidationResult.Failure("NOT_YET_VALID", "Payment link is not yet active.");
        }

        if (paymentLink.MaxUses.HasValue && paymentLink.UsageCount >= paymentLink.MaxUses.Value)
        {
            return PaymentLinkValidationResult.Failure("MAX_USES_REACHED", "Payment link has reached maximum uses.");
        }

        return PaymentLinkValidationResult.Success(paymentLink);
    }

    public async Task<PaymentLinkPaymentResult> RecordPaymentAsync(Guid paymentLinkId, PaymentLinkPayment payment, CancellationToken ct = default)
    {
        var paymentLink = await _paymentLinkRepository.GetByIdAsync(paymentLinkId, ct);
        if (paymentLink == null)
        {
            return PaymentLinkPaymentResult.Failed("Payment link not found.");
        }

        payment.PaymentLinkId = paymentLinkId;
        payment.CurrencyCode = paymentLink.CurrencyCode;

        var savedPayment = await _paymentLinkRepository.AddPaymentAsync(payment, ct);

        // Update usage count if payment is successful
        if (payment.Status == PaymentLinkPaymentStatus.Completed)
        {
            await _paymentLinkRepository.IncrementUsageAsync(paymentLinkId, payment.Amount + payment.TipAmount, ct);
        }

        return PaymentLinkPaymentResult.Succeeded(savedPayment);
    }

    public async Task<PaymentLinkPayment> UpdatePaymentStatusAsync(Guid paymentId, PaymentLinkPaymentStatus status, CancellationToken ct = default)
    {
        var payments = await _paymentLinkRepository.GetPaymentsAsync(paymentId, ct);
        var payment = payments.FirstOrDefault(p => p.Id == paymentId);

        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found.");
        }

        var previousStatus = payment.Status;
        payment.Status = status;

        await _paymentLinkRepository.UpdatePaymentAsync(payment, ct);

        // If payment just became successful, update the payment link
        if (status == PaymentLinkPaymentStatus.Completed && previousStatus != PaymentLinkPaymentStatus.Completed)
        {
            await _paymentLinkRepository.IncrementUsageAsync(payment.PaymentLinkId, payment.Amount + payment.TipAmount, ct);
        }

        return payment;
    }

    public async Task<IReadOnlyList<PaymentLinkPayment>> GetPaymentsAsync(Guid paymentLinkId, CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetPaymentsAsync(paymentLinkId, ct);
    }

    public async Task<PaymentLinkStatistics> GetStatisticsAsync(Guid paymentLinkId, CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetStatisticsAsync(paymentLinkId, ct);
    }

    public async Task<PaymentLink> PauseAsync(Guid id, CancellationToken ct = default)
    {
        var paymentLink = await _paymentLinkRepository.GetByIdAsync(id, ct);
        if (paymentLink == null)
        {
            throw new InvalidOperationException("Payment link not found.");
        }

        paymentLink.Status = PaymentLinkStatus.Paused;
        return await _paymentLinkRepository.UpdateAsync(paymentLink, ct);
    }

    public async Task<PaymentLink> ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var paymentLink = await _paymentLinkRepository.GetByIdAsync(id, ct);
        if (paymentLink == null)
        {
            throw new InvalidOperationException("Payment link not found.");
        }

        paymentLink.Status = PaymentLinkStatus.Active;
        paymentLink.IsActive = true;
        return await _paymentLinkRepository.UpdateAsync(paymentLink, ct);
    }

    public async Task<PaymentLink> ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var paymentLink = await _paymentLinkRepository.GetByIdAsync(id, ct);
        if (paymentLink == null)
        {
            throw new InvalidOperationException("Payment link not found.");
        }

        paymentLink.Status = PaymentLinkStatus.Archived;
        paymentLink.IsActive = false;
        return await _paymentLinkRepository.UpdateAsync(paymentLink, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _paymentLinkRepository.SoftDeleteAsync(id, ct);
    }

    public async Task<int> DeactivateExpiredAsync(CancellationToken ct = default)
    {
        var expiredLinks = await _paymentLinkRepository.GetExpiredActiveAsync(ct);
        var count = 0;

        foreach (var link in expiredLinks)
        {
            link.Status = PaymentLinkStatus.Expired;
            link.IsActive = false;
            await _paymentLinkRepository.UpdateAsync(link, ct);
            count++;
        }

        return count;
    }

    public async Task<IReadOnlyList<PaymentLink>> GetExpiringSoonAsync(int days = 30, CancellationToken ct = default)
    {
        return await _paymentLinkRepository.GetExpiringSoonAsync(days, ct);
    }

    public async Task<PagedResult<PaymentLink>> SearchAsync(
        string? searchTerm = null,
        PaymentLinkStatus? status = null,
        PaymentLinkType? type = null,
        Guid? storeId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        return await _paymentLinkRepository.SearchAsync(searchTerm, status, type, storeId, page, pageSize, ct);
    }

    public string GetFullUrl(PaymentLink paymentLink, string baseUrl)
    {
        return paymentLink.GetFullUrl(baseUrl);
    }

    public async Task<PaymentLink> DuplicateAsync(Guid id, CancellationToken ct = default)
    {
        var source = await _paymentLinkRepository.GetByIdAsync(id, ct);
        if (source == null)
        {
            throw new InvalidOperationException("Payment link not found.");
        }

        var duplicate = new PaymentLink
        {
            StoreId = source.StoreId,
            Code = await GenerateCodeAsync(ct: ct),
            Name = $"{source.Name} (Copy)",
            Description = source.Description,
            Type = source.Type,
            Amount = source.Amount,
            CurrencyCode = source.CurrencyCode,
            MinimumAmount = source.MinimumAmount,
            MaximumAmount = source.MaximumAmount,
            SuggestedAmountsJson = source.SuggestedAmountsJson,
            AllowTip = source.AllowTip,
            TipPercentagesJson = source.TipPercentagesJson,
            ProductId = source.ProductId,
            ProductVariantId = source.ProductVariantId,
            AllowQuantity = source.AllowQuantity,
            MaxQuantity = source.MaxQuantity,
            RequireEmail = source.RequireEmail,
            RequirePhone = source.RequirePhone,
            RequireBillingAddress = source.RequireBillingAddress,
            RequireShippingAddress = source.RequireShippingAddress,
            CustomFieldsJson = source.CustomFieldsJson,
            SuccessMessage = source.SuccessMessage,
            SuccessRedirectUrl = source.SuccessRedirectUrl,
            CancelUrl = source.CancelUrl,
            AllowedPaymentMethodsJson = source.AllowedPaymentMethodsJson,
            BrandColor = source.BrandColor,
            LogoUrl = source.LogoUrl,
            TermsUrl = source.TermsUrl,
            NotificationEmail = source.NotificationEmail,
            SendCustomerReceipt = source.SendCustomerReceipt,
            ReceiptEmailTemplateId = source.ReceiptEmailTemplateId,
            Notes = source.Notes,
            TagsJson = source.TagsJson,
            Status = PaymentLinkStatus.Active,
            IsActive = true
        };

        return await _paymentLinkRepository.AddAsync(duplicate, ct);
    }
}
