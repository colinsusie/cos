// Written by Colin on 2024-9-22

using CoLib.Message;
using CoLib.ObjectPools;
using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

public class StEventLoopTaskScheduler: TaskScheduler
{
    private readonly StEventLoop _eventLoop;
    private bool _started;

    public StEventLoopTaskScheduler(StEventLoop eventLoop) => _eventLoop = eventLoop;

    protected override void QueueTask(Task task)
    {
        if (_started)
        {
            _eventLoop.Execute(TaskRunnable.Create(this, task));
        }
        else
        {
            _started = true;
            TryExecuteTask(task);
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return !taskWasPreviouslyQueued && _eventLoop.InEventLoop && TryExecuteTask(task);
    }

    protected override IEnumerable<Task>? GetScheduledTasks() => null;
    protected override bool TryDequeue(Task task) => false;


    private class TaskRunnable: IRunnable, IDisposable, ICleanable
    {
        private StEventLoopTaskScheduler? _scheduler;
        private Task? _task;

        [ThreadStatic] private static StObjectPool<TaskRunnable>? _pool;
        private static StObjectPool<TaskRunnable> Pool => _pool ??= new StObjectPool<TaskRunnable>(128, () => new TaskRunnable());
    
        public static TaskRunnable Create(StEventLoopTaskScheduler scheduler, Task task)
        {
            return Pool.Get().Initialize(scheduler, task);
        }
    
        private TaskRunnable Initialize(StEventLoopTaskScheduler scheduler, Task task)
        {
            _scheduler = scheduler;
            _task = task;
            return this;
        }
    
        public void Run()
        {
            _scheduler!.TryExecuteTask(_task!);
        }
    
        public void Cleanup()
        {
            _scheduler = null;
            _task = null;
        }

        public void Dispose()
        {
            Pool.Return(this);
        }
    }
}