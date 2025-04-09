// Written by Colin on 2025-1-25

using CoLib.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CoRuntime;

// 运行时处理器
public interface IRuntimeHandler
{
    /// 绑定选项，注意此时不能使用RuntimeEnv里面的所有字段，因为还没有初始化完毕,
    /// 该函数只用于初始化配置和选项类的绑定 
    void InitOptions(OptionsManager optionsMgr);
    /// 初始化自定义服务
    Task InitServices(CancellationToken cancellationToken);
    /// 运行时启动完毕 
    void OnStarted();
    /// 运行时停止完毕 
    void OnStopped();
}

/// <summary>
/// 默认实现，子类可以覆盖需要实现的方法
/// </summary>
public class BaseRuntimeHandler: IRuntimeHandler
{
    public virtual void InitOptions(OptionsManager optionsMgr)
    {
    }

    public virtual Task InitServices(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual void OnStarted()
    {
    }

    public virtual void OnStopped()
    {
    }
}