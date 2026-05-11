using DDD.AspireShop.Domain.Common;

namespace DDD.AspireShop.Domain.Orders;

public sealed record OrderSubmittedDomainEvent(
    Guid OrderId,
    string CustomerName,
    decimal TotalAmount,
    DateTimeOffset SubmittedAt) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
