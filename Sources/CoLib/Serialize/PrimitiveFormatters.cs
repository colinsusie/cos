// Written by Colin on 2024-10-10

using System.Buffers;

namespace CoLib.Serialize;

public sealed class Int8Formatter : ICoPackFormatter<sbyte>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in sbyte value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public sbyte Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadInt8();
    }
}

public sealed class UInt8Formatter : ICoPackFormatter<byte>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in byte value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public byte Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadUInt8();
    }
}

public sealed class Int16Formatter : ICoPackFormatter<short>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in short value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public short Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadInt16();
    }
}

public sealed class UInt16Formatter : ICoPackFormatter<ushort>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ushort value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public ushort Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadUInt16();
    }
}

public sealed class Int32Formatter : ICoPackFormatter<int>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in int value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public int Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadInt32();
    }
}

public sealed class UInt32Formatter : ICoPackFormatter<uint>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in uint value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public uint Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadUInt32();
    }
}

public sealed class Int64Formatter : ICoPackFormatter<long>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in long value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public long Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadInt64();
    }
}

public sealed class UInt64Formatter : ICoPackFormatter<ulong>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ulong value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarInt(value);
    }

    public ulong Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadUInt64();
    }
}

public sealed class StringFormatter : ICoPackFormatter<string>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in string? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteString(value);
    }

    public string? Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadString();
    }
}

public sealed class BytesFormatter : ICoPackFormatter<byte[]>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in byte[]? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBytes(value);
    }

    public byte[]? Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadBytes();
    }
}

public sealed class BoolFormatter : ICoPackFormatter<bool>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in bool value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBool(value);
    }

    public bool Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadBool();
    }
}

public sealed class FloatFormatter : ICoPackFormatter<float>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in float value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteFloat(value);
    }

    public float Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadFloat();
    }
}

public sealed class DoubleFormatter : ICoPackFormatter<double>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in double value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteFloat(value);
    }

    public double Read(ref CoPackReader reader, object? state)
    {
        return reader.ReadDouble();
    }
}

