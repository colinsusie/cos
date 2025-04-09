// Written by Colin on 2024-11-26

namespace CoRuntime.Gate;

public sealed class GateServerOptions
{
    public bool EnableLogging { get; private set; }
    /// 监听地址
    public string ListenHost { get; private set; } = string.Empty;
    // / 监听端口
    public int ListenPort { get; private set;}
    /// 最大的消息大小，包括包头
    public int MaxPacketSize { get; private set;} = 2 * 1024 * 1024;
}