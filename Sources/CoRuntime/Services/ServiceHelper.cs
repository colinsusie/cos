// Written by Colin on 2025-2-9

using CoRuntime.Services.Cluster;

namespace CoRuntime.Services;

/// <summary>
/// 服务辅助函数
/// </summary>
public static class ServiceHelper
{
    /// 尝试取一个本地服务的地址
    public static bool TryGetLocalServiceAddr(string serviceName, ServiceSelectPolicy policy, out ServiceAddr serviceAddr)
    {
        serviceAddr = default;
        if (!RuntimeEnv.ServiceMgr.TryGetServiceId(serviceName, policy, out var serviceId))
            return false;

        serviceAddr = new ServiceAddr
        {
            NodeName = RuntimeEnv.Config.NodeName,
            ServiceId = serviceId
        };
        return true;
    }
    
    /// 取一个本地服务的地址
    public static ServiceAddr GetLocalServiceAddr(string serviceName, ServiceSelectPolicy policy = ServiceSelectPolicy.First)
    {
        var serviceId = RuntimeEnv.ServiceMgr.GetServiceId(serviceName, policy);
        return new ServiceAddr()
        {
            NodeName = RuntimeEnv.Config.NodeName,
            ServiceId = serviceId
        };
    }

    /// 尝试取集群中的一个服务
    public static bool TryGetClusterServiceAddr(string serviceName, ServiceSelectPolicy policy, out ServiceAddr serviceAddr)
    {
        serviceAddr = default;
        if (!RuntimeEnv.GObjectMgr.TryGet<ClusterManager>(out var clusterManager))
            return false;
        
        return clusterManager.TryGetServiceAddr(serviceName, policy, out serviceAddr);
    }

    /// 取一个集群中的服务
    public static ServiceAddr GetClusterServiceAddr(string serviceName, ServiceSelectPolicy policy = ServiceSelectPolicy.First)
    {
        if (!TryGetClusterServiceAddr(serviceName, policy, out var serviceAddr))
            throw new InvalidOperationException($"ServiceHelper.GetClusterServiceAddr, {serviceName} not found");
        return serviceAddr;
    }
    
    /// 尝试取集群中的一个服务
    public static bool TryGetClusterServiceAddr(string appName, string serviceName, ServiceSelectPolicy policy, out ServiceAddr serviceAddr)
    {
        serviceAddr = default;
        if (!RuntimeEnv.GObjectMgr.TryGet<ClusterManager>(out var clusterManager))
            return false;
        
        return clusterManager.TryGetServiceAddr(appName, serviceName, policy, out serviceAddr);
    }

    /// 取一个集群中的服务
    public static ServiceAddr GetClusterServiceAddr(string appName, string serviceName, ServiceSelectPolicy policy = ServiceSelectPolicy.First)
    {
        if (!TryGetClusterServiceAddr(appName, serviceName, policy, out var serviceAddr))
            throw new InvalidOperationException($"ServiceHelper.GetClusterServiceAddr, {serviceName} not found");
        return serviceAddr;
    }
}