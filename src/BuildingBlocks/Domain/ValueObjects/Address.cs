namespace BuildingBlocks.Domain.ValueObjects;

public record Address(
	string Street,
	string City,
	string ZipCode,
	string Country);
