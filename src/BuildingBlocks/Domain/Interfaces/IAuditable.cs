namespace BuildingBlocks.Domain.Interfaces;

public interface IAuditable
{
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; }
}