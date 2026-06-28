using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public sealed class OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger)
	: INotificationHandler<OrderCreatedDomainEvent>
{
	private readonly ILogger<OrderCreatedDomainEventHandler> _logger = logger;

	public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
	{
		// This is where would trigger other side-effects local to this microservice context,
		// or prepare/publish the Integration Event to the message broker.

		return Task.CompletedTask;
	}
}
