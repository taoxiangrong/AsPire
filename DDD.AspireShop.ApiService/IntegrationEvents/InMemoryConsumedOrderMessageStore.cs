namespace DDD.AspireShop.ApiService.IntegrationEvents;

public sealed class InMemoryConsumedOrderMessageStore
{
    private readonly Lock _lock = new();
    private readonly List<ConsumedOrderMessage> _messages = [];

    public void Add(ConsumedOrderMessage message)
    {
        lock (_lock)
        {
            _messages.Add(message);
        }
    }

    public IReadOnlyCollection<ConsumedOrderMessage> List()
    {
        lock (_lock)
        {
            return _messages.OrderByDescending(message => message.ConsumedAt).ToArray();
        }
    }
}
