// Written by Colin on 2024-7-27

using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

public class StateFuncWithContextTask<T>: IRunnable
{
    readonly TaskCompletionSource<T> _promise;
    readonly CancellationToken _cancellationToken;
    readonly Func<object, object, T> _func;
    readonly object _context;
    
    public static StateFuncWithContextTask<T> Create(Func<object, object, T> func, object context, object state, 
        CancellationToken cancellationToken)
    {
        return new StateFuncWithContextTask<T>(func, context, state, cancellationToken);
    }
    
    private StateFuncWithContextTask(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken)
    {
        _promise = new TaskCompletionSource<T>(state);
        _cancellationToken = cancellationToken;
        _context = context;
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
            var result = _func(_context, Completion.AsyncState!);
            _promise.TrySetResult(result);
        }
        catch (Exception ex)
        {
            _promise.TrySetException(ex);
        }
    }
}