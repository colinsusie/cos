// Written by Colin on 2024-9-11

namespace CoRuntime.Rpc;

/// <summary>
/// Rpc选项
/// </summary>
public sealed class RpcOptions
{
    /// 事件循环组
    public string EventLoopGroup { get; set;} = string.Empty;
    /// 是否输出网络调试日志
    public bool EnableLogging { get; set;}
    /// 最大的消息大小，包括包头
    public int MaxPacketSize { get; set;} = 4 * 1024 * 1024;
    /// 请求超时
    public TimeSpan RequestTimeOut { get; set;} = TimeSpan.FromSeconds(15);
    /// 刷新消息的数量
    public int WriteFlushCount { get; set;} = 64;

    /// 心跳间隔：false表示不启用心跳机制，心跳机制的逻辑，是否启用由client决定
    ///   client: 每HeartbeatInterval发送一个Ping消息，如果HeartbeatTimeOut没有收到Pong消息则断开
    public bool EnableHeartbeat { get; set; } = true;
    public TimeSpan HeartbeatInterval { get; set;} = TimeSpan.FromSeconds(5);
    public TimeSpan HeartbeatTimeOut { get; set;} = TimeSpan.FromSeconds(16);
    /// 连接的超时
    public TimeSpan ConnectTimeout { get; set;} = TimeSpan.FromSeconds(15);
    /// 重连间隔
    public TimeSpan ReconnectInterval { get; set;} = TimeSpan.FromSeconds(2);
    /// 监听地址
    public string Host { get; set; } = string.Empty;
    /// 监听端口
    public int Port { get; set;}
}
