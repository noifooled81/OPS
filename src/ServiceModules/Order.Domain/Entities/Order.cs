using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.ValueObjects;
using Order.Domain.Enums;

namespace Order.Domain.Entities;

public class Order : AuditableEntity
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public Guid CustomerId { get; private set; }
	public OrderStatus Status { get; private set; } = null!;
	public Money TotalAmount { get; private set; } = null!;
	public Guid IdempotencyKey { get; private set; }
	public uint Version { get; private set; }
	public Guid? PaymentId { get; private set; }
	public Address ShippingAddress { get; private set; } = null!;

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// Required for ORM / Parameterless initialization
	private Order() { }

	public Order(
		Guid customerId,
		Money totalAmount,
		Guid idempotencyKey,
		Address shippingAddress)
	{
		if (customerId == Guid.Empty)
			throw new ArgumentException("customerId must not be empty", nameof(customerId));
		if (idempotencyKey == Guid.Empty)
			throw new ArgumentException("idempotencyKey must not be empty", nameof(idempotencyKey));

		ArgumentNullException.ThrowIfNull(totalAmount);
		ArgumentNullException.ThrowIfNull(shippingAddress);

		CustomerId = customerId;
		TotalAmount = totalAmount;
		IdempotencyKey = idempotencyKey;
		ShippingAddress = shippingAddress;

		Status = OrderStatus.Created;
		Version = 1;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Transitions
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public void StartInventoryReservation()
	{
		Status = Status.OnStartInventoryReservation();
		Version++;
	}

	public void InventoryReserved()
	{
		Status = Status.OnInventoryReserved();
		Version++;
	}

	public void PaymentValidated(Guid paymentId)
	{
		PaymentId = paymentId;
		Status = Status.OnPaymentValidated();
		Version++;
	}
}
