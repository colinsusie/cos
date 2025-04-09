// Written by Colin on 2024-7-19

using System.Diagnostics.CodeAnalysis;
using CoLib.Common;

namespace CoLib.Container;

/// <summary>
/// 对象容器的默认实现，注意该对象不是线程安全的，
/// 在TryGet/GetRequired之前，把所需的对象Register好
/// </summary>
public class ObjectContainer: IObjectContainer
{
    // 提供一个全局的对象容器
    public static ObjectContainer Shared = new();
    
    private readonly Dictionary<Type, object> _objects = new();
    
    public void Register<T>(T obj) where T: class
    {
        _objects.Add(typeof(T), obj);
    }

    public bool TryGet<T>([MaybeNullWhen(false)] out T type) where T : class
    {
        type = _objects.TryGetValue(typeof(T), out var obj) ? obj as T : null;
        return type != null;
    }

    public T GetRequired<T>() where T: class
    {
        if (!TryGet<T>(out var type))
        {
            ThrowHelper.ThrowInvalidOperationException($"GetRequired<{typeof(T)}> failed");
        }
        return type;
    }
}