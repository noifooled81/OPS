using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class OrderItem : Entity
{
	public string ProductSku { get; private set; } = null!;
	public int Quantity { get; private set; }
	public Money UnitPrice { get; private set; } = null!;

	private OrderItem() { }

	public OrderItem(string productSku, int quantity, Money unitPrice)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(productSku, nameof(productSku));
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));
		ArgumentNullException.ThrowIfNull(unitPrice, nameof(unitPrice));

		ProductSku = productSku;
		Quantity = quantity;
		UnitPrice = unitPrice;
	}
}
