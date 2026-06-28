using BuildingBlocks.Domain.Common;

namespace Inventory.Domain.Enums;

public sealed class ReservationStatus(string name, int value) : SmartEnum<ReservationStatus, int>(name, value)
{
	public static readonly ReservationStatus Active = new("ACTIVE", 1);
	public static readonly ReservationStatus Consumed = new("CONSUMED", 2);
	public static readonly ReservationStatus Released = new("RELEASED", 3);
}
