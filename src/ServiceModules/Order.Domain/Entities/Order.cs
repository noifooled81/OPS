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

	// --- Feature: Inventory Phase ---
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

	public void InventoryReservationFailed()
	{
		Status = Status.OnInventoryReservationFailed();
		Version++;
	}

	// --- Feature: Payment Phase ---
	public void StartPayment()
	{
		Status = Status.OnStartPayment();
		Version++;
	}

	public void PaymentValidated(Guid paymentId)
	{
		if (paymentId == Guid.Empty)
			throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));

		PaymentId = paymentId;
		Status = Status.OnPaymentValidated();
		Version++;
	}

	public void PaymentFailed()
	{
		Status = Status.OnPaymentFailed();
		Version++;
	}

	// Triggered when the 15-minute inventory reservation TTL expires
	public void PaymentTimedOut()
	{
		Status = Status.OnPaymentTimeout();
		Version++;
	}

	// --- Feature: Compensation & Finalization ---

	// Triggered when the Inventory Service confirms stock has been freed.
	public void StockReleased()
	{
		Status = Status.OnStockReleased();
		Version++;
	}

	public void OrderApproved()
	{
		Status = Status.OnApproveOrder();
		Version++;
	}

	public void OrderCompleted()
	{
		Status = Status.OnCompleteOrder();
		Version++;
	}

	// Triggered when the Payment Service handles a late success compensation refund.
	public void PaymentRefunded()
	{
		Status = Status.OnPaymentRefundedByGateway();
		Version++;
	}
}
