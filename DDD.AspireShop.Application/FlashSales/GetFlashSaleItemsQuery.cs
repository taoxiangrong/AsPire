using DDD.AspireShop.Application.Abstractions;
using MediatR;

namespace DDD.AspireShop.Application.FlashSales;

public sealed record GetFlashSaleItemsQuery : IRequest<IReadOnlyCollection<FlashSaleItemDto>>;

internal sealed class GetFlashSaleItemsQueryHandler(IProductRepository products)
    : IRequestHandler<GetFlashSaleItemsQuery, IReadOnlyCollection<FlashSaleItemDto>>
{
    public async Task<IReadOnlyCollection<FlashSaleItemDto>> Handle(GetFlashSaleItemsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startsAt = now.AddMinutes(-15);
        var endsAt = now.AddHours(2);
        var items = await products.ListAsync(cancellationToken);

        return items
            .Where(product => product.IsActive)
            .OrderBy(product => product.Price.Amount)
            .Take(4)
            .Select(product => new FlashSaleItemDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price.Amount,
                Math.Round(product.Price.Amount * 0.72m, 2),
                product.Price.Currency,
                product.Stock,
                2,
                startsAt,
                endsAt,
                product.Stock > 0 && now >= startsAt && now <= endsAt))
            .ToArray();
    }
}
