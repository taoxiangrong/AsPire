namespace DDD.AspireShop.Application.Orders;

public sealed record OrderSubmittedIntegrationEvent(
    Guid OrderId,
    string CustomerName,
    decimal TotalAmount,
    DateTimeOffset SubmittedAt);
