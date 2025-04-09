// Written by Colin on 2023-11-21

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace CoLib.Container;

/// <summary>
/// 代表一个Int32索引Id: | Index | Version |
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct IntIndexId:
    IComparable, 
    IComparable<IntIndexId>,
    IEquatable<IntIndexId>,
    ISpanFormattable
{
    [FieldOffset(0)] public readonly int Value = 0;
    [FieldOffset(0)] public readonly ushort Version = 0;
    [FieldOffset(2)] public readonly short Index = 0;
    

    public IntIndexId(short index, ushort version)
    {
        Index = index;
        Version = version;
    }
    
    public IntIndexId(int value)
    {
        Value = value;
    }
    
    public static implicit operator int(IntIndexId id) => id.Value;
    public static explicit operator IntIndexId(int value) => new (value);

    public int CompareTo(object? value)
    {
        if (value == null)
            return 1;
        if (value is not IntIndexId id)
            throw new ArgumentException("value must be IntIndexId", nameof(value));
        if (Value < id.Value)
            return -1;
        return Value > id.Value ? 1 : 0;
    }


    public int CompareTo(IntIndexId other)
    {
        if (Value < other.Value)
            return -1;
        return Value > other.Value ? 1 : 0;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IntIndexId id && Value == id.Value;
    }
    
    public bool Equals(IntIndexId other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Index ^ Version;
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }

    public string ToString(IFormatProvider? provider)
    {
        return Value.ToString(provider);
    }

    public string ToString(string? format)
    {
        return Value.ToString(format);
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
        return Value.ToString(format, provider);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, 
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }
    
    public static bool operator ==(IntIndexId left, IntIndexId right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(IntIndexId left, IntIndexId right)
    {
        return left.Value != right.Value;
    }

    public static bool operator >(IntIndexId left, IntIndexId right)
    {
        return left.Value > right.Value;
    }

    public static bool operator >=(IntIndexId left, IntIndexId right)
    {
        return left.Value >= right.Value;
    }

    public static bool operator <(IntIndexId left, IntIndexId right)
    {
        return left.Value < right.Value;
    }

    public static bool operator <=(IntIndexId left, IntIndexId right)
    {
        return left.Value <= right.Value;
    }

    public static IntIndexId Parse(string s, IFormatProvider? provider)
    {
        return new IntIndexId(int.Parse(s, provider));
    }
}