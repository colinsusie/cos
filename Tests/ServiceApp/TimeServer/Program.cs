using CoRuntime;
using CoRuntime.Services;
using CoRuntime.Services.Cluster;
using Microsoft.Extensions.Hosting;
using TimeServer.TimeService;

namespace TimeServer;

class Program
{
    static async Task Main(string[] args)
    {
        var appBuilder = Host.CreateApplicationBuilder(args);
        RuntimeBootstrap.Startup(new RuntimeHandler(), appBuilder.Services, appBuilder.Configuration, args);
        var host = appBuilder.Build();
        await host.RunAsync();
    }
}

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
        
        // 时间服务
        var serviceOptions = new ServiceOptions()
        {
            ServiceName = nameof(TimeService),
        };
        await RuntimeEnv.ServiceMgr.CreateService(serviceOptions, new TimerServicePlugin(), cancellationToken);
    }
}