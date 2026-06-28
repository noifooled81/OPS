using BuildingBlocks.CQRS.Handlers;
using Order.Application.Common.DTOs;
using Order.Domain.Repositories;

namespace Order.Application.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(
	IOrderReadRepository orderReadRepository)
	: QueryHandlerBase<GetOrderByIdQuery, OrderDto?>
{
	private readonly IOrderReadRepository _orderReadRepository = orderReadRepository
		?? throw new ArgumentNullException(nameof(orderReadRepository));

	public override async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
	{
		var order = await _orderReadRepository.GetByIdAsync(query.OrderId, cancellationToken);

		if (order is null) return null;

		var items = order.Items?.ToList() ?? [];
		if (items.Count == 0)
			throw new InvalidOperationException($"Order {order.Id} has no items.");

		return new OrderDto(
			order.Id,
			order.CustomerId,
			order.Status.Name,
			[.. items.Select(oi => new OrderItemDto(
				oi.ProductSku,
				oi.Quantity,
				oi.UnitPrice.AmountMajor
			))],
			order.TotalAmount.AmountMajor,
			order.TotalAmount.Currency.Name,
			order.ShippingAddress.Street,
			order.ShippingAddress.City,
			order.ShippingAddress.ZipCode,
			order.ShippingAddress.Country
		);
	}
}
