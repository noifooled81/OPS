namespace BuildingBlocks.Messaging.Interfaces;

public interface IOutboxService
{
    // Event handlers call this to queue the message
    void QueueMessage<T>(T integrationEvent) where T : class;
}
