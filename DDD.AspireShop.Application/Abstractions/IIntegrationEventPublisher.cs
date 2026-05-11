namespace DDD.AspireShop.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TMessage>(string name, TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;
}
