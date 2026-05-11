using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Domain.Orders;

namespace DDD.AspireShop.Infrastructure;

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Lock _lock = new();
    private readonly List<Order> _orders = [];

    public Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyCollection<Order>>(_orders.ToArray());
        }
    }

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }
    }
}
