namespace BuildingBlocks.Domain.Interfaces;

public interface IReadOnlyRepository
{
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
}