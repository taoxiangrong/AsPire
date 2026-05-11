using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Domain.Common;
using DDD.AspireShop.Domain.Orders;
using DDD.AspireShop.Domain.Products;
using MediatR;

namespace DDD.AspireShop.Application.Orders;

public sealed record SubmitOrderCommand(string CustomerName, IReadOnlyCollection<CreateOrderLineRequest> Lines) : IRequest<OrderDto>
{
    public static SubmitOrderCommand FromRequest(CreateOrderRequest request) =>
        new(request.CustomerName, request.Lines);
}

internal sealed class SubmitOrderCommandHandler(
    IProductRepository products,
    IOrderRepository orders,
    IDomainEventDispatcher domainEvents)
    : IRequestHandler<SubmitOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var requestedLines = request.Lines
            .Where(line => line.Quantity > 0)
            .GroupBy(line => line.ProductId)
            .Select(group => new CreateOrderLineRequest(group.Key, group.Sum(line => line.Quantity)))
            .ToArray();

        if (requestedLines.Length == 0)
        {
            throw new DomainException("订单至少需要一个有效商品。");
        }

        var orderItems = new List<(Product Product, int Quantity)>();
        foreach (var line in requestedLines)
        {
            var product = await products.GetAsync(line.ProductId, cancellationToken)
                ?? throw new DomainException("商品不存在。");
            orderItems.Add((product, line.Quantity));
        }

        var order = Order.Submit(request.CustomerName, orderItems);
        await orders.SaveAsync(order, cancellationToken);

        foreach (var (product, _) in orderItems)
        {
            await products.SaveAsync(product, cancellationToken);
        }

        var events = order.DomainEvents.ToArray();
        order.ClearDomainEvents();
        await domainEvents.DispatchAsync(events, cancellationToken);

        return OrderMapping.ToDto(order);
    }
}
