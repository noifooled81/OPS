using BuildingBlocks.CQRS.Interfaces;
using BuildingBlocks.Domain.Interfaces;

namespace BuildingBlocks.CQRS.Handlers;

public abstract class QueryHandlerBase<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    protected readonly IReadOnlyRepository _readOnlyRepository;

    protected QueryHandlerBase(IReadOnlyRepository readOnlyRepository)
    {
        _readOnlyRepository = readOnlyRepository;
    }

    // The derived class MUST implement this specific method with its business logic
    public abstract Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}