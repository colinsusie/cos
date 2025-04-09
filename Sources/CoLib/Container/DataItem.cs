// Written by Colin on 2024-1-11

using System.Diagnostics.CodeAnalysis;
using CoLib.ObjectPools;
using CoLib.Common;

namespace CoLib.Container;

/// <summary>
/// 数据项，可以包含任务类型的数据
/// </summary>
public class DataItem: IDisposable, ICleanable
{
    [ThreadStatic] private static StObjectPool<DataItem>? _pool;
    private static StObjectPool<DataItem> Pool =>
        _pool ??= new StObjectPool<DataItem>(256, () => new DataItem());
    
    private IDisposable? _entry;
    
    /// <summary>
    /// 创建数据项
    /// </summary>
    public static DataItem Create()
    {
        return Pool.Get();
    }

    private DataItem()
    {
    }
    
    public void Dispose()
    {
        Pool.Return(this);
    }

    void ICleanable.Cleanup()
    {
        _entry?.Dispose();
        _entry = null;
    }

    /// <summary>
    /// 设置数据项
    /// </summary>
    public DataItem Set<T>(in T value)
    {
        _entry?.Dispose();
        _entry = DataEntry<T>.Create(value);
        return this;
    }

    /// <summary>
    /// 尝试取数据项
    /// </summary>
    public bool TryGet<T>([MaybeNullWhen(false)] out T value)
    {
        if (_entry is not DataEntry<T> itemT)
        {
            value = default;
            return false;
        }

        value = itemT.Value;
        return true;
    }

    /// <summary>
    /// 取数据项，如果取不到会抛异常
    /// </summary>
    public T Get<T>()
    {
        var entryT = _entry as DataEntry<T>;
        if (entryT == null)
        {
            ThrowHelper.ThrowInvalidCastException($"Unable cast from {_entry?.GetType()} to {typeof(DataEntry<T>)}");
        }
        return entryT.Value;
    }
}