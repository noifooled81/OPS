using MediatR;

namespace BuildingBlocks.Domain.Interfaces;

public interface IDomainEvent : INotification
{
	DateTimeOffset OccurredOn { get; }
}
