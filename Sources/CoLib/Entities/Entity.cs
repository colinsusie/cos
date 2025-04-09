// Written by Colin on 2024-1-10

using System.Diagnostics.CodeAnalysis;
using CoLib.Common;
using CoLib.Container;

namespace CoLib.Entities;

/// <summary>
/// 实体状态 
/// </summary>
public enum EntityState: byte
{
    // 新建 
    New,
    // 就绪
    Ready,
    // 正在释放
    Destroying,
    // 已释放
    Destroyed,
}

/// <summary>
/// 实体基类
/// </summary>
public abstract class Entity: IDisposable
{
    private const int DictThreshold = 4;
    
    // 组件列表
    private Component[] _compList = Array.Empty<Component>();
    private Dictionary<Type, Component>? _compDict;
    
    // 当前状态 
    public EntityState State { get; private set; } = EntityState.New;
    // 是否已释放
    public bool IsDestroyed => State == EntityState.Destroyed;

    protected Entity()
    {
    }
    
    internal void InitComponents(List<Component>? components, DataSet dataSet)
    {
        if (components != null)
        {
            _compList = components.ToArray();
        }
        
        if (_compList.Length > DictThreshold)
        {
            _compDict = new Dictionary<Type, Component>(_compList.Length);
            foreach (var comp in _compList)
            {
                _compDict.Add(comp.GetType(), comp);
            }
        }
        
        foreach (var comp in _compList)
        {
            comp.Init(dataSet);
        }

        OnInit(dataSet);
    }

    /// <summary>
    /// 初始化数据，子类处理
    /// </summary>
    protected virtual void OnInit(DataSet dataSet)
    {
    }

    public void InitCompleted(DataSet dataSet)
    {
        State = EntityState.Ready;
        
        foreach (var comp in _compList)
        {
            comp.InitCompleted(dataSet);
        }
        
        OnInitCompleted(dataSet);
    }
    
    /// <summary>
    /// 初始化完成，此时组件已经就绪
    /// </summary>
    protected virtual void OnInitCompleted(DataSet dataSet)
    {
    }
    
    public void Dispose()
    {
        if (State >= EntityState.Destroying)
        {
            throw new DuplicateDestroyException($"Duplicate entity: {this}");
        }

        State = EntityState.Destroying;
        OnDispose();

        foreach (var comp in _compList)
        {
            comp.Dispose();
        }
        
        State = EntityState.Destroyed;
    }

    /// <summary>
    /// 消毁，子类处理
    /// </summary>
    protected virtual void OnDispose()
    {
    }

    /// <summary>
    /// 取某个类型的组件
    /// </summary>
    public T GetComponent<T>() where T : Component
    {
        if (!TryGetComponent<T>(out var comp))
        {
            ThrowHelper.ThrowInvalidOperationException($"Get Component failed: {typeof(T)}");
        }

        return comp;
    }

    /// <summary>
    /// 尝试取组件
    /// </summary>
    public bool TryGetComponent<T>([MaybeNullWhen(false)] out T comp) where T : Component
    {
        if (_compDict != null)
        {
            comp = _compDict.TryGetValue(typeof(T), out var aComp) ? aComp as T : null;
            return comp != null;    
        }

        foreach (var aComp in _compList)
        {
            if (aComp is not T tComp) 
                continue;
            comp = tComp;
            return true;
        }

        comp = null;
        return false;
    }
}