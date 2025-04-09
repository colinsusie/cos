// Written by Colin on 2024-7-28

using DotNetty.Common;
using TaskCompletionSource = DotNetty.Common.Concurrency.TaskCompletionSource;

namespace CoLib.EventLoop;

public class ActionScheduledAsyncTask: ScheduledAsyncTask
{
    private readonly Action _action;

    public ActionScheduledAsyncTask(StEventLoop executor, Action action, PreciseTimeSpan deadline, CancellationToken cancellationToken)
        : base(executor, deadline, new TaskCompletionSource(), cancellationToken)
    {
        _action = action;
    }

    protected override void Execute() => this._action();
}