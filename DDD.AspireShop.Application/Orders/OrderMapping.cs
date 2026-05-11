using DDD.AspireShop.Domain.Orders;

namespace DDD.AspireShop.Application.Orders;

public static class OrderMapping
{
    public static OrderDto ToDto(Order order) =>
        new(
            order.Id,
            order.CustomerName,
            order.CreatedAt,
            order.Status.ToString(),
            order.TotalAmount,
            order.Lines.Select(line => new OrderLineDto(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity, line.LineTotal)).ToArray());
}
