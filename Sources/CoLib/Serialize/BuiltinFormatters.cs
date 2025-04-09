// Written by Colin on 2024-10-11

using System.Buffers;
using System.Numerics;
using CoLib.Common;

namespace CoLib.Serialize;

public sealed class DateTimeOffsetFormatter: ICoPackFormatter<DateTimeOffset>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in DateTimeOffset value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteListHeader(2);
        writer.WriteVarInt(value.Ticks);
        writer.WriteVarInt((short)value.Offset.TotalMinutes);
    }

    public DateTimeOffset Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 2)
        {
            CoPackException.ThrowReadUnexpectedLength(len, 2);
        }

        var ticks = reader.ReadInt64();
        var minutes = reader.ReadInt64();
        return new DateTimeOffset(ticks, TimeSpan.FromMinutes(minutes));
    }
}

public sealed class DateTimeFormatter: ICoPackFormatter<DateTime>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in DateTime value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value.ToBinary());
    }

    public DateTime Read(ref CoPackReader reader, object? state)
    {
        return DateTime.FromBinary(reader.ReadInt64());
    }
}

public sealed class TimestampFormatter : ICoPackFormatter<Timestamp>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in Timestamp value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value.ToUnixTimeMilliseconds());
    }

    public Timestamp Read(ref CoPackReader reader, object? state)
    {
        return Timestamp.FromUnixTimeMilliseconds(reader.ReadInt64());
    }
}

public sealed class TimeSpanFormatter: ICoPackFormatter<TimeSpan>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in TimeSpan value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value.Ticks);
    }

    public TimeSpan Read(ref CoPackReader reader, object? state)
    {
        return TimeSpan.FromTicks(reader.ReadInt64());
    }
}

public sealed class Vector2Formatter: ICoPackFormatter<Vector2>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in Vector2 value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteListHeader(2);
        writer.WriteFloat(value.X);
        writer.WriteFloat(value.Y);
    }

    public Vector2 Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 2)
            CoPackException.ThrowReadUnexpectedLength(len, 2);
        return new Vector2(reader.ReadFloat(), reader.ReadFloat());
    }
}

public sealed class Vector3Formatter: ICoPackFormatter<Vector3>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in Vector3 value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteListHeader(3);
        writer.WriteFloat(value.X);
        writer.WriteFloat(value.Y);
        writer.WriteFloat(value.Z);
    }

    public Vector3 Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 3)
            CoPackException.ThrowReadUnexpectedLength(len, 3);
        return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
    }
}

public sealed class Vector4Formatter: ICoPackFormatter<Vector4>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in Vector4 value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteListHeader(4);
        writer.WriteFloat(value.X);
        writer.WriteFloat(value.Y);
        writer.WriteFloat(value.Z);
        writer.WriteFloat(value.W);
    }

    public Vector4 Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 4)
            CoPackException.ThrowReadUnexpectedLength(len, 4);
        return new Vector4(reader.ReadFloat(), reader.ReadFloat(), 
            reader.ReadFloat(), reader.ReadFloat());
    }
}

/// <summary>
/// 表示错误的格式化器
/// </summary>
public sealed class ErrorFormatter<T> : ICoPackFormatter<T>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in T? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        CoPackException.ThrowNotRegisteredInProvider(typeof(T));
    }

    public T Read(ref CoPackReader reader, object? state)
    {
        CoPackException.ThrowNotRegisteredInProvider(typeof(T));
        return default;
    }
}