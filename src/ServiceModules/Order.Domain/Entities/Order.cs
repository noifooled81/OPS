using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.ValueObjects;
using Order.Domain.Enums;
using Order.Domain.Events;

namespace Order.Domain.Entities;

public sealed class Order : Entity
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
	public Address BillingAddress { get; private set; } = null!;
	public Address ShippingAddress { get; private set; } = null!;

	private readonly List<OrderItem> _items = [];
	public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// Required for ORM / Parameterless initialization
	private Order() { }

	public Order(
		Guid customerId,
		IEnumerable<OrderItem> items,
		Guid idempotencyKey,
		Address billingAddress,
		Address shippingAddress)
	{
		if (customerId == Guid.Empty)
			throw new ArgumentException("customerId must not be empty", nameof(customerId));
		if (idempotencyKey == Guid.Empty)
			throw new ArgumentException("idempotencyKey must not be empty", nameof(idempotencyKey));

		ArgumentNullException.ThrowIfNull(items, nameof(items));
		ArgumentNullException.ThrowIfNull(billingAddress, nameof(billingAddress));
		ArgumentNullException.ThrowIfNull(shippingAddress, nameof(shippingAddress));

		var itemList = items.ToList();
		ArgumentOutOfRangeException.ThrowIfZero(itemList.Count, nameof(items));

		CustomerId = customerId;
		_items.AddRange(itemList);
		IdempotencyKey = idempotencyKey;
		BillingAddress = billingAddress;
		ShippingAddress = shippingAddress;

		// Calculate TotalAmount dynamically ensuring same currency
		var currency = itemList[0].UnitPrice.Currency;
		int totalSum = 0;
		foreach (var item in itemList)
		{
			if (item.UnitPrice.Currency != currency)
				throw new InvalidOperationException("All items in an order must use the same currency.");
			totalSum += item.UnitPrice.Amount * item.Quantity;
		}

		TotalAmount = Money.Create(totalSum, currency);
		Status = OrderStatus.Created;
		Version = 1;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   State Transitions
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public void StartProcessing()
	{
		if (Status != OrderStatus.Created)
			throw new InvalidOperationException("Can only process newly created orders.");

		Status = Status.Process();
		Version++;

		AddDomainEvent(new OrderProcessingStartedDomainEvent(
			Id,
			CustomerId,
			IdempotencyKey,
			Items.Select(item =>
				new OrderEventItem(
					item.ProductSku,
					item.Quantity,
					item.UnitPrice
				)
			).ToList(),
			TotalAmount,
			BillingAddress,
			ShippingAddress
			));
	}

	public void Approve(Guid paymentId)
	{
		if (paymentId == Guid.Empty)
			throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));

		PaymentId = paymentId;

		Status = Status.Approve();
		Version++;
	}

	public void Complete()
	{
		Status = Status.Complete();
		Version++;
	}

	public void Cancel()
	{
		Status = Status.Cancel();
		Version++;
	}
}
