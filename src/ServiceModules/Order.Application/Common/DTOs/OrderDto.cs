namespace Order.Application.Common.DTOs;

public record OrderDto(
	Guid Id,
	Guid CustomerId,
	string Status,
	List<OrderItemDto> Items,
	decimal TotalAmount,
	string Currency,
	string Street,
	string City,
	string ZipCode,
	string Country);
