namespace Order.Domain.Repositories;

public interface IOrderRepository
{
	Task AddAsync(Entities.Order order, CancellationToken cancellationToken);
	Task<Entities.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
