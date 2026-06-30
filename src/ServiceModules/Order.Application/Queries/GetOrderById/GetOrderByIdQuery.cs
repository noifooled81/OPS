using BuildingBlocks.CQRS.Interfaces;
using Order.Application.Queries.Dtos;

namespace Order.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderQueryDto?>;
