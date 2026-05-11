using DDD.AspireShop.Domain.Orders;

namespace DDD.AspireShop.Application.Abstractions;

public interface IOrderRepository
{
    Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}
