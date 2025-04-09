// Written by Colin on 2024-7-20

using CoLib.ObjectPools;

namespace CoLib.Container;

/// <summary>
/// 数据项
/// </summary>
internal class DataEntry<T>: IDisposable, ICleanable
{
    [ThreadStatic] private static StObjectPool<DataEntry<T>>? _pool;
    private static StObjectPool<DataEntry<T>> Pool =>
        _pool ??= new StObjectPool<DataEntry<T>>(64, () => new DataEntry<T>());
    
    public static DataEntry<T> Create(in T value)
    {
        var item = Pool.Get();
        item.Initialize(value);
        return item;
    }

    private DataEntry()
    {
    }

    private void Initialize(in T value)
    {
        Value = value;
    }
    
    void IDisposable.Dispose()
    {
        Pool.Return(this);
    }

    void ICleanable.Cleanup()
    {
        Value = default!;
    }

    public T Value { get; private set; } = default!;
}