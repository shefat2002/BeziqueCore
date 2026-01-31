using System.Collections.Concurrent;
using BeziqueCore.Core.API.Events;

namespace BeziqueCore.Core.API.Factory;

public delegate bool EventPredicate<TEventArgs>(TEventArgs args) where TEventArgs : GameEventArgs;

public class EventSubscriptionManager
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, (Delegate handler, Delegate? filter)>> _subscribers = new();
    private readonly ConcurrentQueue<GameEventArgs> _eventBatch = new();
    private readonly object _batchLock = new();
    private bool _isBatching;

    public void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler, EventPredicate<TEventArgs>? filter = null) where TEventArgs : GameEventArgs
    {
        var eventType = typeof(TEventArgs);
        var subscribers = _subscribers.GetOrAdd(eventType, _ => new ConcurrentDictionary<Guid, (Delegate, Delegate?)>());

        var subscriptionId = Guid.NewGuid();
        subscribers.TryAdd(subscriptionId, (handler, filter));
    }

    public void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
    {
        var eventType = typeof(TEventArgs);
        if (_subscribers.TryGetValue(eventType, out var subscribers))
        {
            var toRemove = subscribers.Where(kvp => kvp.Value.handler == handler).Select(kvp => kvp.Key).ToList();
            foreach (var key in toRemove)
            {
                subscribers.TryRemove(key, out _);
            }
        }
    }

    public void UnsubscribeAll<TEventArgs>() where TEventArgs : GameEventArgs
    {
        var eventType = typeof(TEventArgs);
        _subscribers.TryRemove(eventType, out _);
    }

    public void Publish<TEventArgs>(object? sender, TEventArgs args) where TEventArgs : GameEventArgs
    {
        if (_isBatching)
        {
            lock (_batchLock)
            {
                _eventBatch.Enqueue(args);
            }
            return;
        }

        PublishInternal(sender, args);
    }

    private void PublishInternal<TEventArgs>(object? sender, TEventArgs args) where TEventArgs : GameEventArgs
    {
        var eventType = typeof(TEventArgs);
        if (!_subscribers.TryGetValue(eventType, out var subscribers))
            return;

        foreach (var subscriber in subscribers.Values)
        {
            try
            {
                if (subscriber.filter is EventPredicate<TEventArgs> filter && !filter(args))
                    continue;

                if (subscriber.handler is EventHandler<TEventArgs> typedHandler)
                {
                    typedHandler.Invoke(sender, args);
                }
            }
            catch
            {
            }
        }
    }

    public void BeginBatch()
    {
        _isBatching = true;
    }

    public void EndBatch(object? sender)
    {
        _isBatching = false;

        lock (_batchLock)
        {
            while (_eventBatch.TryDequeue(out var evt))
            {
                PublishInternal(sender, evt);
            }
        }
    }

    public void FlushBatch(object? sender)
    {
        EndBatch(sender);
        BeginBatch();
    }

    public int GetSubscriberCount<TEventArgs>() where TEventArgs : GameEventArgs
    {
        var eventType = typeof(TEventArgs);
        return _subscribers.TryGetValue(eventType, out var subscribers) ? subscribers.Count : 0;
    }

    public void ClearAll()
    {
        _subscribers.Clear();
    }
}

public static class EventSubscriptionExtensions
{
    public static void SubscribeToCardPlayed(this EventSubscriptionManager manager, Action<CardPlayedEventArgs> action, EventPredicate<CardPlayedEventArgs>? filter = null)
    {
        manager.Subscribe((s, e) => action(e), filter);
    }

    public static void SubscribeToMeldDeclared(this EventSubscriptionManager manager, Action<MeldDeclaredEventArgs> action, EventPredicate<MeldDeclaredEventArgs>? filter = null)
    {
        manager.Subscribe((s, e) => action(e), filter);
    }

    public static void SubscribeToTrickResolved(this EventSubscriptionManager manager, Action<TrickResolvedEventArgs> action, EventPredicate<TrickResolvedEventArgs>? filter = null)
    {
        manager.Subscribe((s, e) => action(e), filter);
    }

    public static void SubscribeToPlayerTurnChanged(this EventSubscriptionManager manager, Action<PlayerTurnChangedEventArgs> action, EventPredicate<PlayerTurnChangedEventArgs>? filter = null)
    {
        manager.Subscribe((s, e) => action(e), filter);
    }

    public static void SubscribeToRoundEnded(this EventSubscriptionManager manager, Action<RoundEndedEventArgs> action, EventPredicate<RoundEndedEventArgs>? filter = null)
    {
        manager.Subscribe((s, e) => action(e), filter);
    }

    public static void SubscribeToGameEnded(this EventSubscriptionManager manager, Action<GameEndedEventArgs> action, EventPredicate<GameEndedEventArgs>? filter = null)
    {
        manager.Subscribe((s, e) => action(e), filter);
    }

    public static void SubscribeToError(this EventSubscriptionManager manager, Action<GameErrorEventArgs> action, EventPredicate<GameErrorEventArgs>? filter = null)
    {
        manager.Subscribe((s, e) => action(e), filter);
    }

    public static void SubscribeWhenPlayerIs(this EventSubscriptionManager manager, string playerName, EventHandler<CardPlayedEventArgs> handler)
    {
        EventPredicate<CardPlayedEventArgs> filter = (e) => e.PlayerName == playerName;
        manager.Subscribe(handler, filter);
    }
}
