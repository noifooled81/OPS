using BuildingBlocks.Domain.Common;

namespace Inventory.Domain.Entities;

public sealed class ReservationStatus(string name, int value) : SmartEnum<ReservationStatus, int>(name, value)
{
    public static readonly ReservationStatus Active = new ReservationStatus("ACTIVE", 1);
    public static readonly ReservationStatus Consumed = new ReservationStatus("CONSUMED", 2);
    public static readonly ReservationStatus Released = new ReservationStatus("RELEASED", 3);
}