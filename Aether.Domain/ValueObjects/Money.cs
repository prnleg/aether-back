namespace Aether.Domain.ValueObjects;

public record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot add {left.Currency} and {right.Currency}.");
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot subtract {left.Currency} and {right.Currency}.");
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);
}
