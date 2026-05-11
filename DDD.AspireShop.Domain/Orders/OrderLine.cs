using DDD.AspireShop.Domain.Common;

namespace DDD.AspireShop.Domain.Orders;

public sealed record OrderLine
{
    public OrderLine(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("订单行数量必须大于 0。");
        }

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid ProductId { get; }

    public string ProductName { get; }

    public decimal UnitPrice { get; }

    public int Quantity { get; }

    public decimal LineTotal => UnitPrice * Quantity;
}
