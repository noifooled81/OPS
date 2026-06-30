using BuildingBlocks.Domain.ValueObjects;

namespace Order.Application.Clients.Dtos;

public record ProductPriceDto(string ProductSku, Money Price);