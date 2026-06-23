namespace BuildingBlocks.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Commits the changes to the database
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Methods for explicit transaction management
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}