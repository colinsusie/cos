// Written by Colin on ${2023}-12-16

using CoLib.Container;
using CoLib.Plugin;

namespace CoRuntime.Services;

/// <summary>
/// 服务插件, 用于创建服务实例
/// </summary>
public interface IServicePlugin: IPlugin
{
    /// 创建一个服务
    public Service CreateService(ServiceContext context);
}