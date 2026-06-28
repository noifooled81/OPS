namespace Order.Application.IntegrationEvents;

public record OrderItemIntegrationEventDto(Guid ProductId, int Quantity);

public record OrderCreatedIntegrationEvent(
	Guid OrderId,
	Guid CustomerId,
	decimal TotalAmount,
	string Currency,
	List<OrderItemIntegrationEventDto> Items,
	Guid IdempotencyKey);
