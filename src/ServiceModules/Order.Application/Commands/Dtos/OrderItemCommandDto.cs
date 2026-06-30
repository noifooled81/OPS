namespace Order.Application.Commands.Dtos;

public record OrderItemCommandDto(string ProductSku, int Quantity);