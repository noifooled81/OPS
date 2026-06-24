namespace BuildingBlocks.CQRS.Interfaces;

// For commands that do not return a result
public interface ICommandHandler<in TCommand>
	: ICqrsHandler<TCommand>
	where TCommand : ICommand
{ }

// For commands that return a result
public interface ICommandHandler<in TCommand, TResponse>
	: ICqrsHandler<TCommand, TResponse>
	where TCommand : ICommand<TResponse>
{ }
