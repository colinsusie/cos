// Written by Colin on 2024-7-27

using CoLib.ObjectPools;
using DotNetty.Common;

namespace CoLib.EventLoop;

public class ActionScheduledTask: ScheduledTask, ICleanable, IDisposable
{
    private Action? _action;
    
    [ThreadStatic] private static StObjectPool<ActionScheduledTask>? _pool;
    private static StObjectPool<ActionScheduledTask> Pool => _pool ??= new (128, () => new ());

    public static ActionScheduledTask Create(Action action, PreciseTimeSpan deadline)
    {
        var task = Pool.Get();
        task.Initialize(action, deadline);
        return task;
    }

    private ActionScheduledTask()
    {
    }
    
    private void Initialize(Action action, PreciseTimeSpan deadline)
    {
        Deadline = deadline;
        _action = action;
    }

    void ICleanable.Cleanup()
    {
        _action = null;
    }

    public void Dispose()
    {
        Pool.Return(this);
    }

    protected override void Execute()
    {
        _action?.Invoke();
    }
}