using BuildingBlocks.Domain.Common;

namespace Order.Domain.Enums;

public abstract class OrderStatus(string name, int value) : SmartEnum<OrderStatus, int>(name, value)
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Instances
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public static readonly OrderStatus Created = new CreatedState();
	public static readonly OrderStatus InventoryPending = new InventoryPendingState();
	public static readonly OrderStatus InventoryReserved = new InventoryReservedState();
	public static readonly OrderStatus PaymentPending = new PaymentPendingState();
	public static readonly OrderStatus PaymentValidated = new PaymentValidatedState();
	public static readonly OrderStatus ReleaseStockPending = new ReleaseStockPendingState();
	public static readonly OrderStatus Approved = new ApprovedState();
	public static readonly OrderStatus Completed = new CompletedState();
	public static readonly OrderStatus Cancelled = new CancelledState();
	public static readonly OrderStatus Refunded = new RefundedState();

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Machine Events
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// --- Feature: Inventory Phase ---
	public virtual OrderStatus OnStartInventoryReservation() => InvalidTransition();
	public virtual OrderStatus OnInventoryReserved() => InvalidTransition();
	public virtual OrderStatus OnInventoryReservationFailed() => InvalidTransition();

	// --- Feature: Payment Phase ---
	public virtual OrderStatus OnStartPayment() => InvalidTransition();
	public virtual OrderStatus OnPaymentValidated() => InvalidTransition();
	public virtual OrderStatus OnPaymentFailed() => InvalidTransition();

	// --- Feature: Compensation & Finalization ---
	public virtual OrderStatus OnStockReleased() => InvalidTransition();
	public virtual OrderStatus OnApproveOrder() => InvalidTransition();
	public virtual OrderStatus OnCompleteOrder() => InvalidTransition();
	public virtual OrderStatus OnRefundOrder() => InvalidTransition();

	protected OrderStatus InvalidTransition() =>
	   throw new InvalidOperationException($"Invalid transition from {Name}");


	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Definitions
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	private sealed class CreatedState() : OrderStatus("CREATED", 1)
	{
		public override OrderStatus OnStartInventoryReservation() => InventoryPending;
	}

	private sealed class InventoryPendingState() : OrderStatus("INVENTORY_PENDING", 2)
	{
		public override OrderStatus OnInventoryReserved() => InventoryReserved;
		public override OrderStatus OnInventoryReservationFailed() => Cancelled;
	}

	private sealed class InventoryReservedState() : OrderStatus("INVENTORY_RESERVED", 3)
	{
		public override OrderStatus OnStartPayment() => PaymentPending;
	}

	private sealed class PaymentPendingState() : OrderStatus("PAYMENT_PENDING", 4)
	{
		public override OrderStatus OnPaymentValidated() => PaymentValidated;
		public override OrderStatus OnPaymentFailed() => ReleaseStockPending;
	}

	private sealed class PaymentValidatedState() : OrderStatus("PAYMENT_VALIDATED", 5)
	{
		public override OrderStatus OnApproveOrder() => Approved;
	}

	private sealed class ReleaseStockPendingState() : OrderStatus("RELEASE_STOCK_PENDING", 6)
	{
		// Triggered when Inventory Service broadcasts via MQ that stock was released
		public override OrderStatus OnStockReleased() => Cancelled;
	}

	private sealed class ApprovedState() : OrderStatus("APPROVED", 7)
	{
		public override OrderStatus OnCompleteOrder() => Completed;
		public override OrderStatus OnRefundOrder() => Refunded;
	}

	private sealed class CompletedState() : OrderStatus("COMPLETED", 8) { }
	private sealed class CancelledState() : OrderStatus("CANCELLED", 9) { }
	private sealed class RefundedState() : OrderStatus("REFUNDED", 10) { }
}
