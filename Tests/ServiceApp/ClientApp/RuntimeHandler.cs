// Written by Colin on 2025-1-24

using ClientApp.ClientService;
using CoRuntime;
using CoRuntime.Services;
using CoRuntime.Services.Cluster;
using CoRuntime.Services.Uid;

namespace ClientApp;

public class RuntimeHandler: BaseRuntimeHandler
{
    public override void OnStarted()
    {
        RuntimeEnv.Logger.Info($"{RuntimeEnv.Config.AppName} started");
    }

    public override void OnStopped()
    {
        RuntimeEnv.Logger.Info($"{RuntimeEnv.Config.AppName} stopped");
    }

    public override async Task InitServices(CancellationToken cancellationToken)
    {
        await RuntimeEnv.ServiceMgr.CreateClusterService(cancellationToken);
        await RuntimeEnv.ServiceMgr.CreateUidService(cancellationToken);
        await RuntimeEnv.ServiceMgr.CreateService(new ServiceOptions()
        {
            ServiceName = nameof(ClientService),
        }, new ClientServicePlugin(), cancellationToken);
    }
}