// Written by Colin on 2024-10-12

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CoLib.Serialize;

public ref struct CoPackReader
{
    private ReadOnlySpan<byte> _bufferSpan;
    private readonly int _totalSize;
    public int Consumed => _totalSize - _bufferSpan.Length;
    public int Remaining => _bufferSpan.Length;

    public CoPackReader(in ReadOnlySpan<byte> buffer)
    {
        _bufferSpan = buffer;
        _totalSize = _bufferSpan.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> PrepareBuffer(int count)
    {
        if (count > _bufferSpan.Length)
        {
            CoPackException.ThrowReadReachedEnd();
        }

        var result = _bufferSpan.Slice(0, count);
        _bufferSpan = _bufferSpan.Slice(count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Skip(int count)
    {
        if (count > _bufferSpan.Length)
        {
            CoPackException.ThrowReadReachedEnd();
        }

        _bufferSpan = _bufferSpan.Slice(count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (CoPackType, byte) ReadHeader()
    {
        var span = PrepareBuffer(1);
        var header = MemoryMarshal.GetReference(span);
        var type = (CoPackType) (header >> CoPackCode.CookieBits);
        var cookie = (byte)(header & CoPackCode.CookieMask);
        return (type, cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (CoPackType, byte) PeekHeader()
    {
        if (1 > _bufferSpan.Length)
            CoPackException.ThrowReadReachedEnd();

        var header = MemoryMarshal.GetReference(_bufferSpan);
        var type = (CoPackType) (header >> CoPackCode.CookieBits);
        var cookie = (byte)(header & CoPackCode.CookieMask);
        return (type, cookie);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.NullBool)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.NullBool);

        switch (cookie)
        {
            case (byte)CookieNullBoolType.False:
                return false;
            case (byte)CookieNullBoolType.True:
                return true;
            default:
                CoPackException.ThrowReadUnexpectedBool(cookie);
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Float)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Float);
        
        switch (cookie)
        {
            case 0:
                return 0;
            case (byte)CookieFloatType.Float:
                return BinaryPrimitives.ReadSingleLittleEndian(PrepareBuffer(4));
            default:
                CoPackException.ThrowReadUnexpectedFloat(cookie, CookieFloatType.Float);
                return 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Float)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Float);
        
        switch (cookie)
        {
            case 0:
                return 0;
            case (byte)CookieFloatType.Float:  // double 兼容float
                return BinaryPrimitives.ReadSingleLittleEndian(PrepareBuffer(4));
            case (byte)CookieFloatType.Double:
                return BinaryPrimitives.ReadDoubleLittleEndian(PrepareBuffer(8));
            default:
                CoPackException.ThrowReadUnexpectedFloat(cookie, CookieFloatType.Double);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadVarInt8(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return (sbyte)cookie;
            case (byte)CookieIntType.Int8:
                return (sbyte)MemoryMarshal.GetReference(PrepareBuffer(1));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.Int8);
                return 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadInt8()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarInt8(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadVarUInt8(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return cookie;
            case (byte)CookieIntType.UInt8:
                return MemoryMarshal.GetReference(PrepareBuffer(1));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.UInt8);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadUInt8()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarUInt8(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadVarInt16(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return cookie;
            case (byte)CookieIntType.Int8:
                return (sbyte)MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.UInt8:
                return MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.Int16:
                return BinaryPrimitives.ReadInt16LittleEndian(PrepareBuffer(2));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.Int16);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarInt16(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadVarUInt16(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return cookie;
            case (byte)CookieIntType.UInt8:
                return MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.UInt16:
                return BinaryPrimitives.ReadUInt16LittleEndian(PrepareBuffer(2));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.UInt16);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarUInt16(cookie);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadVarInt32(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return cookie;
            case (byte)CookieIntType.Int8:
                return (sbyte)MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.UInt8:
                return MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.Int16:
                return BinaryPrimitives.ReadInt16LittleEndian(PrepareBuffer(2));
            case (byte)CookieIntType.UInt16:
                return BinaryPrimitives.ReadUInt16LittleEndian(PrepareBuffer(2));
            case (byte)CookieIntType.Int32:
                return BinaryPrimitives.ReadInt32LittleEndian(PrepareBuffer(4));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.Int32);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarInt32(cookie);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadInt32(out int value)
    {
        var (type, cookie) = PeekHeader();
        if (type == CoPackType.Int)
        {
            Skip(1);
            value = ReadVarInt32(cookie);
            return true;
        }

        value = 0;
        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadVarUInt32(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return cookie;
            case (byte)CookieIntType.UInt8:
                return MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.UInt16:
                return BinaryPrimitives.ReadUInt16LittleEndian(PrepareBuffer(2));
            case (byte)CookieIntType.UInt32:
                return BinaryPrimitives.ReadUInt32LittleEndian(PrepareBuffer(4));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.UInt32);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarUInt32(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadVarInt64(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return cookie;
            case (byte)CookieIntType.Int8:
                return (sbyte)MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.UInt8:
                return MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.Int16:
                return BinaryPrimitives.ReadInt16LittleEndian(PrepareBuffer(2));
            case (byte)CookieIntType.UInt16:
                return BinaryPrimitives.ReadUInt16LittleEndian(PrepareBuffer(2));
            case (byte)CookieIntType.Int32:
                return BinaryPrimitives.ReadInt32LittleEndian(PrepareBuffer(4));
            case (byte)CookieIntType.UInt32:
                return BinaryPrimitives.ReadUInt32LittleEndian(PrepareBuffer(4));
            case (byte)CookieIntType.Int64:
                return BinaryPrimitives.ReadInt64LittleEndian(PrepareBuffer(8));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.Int64);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarInt64(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadVarUInt64(byte cookie)
    {
        switch (cookie)
        {
            case <= CoPackCode.CookieIntMaxValue:
                return cookie;
            case (byte)CookieIntType.UInt8:
                return MemoryMarshal.GetReference(PrepareBuffer(1));
            case (byte)CookieIntType.UInt16:
                return BinaryPrimitives.ReadUInt16LittleEndian(PrepareBuffer(2));
            case (byte)CookieIntType.UInt32:
                return BinaryPrimitives.ReadUInt32LittleEndian(PrepareBuffer(4));
            case (byte)CookieIntType.UInt64:
                return BinaryPrimitives.ReadUInt64LittleEndian(PrepareBuffer(8));
            default:
                CoPackException.ThrowReadUnexpectedInt(cookie, CookieIntType.UInt64);
                return 0;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Int)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Int);
        return ReadVarUInt64(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadNull()
    {
        var (type, cookie) = PeekHeader();
        if (type == CoPackType.NullBool && cookie == (byte) CookieNullBoolType.Null)
        {
            Skip(1);
            return true;
        }

        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? ReadString()
    {
        if (TryReadNull())
            return null;
        
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.String)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.String);

        var len = ReadVarInt32(cookie);
        if (len == 0)
            return string.Empty;

        var span = PrepareBuffer(len);
        return Encoding.UTF8.GetString(span);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[]? ReadBytes()
    {
        if (TryReadNull())
            return null;
        
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Bytes)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Bytes);

        var len = ReadVarInt32(cookie);
        if (len == 0)
            return Array.Empty<byte>();

        var span = PrepareBuffer(len);
        return span.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadValue<T>(object? state = null)
    {
        var formatter = CoPackFormatterProvider.GetFormatter<T>();
        return formatter.Read(ref this, state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadListHeader()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.List)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.List);
        return ReadVarInt32(cookie);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadMapHeader()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Map)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Map);
        if (type == 0)  // 0 is object
            CoPackException.ThrowReadUnexpectedMap(cookie);
        return ReadVarInt32(cookie) - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadUnionHeader()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Union)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Union);
        return ReadVarInt32(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadObjectHeader()
    {
        var (type, cookie) = ReadHeader();
        if (type != CoPackType.Map)
            CoPackException.ThrowReadUnexpectedType(type, CoPackType.Map);
        if (cookie != 0)    // !0 is map
            CoPackException.ThrowReadUnexpectedObject(cookie);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadObjectHeader()
    {
        var (type, cookie) = PeekHeader();
        if (type == CoPackType.Map && cookie == 0)
        {
            Skip(1);
            return true;
        }

        return false;
    }
}