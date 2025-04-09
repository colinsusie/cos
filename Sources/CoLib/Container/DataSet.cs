// Written by Colin on 2024-7-20

using System.Diagnostics.CodeAnalysis;
using CoLib.ObjectPools;
using CoLib.Common;

namespace CoLib.Container;

/// <summary>
/// 代表一个数据集，可以往里面添加各种类型的数据
/// </summary>
public sealed class DataSet: IDisposable, ICleanable
{
    [ThreadStatic] private static StObjectPool<DataSet>? _pool;
    private static StObjectPool<DataSet> Pool =>
        _pool ??= new StObjectPool<DataSet>(256, () => new DataSet());

    private readonly Dictionary<Type, IDisposable> _items = new();

    /// <summary>
    /// 创建数据集
    /// </summary>
    public static DataSet Create()
    {
        return Pool.Get();
    }

    private DataSet()
    {
    }
    
    public void Dispose()
    {
        Pool.Return(this);        
    }

    void ICleanable.Cleanup()
    {
        foreach (var item in _items.Values)
        {
            item.Dispose();
        }
        _items.Clear();
    }

    /// <summary>
    /// 添加数据项
    /// </summary>
    public DataSet Add<T>(in T value)
    {
        var key = typeof(T);
        if (_items.ContainsKey(key))
            ThrowHelper.ThrowInvalidOperationException($"Cannot add duplicate data:{key}");
        var item = DataEntry<T>.Create(value);
        _items[key] = item;
        return this;
    }

    /// <summary>
    /// 尝试取数据项
    /// </summary>
    public bool TryGet<T>([MaybeNullWhen(false)] out T value)
    {
        var item = _items.TryGetValue(typeof(T), out var dis) ? dis as DataEntry<T> : null;
        if (item == null)
        {
            value = default;
            return false;
        }

        value = item.Value;
        return true;
    }

    /// <summary>
    /// 取数据项，取不到抛异常
    /// </summary>
    public T Get<T>()
    {
        var item = _items.TryGetValue(typeof(T), out var dis) ? dis as DataEntry<T> : null;
        if (item == null)
        {
            ThrowHelper.ThrowInvalidCastException($"{typeof(T)} not found");
        }

        return item.Value;
    }
}