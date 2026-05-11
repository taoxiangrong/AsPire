using DDD.AspireShop.Application.Abstractions;
using MediatR;

namespace DDD.AspireShop.Application.Catalog;

public sealed record GetProductsQuery : IRequest<IReadOnlyCollection<ProductDto>>;

internal sealed class GetProductsQueryHandler(IProductRepository products)
    : IRequestHandler<GetProductsQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var items = await products.ListAsync(cancellationToken);
        return items
            .OrderBy(product => product.Name)
            .Select(ProductAppService.ToDto)
            .ToArray();
    }
}
