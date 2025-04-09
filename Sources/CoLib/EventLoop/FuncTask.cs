// Written by Colin on 2024-7-27

using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

public class FuncTask<T>: IRunnable
{
    readonly TaskCompletionSource<T> _promise;
    readonly CancellationToken _cancellationToken;
    readonly Func<T> _func;

    public static FuncTask<T> Create(Func<T> func, CancellationToken cancellationToken)
    {
        return new FuncTask<T>(func, cancellationToken);
    }
    
    private FuncTask(Func<T> func, CancellationToken cancellationToken)
    {
        _promise = new TaskCompletionSource<T>();
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
            var result = _func();
            _promise.TrySetResult(result);
        }
        catch (Exception ex)
        {
            _promise.TrySetException(ex);
        }
    }
}