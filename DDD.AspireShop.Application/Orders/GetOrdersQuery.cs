using DDD.AspireShop.Application.Abstractions;
using MediatR;

namespace DDD.AspireShop.Application.Orders;

public sealed record GetOrdersQuery : IRequest<IReadOnlyCollection<OrderDto>>;

internal sealed class GetOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetOrdersQuery, IReadOnlyCollection<OrderDto>>
{
    public async Task<IReadOnlyCollection<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var items = await orders.ListAsync(cancellationToken);
        return items
            .OrderByDescending(order => order.CreatedAt)
            .Select(OrderMapping.ToDto)
            .ToArray();
    }
}
