// Written by Colin on 2024-9-21

using System.Threading.Tasks.Sources;
using CoLib.Extensions;
using CoLib.ObjectPools;

namespace CoRuntime.Rpc;

public class RpcResponse<T>: IRpcResponse, ICleanable, IValueTaskSource<T>
{
    [ThreadStatic] private static StObjectPool<RpcResponse<T>>? _pool;
    private static StObjectPool<RpcResponse<T>> Pool =>
        _pool ??= new StObjectPool<RpcResponse<T>>(64, () => new RpcResponse<T>());
    
    private ManualResetValueTaskSourceCore<T> _source;
    private CancellationTokenRegistration _cancellationRegistration;
    public TimeSpan StartTime { get; private set; }

    public static RpcResponse<T> Create(CancellationToken token)
    {
        return Pool.Get().Initialize(token);
    }

    private RpcResponse()
    {
    }

    private RpcResponse<T> Initialize(CancellationToken token)
    {
        StartTime = TimeSpanExt.FromStart();
        _source.RunContinuationsAsynchronously = true;
        _cancellationRegistration = token.UnsafeRegister(static state =>
        {
            var response = (RpcResponse)state!;
            // TODO: 要提供Try版本，并且是原子的，不然会有并发问题
            response.SetException(new TaskCanceledException());
        }, this);
        return this;
    }
    
    public ValueTask<T> ValueTask => new(this, _source.Version);

    public void Cleanup()
    {
        _cancellationRegistration.Dispose();
        _source.Reset();
    }
    
    public T GetResult(short token)
    {
        var isValid = token == _source.Version;
        try
        {
            return _source.GetResult(token);
        }
        finally
        {
            if (isValid) Pool.Return(this);    
        }
    }
    
    public ValueTaskSourceStatus GetStatus(short token) 
        => _source.GetStatus(token);

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        => _source.OnCompleted(continuation, state, token, flags);

    // 设置结果
    public void SetResult(RpcResponseMessage msg)
    {
        try
        {
            var resp = RpcMessageSerializer.Deserialize1<T>(msg);
            _source.SetResult(resp!);
        }
        catch (Exception e)
        {
            _source.SetException(e);
        }
    }

    // 设置异常
    public void SetException(Exception exception)
    {
        _source.SetException(exception);
    }
}

public class RpcResponse: IRpcResponse, ICleanable, IValueTaskSource
{
    [ThreadStatic] private static StObjectPool<RpcResponse>? _pool;
    private static StObjectPool<RpcResponse> Pool =>
        _pool ??= new StObjectPool<RpcResponse>(128, () => new RpcResponse());

    private ManualResetValueTaskSourceCore<bool> _source;
    private CancellationTokenRegistration _cancellationRegistration;
    public TimeSpan StartTime { get; private set; }
    

    public static RpcResponse Create(CancellationToken token)
    {
        return Pool.Get().Initialize(token);
    }

    private RpcResponse()
    {
    }

    private RpcResponse Initialize(CancellationToken token)
    {
        StartTime = TimeSpanExt.FromStart();
        _source.RunContinuationsAsynchronously = true;
        _cancellationRegistration = token.UnsafeRegister(static state =>
        {
            var response = (RpcResponse)state!;
            response.SetException(new TaskCanceledException());
        }, this);
        return this;
    }

    public ValueTask ValueTask => new(this, _source.Version);

    public void Cleanup()
    {
        _cancellationRegistration.Dispose();
        _source.Reset();
    }
    
    public void GetResult(short token)
    {
        var isValid = token == _source.Version;
        try
        {
            _source.GetResult(token);
        }
        finally
        {
            if (isValid) Pool.Return(this);    
        }
    }
    
    public ValueTaskSourceStatus GetStatus(short token)
        => _source.GetStatus(token);
    
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        => _source.OnCompleted(continuation, state, token, flags);

    // 设置结果
    public void SetResult(RpcResponseMessage msg)
    {
        _source.SetResult(true);
    }

    // 设置异常
    public void SetException(Exception exception)
    {
        _source.SetException(exception);
    }
}