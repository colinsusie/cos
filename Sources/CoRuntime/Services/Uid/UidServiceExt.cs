// Written by Colin on 2025-02-20

namespace CoRuntime.Services.Uid;

public static class UidServiceExt
{
    /// 创建Cluster服务
    public static async Task CreateUidService(this ServiceManager serviceMgr, CancellationToken cancellationToken)
    {
        var serviceName = nameof(UidService);
        if (serviceMgr.HasService(serviceName))
            throw new InvalidOperationException($"{serviceName} already exists.");
            
        var options = new ServiceOptions() {ServiceName = serviceName};
        await serviceMgr.CreateService(options, new UidServicePlugin(), cancellationToken);
    }
}