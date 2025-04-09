// Written by Colin on 2024-10-10

using System.Buffers;

namespace CoLib.Serialize;

/// <summary>
/// 格式化器
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICoPackFormatter<T>
{
    /// <summary>
    /// 序列化value
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="flags">用于决定对象的字段是否序列化</param>
    /// <typeparam name="TBufferWriter"></typeparam>
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in T? value, PackFlags flags)
        where TBufferWriter : IBufferWriter<byte>;

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="state">对象需要实现ICoPackCreate接口，然后可以传state给对象作初始化用</param>
    /// <returns></returns>
    public T? Read(ref CoPackReader reader, object? state);
}