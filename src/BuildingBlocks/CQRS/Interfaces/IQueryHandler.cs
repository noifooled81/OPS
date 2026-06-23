namespace BuildingBlocks.CQRS.Interfaces;

public interface IQueryHandler<in TQuery, TResponse> : ICqrsHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
{
}