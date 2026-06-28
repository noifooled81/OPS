using BuildingBlocks.CQRS.Interfaces;
using Order.Application.Common.DTOs;

namespace Order.Application.Commands.CreateOrder;

public record CreateOrderCommand(
	Guid CustomerId,
	string Currency,
	List<OrderItemDto> Items,
	Guid IdempotencyKey,
	string Street,
	string City,
	string ZipCode,
	string Country)
	: ICommand<Guid>;
