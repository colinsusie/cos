// Written by Colin on 2023-12-15

using CoLib.ObjectPools;

namespace CoLib.Message;

/// <summary>
/// Task类消息，用于把Task的执行调度到当前线程执行
/// </summary>
internal class TaskMessage: IMessage, ICleanable
{
    private MessageScheduler? _scheduler;
    private Task? _task;

    [ThreadStatic] private static StObjectPool<TaskMessage>? _pool;
    private static StObjectPool<TaskMessage> Pool => _pool ??= new StObjectPool<TaskMessage>(128, () => new TaskMessage());
    public static TaskMessage Create(MessageScheduler scheduler, Task task)
    {
        var message = Pool.Get();
        message.Initialize(scheduler, task);
        return message;
    }
    
    private void Initialize(MessageScheduler scheduler, Task task)
    {
        _scheduler = scheduler;
        _task = task;
    }
    
    public void Process()
    {
        _scheduler!.TryExecuteTask(_task!);
    }

    public void Dispose()
    {
        Pool.Return(this);
    }
    
    public void Cleanup()
    {
        _scheduler = null;
        _task = null;
    }
}