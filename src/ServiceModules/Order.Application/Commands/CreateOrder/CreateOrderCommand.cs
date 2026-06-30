using BuildingBlocks.CQRS.Interfaces;
using Order.Application.Commands.Dtos;

namespace Order.Application.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid CustomerId,
    string Currency,
    List<OrderItemCommandDto> Items,
    Guid IdempotencyKey,
    string BillingStreet,
    string BillingCity,
    string BillingZipCode,
    string BillingCountry,
    string ShippingStreet,
    string ShippingCity,
    string ShippingZipCode,
    string ShippingCountry) : ICommand<Guid>;
