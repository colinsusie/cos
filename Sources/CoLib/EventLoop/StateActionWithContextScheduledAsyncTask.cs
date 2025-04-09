// Written by Colin on 2024-7-28

using DotNetty.Common;
using TaskCompletionSource = DotNetty.Common.Concurrency.TaskCompletionSource;

namespace CoLib.EventLoop;

public class StateActionWithContextScheduledAsyncTask: ScheduledAsyncTask
{
    private readonly Action<object, object> _action;
    private readonly object _context;

    public StateActionWithContextScheduledAsyncTask(StEventLoop executor, Action<object, object> action, object context, object state,
        PreciseTimeSpan deadline, CancellationToken cancellationToken)
        : base(executor, deadline, new TaskCompletionSource(state), cancellationToken)
    {
        _action = action;
        _context = context;
    }

    protected override void Execute() => _action(_context, Completion.AsyncState!);
}