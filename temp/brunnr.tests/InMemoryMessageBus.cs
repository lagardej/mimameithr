using Brunnr.Messaging;

namespace Brunnr.Tests;

public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    public void Publish<TPayload>(Envelope<TPayload> envelope)
    {
        if (!_subscribers.TryGetValue(typeof(TPayload), out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            ((Action<Envelope<TPayload>>) handler)(envelope);
        }
    }

    public IDisposable Subscribe<TPayload>(Action<Envelope<TPayload>> handler)
    {
        if (!_subscribers.TryGetValue(typeof(TPayload), out var handlers))
        {
            _subscribers[typeof(TPayload)] = handlers = new List<Delegate>();
        }

        handlers.Add(handler);
        return new Subscription(() => handlers.Remove(handler));
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        public void Dispose() => unsubscribe();
    }
}
