using MediatR;

namespace BuildingBlocks.CQRS.Interfaces;

// For requests that do not return a result
public interface ICqrsHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICqrsRequest
{
}

// For requests that return a result
public interface ICqrsHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICqrsRequest<TResponse>
{
}