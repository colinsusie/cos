// Written by Colin on 2025-1-24

using CoRuntime;
using CoRuntime.Services.Cluster;

namespace ServiceApp;

public class RuntimeHandler: BaseRuntimeHandler
{
    public override async Task InitServices(CancellationToken cancellationToken)
    {
        await RuntimeEnv.ServiceMgr.CreateClusterService(cancellationToken);
    }
}