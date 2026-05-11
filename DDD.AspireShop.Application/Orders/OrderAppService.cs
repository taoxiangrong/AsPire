using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Domain.Common;
using DDD.AspireShop.Domain.Orders;
using DDD.AspireShop.Domain.Products;

namespace DDD.AspireShop.Application.Orders;

public sealed class OrderAppService(IProductRepository products, IOrderRepository orders) : IOrderAppService
{
    public async Task<IReadOnlyCollection<OrderDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await orders.ListAsync(cancellationToken);
        return items
            .OrderByDescending(order => order.CreatedAt)
            .Select(OrderMapping.ToDto)
            .ToArray();
    }

    public async Task<OrderDto> SubmitAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
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

        return OrderMapping.ToDto(order);
    }
}
