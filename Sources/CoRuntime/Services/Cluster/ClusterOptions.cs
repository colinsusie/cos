// Written by Colin on 2025-02-04

namespace CoRuntime.Services.Cluster;

/// <summary>
/// Consul配置
/// </summary>
public sealed class ClusterOptions
{
    /// 将自己注册到集群里
    public bool RegisterNode { get; set; } = true;
    /// 更新节点的间隔
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(5);
    /// 节点健康检查的TTL
    public TimeSpan StatusTtl { get; set; } = TimeSpan.FromSeconds(30);
    /// 节点健康检查的间隔
    public TimeSpan StatusInterval { get; set; } = TimeSpan.FromSeconds(9);
    /// Watch多久返回
    public TimeSpan WatchTime { get; set; } = TimeSpan.FromSeconds(60);
    /// Watch一次的间隔
    public TimeSpan WatchInterval { get; set; } = TimeSpan.FromSeconds(5);
    // / 当节点为Critical多久后反注册
    public TimeSpan DeregisterTtl { get; set; } = TimeSpan.FromSeconds(120);
    /// 集群APP列表
    public List<ClusterAppOptions> WatchApps { get; set; } = new(); 
}

/// <summary>
/// 集群APP信息
/// </summary>
public sealed class ClusterAppOptions
{
    /// APP名
    public string AppName { get; set; } = string.Empty;
    /// 是否对该APP进行允许RPC
    public bool EnableRpc { get; set; }
}