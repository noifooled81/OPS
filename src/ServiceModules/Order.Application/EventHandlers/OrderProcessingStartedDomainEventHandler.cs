using BuildingBlocks.Domain.Interfaces;
using BuildingBlocks.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Order.Application.IntegrationEvents;
using Order.Domain.Events;

namespace Order.Application.EventHandlers;

public sealed class OrderProcessingStartedDomainEventHandler(IOutboxService outboxService)
	: IDomainEventHandler<OrderProcessingStartedDomainEvent>
{
	private readonly IOutboxService _outboxService = outboxService
		?? throw new ArgumentNullException(nameof(outboxService));

	public Task Handle(OrderProcessingStartedDomainEvent notification, CancellationToken cancellationToken)
	{
		var integrationEvent = new OrderProcessingStartedIntegrationEvent(
			notification.OrderId,
			notification.CustomerId,
			notification.IdempotencyKey,
			notification.Items.Select(item =>
				new OrderItemIntegrationEventDto(
					item.ProductSku,
					item.Quantity,
					item.UnitPrice
				)
			).ToList(),
			notification.TotalAmount,
			notification.BillingAddress,
			notification.ShippingAddress
		);

		_outboxService.QueueMessage(integrationEvent);
		return Task.CompletedTask;
	}

}
