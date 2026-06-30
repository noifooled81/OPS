using BuildingBlocks.Domain.Common;

namespace Order.Domain.Enums;

public abstract class OrderStatus(string name, int value) : SmartEnum<OrderStatus, int>(name, value)
{
	public static readonly OrderStatus Created = new CreatedState();
	public static readonly OrderStatus Processing = new ProcessingState();
	public static readonly OrderStatus Approved = new ApprovedState();
	public static readonly OrderStatus Completed = new CompletedState();
	public static readonly OrderStatus Cancelled = new CancelledState();

	public virtual OrderStatus Process() => throw new InvalidOperationException();
	public virtual OrderStatus Approve() => throw new InvalidOperationException();
	public virtual OrderStatus Cancel() => throw new InvalidOperationException();
	public virtual OrderStatus Complete() => throw new InvalidOperationException();

	private sealed class CreatedState() : OrderStatus("CREATED", 1)
	{
		public override OrderStatus Process() => Processing;
	}


	private sealed class ProcessingState() : OrderStatus("PROCESSING", 2)
	{
		public override OrderStatus Approve() => Approved;
		public override OrderStatus Cancel() => Cancelled;
	}

	private sealed class ApprovedState() : OrderStatus("APPROVED", 3)
	{
		public override OrderStatus Complete() => Completed;
		public override OrderStatus Cancel() => Cancelled; // Allow refund/cancel before shipping
	}

	private sealed class CompletedState() : OrderStatus("COMPLETED", 4) { }
	private sealed class CancelledState() : OrderStatus("CANCELLED", 5) { }
}
