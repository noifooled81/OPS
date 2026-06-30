namespace Order.Domain.Repositories;

public interface IOrderReadRepository
{
	Task<Entities.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
