using BuildingBlocks.Domain.Common;

namespace Payment.Domain.Enums;

public abstract class PaymentStatus(string name, int value) : SmartEnum<PaymentStatus, int>(name, value)
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Instances
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public static readonly PaymentStatus Pending = new PendingState();
	public static readonly PaymentStatus Completed = new CompletedState();
	public static readonly PaymentStatus Refunded = new RefundedState();
	public static readonly PaymentStatus Failed = new FailedState();

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Machine Events
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public virtual PaymentStatus OnComplete() => InvalidTransition();
	public virtual PaymentStatus OnFail() => InvalidTransition();
	public virtual PaymentStatus OnRefund() => InvalidTransition();

	protected PaymentStatus InvalidTransition() =>
		throw new InvalidOperationException($"Invalid transition from {Name}");

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Definitions
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	private sealed class PendingState() : PaymentStatus("PENDING", 1)
	{
		public override PaymentStatus OnComplete() => Completed;
		public override PaymentStatus OnFail() => Failed;
	}

	private sealed class CompletedState() : PaymentStatus("COMPLETED", 2)
	{
		public override PaymentStatus OnRefund() => Refunded;

		// Idempotency Guard: Safe retry handler for completing payment
		public override PaymentStatus OnComplete() => this;
	}

	private sealed class RefundedState() : PaymentStatus("REFUNDED", 3)
	{
		// Idempotency Guard: Safe retry handler for refunding payment
		public override PaymentStatus OnRefund() => this;
	}

	private sealed class FailedState() : PaymentStatus("FAILED", 4)
	{
		// Idempotency Guard: Safe retry handler for failing payment
		public override PaymentStatus OnFail() => this;
	}
}
