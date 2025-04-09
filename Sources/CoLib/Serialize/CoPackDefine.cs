// Written by Colin on 2024-10-10

namespace CoLib.Serialize;

public enum CoPackType: byte
{
    NullBool = 0,
    Int = 1,
    Float = 2,
    List = 3,
    Map = 4,
    Union = 5,
    Bytes = 6,
    String = 7,
}

public enum CookieNullBoolType : byte
{
    Null = 0,
    False = 1,
    True = 2,
}

public enum CookieIntType : byte
{
    Int8 = 0b0001_0000,
    UInt8 = 0b0001_0001,
    Int16 = 0b0001_0010,
    UInt16 = 0b0001_0011,
    Int32 = 0b0001_0100,
    UInt32 = 0b0001_0101,
    Int64 = 0b0001_0110,
    UInt64 = 0b0001_0111,
}

public enum CookieFloatType : byte
{
    Float = 1,
    Double = 2,
}

public static class CoPackCode
{
    public const int CookieBits = 5;
    public const byte CookieMask = 0b0001_1111;
    public const byte CookieIntMaxValue = 0b0000_1111;
}