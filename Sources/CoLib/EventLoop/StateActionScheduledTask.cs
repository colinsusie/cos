// Written by Colin on 2024-7-27

using CoLib.ObjectPools;
using DotNetty.Common;

namespace CoLib.EventLoop;

public class StateActionScheduledTask: ScheduledTask, ICleanable, IDisposable
{
    private Action<object>? _action;
    private object? _state;
    
    [ThreadStatic] private static StObjectPool<StateActionScheduledTask>? _pool;
    private static StObjectPool<StateActionScheduledTask> Pool => _pool ??= new (128, () => new ());

    public static StateActionScheduledTask Create(Action<object> action, object state, PreciseTimeSpan deadline)
    {
        var task = Pool.Get();
        task.Initialize(action, state, deadline);
        return task;
    }

    private StateActionScheduledTask()
    {
    }
    
    private void Initialize(Action<object> action, object state, PreciseTimeSpan deadline)
    {
        Deadline = deadline;
        _action = action;
        _state = state;
    }

    void ICleanable.Cleanup()
    {
        _action = null;
        _state = null;
    }

    public void Dispose()
    {
        Pool.Return(this);
    }

    protected override void Execute()
    {
        _action?.Invoke(_state!);
    }
}