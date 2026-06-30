namespace BuildingBlocks.Messaging.Enums;

public enum MessageStatus : short // Forces the enum to utilize exactly 2 bytes
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3
}