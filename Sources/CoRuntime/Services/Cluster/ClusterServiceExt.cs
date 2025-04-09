// Written by Colin on 2025-02-20

namespace CoRuntime.Services.Cluster;

public static class ClusterServiceExt
{
    /// 创建Cluster服务
    public static async Task CreateClusterService(this ServiceManager serviceMgr, CancellationToken cancellationToken)
    {
        var serviceName = nameof(ClusterService);
        if (serviceMgr.HasService(serviceName))
            throw new InvalidOperationException($"{serviceName} already exists.");
            
        var options = new ServiceOptions() {ServiceName = serviceName};
        await serviceMgr.CreateService(options, new ClusterServicePlugin(), cancellationToken);
    }

    /// 将服务注册到集群
    public static void RegisterToCluster(this Service service)
    {
        if (service.ServiceName == nameof(ClusterService))
            throw new InvalidOperationException($"Can not register {nameof(ClusterService)} to cluster");
        var serviceAddr = ServiceHelper.GetLocalServiceAddr(nameof(ClusterService));

        service.Logger.Info($"Register service to cluster, ServiceName:{service.ServiceName}, ServiceId:{service.ServiceId}");
        var proxy = ClusterServiceFactory.Create(serviceAddr);
        proxy.RegisterService(service.ServiceName, service.ServiceId);
    }
}