using BuildingBlocks.Messaging.Enums;
using Newtonsoft.Json.Linq;

namespace BuildingBlocks.Messaging.Entities;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public JObject? Payload { get; set; }
    public MessageStatus Status { get; set; }
    public int Attempt { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
}