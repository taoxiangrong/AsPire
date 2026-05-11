using DDD.AspireShop.Domain.Products;

namespace DDD.AspireShop.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveAsync(Product product, CancellationToken cancellationToken = default);
}
