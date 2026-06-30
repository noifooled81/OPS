using BuildingBlocks.Domain.Interfaces;

public abstract record OrderDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid IdempotencyKey) : IDomainEvent
{
    // Standard metadata fields automatically added to every event
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTime.UtcNow;
}