// Written by Colin on 2024-7-28

using DotNetty.Common;
using TaskCompletionSource = DotNetty.Common.Concurrency.TaskCompletionSource;

namespace CoLib.EventLoop;

public class StateActionScheduledAsyncTask: ScheduledAsyncTask
{
    private readonly Action<object> _action;

    public StateActionScheduledAsyncTask(StEventLoop executor, Action<object> action, object state, PreciseTimeSpan deadline,
        CancellationToken cancellationToken)
        : base(executor, deadline, new TaskCompletionSource(state), cancellationToken)
    {
        _action = action;
    }

    protected override void Execute() => _action(Completion.AsyncState!);
}