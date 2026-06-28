using BuildingBlocks.Domain.Common;
using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public sealed class Reservation : Entity
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public Guid OrderId { get; private init; }
	public string ProductSku { get; private init; } = null!;
	public int Quantity { get; private init; }
	public ReservationStatus Status { get; private set; } = null!;
	public DateTimeOffset ExpiresAt { get; private init; }

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	private Reservation() { }

	public Reservation(Guid orderId, string productSku, int quantity, DateTimeOffset expiresAt)
	{
		if (orderId == Guid.Empty)
			throw new ArgumentException("orderId must not be empty", nameof(orderId));

		ArgumentException.ThrowIfNullOrWhiteSpace(productSku, nameof(productSku));
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiresAt, DateTimeOffset.UtcNow, nameof(expiresAt));

		OrderId = orderId;
		ProductSku = productSku;
		Quantity = quantity;
		Status = ReservationStatus.Active;
		ExpiresAt = expiresAt;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Internal Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	internal void MarkAsReleased() => Status = ReservationStatus.Released;
	internal void MarkAsConsumed() => Status = ReservationStatus.Consumed;
}
