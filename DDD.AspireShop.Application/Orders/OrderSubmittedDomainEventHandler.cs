using DDD.AspireShop.Application.Abstractions;
using DDD.AspireShop.Application.DomainEvents;
using DDD.AspireShop.Domain.Orders;
using MediatR;

namespace DDD.AspireShop.Application.Orders;

internal sealed class OrderSubmittedDomainEventHandler(IIntegrationEventPublisher integrationEvents)
    : INotificationHandler<DomainEventNotification<OrderSubmittedDomainEvent>>
{
    public Task Handle(DomainEventNotification<OrderSubmittedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var integrationEvent = new OrderSubmittedIntegrationEvent(
            domainEvent.OrderId,
            domainEvent.CustomerName,
            domainEvent.TotalAmount,
            domainEvent.SubmittedAt);

        return integrationEvents.PublishAsync("shop.orders.submitted", integrationEvent, cancellationToken);
    }
}
