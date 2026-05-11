using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Application.Orders;
using DDD.AspireShop.Domain.Common;
using DDD.AspireShop.Domain.Orders;
using DDD.AspireShop.Domain.Products;
using MediatR;

namespace DDD.AspireShop.Application.FlashSales;

public sealed record SubmitFlashSaleOrderCommand(string CustomerName, Guid ProductId, int Quantity) : IRequest<OrderDto>
{
    public static SubmitFlashSaleOrderCommand FromRequest(SubmitFlashSaleOrderRequest request) =>
        new(request.CustomerName, request.ProductId, request.Quantity);
}

internal sealed class SubmitFlashSaleOrderCommandHandler(
    IProductRepository products,
    IOrderRepository orders,
    IDomainEventDispatcher domainEvents)
    : IRequestHandler<SubmitFlashSaleOrderCommand, OrderDto>
{
    private const int LimitPerOrder = 2;

    public async Task<OrderDto> Handle(SubmitFlashSaleOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            throw new DomainException("秒杀数量必须大于 0。");
        }

        if (request.Quantity > LimitPerOrder)
        {
            throw new DomainException($"秒杀商品每单限购 {LimitPerOrder} 件。");
        }

        var product = await products.GetAsync(request.ProductId, cancellationToken)
            ?? throw new DomainException("秒杀商品不存在。");

        var flashProduct = Product.Create(
            product.Name,
            product.Description,
            new Money(Math.Round(product.Price.Amount * 0.72m, 2), product.Price.Currency),
            product.Stock);

        var order = Order.Submit(request.CustomerName, [(flashProduct, request.Quantity)]);
        product.Allocate(request.Quantity);

        await orders.SaveAsync(order, cancellationToken);
        await products.SaveAsync(product, cancellationToken);

        var events = order.DomainEvents.ToArray();
        order.ClearDomainEvents();
        await domainEvents.DispatchAsync(events, cancellationToken);

        return OrderMapping.ToDto(order);
    }
}
