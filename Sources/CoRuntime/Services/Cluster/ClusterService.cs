// Written by Colin on 2025-02-04

using Consul;
using System.Text.Json;

namespace CoRuntime.Services.Cluster;

/// <summary>
/// Consul服务：用于服务发现
/// </summary>
public partial class ClusterService: Service, IClusterService
{
    private const string MetaKeyServices = "Servies";
    
    private readonly ClusterOptions _options;
    private readonly IConsulClient _consulClient;
    /// 服务信息是否有更改
    private bool _isModified = true;
    private readonly Dictionary<string, List<short>> _services = new ();
    private readonly ClusterManager _clusterMgr = new();
    
    public ClusterService(ServiceContext serviceCtx) : base(serviceCtx)
    {
        _options = RuntimeEnv.OptionsMgr.GetOptions<ClusterOptions>();
        _consulClient = new ConsulClient(config =>
        {
            config.Address = new Uri(RuntimeEnv.Config.ConsulAddress);
        });
        
        RuntimeEnv.GObjectMgr.Register(_clusterMgr);
    }

    protected override async Task DoStartAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"{nameof(ClusterService)} is starting");
        
        // 启动后主动取一下
        foreach (var appOptions in _options.WatchApps)
        {
            await WatchApp(_consulClient, appOptions.AppName, QueryOptions.Default, cancellationToken);
        }
    }

    public override void OnAppStarted()
    {
        PostTask(StartRegisterNode);
        PostTask(StartUpdateNodeStatus);
        foreach (var appOptions in _options.WatchApps)
        {
            PostTask(StartWatchApp, appOptions);
        }
    }

    protected override async Task DoStopAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"{nameof(ClusterService)} is stopping.");

        Logger.Info($"Consul deregister service, Id:{RuntimeEnv.Config.NodeName}");
        try
        {
            await _consulClient.Agent.ServiceDeregister(RuntimeEnv.Config.NodeName, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.Error($"ServiceDeregister error, Exception:{e}");
        }
        finally
        {
            _consulClient.Dispose();
        }
    }

    /// 持续注册
    private async Task StartRegisterNode(CancellationToken cancellationToken)
    {
        if (!_options.RegisterNode)
            return;
        
        await Task.Delay(500, cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_isModified)
            {
                _isModified = false;
                var succeed = await RegisterNode(_consulClient, cancellationToken);
                if (!succeed)
                {
                    // 注册失败，等一会再注册
                    _isModified = true;
                }
            }
            await Task.Delay(_options.UpdateInterval, cancellationToken);
        }
    }

    /// 注册
    private async Task<bool> RegisterNode(IConsulClient consulClient, CancellationToken cancellationToken)
    {
        if (!_options.RegisterNode)
            return true;
        
        try
        {
            var registration = new AgentServiceRegistration
            {
                ID = RuntimeEnv.Config.NodeName,
                Name = RuntimeEnv.Config.AppName,
                Address = RuntimeEnv.RpcMgr.Options.Host,
                Port = RuntimeEnv.RpcMgr.Options.Port,
                Meta = new Dictionary<string, string>
                {
                    {MetaKeyServices, JsonSerializer.Serialize(_services)}
                },
                Check = new AgentServiceCheck
                {
                    TTL = _options.StatusTtl,  // 通过TTL（Time-to-Live）机制检查服务的健康状态
                    DeregisterCriticalServiceAfter = _options.DeregisterTtl
                }
            };
            
            Logger.Info($"Consul register node，Registration:{JsonSerializer.Serialize(registration)}");
            var result = await consulClient.Agent.ServiceRegister(registration, cancellationToken);
            Logger.Info($"Consul register node, Result:{result.StatusCode}");
            return result.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (OperationCanceledException e)
        {
            Logger.Info($"Task cancel: {e}");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error($"Error, Exception:{e}");
            return false;
        }
    }

    /// 执行更新状态
    private async Task StartUpdateNodeStatus(CancellationToken cancellationToken)
    {
        if (!_options.RegisterNode)
            return;
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_options.StatusInterval, cancellationToken);
            await UpdateNodeStatus(_consulClient, cancellationToken);
        }
    }

    /// 更新服务状态
    private async Task UpdateNodeStatus(IConsulClient consulClient, CancellationToken cancellationToken)
    {
        if (!_options.RegisterNode)
            return;
        
        try
        {
            // TODO: 根据不同情况更新状态，比如过载等
            await consulClient.Agent.PassTTL($"service:{RuntimeEnv.Config.NodeName}", "Node is healthy", cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Logger.Info($"Task cancel: {e}");
        }
        catch (Exception e)
        {
            Logger.Error($"Error, Exception:{e}");
        }
    }

    /// 持续监控一类APP的服务
    private async Task StartWatchApp(ClusterAppOptions appOptions, CancellationToken cancellationToken)
    {
        // 定义Watch参数
        var queryOptions = new QueryOptions
        {
            WaitIndex = 0,                          // 初始索引，设置为0表示从头开始监听
            WaitTime = _options.WatchTime           // 长轮询的超时时间
        };
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await WatchApp(_consulClient, appOptions.AppName, queryOptions, cancellationToken);
            await Task.Delay(_options.WatchInterval, cancellationToken);
        }
    }

    private async Task WatchApp(IConsulClient consulClient, string appName, QueryOptions queryOptions,
        CancellationToken cancellationToken)
    {
        try
        {
            // 使用Watch机制监听服务变化
            var services = await consulClient.Health.Service(appName, string.Empty, true, queryOptions, cancellationToken);
            if (services.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Logger.Error($"Health.Service error, StatusCode:{services.StatusCode}");
                return;
            }

            if (services.LastIndex == queryOptions.WaitIndex)
            {
                // 没有变化
                return;
            }

            // 更新WaitIndex，以便下次请求从最新的变化开始
            queryOptions.WaitIndex = services.LastIndex;

            // 处理服务变化
            Logger.Info($"{appName} changed, WaitIndex:{queryOptions.WaitIndex}");

            var appInfo = new ClusterAppInfo(appName, services.LastIndex, IsAppEnableRpc(appName));
            foreach (var serviceEntry in services.Response)
            {
                var service = serviceEntry.Service;
                Logger.Debug($"Service:{JsonSerializer.Serialize(serviceEntry.Service)}");
                var nodeInfo = new ClusterNodeInfo(service.ID, service.Address, service.Port);
                appInfo.Nodes.Add(service.ID, nodeInfo);
                
                var nodeServices = JsonSerializer.Deserialize<Dictionary<string, List<short>>>(service.Meta[MetaKeyServices])!;
                foreach (var (serviceName, serviceIds) in nodeServices)
                {
                    if (!appInfo.Services.TryGetValue(serviceName, out var serviceInfos))
                    {
                        serviceInfos = new();
                        appInfo.Services[serviceName] = serviceInfos;
                    }
                    foreach (var serviceId in serviceIds)
                    {
                        serviceInfos.Add(new ClusterServiceInfo(service.ID, serviceId));
                    }
                }
            }
            
            // 更新集群管理器
            _clusterMgr.SetAppInfo(appInfo);
            
            // 更新RPC
            UpdateRpc(appName, appInfo);
        }
        catch (OperationCanceledException e)
        {
            Logger.Info($"Task cancel: {e}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error, Exception:{ex}");
        }
    }

    private bool IsAppEnableRpc(string appName)
    {
        foreach (var appOptions in _options.WatchApps)
        {
            if (appOptions.AppName != appName)
                continue;
            return appOptions.EnableRpc;
        }

        return false;
    }

    private void UpdateRpc(string appName, ClusterAppInfo appInfo)
    {
        if (appInfo.EnableRpc)
        {
            RuntimeEnv.RpcMgr.UpdateClients(appInfo.Nodes, _clusterMgr);
        }
    }

    #region IClusterService

    public void RegisterService(string serviceName, short serviceId)
    {
        if (!_services.TryGetValue(serviceName, out var services))
        {
            services = new();
            _services[serviceName] = services;
        }

        if (!services.Contains(serviceId))
        {
            services.Add(serviceId);
            _isModified = true;
        }
    }
    
    #endregion
}