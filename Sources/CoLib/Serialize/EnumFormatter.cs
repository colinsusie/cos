// Written by Colin on 2024-10-11

using System.Buffers;
using System.Runtime.CompilerServices;

namespace CoLib.Serialize;

/// <summary>
/// 枚举，只支持小于int的枚举值，uint, long, ulong不支持
/// </summary>
/// <typeparam name="TEnum"></typeparam>
public sealed class EnumFormatter<TEnum> : ICoPackFormatter<TEnum>
    where TEnum: struct, Enum
{
    public EnumFormatter()
    {
        var underlyingType = typeof(TEnum).GetEnumUnderlyingType();
        var typeCode = Type.GetTypeCode(underlyingType);
        switch (typeCode)
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
                break;
            default:
                CoPackException.ThrowUnSupportEnum(typeof(TEnum));
                break;
        }
    }
    
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in TEnum value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        var value2 = value;
        var intValue = Unsafe.As<TEnum, int>(ref value2);
        writer.WriteVarInt(intValue);
    }

    public TEnum Read(ref CoPackReader reader, object? state)
    {
        var value = reader.ReadInt32();
        var result = Unsafe.As<int, TEnum>(ref value);
        if (!Enum.IsDefined(result))
            CoPackException.ThrowEnumValueNoDefined(typeof(TEnum), value);

        return result;
;    }
}
