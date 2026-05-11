using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Domain.Products;

namespace DDD.AspireShop.Application.Catalog;

public sealed class ProductAppService(IProductRepository products) : IProductAppService
{
    public async Task<IReadOnlyCollection<ProductDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await products.ListAsync(cancellationToken);
        return items
            .OrderBy(product => product.Name)
            .Select(ToDto)
            .ToArray();
    }

    public static ProductDto ToDto(Product product) =>
        new(product.Id, product.Name, product.Description, product.Price.Amount, product.Price.Currency, product.Stock, product.IsActive);
}
