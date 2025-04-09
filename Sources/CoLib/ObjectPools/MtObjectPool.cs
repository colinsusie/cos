// Written by Colin on 2023-11-18

using System.Collections.Concurrent;
using Microsoft.Extensions.ObjectPool;

namespace CoLib.ObjectPools;

/// <summary>
/// 多线程的对象池，线程安全，强制要求实现IResettable接口
/// </summary>
/// <typeparam name="T"></typeparam>
public class MtObjectPool<T>: ObjectPool<T> where T: class, ICleanable
{
    private readonly Func<T> _createFunc;
    private int _numItems;
    private readonly ConcurrentQueue<T> _items = new();
    private T? _fastItem;
    
    public MtObjectPool(int maxCapacity, Func<T> createFunc)
    {
        // cache the target interface methods, to avoid interface lookup overhead
        _createFunc = createFunc;
        MaxCapacity = maxCapacity - 1;  // -1 to account for _fastItem
    }

    /// <summary>
    /// 池中的数量
    /// </summary>
    public int Count => _fastItem != null ? _numItems + 1 : _numItems;

    /// <summary>
    /// 最大容量
    /// </summary>
    public int MaxCapacity { get; }

    /// <summary>
    /// 从对象池中创建一个对象
    /// </summary>
    /// <returns></returns>
    public override T Get()
    {
        var item = _fastItem;
        if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
        {
            if (_items.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _numItems);
                return item;
            }
 
            // no object available, so go get a brand new one
            return _createFunc();
        }
 
        return item;
    }

    /// <summary>
    /// 将对象返还给对象池，如果超出对象池最大容量将丢弃
    /// </summary>
    /// <param name="item"></param>
    public override void Return(T item)
    {
        item.Cleanup();
        
        if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, item, null) != null)
        {
            if (Interlocked.Increment(ref _numItems) <= MaxCapacity)
            {
                _items.Enqueue(item);
                return;
            }
 
            Interlocked.Decrement(ref _numItems);
        }
    }
}