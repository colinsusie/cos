// Written by Colin on 2024-10-10

using System.Diagnostics.CodeAnalysis;

namespace CoLib.Serialize;

public class CoPackException(string message) : Exception(message)
{
    [DoesNotReturn]
    public static void ThrowReachedDepthLimit(Type type)
    {
        throw new CoPackException($"Serializing Type '{type}' reached depth limit.");
    }
    
    [DoesNotReturn]
    public static void ThrowReachedDepthLimit()
    {
        throw new CoPackException($"Serializing reached depth limit.");
    }
    
    [DoesNotReturn]
    public static void ThrowUnSupportEnum(Type type)
    {
        throw new CoPackException($"Enum Type '{type}' unsupported, value must be less than int");
    }
    
    [DoesNotReturn]
    public static void ThrowEnumValueNoDefined(Type type, int value)
    {
        throw new CoPackException($"{value} is not defined in the enumeration type: {type}");
    }
    
    [DoesNotReturn]
    public static void ThrowNotRegisteredInProvider(Type type)
    {
        throw new CoPackException($"{type.FullName} is not registered in CoPackFormatterProvider");
    }
    
    [DoesNotReturn]
    public static void ThrowReadReachedEnd()
    {
        throw new CoPackException("Buffer reached end");
    }
    
    [DoesNotReturn]
    public static void ThrowNotFoundInUnionType(Type actualType, Type baseType)
    {
        throw new CoPackException($"Type {actualType} is not annotated in {baseType} CoPackUnion.");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedType(CoPackType actual, CoPackType expected)
    {
        throw new CoPackException($"Expected to read {expected}, but actually read {actual}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedBool(byte cookie)
    {
        throw new CoPackException($"Expected to read bool type, cookie: {cookie}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedInt(byte cookie, CookieIntType expected)
    {
        throw new CoPackException($"Expected to read int type {expected}, but actually read cookie {cookie}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedInt(byte cookie)
    {
        throw new CoPackException($"Read Unexpected int type, cookie: {cookie}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedFloat(byte cookie, CookieFloatType expected)
    {
        throw new CoPackException($"Expected to float type {expected}, but actually read cookie {cookie}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedFloat(byte cookie)
    {
        throw new CoPackException($"Read Unexpected float type {cookie}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedObject(byte cookie)
    {
        throw new CoPackException($"Read Unexpected object type {cookie}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedMap(byte cookie)
    {
        throw new CoPackException($"Read Unexpected map type {cookie}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedLength(int actual, int expected)
    {
        throw new CoPackException($"Expected to read a length of {expected}, but actually read a length of {actual}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedObjectTag(int tag)
    {
        throw new CoPackException($"Read an unexpected object tag: {tag}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedObjectTag(string? tag)
    {
        throw new CoPackException($"Read an unexpected object tag: {tag}");
    }
    
    [DoesNotReturn]
    public static void ThrowReadUnexpectedUnionTag(int tag)
    {
        throw new CoPackException($"Read an unexpected union tag: {tag}");
    }
}