namespace DDD.AspireShop.Application.Orders;

public interface IOrderAppService
{
    Task<IReadOnlyCollection<OrderDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<OrderDto> SubmitAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
}
