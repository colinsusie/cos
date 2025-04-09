// Written by Colin on 2024-11-26

using DotNetty.Transport.Channels;

namespace CoRuntime.Gate;

/// <summary>
/// 网关处理器
/// </summary>
public interface IGateHandler
{
    /// 创建一个网关连接
    IGateConnection CreateConnection(IGateServer server, IChannel channel);
    /// 网关连接开启
    void OnConnectionOpen(IGateServer server, IGateConnection connection);
    /// 网关连接关闭
    void OnConnectionClose(IGateServer server, IGateConnection connection);
}