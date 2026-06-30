using BuildingBlocks.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Order.Application.Commands.CreateOrder;
using Order.Domain.Repositories;
using Xunit;
using OrderEntity = Order.Domain.Entities.Order;
using OrderStatus = Order.Domain.Enums.OrderStatus;
using OrderItemCommandDto = Order.Application.Commands.Dtos.OrderItemCommandDto;
using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Domain.Enums;
using Order.Application.Clients.Interfaces;
using Order.Application.Clients.Dtos;
using Microsoft.Extensions.Logging;

namespace Order.Commands.Tests;

public sealed class CreateOrderCommandHandlerTests
{
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<IOrderRepository> _orderRepositoryMock;
	private readonly Mock<IInventoryServiceClient> _inventoryServiceClientMock;
	private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
	private readonly CreateOrderCommandHandler _handler;
	private readonly CancellationToken _ct;

	public CreateOrderCommandHandlerTests()
	{
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_orderRepositoryMock = new Mock<IOrderRepository>();
		_inventoryServiceClientMock = new Mock<IInventoryServiceClient>();
		_loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();

		_handler = new CreateOrderCommandHandler(
			_unitOfWorkMock.Object,
			_orderRepositoryMock.Object,
			_inventoryServiceClientMock.Object,
			_loggerMock.Object);

		_ct = CancellationToken.None;

		// Default mock setup for pricing client
		_inventoryServiceClientMock
			.Setup(c => c.GetProductPricesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((IEnumerable<string> skus, string cur, CancellationToken ct) =>
			{
				var currency = Currency.FromName(cur);
				return skus.Select(s => new ProductPriceDto(s, Money.Create(1000, currency))).ToList();
			});
	}

	[Fact]
	public async Task Handle_WithValidVndOrder_ShouldCreateOrderSuccessfully()
	{
		var command = CreateCommand(currency: "VND");

		OrderEntity? capturedOrder = null;
		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Callback<OrderEntity, CancellationToken>((o, _) => capturedOrder = o)
			.Returns(Task.CompletedTask);

		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ReturnsAsync(1);

		var result = await _handler.Handle(command, _ct);

		result.Should().NotBeEmpty();
		result.Should().Be(capturedOrder!.Id);

		capturedOrder.CustomerId.Should().Be(command.CustomerId);
		capturedOrder.IdempotencyKey.Should().Be(command.IdempotencyKey);
		capturedOrder.Status.Should().Be(OrderStatus.Processing); // Since running StartProcessing() after order creation
		capturedOrder.Version.Should().Be(2); // Since running StartProcessing() after order creation

		_orderRepositoryMock.Verify(r => r.AddAsync(capturedOrder, _ct), Times.Once);
		_unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
	}

	[Fact]
	public async Task Handle_WithUsdCurrency_ShouldConvertUnitPricesToCents()
	{
		var command = CreateCommand(
			currency: "USD",
			items:
			[
				new OrderItemCommandDto("SKU-001", 2),
				new OrderItemCommandDto("SKU-002", 1),
			]);

		_inventoryServiceClientMock
			.Setup(c => c.GetProductPricesAsync(It.IsAny<IEnumerable<string>>(), "USD", _ct))
			.ReturnsAsync([
				new ProductPriceDto("SKU-001", Money.Create(1999, Currency.Usd)),
				new ProductPriceDto("SKU-002", Money.Create(550, Currency.Usd))
			]);

		OrderEntity? capturedOrder = null;
		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Callback<OrderEntity, CancellationToken>((o, _) => capturedOrder = o)
			.Returns(Task.CompletedTask);
		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ReturnsAsync(1);

		await _handler.Handle(command, _ct);

		capturedOrder!.Items.Should().HaveCount(2);
		capturedOrder.Items.ElementAt(0).UnitPrice.Amount.Should().Be(1999);
		capturedOrder.Items.ElementAt(1).UnitPrice.Amount.Should().Be(550);
		capturedOrder.TotalAmount.Amount.Should().Be(4548);
	}

	[Fact]
	public async Task Handle_WithVndCurrency_ShouldKeepUnitPricesUnchanged()
	{
		var command = CreateCommand(
			currency: "VND",
			items:
			[
				new OrderItemCommandDto("SKU-001", 1),
				new OrderItemCommandDto("SKU-002", 2),
			]);

		_inventoryServiceClientMock
			.Setup(c => c.GetProductPricesAsync(It.IsAny<IEnumerable<string>>(), "VND", _ct))
			.ReturnsAsync([
				new ProductPriceDto("SKU-001", Money.Create(15000, Currency.Vnd)),
				new ProductPriceDto("SKU-002", Money.Create(2500, Currency.Vnd))
			]);

		OrderEntity? capturedOrder = null;
		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Callback<OrderEntity, CancellationToken>((o, _) => capturedOrder = o)
			.Returns(Task.CompletedTask);
		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ReturnsAsync(1);

		await _handler.Handle(command, _ct);

		capturedOrder!.Items.ElementAt(0).UnitPrice.Amount.Should().Be(15000);
		capturedOrder.Items.ElementAt(1).UnitPrice.Amount.Should().Be(2500);
		capturedOrder.TotalAmount.Amount.Should().Be(20000);
	}

	[Fact]
	public async Task Handle_ShouldReturnOrderId()
	{
		var command = CreateCommand();

		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Returns(Task.CompletedTask);
		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ReturnsAsync(1);

		var result = await _handler.Handle(command, _ct);

		result.Should().NotBeEmpty();
	}

	[Fact]
	public async Task Handle_ShouldRaiseInventoryReservationStartedDomainEvent()
	{
		var command = CreateCommand();

		OrderEntity? capturedOrder = null;
		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Callback<OrderEntity, CancellationToken>((o, _) => capturedOrder = o)
			.Returns(Task.CompletedTask);
		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ReturnsAsync(1);

		await _handler.Handle(command, _ct);

		capturedOrder!.DomainEvents.Should().ContainSingle();
		var domainEvent = capturedOrder.DomainEvents.Single();
		domainEvent.Should().BeOfType<Domain.Events.OrderProcessingStartedDomainEvent>(); // Since running StartProcessing() after order creation
	}

	[Fact]
	public async Task Handle_WithInvalidCurrency_ShouldThrowKeyNotFoundException()
	{
		var command = CreateCommand(currency: "XYZ");

		var act = () => _handler.Handle(command, _ct);

		await act.Should().ThrowAsync<KeyNotFoundException>();
	}

	[Fact]
	public async Task Handle_WhenAddAsyncThrows_ShouldPropagateException()
	{
		var command = CreateCommand();
		var expectedException = new InvalidOperationException("Database failure");

		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.ThrowsAsync(expectedException);

		var act = () => _handler.Handle(command, _ct);

		(await act.Should().ThrowAsync<InvalidOperationException>())
			.Which.Should().BeSameAs(expectedException);
	}

	[Fact]
	public async Task Handle_WhenSaveChangesThrows_ShouldPropagateException()
	{
		var command = CreateCommand();
		var expectedException = new InvalidOperationException("Save failure");

		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Returns(Task.CompletedTask);
		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ThrowsAsync(expectedException);

		var act = () => _handler.Handle(command, _ct);

		(await act.Should().ThrowAsync<InvalidOperationException>())
			.Which.Should().BeSameAs(expectedException);
	}

	[Theory]
	[InlineData("usd")]
	[InlineData("Usd")]
	[InlineData("USD")]
	[InlineData("vnd")]
	[InlineData("VND")]
	[InlineData("Vnd")]
	public async Task Handle_WithDifferentCurrencyCases_ShouldResolveSuccessfully(string currencyCode)
	{
		var command = CreateCommand(currency: currencyCode);

		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Returns(Task.CompletedTask);
		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ReturnsAsync(1);

		var act = () => _handler.Handle(command, _ct);

		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task Handle_WithEmptyCurrency_ShouldThrowArgumentNullException()
	{
		var command = CreateCommand(currency: "");

		var act = () => _handler.Handle(command, _ct);

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task Handle_WithNullCurrency_ShouldThrowArgumentNullException()
	{
		var command = CreateCommand(currency: null!);

		var act = () => _handler.Handle(command, _ct);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	private static CreateOrderCommand CreateCommand(
		string currency = "USD",
		List<OrderItemCommandDto>? items = null)
	{
		items ??=
		[
			new OrderItemCommandDto("SKU-001", 1),
		];

		return new CreateOrderCommand(
			CustomerId: Guid.NewGuid(),
			Currency: currency,
			Items: items,
			IdempotencyKey: Guid.NewGuid(),
			BillingStreet: "123 Main St",
			BillingCity: "Springfield",
			BillingZipCode: "12345",
			BillingCountry: "USA",
			ShippingStreet: "123 Main St",
			ShippingCity: "Springfield",
			ShippingZipCode: "12345",
			ShippingCountry: "USA");
	}
}
