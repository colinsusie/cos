// Written by Colin on 2024-10-14

using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using CoLib.Container;

namespace CoLib.Serialize;

/// <summary>
/// 序列化器
/// </summary>
public static class CoPackSerializer
{
    /// 序列化一个对象
    public static void Serialize<T, TBufferWriter>(in TBufferWriter bufferWriter, in T? value, PackFlags flags = PackFlags.All) 
        where TBufferWriter : IBufferWriter<byte>
    {
        var writer = new CoPackWriter<TBufferWriter>(ref Unsafe.AsRef(in bufferWriter));
        writer.WriteValue(value, flags);
    }

    /// 反序列化出一个对象，state是自定义值，传给对象的构造函数
    public static T? Deserialize<T>(ReadOnlySpan<byte> buffer, object? state = null)
    {
        var reader = new CoPackReader(buffer);
        return reader.ReadValue<T>(state);
    }
    
    /// 反序列化出一个对象，state是自定义值，传给对象的构造函数
    /// 返回buffer消耗的大小
    public static int Deserialize<T>(ReadOnlySpan<byte> buffer, out T? value, object? state = null)
    {
        var reader = new CoPackReader(buffer);
        value = reader.ReadValue<T>(state);
        return reader.Consumed;
    }
    
    /// 将对象序列化为等价的json字符串
    public static string ToJson<T>(in T? value, PackFlags flags = PackFlags.All)
    {
        using var bufferWriter = new PooledByteBufferWriter();
        Serialize(bufferWriter, value, flags);
        return ToJson(bufferWriter.WrittenSpan);
    }

    /// 将CoPack二进制序列化为等价的json字符串
    public static string ToJson(ReadOnlySpan<byte> buffer)
    {
        return CoPackConverter.ToJson(buffer);
    }
}