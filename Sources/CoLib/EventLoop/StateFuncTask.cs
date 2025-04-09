// Written by Colin on 2024-7-27

using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

public class StateFuncTask<T>: IRunnable
{
    readonly TaskCompletionSource<T> _promise;
    readonly CancellationToken _cancellationToken;
    readonly Func<object, T> _func;
    
    public static StateFuncTask<T> Create(Func<object, T> func, object state, CancellationToken cancellationToken)
    {
        return new StateFuncTask<T>(func, state, cancellationToken);
    }
    
    private StateFuncTask(Func<object, T> func, object state, CancellationToken cancellationToken)
    {
        _promise = new TaskCompletionSource<T>(state);
        _cancellationToken = cancellationToken;
        _func = func;
    }

    public Task<T> Completion => _promise.Task;

    public void Run()
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            _promise.TrySetCanceled();
            return;
        }

        try
        {
            var result = _func(Completion.AsyncState!);
            _promise.TrySetResult(result);
        }
        catch (Exception ex)
        {
            _promise.TrySetException(ex);
        }
    }
}