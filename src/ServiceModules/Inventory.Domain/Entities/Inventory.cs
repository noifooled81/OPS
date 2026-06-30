using BuildingBlocks.Domain.ValueObjects;
using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public sealed class Inventory
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public string ProductSku { get; private init; } = null!;
	public List<Money> UnitPrices { get; private set; } = null!;
	public int AvailableStock { get; private set; }
	public int ReservedStock { get; private set; }

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	private Inventory() { }

	public Inventory(string productSku, List<Money> unitPrices, int availableStock, int reservedStock)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(productSku, nameof(productSku));
		ArgumentNullException.ThrowIfNull(unitPrices, nameof(unitPrices));
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(availableStock, nameof(availableStock));
		ArgumentOutOfRangeException.ThrowIfNegative(reservedStock, nameof(reservedStock));

		ProductSku = productSku;
		UnitPrices = unitPrices;
		AvailableStock = availableStock;
		ReservedStock = reservedStock;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public Reservation Reserve(Guid orderId, int quantity, DateTimeOffset expiresAt)
	{
		if (quantity > AvailableStock)
			throw new InvalidOperationException("Insufficient stock available for reservation.");

		// Mutate stock states
		AvailableStock -= quantity;
		ReservedStock += quantity;

		// Factory behavior: Inventory creates the tracking reservation
		return new Reservation(orderId, ProductSku, quantity, expiresAt);
	}

	public void Release(Reservation reservation)
	{
		if (reservation.ProductSku != ProductSku)
			throw new ArgumentException("Reservation does not match this Inventory SKU.");
		if (reservation.Status != ReservationStatus.Active)
			throw new InvalidOperationException("Only active reservations can be released.");

		// Mutate stock states (Return stock back to pool)
		AvailableStock += reservation.Quantity;
		ReservedStock -= reservation.Quantity;

		reservation.MarkAsReleased();
	}

	public void Consume(Reservation reservation)
	{
		if (reservation.ProductSku != ProductSku)
			throw new ArgumentException("Reservation does not match this Inventory SKU.");
		if (reservation.Status != ReservationStatus.Active)
			throw new InvalidOperationException("Only active reservations can be consumed.");
		if (reservation.ExpiresAt <= DateTimeOffset.UtcNow)
			throw new InvalidOperationException("Cannot consume an expired reservation.");

		// Mutate stock states (Stock leaves the building entirely)
		ReservedStock -= reservation.Quantity;

		reservation.MarkAsConsumed();
	}
}
