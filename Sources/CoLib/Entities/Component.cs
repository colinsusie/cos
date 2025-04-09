// Written by Colin on 2024-1-10

using CoLib.Container;

namespace CoLib.Entities;

/// 组件基类
public abstract class Component: IDisposable
{
    public readonly Entity Entity;

    protected Component(Entity entity)
    {
        Entity = entity;
    }
    
    internal void Init(DataSet dataSet)
    {
        OnInit(dataSet);
    }
    
    /// 初始化数据，此时其他组件还未就绪
    protected virtual void OnInit(DataSet dataSet)
    {
    }

    internal void InitCompleted(DataSet dataSet)
    {
        OnInitCompleted(dataSet);
    }
    
    /// 初始化完成，此时所有组件已就绪
    protected virtual void OnInitCompleted(DataSet dataSet)
    {
    }
    
    public void Dispose()
    {
        OnDispose();
    }

    /// 释放组件
    protected virtual void OnDispose()
    {
    }
}