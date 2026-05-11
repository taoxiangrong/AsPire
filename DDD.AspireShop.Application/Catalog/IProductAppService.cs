namespace DDD.AspireShop.Application.Catalog;

public interface IProductAppService
{
    Task<IReadOnlyCollection<ProductDto>> ListAsync(CancellationToken cancellationToken = default);
}
