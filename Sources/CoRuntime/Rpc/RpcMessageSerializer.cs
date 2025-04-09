// Written by Colin on 2024-9-1

using CoLib.Container;
using CoLib.Serialize;

namespace CoRuntime.Rpc;

/// <summary>
/// RPC消息序列化器，参数支持0~8个；参数可以是基本类型，结构，类；结构和类需要加上[MemoryPackable]属性<br/>
/// 只有1个参数且为值类型的序列化效率最高
/// </summary>
public static class RpcMessageSerializer
{
    public static RpcNotifyMessage SerializeNotify0(short serviceId, short methodId)
    {
        return RpcNotifyMessage.Create(serviceId, methodId, null);
    }
    
    public static RpcNotifyMessage SerializeNotify1<T1>(short serviceId, short methodId, T1 a1)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }

    public static RpcNotifyMessage SerializeNotify2<T1, T2>(short serviceId, short methodId, T1 a1, 
        T2 a2)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }
    
    public static RpcNotifyMessage SerializeNotify3<T1, T2, T3>(short serviceId, short methodId, T1 a1, 
        T2 a2, T3 a3)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }
    
    public static RpcNotifyMessage SerializeNotify4<T1, T2, T3, T4>(short serviceId, short methodId, 
        T1 a1, T2 a2, T3 a3, T4 a4)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }
    
    public static RpcNotifyMessage SerializeNotify5<T1, T2, T3, T4, T5>(short serviceId, short methodId, 
        T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }
    
    public static RpcNotifyMessage SerializeNotify6<T1, T2, T3, T4, T5, T6>(short serviceId, 
        short methodId, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        CoPackSerializer.Serialize(buffer, a6);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }
    
    public static RpcNotifyMessage SerializeNotify7<T1, T2, T3, T4, T5, T6, T7>(short serviceId, 
        short methodId, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        CoPackSerializer.Serialize(buffer, a6);
        CoPackSerializer.Serialize(buffer, a7);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }
    
    public static RpcNotifyMessage SerializeNotify8<T1, T2, T3, T4, T5, T6, T7, T8>(
        short serviceId, short methodId, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        CoPackSerializer.Serialize(buffer, a6);
        CoPackSerializer.Serialize(buffer, a7);
        CoPackSerializer.Serialize(buffer, a8);
        return RpcNotifyMessage.Create(serviceId, methodId, buffer);
    }

    public static RpcRequestMessage SerializeRequest0(short serviceId, short methodId, int requestId)
    {
        return RpcRequestMessage.Create(serviceId, methodId, requestId, null);
    }
    
    public static RpcRequestMessage SerializeRequest1<T1>(short serviceId, short methodId, int requestId, T1 a1)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }
    
    public static RpcRequestMessage SerializeRequest2<T1, T2>(short serviceId, short methodId, int requestId,
        T1 a1, T2 a2)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }
    
    public static RpcRequestMessage SerializeRequest3<T1, T2, T3>(short serviceId, short methodId, int requestId,
        T1 a1, T2 a2, T3 a3)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }
    
    public static RpcRequestMessage SerializeRequest4<T1, T2, T3, T4>(short serviceId, short methodId, int requestId,
        T1 a1, T2 a2, T3 a3, T4 a4)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }
    
    public static RpcRequestMessage SerializeRequest5<T1, T2, T3, T4, T5>(short serviceId, short methodId, int requestId,
        T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }
    
    public static RpcRequestMessage SerializeRequest6<T1, T2, T3, T4, T5, T6>(short serviceId, short methodId, 
        int requestId, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        CoPackSerializer.Serialize(buffer, a6);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }
    
    public static RpcRequestMessage SerializeRequest7<T1, T2, T3, T4, T5, T6, T7>(short serviceId, short methodId, 
        int requestId, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        CoPackSerializer.Serialize(buffer, a6);
        CoPackSerializer.Serialize(buffer, a7);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }

    public static RpcRequestMessage SerializeRequest8<T1, T2, T3, T4, T5, T6, T7, T8>(short serviceId,
        short methodId, int requestId, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a1);
        CoPackSerializer.Serialize(buffer, a2);
        CoPackSerializer.Serialize(buffer, a3);
        CoPackSerializer.Serialize(buffer, a4);
        CoPackSerializer.Serialize(buffer, a5);
        CoPackSerializer.Serialize(buffer, a6);
        CoPackSerializer.Serialize(buffer, a7);
        CoPackSerializer.Serialize(buffer, a8);
        return RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
    }
    
    public static T1? Deserialize1<T1>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        var a1 = CoPackSerializer.Deserialize<T1>(msg.Buffer.WrittenSpan);
        return a1;
    }
    
    public static (T1?, T2?) Deserialize2<T1, T2>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        ReadOnlySpan<byte> span = msg.Buffer.WrittenSpan;
        var consumed = CoPackSerializer.Deserialize<T1>(span, out var a1);
        span = span[consumed..];
        CoPackSerializer.Deserialize<T2>(span, out var a2);

        return (a1, a2);
    }
    
    public static (T1?, T2?, T3?) Deserialize3<T1, T2, T3>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        ReadOnlySpan<byte> span = msg.Buffer.WrittenSpan;
        var consumed = CoPackSerializer.Deserialize<T1>(span, out var a1);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T2>(span, out var a2);
        span = span[consumed..];
        CoPackSerializer.Deserialize<T3>(span, out var a3);

        return (a1, a2, a3);
    }
    
    public static (T1?, T2?, T3?, T4?) Deserialize4<T1, T2, T3, T4>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        ReadOnlySpan<byte> span = msg.Buffer.WrittenSpan;
        var consumed = CoPackSerializer.Deserialize<T1>(span, out var a1);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T2>(span, out var a2);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T3>(span, out var a3);
        span = span[consumed..];
        CoPackSerializer.Deserialize<T4>(span, out var a4);

        return (a1, a2, a3, a4);
    }
    
    public static (T1?, T2?, T3?, T4?, T5?) Deserialize4<T1, T2, T3, T4, T5>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        ReadOnlySpan<byte> span = msg.Buffer.WrittenSpan;
        var consumed = CoPackSerializer.Deserialize<T1>(span, out var a1);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T2>(span, out var a2);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T3>(span, out var a3);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T4>(span, out var a4);
        span = span[consumed..];
        CoPackSerializer.Deserialize<T5>(span, out var a5);

        return (a1, a2, a3, a4, a5);
    }
    
    public static (T1?, T2?, T3?, T4?, T5?, T6?) Deserialize4<T1, T2, T3, T4, T5, T6>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        ReadOnlySpan<byte> span = msg.Buffer.WrittenSpan;
        var consumed = CoPackSerializer.Deserialize<T1>(span, out var a1);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T2>(span, out var a2);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T3>(span, out var a3);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T4>(span, out var a4);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T5>(span, out var a5);
        span = span[consumed..];
        CoPackSerializer.Deserialize<T6>(span, out var a6);

        return (a1, a2, a3, a4, a5, a6);
    }
    
    public static (T1?, T2?, T3?, T4?, T5?, T6?, T7?) Deserialize4<T1, T2, T3, T4, T5, T6,T7>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        ReadOnlySpan<byte> span = msg.Buffer.WrittenSpan;
        var consumed = CoPackSerializer.Deserialize<T1>(span, out var a1);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T2>(span, out var a2);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T3>(span, out var a3);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T4>(span, out var a4);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T5>(span, out var a5);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T6>(span, out var a6);
        span = span[consumed..];
        CoPackSerializer.Deserialize<T7>(span, out var a7);

        return (a1, a2, a3, a4, a5, a6, a7);
    }
    
    public static (T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?) Deserialize4<T1, T2, T3, T4, T5, T6,T7, T8>(RpcMessage msg)
    {
        if (msg.Buffer == null) throw new RpcException($"RpcMessage buffer is null, msg:{msg}");

        ReadOnlySpan<byte> span = msg.Buffer.WrittenSpan;
        var consumed = CoPackSerializer.Deserialize<T1>(span, out var a1);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T2>(span, out var a2);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T3>(span, out var a3);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T4>(span, out var a4);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T5>(span, out var a5);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T6>(span, out var a6);
        span = span[consumed..];
        consumed = CoPackSerializer.Deserialize<T7>(span, out var a7);
        span = span[consumed..];
        CoPackSerializer.Deserialize<T8>(span, out var a8);

        return (a1, a2, a3, a4, a5, a6, a7, a8);
    }

    public static RpcResponseMessage SerializeResponse<T>(int requestId, T a)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, a);
        return RpcResponseMessage.Create(requestId, buffer);
    }
    
    public static RpcExceptionMessage SerializeException(int requestId, string message)
    {
        var buffer = PooledByteBufferWriter.Create();
        CoPackSerializer.Serialize(buffer, message);
        return RpcExceptionMessage.Create(requestId, buffer);
    }
}