namespace DDD.AspireShop.Application.Orders;

public sealed record CreateOrderRequest(string CustomerName, IReadOnlyCollection<CreateOrderLineRequest> Lines);

public sealed record CreateOrderLineRequest(Guid ProductId, int Quantity);
