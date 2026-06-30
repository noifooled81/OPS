namespace Order.Application.Queries.Dtos;

public record OrderItemQueryDto(string ProductSku, int Quantity, decimal UnitPrice);
