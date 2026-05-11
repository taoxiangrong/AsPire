using DDD.AspireShop.Application.Abstractions;
using DotNetCore.CAP;

namespace DDD.AspireShop.ApiService.IntegrationEvents;

public sealed class CapIntegrationEventPublisher(ICapPublisher publisher) : IIntegrationEventPublisher
{
    public Task PublishAsync<TMessage>(string name, TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class =>
        publisher.PublishAsync(name, message, cancellationToken: cancellationToken);
}
