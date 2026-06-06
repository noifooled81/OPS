namespace BuildingBlocks.Domain.Interfaces;

public interface ISoftDelete
{
	DateTimeOffset? DeletedAt { get; }

	protected void Delete();
}
