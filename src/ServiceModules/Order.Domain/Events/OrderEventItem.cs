using BuildingBlocks.Domain.ValueObjects;

namespace Order.Domain.Events;

public record OrderEventItem(string ProductSku, int Quantity, Money UnitPrice);