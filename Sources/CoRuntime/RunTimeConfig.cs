// Written by Colin on 2025-02-20

namespace CoRuntime;

/// <summary>
/// 用于存放一些全局的配置
/// </summary>
public class RunTimeConfig
{
    /// APP名字
    public string AppName { get; internal set; } = null!;
    /// 节点名字：唯一标识这个节点
    public string NodeName { get; internal set; } = null!;
    /// 节点ID：和节点名字一一对应，也是唯一标识这个节点
    public short NodeId { get; internal set; }
    
    /// Consul的地址
    public string ConsulAddress { get; internal set; } = null!;
}

