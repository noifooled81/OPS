using BuildingBlocks.CQRS.Interfaces;

namespace Order.Application.Commands.ApproveOrder;

public record ApproveOrderCommand(Guid OrderId) : ICommand;
