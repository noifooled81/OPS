namespace Order.Application.Common.DTOs;

public record OrderItemDto(string ProductSku, int Quantity, decimal UnitPrice);
