using DDD.AspireShop.Domain.Common;

namespace DDD.AspireShop.Domain.Products;

public sealed record Money
{
    public Money(decimal amount, string currency = "CNY")
    {
        if (amount < 0)
        {
            throw new DomainException("金额不能小于 0。");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("货币不能为空。");
        }

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Zero(string currency = "CNY") => new(0, currency);
}
