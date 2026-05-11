namespace DDD.AspireShop.ApiService.IntegrationEvents;

public sealed record ConsumedOrderMessage(
    Guid OrderId,
    string CustomerName,
    decimal TotalAmount,
    DateTimeOffset SubmittedAt,
    DateTimeOffset ConsumedAt);
