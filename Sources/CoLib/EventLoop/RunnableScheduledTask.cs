// Written by Colin on 2024-7-27

using CoLib.ObjectPools;
using DotNetty.Common;
using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

public class RunnableScheduledTask: ScheduledTask, ICleanable, IDisposable
{
    private IRunnable? _runnable;
    
    [ThreadStatic] private static StObjectPool<RunnableScheduledTask>? _pool;
    private static StObjectPool<RunnableScheduledTask> Pool => _pool ??= new (128, () => new ());

    public static RunnableScheduledTask Create(IRunnable runnable, PreciseTimeSpan deadline)
    {
        var task = Pool.Get();
        task.Initialize(runnable, deadline);
        return task;
    }

    private RunnableScheduledTask()
    {
    }
    
    private void Initialize(IRunnable runnable, PreciseTimeSpan deadline)
    {
        Deadline = deadline;
        _runnable = runnable;
    }

    void ICleanable.Cleanup()
    {
        _runnable = null;
    }

    public void Dispose()
    {
        Pool.Return(this);
    }

    protected override void Execute()
    {
        _runnable?.Run();
    }
}