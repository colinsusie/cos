// Written by Colin on 2024-10-10

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CoLib.Serialize;

public ref struct CoPackWriter<TBufferWriter> where TBufferWriter: IBufferWriter<byte>
{
    private const int DepthLimit = 256;
    
    private ref TBufferWriter _bufferWriter;
    private Span<byte> _bufferSpan;
    private int _depth;

    public CoPackWriter(ref TBufferWriter writer)
    {
        _bufferWriter = ref writer;
        _bufferSpan = default;
        _depth = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PrepareBuffer(int sizeHint)
    {
        if (sizeHint > _bufferSpan.Length)
        {
            _bufferSpan = _bufferWriter.GetSpan(sizeHint);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int count)
    {
        if (count == 0)
            return;
        _bufferSpan = _bufferSpan.Slice(count);
        _bufferWriter.Advance(count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte MakeHeader(CoPackType type, byte cookie)
    {
        return (byte)(((byte)type << CoPackCode.CookieBits) | (cookie & CoPackCode.CookieMask));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteHeader(CoPackType type, byte cookie)
    {
        var header = MakeHeader(type, cookie);
        PrepareBuffer(1);
        MemoryMarshal.GetReference(_bufferSpan) = header;
        Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNull()
    {
        WriteHeader(CoPackType.NullBool, (byte)CookieNullBoolType.Null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool value)
    {
        WriteHeader(CoPackType.NullBool, value ? 
            (byte)CookieNullBoolType.True : (byte)CookieNullBoolType.False);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, sbyte value)
    {
        PrepareBuffer(2);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.Int8);
        Advance(1);
        MemoryMarshal.GetReference(_bufferSpan) = (byte) value;
        Advance(1);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, byte value)
    {
        PrepareBuffer(2);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.UInt8);
        Advance(1);
        MemoryMarshal.GetReference(_bufferSpan) = value;
        Advance(1);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, short value)
    {
        PrepareBuffer(3);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.Int16);
        Advance(1);
        BinaryPrimitives.WriteInt16LittleEndian(_bufferSpan, value);
        Advance(2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, ushort value)
    {
        PrepareBuffer(3);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.UInt16);
        Advance(1);
        BinaryPrimitives.WriteUInt16LittleEndian(_bufferSpan, value);
        Advance(2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, int value)
    {
        PrepareBuffer(5);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.Int32);
        Advance(1);
        BinaryPrimitives.WriteInt32LittleEndian(_bufferSpan, value);
        Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, uint value)
    {
        PrepareBuffer(5);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.UInt32);
        Advance(1);
        BinaryPrimitives.WriteUInt32LittleEndian(_bufferSpan, value);
        Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, long value)
    {
        PrepareBuffer(9);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.Int64);
        Advance(1);
        BinaryPrimitives.WriteInt64LittleEndian(_bufferSpan, value);
        Advance(8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedIntWithType(CoPackType type, ulong value)
    {
        PrepareBuffer(5);
        MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(type, (byte)CookieIntType.UInt64);
        Advance(1);
        BinaryPrimitives.WriteUInt64LittleEndian(_bufferSpan, value);
        Advance(8);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, sbyte value)
    {
        if (0 <= value && value <= CoPackCode.CookieIntMaxValue)
        {
            WriteHeader(type, (byte)value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(sbyte value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(byte value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(short value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(ushort value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(int value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(uint value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(long value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixedInt(ulong value)
    {
        WriteFixedIntWithType(CoPackType.Int, value);   
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, byte value)
    {
        if (value <= CoPackCode.CookieIntMaxValue)
        {
            WriteHeader(type, value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, short value)
    {
        if (value is >= 0 and <= byte.MaxValue)
        {
            WriteVarIntWithType(type, (byte)value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, ushort value)
    {
        if (value <= byte.MaxValue)
        {
            WriteVarIntWithType(type, (byte)value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, int value)
    {
        if (value is >= 0 and <= ushort.MaxValue)
        {
            WriteVarIntWithType(type, (ushort)value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, uint value)
    {
        if (value <= ushort.MaxValue)
        {
            WriteVarIntWithType(type, (ushort)value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, long value)
    {
        if (value is >= 0 and <= uint.MaxValue)
        {
            WriteVarIntWithType(type, (uint)value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarIntWithType(CoPackType type, ulong value)
    {
        if (value <= uint.MaxValue)
        {
            WriteVarIntWithType(type, (uint)value);
        }
        else
        {
            WriteFixedIntWithType(type, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(sbyte value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(byte value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(short value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(ushort value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(int value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(uint value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(long value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(ulong value)
    {
        WriteVarIntWithType(CoPackType.Int, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat(float value)
    {
        switch (value)
        {
            case 0:
                WriteHeader(CoPackType.Float, 0);
                break;
            default:
                PrepareBuffer(5);
                MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(CoPackType.Float, (byte)CookieFloatType.Float);
                Advance(1);
                BinaryPrimitives.WriteSingleLittleEndian(_bufferSpan, value);
                Advance(4);
                break;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat(double value)
    {
        switch (value)
        {
            case 0:
                WriteHeader(CoPackType.Float, 0);
                break;
            default:
                PrepareBuffer(9);
                MemoryMarshal.GetReference(_bufferSpan) = MakeHeader(CoPackType.Float, (byte)CookieFloatType.Double);
                Advance(1);
                BinaryPrimitives.WriteDoubleLittleEndian(_bufferSpan, value);
                Advance(8);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(byte[]? value)
    {
        if (value == null)
        {
            WriteNull();
            return;
        }
        var len = value.Length;
        WriteVarIntWithType(CoPackType.Bytes, len);
        if (len > 0)
        {
            PrepareBuffer(len);
            value.CopyTo(_bufferSpan);
            Advance(len);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string? value)
    {
        if (value == null)
        {
            WriteNull();
            return;
        }
        var byteCount = Encoding.UTF8.GetByteCount(value);
        WriteVarIntWithType(CoPackType.String, byteCount);
        if (byteCount > 0)
        {
            PrepareBuffer(byteCount);
            Encoding.UTF8.GetBytes(value, _bufferSpan);
            Advance(byteCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValue<T>(in T? value, PackFlags flags = PackFlags.All)
    {
        _depth++;
        if (_depth == DepthLimit) CoPackException.ThrowReachedDepthLimit(typeof(T));
        CoPackFormatterProvider.GetFormatter<T>().Write(ref this, in value, flags);
        _depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnion<T>(int tag, in T? value, PackFlags flags = PackFlags.All)
    {
        WriteVarIntWithType(CoPackType.Union, tag);
        WriteValue(value, flags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteMapHeader(int count)
    {
        WriteVarIntWithType(CoPackType.Map, count+1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteListHeader(int count)
    {
        WriteVarIntWithType(CoPackType.List, count);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteObjectHeader()
    {
        WriteVarIntWithType(CoPackType.Map, 0);
    }
}