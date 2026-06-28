using BuildingBlocks.CQRS.Handlers;
using BuildingBlocks.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Order.Domain.Repositories;

namespace Order.Application.Commands.ApproveOrder;

public sealed class ApproveOrderCommandHandler(
	IUnitOfWork unitOfWork,
	IOrderRepository orderRepository,
	ILogger<ApproveOrderCommandHandler> logger)
	: CommandHandlerBase<ApproveOrderCommand>(unitOfWork)
{
	private readonly ILogger<ApproveOrderCommandHandler> _logger = logger;

	private readonly IOrderRepository _orderRepository = orderRepository
		?? throw new ArgumentNullException(nameof(orderRepository));

	public override async Task Handle(ApproveOrderCommand command, CancellationToken cancellationToken)
	{
		var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken)
			?? throw new KeyNotFoundException($"Order with ID {command.OrderId} was not found.");

		// Invoke domain logic state transition
		order.OrderApproved();

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Order {OrderId} approved",
			order.Id);
	}
}
