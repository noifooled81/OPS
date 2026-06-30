using BuildingBlocks.Domain.ValueObjects;

namespace Order.Domain.Events;

public record OrderProcessingStartedDomainEvent(
	Guid OrderId,
	Guid CustomerId,
	Guid IdempotencyKey,
	List<OrderEventItem> Items,
	Money TotalAmount,
	Address BillingAddress,
	Address ShippingAddress)
	: OrderDomainEvent(OrderId, CustomerId, IdempotencyKey);
