// Written by Colin on 2023-11-21

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CoLib.Common;

namespace CoLib.Container;

/// <summary>
/// 基于索引的映射表，提供比哈希表更快的查找速度
/// </summary>
/// <typeparam name="T"></typeparam>
public class LongIndexMap<T> where T: class
{
    private const int DefaultCapacity = 4096;
    private const int DoubleThreshold = 16384;

    // 可用的索引表
    private readonly Queue<LongIndexId> _indexes;
    private T?[] _array;
    // 当前分配的最大索引
    private int _maxIndex;

    public LongIndexMap() : this(DefaultCapacity)
    {
    }


    public LongIndexMap(int capacity, int indexCapacity = 0)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (indexCapacity < 0) throw new ArgumentOutOfRangeException(nameof(indexCapacity));
        
        _indexes = new (indexCapacity);
        _array = new T[capacity];
    }
    
    /// <summary>
    /// 尝试获取对象
    /// </summary>
    public bool TryGet(LongIndexId indexId, [MaybeNullWhen(false)] out T value)
    {
        if (indexId.Index < 0 || indexId.Index >= _array.Length)
        {
            value = default;
            return false;
        }
        
        value = _array[indexId.Index];
        return value != null;
    }
    
    /// <summary>
    /// 增加对象，并返回索引Id
    /// </summary>
    public LongIndexId Add(T value)
    {
        // 先从数组尾分配
        if (_maxIndex < _array.Length)
        {
            _array[_maxIndex] = value;
            return new LongIndexId(_maxIndex++, 1);     // 版本号从1开始
        }

        // 再从缓存中分配
        if (_indexes.TryDequeue(out var cacheIndex))
        {
            _array[cacheIndex.Index] = value;
            var newVersion = cacheIndex.Version + 1;
            if (newVersion >= uint.MaxValue) newVersion = 1;
            return new LongIndexId(cacheIndex.Index, newVersion);
        }

        if (_array.Length >= Array.MaxLength)
        {
            ThrowHelper.ThrowSizeOutOfRangeException("_array.Length exceeds the maximum length of the array");
        }
        
        var newSize = CalcNewCapacity(_array.Length, Array.MaxLength);
        Array.Resize(ref _array, newSize);
        _array[_maxIndex] = value;
        return new LongIndexId(_maxIndex++, 0);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CalcNewCapacity(int capacity, int maxCapacity)
    {
        var newCapacity = capacity switch
        {
            <= 0 => 16,
            < DoubleThreshold => capacity * 2,
            _ => capacity + DefaultCapacity
        };
        return Math.Min(newCapacity, maxCapacity);
    }

    /// <summary>
    /// 尝试删除对象
    /// </summary>
    public bool TryRemove(LongIndexId indexId)
    {
        return TryRemove(indexId, out _);
    }

    /// <summary>
    /// 尝试删除对象
    /// </summary>
    public bool TryRemove(LongIndexId indexId, [MaybeNullWhen(false)] out T value)
    {
        if (indexId.Index < 0 || indexId.Index >= _array.Length)
        {
            value = default;
            return false;
        }
        
        value = _array[indexId.Index];
        if (value == null)
        {
            value = default;
            return false;
        }
        
        _array[indexId.Index] = null;
        _indexes.Enqueue(indexId);
        return true;
    }
}