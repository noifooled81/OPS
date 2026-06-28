using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.ValueObjects;
using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

public sealed class Payment : Entity
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public Guid OrderId { get; private set; }
	public string? ExternalRef { get; private set; }
	public PaymentStatus Status { get; private set; } = null!;
	public Money Amount { get; private set; } = null!;
	public string IdempotencyKey { get; private set; } = null!;
	public string? FailureReason { get; private set; }

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// Required for ORM / Parameterless initialization
	private Payment() { }

	public Payment(Guid orderId, Money amount, string idempotencyKey)
	{
		if (orderId == Guid.Empty)
			throw new ArgumentException("orderId must not be empty", nameof(orderId));

		ArgumentNullException.ThrowIfNull(amount, nameof(amount));
		ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey, nameof(idempotencyKey));

		OrderId = orderId;
		Amount = amount;
		IdempotencyKey = idempotencyKey;
		Status = PaymentStatus.Pending;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Transitions
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	public void Complete(string externalRef)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(externalRef, nameof(externalRef));

		Status = Status.OnComplete();
		ExternalRef = externalRef;
	}

	public void Fail(string failureReason)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(failureReason, nameof(failureReason));

		Status = Status.OnFail();
		FailureReason = failureReason;
	}

	public void Refund()
	{
		Status = Status.OnRefund();
	}
}
