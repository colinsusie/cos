// Written by Colin on 2023-11-18

using Microsoft.Extensions.ObjectPool;

namespace CoLib.ObjectPools;

/// <summary>
/// 单线程的对象，非线程安全，强制要求实现IResettable接口
/// </summary>
/// <typeparam name="T"></typeparam>
public class StObjectPool<T>: ObjectPool<T> where T: class, ICleanable
{
    private readonly Func<T> _createFunc;
    private readonly Queue<T> _items = new();

    public StObjectPool(int maxCapacity, Func<T> createFunc)
    {
        _createFunc = createFunc;
        MaxCapacity = maxCapacity;
    }

    /// <summary>
    /// 池中的数量
    /// </summary>
    public int Count => _items.Count;

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
        return _items.TryDequeue(out var item) ? item : _createFunc();
    }

    /// <summary>
    /// 将对象返还给对象池，如果超出对象池最大容量将丢弃
    /// </summary>
    /// <param name="item"></param>
    public override void Return(T item)
    {
        item.Cleanup();
        
        if (_items.Count < MaxCapacity)
        {
            _items.Enqueue(item);
        }
    }
}