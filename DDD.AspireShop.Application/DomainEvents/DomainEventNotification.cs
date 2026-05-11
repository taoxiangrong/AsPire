using DDD.AspireShop.Domain.Common;
using MediatR;

namespace DDD.AspireShop.Application.DomainEvents;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
