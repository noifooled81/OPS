using MediatR;

namespace BuildingBlocks.CQRS.Interfaces;

// Marker interface for void requests
public interface ICqrsRequest : IRequest { }

// Marker interface for requests that return data
public interface ICqrsRequest<out TResponse> : IRequest<TResponse> { }