// Written by Colin on 2024-7-28

using System.Runtime.CompilerServices;
using DotNetty.Common;
using DotNetty.Common.Concurrency;
using TaskCompletionSource = DotNetty.Common.Concurrency.TaskCompletionSource;

namespace CoLib.EventLoop;

public abstract class ScheduledAsyncTask : IScheduledRunnable
{
    const int CancellationProhibited = 1;
    const int CancellationRequested = 1 << 1;

    protected readonly TaskCompletionSource Promise;
    protected readonly StEventLoop Executor;
    private int _volatileCancellationState;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenRegistration _cancellationTokenRegistration;
    
    protected ScheduledAsyncTask(StEventLoop executor, PreciseTimeSpan deadline, 
        TaskCompletionSource promise, CancellationToken cancellationToken)
    {
        Executor = executor;
        Promise = promise;
        Deadline = deadline;
        _cancellationToken = cancellationToken;
        _cancellationTokenRegistration = cancellationToken.Register(s => ((ScheduledAsyncTask)s!).Cancel(), this);
    }

    public PreciseTimeSpan Deadline { get; }

    public bool Cancel()
    {
        if (!AtomicCancellationStateUpdate(CancellationRequested, CancellationProhibited))
        {
            return false;
        }

        bool canceled = Promise.TrySetCanceled();
        if (canceled)
        {
            Executor.RemoveScheduled(this);
        }

        return canceled;
    }

    public Task Completion => Promise.Task;

    public TaskAwaiter GetAwaiter() => Completion.GetAwaiter();

    int IComparable<IScheduledRunnable>.CompareTo(IScheduledRunnable? other)
    {
        return other == null ? 1 : Deadline.CompareTo(other.Deadline);
    }

    public virtual void Run()
    {
        _cancellationTokenRegistration.Dispose();
        if (_cancellationToken.IsCancellationRequested)
        {
            Promise.TrySetCanceled();
            return;
        }
        
        if (TrySetUncancelable())
        {
            try
            {
                Execute();
                Promise.TryComplete();
            }
            catch (Exception ex)
            {
                // todo: check for fatal
                Promise.TrySetException(ex);
            }
        }
    }

    protected abstract void Execute();

    bool TrySetUncancelable() => AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested);

    bool AtomicCancellationStateUpdate(int newBits, int illegalBits)
    {
        int cancellationState = Volatile.Read(ref _volatileCancellationState);
        int oldCancellationState;
        do
        {
            oldCancellationState = cancellationState;
            if ((cancellationState & illegalBits) != 0)
            {
                return false;
            }

            cancellationState = Interlocked.CompareExchange(ref _volatileCancellationState,
                cancellationState | newBits, cancellationState);
        } while (cancellationState != oldCancellationState);

        return true;
    }
}