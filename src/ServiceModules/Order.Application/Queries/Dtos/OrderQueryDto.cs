namespace Order.Application.Queries.Dtos;

public record OrderQueryDto(
	Guid Id,
	Guid CustomerId,
	string Status,
	List<OrderItemQueryDto> Items,
	decimal TotalAmount,
	string Currency,
	string Street,
	string City,
	string ZipCode,
	string Country);
