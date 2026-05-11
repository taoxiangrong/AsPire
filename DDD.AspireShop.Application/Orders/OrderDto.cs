namespace DDD.AspireShop.Application.Orders;

public sealed record OrderDto(
    Guid Id,
    string CustomerName,
    DateTimeOffset CreatedAt,
    string Status,
    decimal TotalAmount,
    IReadOnlyCollection<OrderLineDto> Lines);

public sealed record OrderLineDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
