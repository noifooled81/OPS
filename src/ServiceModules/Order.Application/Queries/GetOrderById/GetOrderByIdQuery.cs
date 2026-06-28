using BuildingBlocks.CQRS.Interfaces;
using Order.Application.Common.DTOs;

namespace Order.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;
