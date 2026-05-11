namespace DDD.AspireShop.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
