using BuildingBlocks.CQRS.Handlers;
using BuildingBlocks.Domain.Interfaces;
using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Domain.Enums;
using Order.Domain.Repositories;
using Order.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
	IUnitOfWork unitOfWork,
	IOrderRepository orderRepository,
	ILogger<CreateOrderCommandHandler> logger)
	: CommandHandlerBase<CreateOrderCommand, Guid>(unitOfWork)
{
	private readonly ILogger<CreateOrderCommandHandler> _logger = logger;

	private readonly IOrderRepository _orderRepository = orderRepository
		?? throw new ArgumentNullException(nameof(orderRepository));

	public override async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
	{
		// Resolve Currency (using SmartEnum lookup)
		var currency = Currency.FromName(command.Currency);

		// Map command items to OrderItem domain entities
		var items = new List<OrderItem>();
		foreach (var itemDto in command.Items)
		{
			// Convert price unit to minor unit if necessary (e.g. USD uses cents)
			int priceInMinorUnit = (int)Math.Round(itemDto.UnitPrice * (currency == Currency.Usd ? 100 : 1), MidpointRounding.AwayFromZero);
			var unitPrice = Money.Create(priceInMinorUnit, currency);

			var orderItem = new OrderItem(itemDto.ProductSku, itemDto.Quantity, unitPrice);
			items.Add(orderItem);
		}

		// Create Address value object
		var address = new Address(command.Street, command.City, command.ZipCode, command.Country);

		// Instantiate the Order aggregate root with its items
		var order = new Domain.Entities.Order(
			command.CustomerId,
			items,
			command.IdempotencyKey,
			address);

		await _orderRepository.AddAsync(order, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Order {OrderId} created for Customer {CustomerId}",
			order.Id,
			order.CustomerId);

		return order.Id;
	}
}
