// Written by Colin on 2024-11-27

using CoLib.Container;
using CoLib.ObjectPools;

namespace CoRuntime.Gate;

/**
 * 网关消息格式：
 *  - Length不包含自身
 *  - 小端字节序
 *
 *  |--Length(4B)--|---RmtNotify(1B)---|---Flags(1B)---|---MessageId(2B)---|---Content(NB)---|
 *
 *  |--Length(4B)--|--RmtRequest(1B)--|---Flags(1B)---|---MessageId(2B)---|---RequestId(4B)---|---Content(NB)---|
 *
 *  |--Length(4B)--|---RmtResponse(1B)---|---Flags(1B)---|---RequestId(4B)---|---Content(NB)---|
 *
 *  |--Length(4B)--|---RmtException(1B)---|---Flags(1B)---|---RequestId(4B)---|---Content(NB)---|
 */

/// <summary>
/// Gate消息类型
/// </summary>
public enum GateMessageType : byte
{
    GmtNotify = 0,
    GmtRequest = 1,
    GmtResponse = 2,
    GmtException = 3,
}

/// <summary>
/// Gate消息标志位
/// </summary>
public enum GateMessageFlags : byte
{
    
}

/// <summary>
/// Gate消息基类
/// </summary>
public abstract class GateMessage: IDisposable, ICleanable
{
    public PooledByteBufferWriter? Buffer { get; private set; }
    public GateMessageFlags Flags { get; private set; }

    protected void Initialize(GateMessageFlags flags, PooledByteBufferWriter? buffer)
    {
        Buffer = buffer;
        Flags = flags;
    }

    public abstract void Dispose();

    public virtual void Cleanup()
    {
        Buffer?.Dispose();
        Buffer = null;
    }
}

/// <summary>
/// 代表一个Gate通知消息
/// </summary>
public class GateNotifyMessage: GateMessage
{
    public const int HeaderLen = 4;
    
    public short MessageId { get; private set; }
    
    [ThreadStatic] private static StObjectPool<GateNotifyMessage>? _pool;
    private static StObjectPool<GateNotifyMessage> Pool =>
        _pool ??= new StObjectPool<GateNotifyMessage>(256, () => new GateNotifyMessage());

    public static GateNotifyMessage Create(GateMessageFlags flags, short methodId, PooledByteBufferWriter? buffer)
    {
        var msg =  Pool.Get();
        msg.Initialize(flags, methodId, buffer);
        return msg;
    }
    
    private GateNotifyMessage()
    {
    }

    private void Initialize(GateMessageFlags flags, short methodId, PooledByteBufferWriter? buffer)
    {
        Initialize(flags, buffer);
        MessageId = methodId;
    }
    
    public override void Dispose()
    {
        Pool.Return(this);
    }

    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(GateNotifyMessage)}:[M:{MessageId},L:{len}]";
    }
}

/// <summary>
/// 代表一个Gate请求消息
/// </summary>
public class GateRequestMessage: GateMessage
{
    public const int HeaderLen = 8;
    
    public short MessageId {get; private set;}
    public int RequestId {get; private set;}
    // // 暂存
    // public IGateResponse? Response;
    
    [ThreadStatic] private static StObjectPool<GateRequestMessage>? _pool;
    private static StObjectPool<GateRequestMessage> Pool =>
        _pool ??= new StObjectPool<GateRequestMessage>(256, () => new GateRequestMessage());
    
    public static GateRequestMessage Create(GateMessageFlags flags, short methodId, int requestId, PooledByteBufferWriter? buffer)
    {
        var msg = Pool.Get();
        msg.Initialize(flags, methodId, requestId, buffer);
        return msg;
    }

    private GateRequestMessage()
    {
    }

    private void Initialize(GateMessageFlags flags, short methodId, int requestId, PooledByteBufferWriter? buffer)
    {
        Initialize(flags, buffer);
        MessageId = methodId;
        RequestId = requestId;
    }

    public override void Dispose()
    {
        Pool.Return(this);
    }
    
    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(GateRequestMessage)}:[M:{MessageId},R:{RequestId},L:{len}]";
    }

    public override void Cleanup()
    {
        base.Cleanup();
        // Response = null;
    }
}

/// <summary>
/// 代表一个Gate响应消息
/// </summary>
public class GateResponseMessage: GateMessage
{
    public const int HeaderLen = 6;
    
    public int RequestId {get; private set;}
    
    [ThreadStatic] private static StObjectPool<GateResponseMessage>? _pool;
    private static StObjectPool<GateResponseMessage> Pool =>
        _pool ??= new StObjectPool<GateResponseMessage>(256, () => new GateResponseMessage());
    
    public static GateResponseMessage Create(GateMessageFlags flags, int requestId, PooledByteBufferWriter? buffer)
    {
        var msg = Pool.Get();
        msg.Initialize(flags, requestId, buffer);
        return msg;
    }

    private GateResponseMessage()
    {
    }

    private void Initialize(GateMessageFlags flags, int requestId, PooledByteBufferWriter? buffer)
    {
        Initialize(flags, buffer);
        RequestId = requestId;
    }

    public override void Dispose()
    {
        Pool.Return(this);
    }
    
    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(GateResponseMessage)}:[R:{RequestId},L:{len}]";
    }
}

/// <summary>
/// 代表一个Gate异常消息
/// </summary>
public class GateExceptionMessage: GateMessage
{
    public const int HeaderLen = 5;
    
    public int RequestId {get; private set;}
    
    [ThreadStatic] private static StObjectPool<GateExceptionMessage>? _pool;
    private static StObjectPool<GateExceptionMessage> Pool =>
        _pool ??= new StObjectPool<GateExceptionMessage>(128, () => new GateExceptionMessage());
    
    public static GateExceptionMessage Create(GateMessageFlags flags, int requestId, PooledByteBufferWriter? buffer)
    {
        var msg = Pool.Get();
        msg.Initialize(flags, requestId, buffer);
        return msg;
    }

    private GateExceptionMessage()
    {
    }

    private void Initialize(GateMessageFlags flags, int requestId, PooledByteBufferWriter? buffer)
    {
        Initialize(flags, buffer);
        RequestId = requestId;
    }

    public override void Dispose()
    {
        Pool.Return(this);
    }
    
    public override string ToString()
    {
        var len = Buffer != null ? Buffer.WrittenMemory.Length : 0;
        return $"{nameof(GateExceptionMessage)}:[R:{RequestId},L:{len}]";
    }
}