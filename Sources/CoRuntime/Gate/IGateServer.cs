// Written by Colin on 2024-11-26

using CoRuntime.Services;

namespace CoRuntime.Gate;

/// <summary>
/// 定义网关服务器的接口
/// </summary>
public interface IGateServer
{
    Service Service { get; }
    /// 启动网关
    Task StartAsync();
    /// 停止网关
    Task StopAsync();
}