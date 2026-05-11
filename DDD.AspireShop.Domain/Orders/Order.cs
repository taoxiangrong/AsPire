using DDD.AspireShop.Domain.Common;
using DDD.AspireShop.Domain.Products;

namespace DDD.AspireShop.Domain.Orders;

public sealed class Order : Entity
{
    private readonly List<OrderLine> _lines = [];

    private Order(Guid id, string customerName)
    {
        Id = id;
        CustomerName = customerName;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = OrderStatus.Submitted;
    }

    public Guid Id { get; }

    public string CustomerName { get; }

    public DateTimeOffset CreatedAt { get; private set; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines;

    public decimal TotalAmount => _lines.Sum(line => line.LineTotal);

    public static Order Rehydrate(Guid id, string customerName, DateTimeOffset createdAt, OrderStatus status, IEnumerable<OrderLine> lines)
    {
        var order = new Order(id, customerName)
        {
            CreatedAt = createdAt,
            Status = status
        };

        order._lines.AddRange(lines);
        return order;
    }

    public static Order Submit(string customerName, IEnumerable<(Product Product, int Quantity)> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new DomainException("客户名称不能为空。");
        }

        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            throw new DomainException("订单至少需要一个商品。");
        }

        var order = new Order(Guid.NewGuid(), customerName.Trim());
        foreach (var (product, quantity) in itemList)
        {
            product.Allocate(quantity);
            order._lines.Add(new OrderLine(product.Id, product.Name, product.Price.Amount, quantity));
        }

        order.AddDomainEvent(new OrderSubmittedDomainEvent(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.CreatedAt));

        return order;
    }
}
