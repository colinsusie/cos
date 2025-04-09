// Written by Colin on 2024-7-20

namespace CoLib.Event;

/// <summary>
/// 事件订阅者，用于方便向发布者订阅事件，并在Dispose时统一删除事件
/// </summary>
public class EventSubscriber: IDisposable
{
    private List<IDisposable>? _subs;
    
    /// <summary>
    /// 向发布者订阅事件
    /// </summary>
    public void SubscribeTo<TEvent>(EventPublisher pub, Action<TEvent> action)
    {
        var sub = pub.Subscribe(action);
        _subs ??= [];
        _subs.Add(sub);
    }

    /// <summary>
    /// 消毁
    /// </summary>
    public void Dispose()
    {
        if (_subs != null)
        {
            foreach (var sub in _subs) 
                sub.Dispose();
            _subs.Clear();
        }
    }
}