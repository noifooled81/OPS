namespace BuildingBlocks.CQRS.Interfaces;

// Marker interface for void commands
public interface ICommand : ICqrsRequest { }

// Marker interface for commands that return data
public interface ICommand<out TResponse> : ICqrsRequest<TResponse> { }