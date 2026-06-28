using MediatR;

namespace BuildingBlocks.Domain.Interfaces;

public interface IDomainEvent : INotification
{
	DateTime OccurredOn { get; }
}
