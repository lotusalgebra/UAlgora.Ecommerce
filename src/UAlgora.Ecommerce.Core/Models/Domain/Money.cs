namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a monetary value with currency.
/// </summary>
public record Money
{
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; } = "USD";

    public Money() { }

    public Money(decimal amount, string currencyCode = "USD")
    {
        Amount = amount;
        CurrencyCode = currencyCode;
    }

    public static Money Zero(string currencyCode = "USD") => new(0, currencyCode);

    public static Money operator +(Money a, Money b)
    {
        if (a.CurrencyCode != b.CurrencyCode)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(a.Amount + b.Amount, a.CurrencyCode);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.CurrencyCode != b.CurrencyCode)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        return new Money(a.Amount - b.Amount, a.CurrencyCode);
    }

    public static Money operator *(Money a, decimal multiplier) =>
        new(a.Amount * multiplier, a.CurrencyCode);

    public static Money operator *(Money a, int multiplier) =>
        new(a.Amount * multiplier, a.CurrencyCode);

    public override string ToString() => $"{Amount:F2} {CurrencyCode}";
}
