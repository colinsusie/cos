// Written by Colin on 2023-11-24

using System.Diagnostics.CodeAnalysis;

namespace CoLib.Common;

public static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowSizeOutOfRangeException(string? message)
    {
        throw new SizeOutOfRangeException(message);
    }

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException(string? message)
    {
        throw new ArgumentOutOfRangeException(message);
    }
    
    [DoesNotReturn]
    public static void ThrowIndexOutOfRangeException(string? message)
    {
        throw new IndexOutOfRangeException(message);
    }
    
    [DoesNotReturn]
    public static void ThrowArgumentException(string? message)
    {
        throw new ArgumentException(message);
    }
    
    [DoesNotReturn]
    public static void ThrowInvalidOperationException(string? message)
    {
        throw new InvalidOperationException(message);
    }
    
    [DoesNotReturn]
    public static void ThrowArgumentNullException(string? param)
    {
        throw new ArgumentNullException(param);
    }
    
    [DoesNotReturn]
    public static void ThrowInvalidCastException(string? message)
    {
        throw new InvalidCastException(message);
    }
    
    [DoesNotReturn]
    public static void ThrowStateException(string? message)
    {
        throw new StateException(message);
    }
}