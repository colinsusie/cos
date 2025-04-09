// Written by Colin on 2024-8-4

using CoLib.Container;
using CoLib.ObjectPools;

namespace CoRuntime.Rpc;

/**
 * Rpc消息格式：
 *  - Length不包含自身
 *  - 小端字节序
 *
 *  |--Length(4B)--|---RmtNotify(1B)---|---ServiceId(2B)---|---MethodId(2B)---|---Content(NB)---|
 *
 *  |--Length(4B)--|--RmtRequest(1B)--|---ServiceId(2B)---|---MethodId(2B)---|---RequestId(4B)---|---Content(NB)---|
 *
 *  |--Length(4B)--|---RmtResponse(1B)---|---RequestId(4B)---|---Content(NB)---|
 *
 *  |--Length(4B)--|---RmtException(1B)---|---RequestId(4B)---|---Content(NB)---|
 */

/// <summary>
/// Rpc消息类型
/// </summary>
public enum RpcMessageType : byte
{
    RmtNotify = 0,
    RmtRequest = 1,
    RmtResponse = 2,
    RmtException = 3,
}

/// <summary>
/// Rpc消息基类
/// </summary>
public abstract class RpcMessage: IDisposable, ICleanable
{
    public PooledByteBufferWriter? Buffer { get; private set; }

    protected void Initialize(PooledByteBufferWriter? buffer)
    {
        Buffer = buffer;
    }

    public abstract void Dispose();

    public virtual void Cleanup()
    {
        Buffer?.Dispose();
        Buffer = null;
    }
}

/// <summary>
/// 代表一个Rpc通知消息
/// </summary>
public class RpcNotifyMessage: RpcMessage
{
    public const int HeaderLen = 5;
    
    public short ServiceId { get; private set; }
    public short MethodId { get; private set; }
    
    [ThreadStatic] private static StObjectPool<RpcNotifyMessage>? _pool;
    private static StObjectPool<RpcNotifyMessage> Pool =>
        _pool ??= new StObjectPool<RpcNotifyMessage>(256, () => new RpcNotifyMessage());

    public static RpcNotifyMessage Create(short serviceId, short methodId, PooledByteBufferWriter? buffer)
    {
        var msg =  Pool.Get();
        msg.Initialize(serviceId, methodId, buffer);
        return msg;
    }
    
    private RpcNotifyMessage()
    {
    }

    private void Initialize(short serviceId, short methodId, PooledByteBufferWriter? buffer)
    {
        Initialize(buffer);
        ServiceId = serviceId;
        MethodId = methodId;
    }
    
    public override void Dispose()
    {
        Pool.Return(this);
    }

    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(RpcNotifyMessage)}:[S:{ServiceId},M:{MethodId},L:{len}]";
    }
}

/// <summary>
/// 代表一个Rpc请求消息
/// </summary>
public class RpcRequestMessage: RpcMessage
{
    public const int HeaderLen = 9;
    
    public short ServiceId {get; private set;}
    public short MethodId {get; private set;}
    public int RequestId {get; private set;}
    // 暂存
    public IRpcResponse? Response;
    
    [ThreadStatic] private static StObjectPool<RpcRequestMessage>? _pool;
    private static StObjectPool<RpcRequestMessage> Pool =>
        _pool ??= new StObjectPool<RpcRequestMessage>(256, () => new RpcRequestMessage());
    
    public static RpcRequestMessage Create(short serviceId, short methodId, int requestId, PooledByteBufferWriter? buffer)
    {
        var msg = Pool.Get();
        msg.Initialize(serviceId, methodId, requestId, buffer);
        return msg;
    }

    private RpcRequestMessage()
    {
    }

    private void Initialize(short serviceId, short methodId, int requestId, PooledByteBufferWriter? buffer)
    {
        Initialize(buffer);
        ServiceId = serviceId;
        MethodId = methodId;
        RequestId = requestId;
    }

    public override void Dispose()
    {
        Pool.Return(this);
    }
    
    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(RpcRequestMessage)}:[S:{ServiceId},M:{MethodId},R:{RequestId},L:{len}]";
    }

    public override void Cleanup()
    {
        base.Cleanup();
        Response = null;
    }
}

/// <summary>
/// 代表一个Rpc响应消息
/// </summary>
public class RpcResponseMessage: RpcMessage
{
    public const int HeaderLen = 5;
    
    public int RequestId {get; private set;}
    
    [ThreadStatic] private static StObjectPool<RpcResponseMessage>? _pool;
    private static StObjectPool<RpcResponseMessage> Pool =>
        _pool ??= new StObjectPool<RpcResponseMessage>(256, () => new RpcResponseMessage());
    
    public static RpcResponseMessage Create(int requestId, PooledByteBufferWriter? buffer)
    {
        var msg = Pool.Get();
        msg.Initialize(requestId, buffer);
        return msg;
    }

    private RpcResponseMessage()
    {
    }

    private void Initialize(int requestId, PooledByteBufferWriter? buffer)
    {
        Initialize(buffer);
        RequestId = requestId;
    }

    public override void Dispose()
    {
        Pool.Return(this);
    }
    
    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(RpcResponseMessage)}:[R:{RequestId},L:{len}]";
    }
}

/// <summary>
/// 代表一个Rpc异常消息
/// </summary>
public class RpcExceptionMessage: RpcMessage
{
    public const int HeaderLen = 5;
    
    public int RequestId {get; private set;}
    
    [ThreadStatic] private static StObjectPool<RpcExceptionMessage>? _pool;
    private static StObjectPool<RpcExceptionMessage> Pool =>
        _pool ??= new StObjectPool<RpcExceptionMessage>(128, () => new RpcExceptionMessage());
    
    public static RpcExceptionMessage Create(int requestId, PooledByteBufferWriter? buffer)
    {
        var msg = Pool.Get();
        msg.Initialize(requestId, buffer);
        return msg;
    }

    private RpcExceptionMessage()
    {
    }

    private void Initialize(int requestId, PooledByteBufferWriter? buffer)
    {
        Initialize(buffer);
        RequestId = requestId;
    }

    public override void Dispose()
    {
        Pool.Return(this);
    }
    
    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(RpcExceptionMessage)}:[R:{RequestId},L:{len}]";
    }
}