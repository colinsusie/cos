namespace CoLib.Message;

/// <summary>
/// 消息循环中的调度器
/// </summary>
internal class MessageScheduler: TaskScheduler
{
    private readonly MessageLoop _messageLoop;

    public MessageScheduler(MessageLoop loop)
    {
        _messageLoop = loop;
    }

    internal new bool TryExecuteTask(Task task)
    {
        return base.TryExecuteTask(task);
    }

    protected override void QueueTask(Task task)
    {
        if (task.AsyncState is MessageLoopBox || _messageLoop.IsStopped)
        {
            // Factory.StartNew调过来的直接执行
            TryExecuteTask(task);
        }
        else
        {
            // 投递到消息队列
            _messageLoop.Post(TaskMessage.Create(this, task));
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
    protected override IEnumerable<Task>? GetScheduledTasks() => null;
    protected override bool TryDequeue(Task task) => false;
}