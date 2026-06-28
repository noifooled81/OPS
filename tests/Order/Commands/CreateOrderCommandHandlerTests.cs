using BuildingBlocks.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Application.Commands.CreateOrder;
using Order.Application.Common.DTOs;
using Order.Domain.Repositories;
using Xunit;
using OrderEntity = Order.Domain.Entities.Order;
using OrderStatus = Order.Domain.Enums.OrderStatus;

namespace Order.Commands.Tests;

public sealed class CreateOrderCommandHandlerTests
{
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<IOrderRepository> _orderRepositoryMock;
	private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
	private readonly CreateOrderCommandHandler _handler;
	private readonly CancellationToken _ct;

	public CreateOrderCommandHandlerTests()
	{
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_orderRepositoryMock = new Mock<IOrderRepository>();
		_loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();

		_handler = new CreateOrderCommandHandler(
			_unitOfWorkMock.Object,
			_orderRepositoryMock.Object,
			_loggerMock.Object);

		_ct = CancellationToken.None;
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
		capturedOrder.Status.Should().Be(OrderStatus.Created);
		capturedOrder.Version.Should().Be(1);

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
				new OrderItemDto("SKU-001", 2, 19.99m),
				new OrderItemDto("SKU-002", 1,  5.50m),
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
				new OrderItemDto("SKU-001", 1, 15000m),
				new OrderItemDto("SKU-002", 2,  2500m),
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
	public async Task Handle_ShouldRaiseOrderCreatedDomainEvent()
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
		domainEvent.Should().BeOfType<Order.Domain.Events.OrderCreatedDomainEvent>();
	}

	[Fact]
	public async Task Handle_ShouldLogInformation_WhenOrderCreated()
	{
		var command = CreateCommand();

		_orderRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), _ct))
			.Returns(Task.CompletedTask);
		_unitOfWorkMock
			.Setup(u => u.SaveChangesAsync(_ct))
			.ReturnsAsync(1);

		await _handler.Handle(command, _ct);

		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("created")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
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
		List<OrderItemDto>? items = null)
	{
		items ??=
		[
			new OrderItemDto("SKU-001", 1, 10.00m),
		];

		return new CreateOrderCommand(
			CustomerId: Guid.NewGuid(),
			Currency: currency,
			Items: items,
			IdempotencyKey: Guid.NewGuid(),
			Street: "123 Main St",
			City: "Springfield",
			ZipCode: "12345",
			Country: "USA");
	}
}
