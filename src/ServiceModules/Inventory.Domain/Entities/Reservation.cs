using BuildingBlocks.Domain.Common;

namespace Inventory.Domain.Entities;

public sealed class Reservation : Entity
{
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //   Public Properties
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    public Guid OrderId { get; private init; }
    public string SkuId { get; private init; } = null!;
    public int Quantity { get; private init; }
    public ReservationStatus Status { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private init; }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //   Constructors
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    private Reservation() { }

    public Reservation(Guid orderId, string skuId, int quantity, DateTimeOffset expiresAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("orderId must not be empty", nameof(orderId));

        ArgumentNullException.ThrowIfNullOrWhiteSpace(skuId, nameof(skuId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiresAt, DateTimeOffset.UtcNow, nameof(expiresAt));

        OrderId = orderId;
        SkuId = skuId;
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