// Written by Colin on 2024-7-26

using CoLib.ObjectPools;
using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

internal sealed class ActionTask: IRunnable, ICleanable, IDisposable
{
    private Action? _action;
    
    [ThreadStatic] private static StObjectPool<ActionTask>? _pool;
    private static StObjectPool<ActionTask> Pool => _pool ??= new (128, () => new ());
    
    public static ActionTask Create(Action action)
    {
        var task = Pool.Get();
        task.Initialize(action);
        return task;
    }

    private ActionTask()
    {
    }
    
    private void Initialize(Action action)
    {
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
    
    public void Run()
    {
        _action?.Invoke();
    }
}