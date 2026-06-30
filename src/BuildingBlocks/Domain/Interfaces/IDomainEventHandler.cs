using MediatR;

namespace BuildingBlocks.Domain.Interfaces;

public interface IDomainEventHandler<in TEvent>
    : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{ }