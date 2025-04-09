// Written by Colin on 2025-1-24

using CoLib.Logging;
using CoRuntime.EventLoop;
using CoRuntime.GObjects;
using CoRuntime.Rpc;
using CoRuntime.Services;
using CoRuntime.Services.Cluster;
using Microsoft.Extensions.Hosting;

namespace CoRuntime;

/// <summary>
/// 运行时主机服务
/// </summary>
public class RuntimeHostedService: IHostedService
{
    private readonly IRuntimeHandler _handler;
    private readonly Logger _logger;

    public RuntimeHostedService(IServiceProvider provider, IRuntimeHandler handler)
    {
        _handler = handler;
        _logger = RuntimeEnv.LogMgr.GetLogger(nameof(RuntimeHostedService));
        RuntimeEnv.ServiceProvider = provider;
        RuntimeEnv.OptionsMgr.SetServiceProvider(provider);
        RuntimeEnv.GObjectMgr = new GObjectManager();
        RuntimeEnv.EventLoopMgr = new EventLoopManager();
        RuntimeEnv.RpcMgr = new RpcManager();
        RuntimeEnv.ServiceMgr = new ServiceManager();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await RuntimeEnv.EventLoopMgr.StartAsync(cancellationToken);
        await RuntimeEnv.RpcMgr.StartAsync(cancellationToken);
        await RuntimeEnv.ServiceMgr.StartAsync(cancellationToken);
        await RuntimeEnv.GObjectMgr.StartAsync(cancellationToken);
        await _handler.InitServices(cancellationToken);
        await InitServices(cancellationToken);
        RuntimeEnv.ServiceMgr.OnAppStarted();
        _handler.OnStarted();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await RuntimeEnv.ServiceMgr.StopAsync(cancellationToken);
        await RuntimeEnv.GObjectMgr.StopAsync(cancellationToken);
        await RuntimeEnv.RpcMgr.StopAsync(cancellationToken);
        await RuntimeEnv.EventLoopMgr.StopAsync(cancellationToken);
        _handler.OnStopped();
        RuntimeEnv.LogMgr.Stop();
    }

    /// 初始化配置服务
    private async Task InitServices(CancellationToken cancellationToken)
    {
        // create service groups
        var options = RuntimeEnv.OptionsMgr.GetOptions<ServiceGroupsOptions>();
        foreach (var serviceGroupOptions in options.Groups)
        {
            _logger.Info($"Create service group, ServiceName:{serviceGroupOptions.Name}, " +
                         $"Count:{serviceGroupOptions.Count}, EventLoopGroup:{serviceGroupOptions.EventLoopGroup}");
            
            var tasks = new List<Task>();
            for (var i = 0; i < serviceGroupOptions.Count; ++i)
            {
                var serviceOptions = new ServiceOptions
                {
                    AssemblyPath = serviceGroupOptions.AssemblyPath,
                    EventLoopGroup = serviceGroupOptions.EventLoopGroup,
                    ServiceName = serviceGroupOptions.Name,
                };
                var task = RuntimeEnv.ServiceMgr.CreateService(serviceOptions, null, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
    }
}