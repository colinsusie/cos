// Written by Colin on 2024-8-25

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using CoLib.ObjectPools;

namespace CoLib.Container;

/// <summary>
/// 池化的字节BufferWriter
/// </summary>
public sealed class PooledByteBufferWriter : IBufferWriter<byte>, IDisposable
{
    private const int MinimumBufferSize = 256;

    private byte[]? _rentedBuffer;
    private int _index;
    
    public static PooledByteBufferWriter Create(int initialCapacity = 0)
    {
        return new PooledByteBufferWriter(initialCapacity);
    }

    public PooledByteBufferWriter() : this(MinimumBufferSize)
    {
    }

    public PooledByteBufferWriter(int initialCapacity)
    {
        if (initialCapacity <= MinimumBufferSize)
            initialCapacity = MinimumBufferSize;
        _rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        _index = 0;
    }

    public ReadOnlyMemory<byte> WrittenMemory
    {
        get
        {
            if (_rentedBuffer == null) ThrowObjectDisposedException();
            return _rentedBuffer.AsMemory(0, _index);
        }
    }

    public ReadOnlySpan<byte> WrittenSpan => WrittenMemory.Span;

    public int WrittenCount
    {
        get
        {
            if (_rentedBuffer == null) ThrowObjectDisposedException();
            return _index;
        }
    }

    public int Capacity
    {
        get
        {
            if (_rentedBuffer == null) ThrowObjectDisposedException();
            return _rentedBuffer.Length;
        }
    }

    public int FreeCapacity
    {
        get
        {
            if (_rentedBuffer == null) ThrowObjectDisposedException();
            return _rentedBuffer.Length - _index;
        }
    }

    public void Clear()
    {
        if (_rentedBuffer == null) ThrowObjectDisposedException();
        _rentedBuffer.AsSpan(0, _index).Clear();
        _index = 0;
    }

    // Returns the rented buffer back to the pool
    public void Dispose()
    {
        _index = 0;
        if (_rentedBuffer == null) return;
        ArrayPool<byte>.Shared.Return(_rentedBuffer);
        _rentedBuffer = null;
    }
    
    public void Advance(int count)
    {
        if (_rentedBuffer == null) ThrowObjectDisposedException();
        ArgumentOutOfRangeException.ThrowIfNegative(count);
 
        if (_index > _rentedBuffer.Length - count)
        {
            ThrowInvalidOperationException(_rentedBuffer.Length);
        }
 
        _index += count;
    }
 
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (_rentedBuffer == null) ThrowObjectDisposedException();
        CheckAndResizeBuffer(sizeHint);
        return _rentedBuffer.AsMemory(_index);
    }
 
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (_rentedBuffer == null) ThrowObjectDisposedException();
        CheckAndResizeBuffer(sizeHint);
        return _rentedBuffer.AsSpan(_index);
    }
 
    private void CheckAndResizeBuffer(int sizeHint)
    {
        if (_rentedBuffer == null) ThrowObjectDisposedException();
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);
 
        if (sizeHint == 0)
        {
            sizeHint = MinimumBufferSize;
        }
 
        var availableSpace = _rentedBuffer.Length - _index;
 
        if (sizeHint > availableSpace)
        {
            var growBy = Math.Max(sizeHint, _rentedBuffer.Length);
 
            var newSize = checked(_rentedBuffer.Length + growBy);
 
            var oldBuffer = _rentedBuffer;
 
            _rentedBuffer = ArrayPool<byte>.Shared.Rent(newSize);
 
            var previousBuffer = oldBuffer.AsSpan(0, _index);
            previousBuffer.CopyTo(_rentedBuffer);
            ArrayPool<byte>.Shared.Return(oldBuffer);
        }
    }
    
    [DoesNotReturn]
    private static void ThrowObjectDisposedException()
    {
        throw new ObjectDisposedException(nameof(ArrayBufferWriter<byte>));
    }
 
    [DoesNotReturn]
    private static void ThrowInvalidOperationException(int capacity)
    {
        throw new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {capacity}.");
    }
}