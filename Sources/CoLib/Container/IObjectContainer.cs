// Written by Colin on 2024-7-19

using System.Diagnostics.CodeAnalysis;

namespace CoLib.Container;

/// <summary>
/// 对象容器，用于存放各种对象
/// </summary>
public interface IObjectContainer
{
    /// <summary>
    /// 注册对象，如果对象已经存在将抛出异常
    /// </summary>
    public void Register<T>(T obj) where T: class;
    /// <summary>
    /// 尝试取某个类型的对象
    /// </summary>
    public bool TryGet<T>([MaybeNullWhen(false)] out T obj) where T: class;
    /// <summary>
    /// 取必须存在的对象
    /// </summary>
    public T GetRequired<T>() where T: class;
}