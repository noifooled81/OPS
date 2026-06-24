using BuildingBlocks.CQRS.Interfaces;
using BuildingBlocks.Domain.Interfaces;

namespace BuildingBlocks.CQRS.Handlers;

public abstract class CommandHandlerBase<TCommand>(IUnitOfWork unitOfWork)
	: ICommandHandler<TCommand>
	where TCommand : ICommand
{
	protected readonly IUnitOfWork _unitOfWork = unitOfWork;

	// The derived class MUST implement this specific method with its business logic
	public abstract Task Handle(TCommand command, CancellationToken cancellationToken);
}

public abstract class CommandHandlerBase<TCommand, TResponse>(IUnitOfWork unitOfWork)
	: ICommandHandler<TCommand, TResponse>
	where TCommand : ICommand<TResponse>
{
	protected readonly IUnitOfWork _unitOfWork = unitOfWork;

	// The derived class MUST implement this specific method with its business logic
	public abstract Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}
