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
	public static readonly OrderStatus SystemRefunded = new RefundedState();

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
	public virtual OrderStatus OnPaymentTimeout() => InvalidTransition();

	// --- Feature: Compensation & Finalization ---
	public virtual OrderStatus OnStockReleased() => InvalidTransition();
	public virtual OrderStatus OnApproveOrder() => InvalidTransition();
	public virtual OrderStatus OnCompleteOrder() => InvalidTransition();
	public virtual OrderStatus OnPaymentRefundedByGateway() => InvalidTransition();

	protected OrderStatus InvalidTransition() =>
		throw new InvalidOperationException($"Invalid transition from {Name}");

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Definitions
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// --- Feature: Trasition States ---
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

		// Idempotency Guard: If the message to start payment is processed twice
		public override OrderStatus OnInventoryReserved() => this;
	}

	private sealed class PaymentPendingState() : OrderStatus("PAYMENT_PENDING", 4)
	{
		public override OrderStatus OnPaymentValidated() => PaymentValidated;
		public override OrderStatus OnPaymentFailed() => ReleaseStockPending;
		public override OrderStatus OnPaymentTimeout() => ReleaseStockPending;

		// Idempotency Guard: If the command message to initiate payment retries
		public override OrderStatus OnStartPayment() => this;
	}

	private sealed class PaymentValidatedState() : OrderStatus("PAYMENT_VALIDATED", 5)
	{
		public override OrderStatus OnApproveOrder() => Approved;

		// Idempotency Guard: Safe retry handler for late/duplicate webhooks
		public override OrderStatus OnPaymentValidated() => this;
	}

	private sealed class ReleaseStockPendingState() : OrderStatus("RELEASE_STOCK_PENDING", 6)
	{
		// Triggered when Inventory Service broadcasts via MQ that stock was released
		public override OrderStatus OnStockReleased() => Cancelled;

		// Idempotency Guards: If payment failure/timeout MQ events arrive late or multiple times
		public override OrderStatus OnPaymentFailed() => this;
		public override OrderStatus OnPaymentTimeout() => this;
	}

	private sealed class ApprovedState() : OrderStatus("APPROVED", 7)
	{
		public override OrderStatus OnCompleteOrder() => Completed;
		public override OrderStatus OnPaymentRefundedByGateway() => SystemRefunded;

		// Idempotency Guard: If late MQ events arrive attempting to re-approve
		public override OrderStatus OnApproveOrder() => this;
		public override OrderStatus OnPaymentValidated() => this;
	}

	// --- Feature: Terminate States ---
	private sealed class CompletedState() : OrderStatus("COMPLETED", 8)
	{
		public override OrderStatus OnCompleteOrder() => this;
	}

	private sealed class CancelledState() : OrderStatus("CANCELLED", 9)
	{
		// If Stripe charges a user 20 minutes later, returning 'this' keeps the order CANCELLED.
		// When the Payment Service checks this status, it sees CANCELLED and fires an immediate refund.
		public override OrderStatus OnPaymentValidated() => this;

		// Idempotency Guards: Swallow late processing events gracefully
		public override OrderStatus OnInventoryReserved() => this;
		public override OrderStatus OnInventoryReservationFailed() => this;
		public override OrderStatus OnPaymentFailed() => this;
		public override OrderStatus OnPaymentTimeout() => this;
		public override OrderStatus OnStockReleased() => this;
	}

	private sealed class RefundedState() : OrderStatus("SYSTEM_REFUNDED", 10)
	{
		public override OrderStatus OnPaymentRefundedByGateway() => this;
	}
}
