// Written by Colin on 2024-7-27

using CoLib.ObjectPools;
using DotNetty.Common;

namespace CoLib.EventLoop;

public class StateActionWithContextScheduledTask: ScheduledTask, ICleanable, IDisposable
{
    private Action<object, object>? _action;
    private object? _context;
    private object? _state;
    
    [ThreadStatic] private static StObjectPool<StateActionWithContextScheduledTask>? _pool;
    private static StObjectPool<StateActionWithContextScheduledTask> Pool => _pool ??= new (128, () => new ());

    public static StateActionWithContextScheduledTask Create(Action<object, object> action, object context, 
        object state, PreciseTimeSpan deadline)
    {
        var task = Pool.Get();
        task.Initialize(action, context, state, deadline);
        return task;
    }

    private StateActionWithContextScheduledTask()
    {
    }
    
    private void Initialize(Action<object, object> action, object context, object state, PreciseTimeSpan deadline)
    {
        Deadline = deadline;
        _action = action;
        _state = state;
        _context = context;
    }

    void ICleanable.Cleanup()
    {
        _action = null;
        _state = null;
        _context = null;
    }

    public void Dispose()
    {
        Pool.Return(this);
    }

    protected override void Execute()
    {
        _action?.Invoke(_context!, _state!);
    }
}