// Written by Colin on 2025-02-20

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CoRuntime.GObjects;

/// <summary>
/// 全局对象管理器，提供了可以动态注册和获得全局对象的能力
/// 这些对象必须自己处理多线程问题
/// 因为这些对象是全局性，一旦获得可以保证一直有效，所以外部可以获得之后保存起来以提高效率
/// </summary>
public class GObjectManager: RuntimeObject
{
    private readonly ConcurrentDictionary<Type, object> _objects = new();
    
    // 注册全局对象
    public bool Register<T>(T obj) where T: class
    {
        return _objects.TryAdd(typeof(T), obj);
    }

    /// 尝试取全局对象
    public bool TryGet<T>([MaybeNullWhen(false)] out T type) where T : class
    {
        type = _objects.TryGetValue(typeof(T), out var obj) ? obj as T : null;
        return type != null;
    }

    /// 取全局对象，如果取不对会抛异常
    public T Get<T>() where T: class
    {
        if (!TryGet<T>(out var type))
            throw new InvalidOperationException($"Get<{typeof(T)}> failed");
        return type;
    }
}