using BuildingBlocks.Domain.ValueObjects;

namespace Order.Application.IntegrationEvents;

public record OrderItemIntegrationEventDto(string ProductSku, int Quantity, Money UnitPrice);

public record OrderProcessingStartedIntegrationEvent(
	Guid OrderId,
	Guid CustomerId,
	Guid IdempotencyKey,
	List<OrderItemIntegrationEventDto> Items,
	Money TotalAmount,
	Address BillingAddress,
	Address ShippingAddress);
