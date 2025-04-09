// Written by Colin on ${2023}-12-17

using System.Threading.Tasks.Sources;
using CoLib.ObjectPools;

namespace CoLib.Message;

/// <summary>
/// 执行带状态的Action的消息
/// </summary>
internal class AsyncFuncMessage<TArgs, TResult>: IMessage, ICleanable, IValueTaskSource<TResult>
{
    private ManualResetValueTaskSourceCore<TResult> _source;
    private Func<TArgs, TResult>? _func;
    private TArgs _args = default!;
    
    [ThreadStatic] private static StObjectPool<AsyncFuncMessage<TArgs, TResult>>? _pool;
    private static StObjectPool<AsyncFuncMessage<TArgs, TResult>> Pool => _pool ??= new (256, () => new ());
    
    public static AsyncFuncMessage<TArgs, TResult> Create(Func<TArgs, TResult> func, in TArgs args)
    {
        var message = Pool.Get();
        message.Initialize(func, args);
        return message;
    }

    private AsyncFuncMessage()
    {
    }

    private void Initialize(Func<TArgs, TResult> func, in TArgs args)
    {
        _func = func;
        _args = args;
        _source.RunContinuationsAsynchronously = true;
    }
    
    public void Cleanup()
    {
        _source.Reset();
        _func = null;
        _args = default!;
    }
    
    public void Dispose()
    {
    }

    public ValueTask<TResult> ValueTask => new(this, _source.Version);

    public void Process()
    {
        try
        {
            var result = _func!.Invoke(_args);
            _source.SetResult(result);
        }
        catch (Exception e)
        {
            _source.SetException(e);
        }
    }

    public TResult GetResult(short token)
    {
        var result = _source.GetResult(token);
        Pool.Return(this);
        return result;
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _source.GetStatus(token);
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _source.OnCompleted(continuation, state, token, flags);
    }
}