// Written by Colin on 2025-02-20

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CoRuntime.Services.Cluster;

public sealed class ClusterAppInfo
{
    public readonly string AppName;
    public readonly ulong Version;
    public readonly bool EnableRpc;
    public readonly Dictionary<string, ClusterNodeInfo> Nodes = new();
    public readonly Dictionary<string, List<ClusterServiceInfo>> Services = new();

    public ClusterAppInfo(string appName, ulong version, bool enableRpc)
    {
        AppName = appName;
        Version = version;
        EnableRpc = enableRpc;
    }
}

public record struct ClusterNodeInfo(string NodeName, string Host, int Port);

public record struct ClusterServiceInfo(string NodeName, short ServiceId);


/// <summary>
/// 管理集群节点信息
/// </summary>
public class ClusterManager
{
    private readonly ConcurrentDictionary<string, ClusterAppInfo> _appInfos = new();

    /// 尝试取程序信息
    public bool TryGetAppInfo(string appName, [MaybeNullWhen(false)] out ClusterAppInfo appInfo)
    {
        return _appInfos.TryGetValue(appName, out appInfo);
    }

    ///  设置程序信息
    public void SetAppInfo(ClusterAppInfo appInfo)
    {
        _appInfos[appInfo.AppName] = appInfo;
    }

    /// 尝试从一个APP中获得一个服务
    public bool TryGetServiceAddr(string appName, string serviceName, ServiceSelectPolicy policy,
        out ServiceAddr serviceAddr)
    {
        serviceAddr = default;
        if (!_appInfos.TryGetValue(appName, out var appInfo))
            return false;
        
        if (!appInfo.Services.TryGetValue(serviceName, out var serviceInfos) || serviceInfos.Count == 0)
            return false;

        int idx;
        switch (policy)
        {
            case ServiceSelectPolicy.First:
                idx = 0;
                break;
            case ServiceSelectPolicy.Random:
            default:
                idx = Random.Shared.Next(0, serviceInfos.Count);
                break;
        }
        var serviceInfo = serviceInfos[idx];
        serviceAddr = new ServiceAddr
        {
            NodeName = serviceInfo.NodeName,
            ServiceId = serviceInfo.ServiceId,
        };
        return true;
    }
    
    /// 尝试从所有关注的APP中获得一个服务
    public bool TryGetServiceAddr(string serviceName, ServiceSelectPolicy policy,
        out ServiceAddr serviceAddr)
    {
        serviceAddr = default;
        foreach (var appName in _appInfos.Keys)
        {
            if (TryGetServiceAddr(appName, serviceName, policy, out serviceAddr))
                return true;
        }

        return false;
    }

    // / 集群中是否有该节点
    public bool ContainsNode(string nodeName)
    {
        foreach (var appInfo in _appInfos.Values)
        {
            if (appInfo.Nodes.ContainsKey(nodeName))
                return true;
        }

        return false;
    }
}