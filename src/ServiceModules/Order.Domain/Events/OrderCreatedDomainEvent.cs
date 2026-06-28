using BuildingBlocks.Domain.Interfaces;

namespace Order.Domain.Events;

public record OrderCreatedDomainEvent(
	Guid OrderId,
	Guid CustomerId,
	Guid IdempotencyKey) : IDomainEvent
{
	public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
