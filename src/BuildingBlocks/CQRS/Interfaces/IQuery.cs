namespace BuildingBlocks.CQRS.Interfaces;

// Marker interface for queries
public interface IQuery<out TResponse> : ICqrsRequest<TResponse> { }