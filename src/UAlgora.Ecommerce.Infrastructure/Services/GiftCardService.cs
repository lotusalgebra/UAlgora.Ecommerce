using System.Security.Cryptography;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for Gift Card operations.
/// </summary>
public class GiftCardService : IGiftCardService
{
    private readonly IGiftCardRepository _giftCardRepository;

    public GiftCardService(IGiftCardRepository giftCardRepository)
    {
        _giftCardRepository = giftCardRepository;
    }

    public async Task<GiftCard?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _giftCardRepository.GetByIdAsync(id, ct);
    }

    public async Task<GiftCard?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _giftCardRepository.GetByCodeAsync(code, ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        return await _giftCardRepository.GetByStoreAsync(storeId, ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _giftCardRepository.GetByCustomerAsync(customerId, ct);
    }

    public async Task<GiftCard> CreateAsync(GiftCard giftCard, CancellationToken ct = default)
    {
        // Generate code if not provided
        if (string.IsNullOrEmpty(giftCard.Code))
        {
            giftCard.Code = await GenerateCodeAsync(ct: ct);
        }

        // Validate unique code
        if (await _giftCardRepository.CodeExistsAsync(giftCard.Code, ct: ct))
        {
            throw new InvalidOperationException($"Gift card with code '{giftCard.Code}' already exists.");
        }

        // Set defaults
        giftCard.IssuedAt = DateTime.UtcNow;
        giftCard.Balance = giftCard.InitialValue;
        giftCard.Status = GiftCardStatus.Active;

        return await _giftCardRepository.AddAsync(giftCard, ct);
    }

    public async Task<string> GenerateCodeAsync(string? prefix = null, CancellationToken ct = default)
    {
        string code;
        do
        {
            code = GenerateUniqueCode(prefix);
        } while (await _giftCardRepository.CodeExistsAsync(code, ct: ct));

        return code;
    }

    private static string GenerateUniqueCode(string? prefix = null)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Excluded confusing chars
        var code = new char[16];
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[16];
        rng.GetBytes(bytes);

        for (int i = 0; i < 16; i++)
        {
            code[i] = chars[bytes[i] % chars.Length];
        }

        // Format as XXXX-XXXX-XXXX-XXXX
        var formatted = $"{new string(code, 0, 4)}-{new string(code, 4, 4)}-{new string(code, 8, 4)}-{new string(code, 12, 4)}";
        return string.IsNullOrEmpty(prefix) ? formatted : $"{prefix}-{formatted}";
    }

    public async Task<GiftCard> UpdateAsync(GiftCard giftCard, CancellationToken ct = default)
    {
        return await _giftCardRepository.UpdateAsync(giftCard, ct);
    }

    public async Task<GiftCardValidationResult> ValidateAsync(string code, decimal orderAmount, CancellationToken ct = default)
    {
        var giftCard = await _giftCardRepository.GetByCodeAsync(code, ct);

        if (giftCard == null)
        {
            return GiftCardValidationResult.Failure("NOT_FOUND", "Gift card not found.");
        }

        if (!giftCard.IsValid)
        {
            if (giftCard.IsExpired)
            {
                return GiftCardValidationResult.Failure("EXPIRED", "Gift card has expired.");
            }
            if (giftCard.Status != GiftCardStatus.Active)
            {
                return GiftCardValidationResult.Failure("INACTIVE", $"Gift card is {giftCard.Status.ToString().ToLower()}.");
            }
            if (giftCard.Balance <= 0)
            {
                return GiftCardValidationResult.Failure("NO_BALANCE", "Gift card has no remaining balance.");
            }
            if (giftCard.ValidFrom.HasValue && giftCard.ValidFrom > DateTime.UtcNow)
            {
                return GiftCardValidationResult.Failure("NOT_YET_VALID", "Gift card is not yet valid.");
            }
        }

        // Check minimum order amount
        if (giftCard.MinimumOrderAmount.HasValue && orderAmount < giftCard.MinimumOrderAmount.Value)
        {
            return GiftCardValidationResult.Failure("MIN_ORDER_NOT_MET",
                $"Minimum order amount of {giftCard.MinimumOrderAmount:C} required.");
        }

        return GiftCardValidationResult.Success(giftCard);
    }

    public async Task<GiftCardRedemptionResult> RedeemAsync(string code, decimal amount, Guid orderId, Guid? customerId, CancellationToken ct = default)
    {
        var giftCard = await _giftCardRepository.GetByCodeAsync(code, ct);
        if (giftCard == null || !giftCard.IsValid)
        {
            return new GiftCardRedemptionResult
            {
                Success = false,
                ErrorMessage = "Gift card is not valid for redemption."
            };
        }

        // Respect max redemption limit
        var actualAmount = amount;
        if (giftCard.MaxRedemptionPerOrder.HasValue && amount > giftCard.MaxRedemptionPerOrder.Value)
        {
            actualAmount = giftCard.MaxRedemptionPerOrder.Value;
        }

        // Don't redeem more than balance
        if (actualAmount > giftCard.Balance)
        {
            actualAmount = giftCard.Balance;
        }

        var success = await _giftCardRepository.DeductBalanceAsync(giftCard.Id, actualAmount, orderId, null, ct);

        if (!success)
        {
            return new GiftCardRedemptionResult
            {
                Success = false,
                ErrorMessage = "Failed to redeem gift card."
            };
        }

        // Reload to get updated balance
        giftCard = await _giftCardRepository.GetByIdAsync(giftCard.Id, ct);

        return new GiftCardRedemptionResult
        {
            Success = true,
            AmountRedeemed = actualAmount,
            RemainingBalance = giftCard?.Balance ?? 0
        };
    }

    public async Task<bool> RefundAsync(Guid giftCardId, decimal amount, Guid orderId, string? performedBy, CancellationToken ct = default)
    {
        return await _giftCardRepository.AddBalanceAsync(giftCardId, amount, performedBy, $"Refund from order {orderId}", ct);
    }

    public async Task<bool> AdjustBalanceAsync(Guid giftCardId, decimal amount, string performedBy, string? notes, CancellationToken ct = default)
    {
        return await _giftCardRepository.AddBalanceAsync(giftCardId, amount, performedBy, notes, ct);
    }

    public async Task<IReadOnlyList<GiftCardTransaction>> GetTransactionsAsync(Guid giftCardId, CancellationToken ct = default)
    {
        return await _giftCardRepository.GetTransactionsAsync(giftCardId, ct);
    }

    public async Task<int> DeactivateExpiredAsync(CancellationToken ct = default)
    {
        var expiredCards = await _giftCardRepository.GetExpiredActiveAsync(ct);
        int count = 0;

        foreach (var card in expiredCards)
        {
            card.Status = GiftCardStatus.Expired;
            card.IsActive = false;
            await _giftCardRepository.UpdateAsync(card, ct);
            count++;
        }

        return count;
    }

    public async Task<IReadOnlyList<GiftCard>> GetExpiringSoonAsync(int days = 30, CancellationToken ct = default)
    {
        return await _giftCardRepository.GetExpiringSoonAsync(days, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _giftCardRepository.SoftDeleteAsync(id, ct);
    }
}
