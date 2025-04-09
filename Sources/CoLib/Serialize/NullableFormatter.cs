// Written by Colin on 2024-10-10

using System.Buffers;

namespace CoLib.Serialize;

public sealed class NullableFormatter<T>: ICoPackFormatter<T?> where T: struct
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in T? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!value.HasValue)
            writer.WriteNull();
        else
            writer.WriteValue(value.Value, flags);
    }

    public T? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;
        return reader.ReadValue<T>(state);
    }
}