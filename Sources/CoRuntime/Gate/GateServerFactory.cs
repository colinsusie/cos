// Written by Colin on 2024-11-26

using CoLib.EventLoop;
using CoRuntime.Services;

namespace CoRuntime.Gate;

/// 网关类型
public enum GateServerType
{
    Tcp,
    Kcp,
}

/// 网关创建工厂
public static class GateServerFactory
{
    /// <summary>
    /// 创建一个网关服务器
    /// </summary>
    /// <param name="type">网关类型</param>
    /// <param name="eventLoop">网关所属的事件循环</param>
    /// <param name="handler">网关处理器</param>
    /// <param name="options">网关选项</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IGateServer CreateGateServer(Service service, GateServerType type, StEventLoop eventLoop, IGateHandler handler, GateServerOptions options)
    {
        switch (type)
        {
            case GateServerType.Tcp:
                return new TcpGateServer(service, handler, options);
            default:
                throw new NotImplementedException($"{type}");
        }
    }
}