using BuildingBlocks.CQRS.Handlers;
using BuildingBlocks.Domain.Interfaces;
using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Domain.Enums;
using Order.Domain.Repositories;
using Order.Domain.Entities;
using Order.Application.Clients.Interfaces;
using Microsoft.Extensions.Logging;

namespace Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
	IUnitOfWork unitOfWork,
	IOrderRepository orderRepository,
	IInventoryServiceClient inventoryServiceClient,
	ILogger<CreateOrderCommandHandler> logger) : CommandHandlerBase<CreateOrderCommand, Guid>(unitOfWork)
{
	private readonly IOrderRepository _orderRepository = orderRepository
		?? throw new ArgumentNullException(nameof(orderRepository));
	private readonly IInventoryServiceClient _inventoryServiceClient = inventoryServiceClient
		?? throw new ArgumentNullException(nameof(inventoryServiceClient));
	private readonly ILogger<CreateOrderCommandHandler> _logger = logger
		?? throw new ArgumentNullException(nameof(logger));

	public override async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
	{
		// Resolve Currency (using SmartEnum lookup)
		var currency = Currency.FromName(command.Currency);

		// Fetch product prices from the Inventory Service API synchronously using ProductSku
		var productSkus = command.Items.Select(x => x.ProductSku).Distinct().ToList();
		var prices = await _inventoryServiceClient.GetProductPricesAsync(productSkus, command.Currency, cancellationToken);
		var priceMap = prices.ToDictionary(p => p.ProductSku, p => p.Price);

		// Map command items to OrderItem domain entities using fetched prices
		var items = new List<OrderItem>();
		foreach (var itemDto in command.Items)
		{
			if (!priceMap.TryGetValue(itemDto.ProductSku, out var price))
			{
				throw new KeyNotFoundException($"Price for SKU '{itemDto.ProductSku}' could not be resolved from the Inventory Service.");
			}

			var orderItem = new OrderItem(itemDto.ProductSku, itemDto.Quantity, price);
			items.Add(orderItem);
		}

		// Create Address value objects
		var billingAddress = new Address(command.BillingStreet, command.BillingCity, command.BillingZipCode, command.BillingCountry);
		var shippingAddress = new Address(command.ShippingStreet, command.ShippingCity, command.ShippingZipCode, command.ShippingCountry);

		// Instantiate the Order aggregate root (calculates TotalAmount internally)
		var order = new Domain.Entities.Order(
			command.CustomerId,
			items,
			command.IdempotencyKey,
			billingAddress,
			shippingAddress);

		// Start Processing Order imediately after creating
		order.StartProcessing();

		// Persist the Order in the repository
		await _orderRepository.AddAsync(order, cancellationToken);

		// Commit transaction
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Successfully created order for Customer: {CustomerId}",
			command.CustomerId);

		return order.Id;
	}
}
