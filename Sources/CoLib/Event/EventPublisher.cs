// Written by Colin on 2024-7-20

using CoLib.Container;
using CoLib.Extensions;
using CoLib.Logging;
using CoLib.ObjectPools;

namespace CoLib.Event;

/// <summary>
/// 事件发布者
/// </summary>
public class EventPublisher
{
    private readonly Logger _logger;
    private Dictionary<Type, object>? _handlers;

    public EventPublisher(Logger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 订阅一个事件
    /// </summary>
    public IDisposable Subscribe<TEvent>(Action<TEvent> action)
    {
        var handlers = GetOrCreateHandlers<TEvent>(typeof(TEvent));
        return handlers.Add(action);
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    public void Publish<TEvent>(in TEvent evt)
    {
        if (_handlers == null || !_handlers.TryGetValue(typeof(TEvent), out var obj))
            return;

        var handlers = (EventHandlers<TEvent>)obj;
        handlers.Invoke(evt);
    }

    private EventHandlers<TEvent> GetOrCreateHandlers<TEvent>(Type type)
    {
        _handlers ??= new();
        if (_handlers.TryGetValue(type, out var obj))
        {
            return (EventHandlers<TEvent>)obj;
        }

        var handler = new EventHandlers<TEvent>(_logger);
        _handlers[type] = handler;
        return handler;
    }
}

internal class EventHandlers<TEvent>
{
    private readonly Logger _logger;
    private readonly HashSet<Action<TEvent>> _actions = new();
    
    public EventHandlers(Logger logger)
    {
        _logger = logger;
    }

    public IDisposable Add(Action<TEvent> action)
    {
        _actions.Add(action);
        return Disposer.Create(this, action);
    }

    public void Invoke(in TEvent evt)
    {
        using LocalList<Action<TEvent>> actions = new(_actions.Count);
        foreach (var action in _actions)
        {
            actions.Add(action);
        }

        foreach (var action in actions)
        {
            try
            {
                action(evt);
            }
            catch (Exception e)
            {
                _logger.Error($"error: {e}");
            }
        }
    }
    
    private class Disposer : IDisposable, ICleanable
    {
        [ThreadStatic] private static StObjectPool<Disposer>? _pool;
        private static StObjectPool<Disposer> Pool =>
            _pool ??= new StObjectPool<Disposer>(256, () => new Disposer());
        
        private Action<TEvent>? _action;
        private EventHandlers<TEvent>? _handlers;

        public static Disposer Create(EventHandlers<TEvent> handlers, Action<TEvent> action)
        {
            var dis = Pool.Get();
            dis.Initialize(handlers, action);
            return dis;
        }

        private Disposer()
        {
        }

        private void Initialize(EventHandlers<TEvent> handlers, Action<TEvent> action)
        {
            _handlers = handlers;
            _action = action;
        }

        void IDisposable.Dispose()
        {
            Pool.Return(this);
        }

        void ICleanable.Cleanup()
        {
            if(_action != null)
            {
                _handlers?._actions.Remove(_action);
            }
            _action = null;
            _handlers = null;
        }
    }
}

