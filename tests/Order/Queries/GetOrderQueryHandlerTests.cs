using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Order.Application.Queries.GetOrderById;
using Order.Domain.Entities;
using Order.Domain.Repositories;
using Xunit;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Queries.Tests;

public sealed class GetOrderQueryHandlerTests
{
	private readonly Mock<IOrderReadRepository> _orderRepositoryMock;
	private readonly GetOrderByIdQueryHandler _handler;
	private readonly CancellationToken _ct;

	public GetOrderQueryHandlerTests()
	{
		_orderRepositoryMock = new Mock<IOrderReadRepository>();

		_handler = new GetOrderByIdQueryHandler(
			_orderRepositoryMock.Object);

		_ct = CancellationToken.None;
	}

	[Fact]
	public async Task Handle_WhenOrderIdIsValid_ReturnsDto()
	{
		var address = new Address("123 Main St", "Springfield", "12345", "USA");
		var order = new OrderEntity(
			Guid.NewGuid(),
			[
				new OrderItem(
					"SKU-001",
					2,
					Money.Create(1200, Currency.Usd) // 12$
				),
				new OrderItem(
					"SKU-002",
					2,
					Money.Create(1200, Currency.Usd) // 12$
				),
			],
			Guid.NewGuid(),
			address,
			address
		);

		_orderRepositoryMock
			.Setup(r => r.GetByIdAsync(order.Id, _ct))
			.ReturnsAsync(order);

		var result = await _handler.Handle(
			new GetOrderByIdQuery(order.Id),
			_ct
		);

		result.Should().NotBeNull();
		result!.Id.Should().Be(order.Id);
		result!.TotalAmount.Should().Be(48m);
		result!.Items.Should().HaveCount(2);
		result!.Items.Should().ContainSingle(i => i.ProductSku == "SKU-001");

		var item = result.Items.Single(i => i.ProductSku == "SKU-001");
		item.Quantity.Should().Be(2);
		item.UnitPrice.Should().Be(12m);
	}

	[Fact]
	public async Task Handle_WhenOrderIdIsNotValid_ReturnsNull()
	{
		var id = Guid.NewGuid();

		_orderRepositoryMock
			.Setup(r => r.GetByIdAsync(id, _ct))
			.ReturnsAsync((OrderEntity?)null);

		var result = await _handler.Handle(
			new GetOrderByIdQuery(id),
			_ct
		);

		result.Should().BeNull();
	}
}
