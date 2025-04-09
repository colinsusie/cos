// Written by Colin on 2023-12-16

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CoLib.Container;
using CoLib.Logging;
using CoLib.Plugin;
using CoRuntime.Rpc;

namespace CoRuntime.Services;

/// <summary>
/// 服务管理器
/// </summary>
public class ServiceManager: RuntimeObject
{
    /// 模块管理
    private readonly PluginManager _pluginMgr;
    /// 用于生成新的服务ID
    private int _nextServiceId;
    /// 服务上下文
    private readonly ConcurrentDictionary<short, ServiceContext> _serviceCtxs = new();
    /// 正在创建中的服务
    private readonly ConcurrentDictionary<short, bool> _creatingServices = new();
    /// 服务名的读写锁
    private readonly ReaderWriterLockSlim _serviceRwLocker = new(LockRecursionPolicy.SupportsRecursion);
    /// 服务名到服务列表的字典
    private readonly Dictionary<string, List<short>> _serviceNameDict = new();
    /// 服务名列表，为了保证有序，名字在前的先初始化
    private readonly List<string> _serviceNameList = new();
    /// 服务选项
    public readonly ServiceManagerOptions Options;
    
#region 生命周期
    public ServiceManager()
    {
        Options = RuntimeEnv.OptionsMgr.GetOptions<ServiceManagerOptions>();
        _pluginMgr = new PluginManager(Options.ServiceBasePath, RuntimeEnv.LogMgr.GetLogger(nameof(PluginManager)));
    }
    
    /// 停止运行时
    protected override async Task DoStopAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 停止所有服务
            {
                List <List<ServiceContext>> serviceCtxsList = new();
                _serviceRwLocker.EnterReadLock();
                try
                {
                    for (var i = _serviceNameList.Count - 1; i >= 0; i--)
                    {
                        if (!_serviceNameDict.TryGetValue(_serviceNameList[i], out var serviceIds))
                            continue;
                        List<ServiceContext> serviceCtxs = new();
                        foreach (var serviceId in serviceIds)
                        {
                            if (!_serviceCtxs.TryGetValue(serviceId, out var serviceCtx))
                                continue;
                            serviceCtxs.Add(serviceCtx);
                        }
                        serviceCtxsList.Add(serviceCtxs);
                    }
                }
                finally
                {
                    _serviceRwLocker.ExitReadLock();
                }

                foreach (var serviceCtxs in serviceCtxsList)
                {
                    try
                    {
                        List<Task> tasks = new();
                        foreach (var serviceCtx in serviceCtxs)
                        {
                            tasks.Add(serviceCtx.StopAsync(cancellationToken));
                        }
                        await Task.WhenAll(tasks);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Stopping ServiceCtx error, Exception:{e}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Raise an exception:{e}");
        }
    }

    // App启动完毕，通知所有服务，注意不能保证顺序
    public void OnAppStarted()
    {
        foreach (var serviceCtx in _serviceCtxs.Values)
        {
            serviceCtx.OnAppStarted();
        }
    }
    
#endregion

#region 服务管理

    /// 生成新的服务ID
    private short GenerateServiceId()
    {
        var newId = Interlocked.Increment(ref _nextServiceId);
        if (newId > short.MaxValue)
            throw new InvalidOperationException(
                $"ServiceManager.GenerateServiceId: Service id must be in (0, {short.MaxValue}]");
        
        return (short)newId;
    }
    
    ///  创建服务，失败抛出异常，成功返回服务ID
    public async Task<short> CreateService(ServiceOptions options, IServicePlugin? servicePlugin, CancellationToken cancellationToken)
    {
        var serviceId = GenerateServiceId();
        
        if (!_creatingServices.TryAdd(serviceId, true))
            throw new InvalidOperationException($"ServiceManager.CreateService: Service currently being created, " +
                                                $"ServiceId:{serviceId}, ServiceName:{options.ServiceName}");

        try
        {
            // id 重复
            if (_serviceCtxs.ContainsKey(serviceId))
                throw new InvalidOperationException($"ServiceManager.CreateService: There already exists a service, ServiceId:{serviceId}, " +
                                                    $"ServiceName:{options.ServiceName}");

            // 如果servicePlugin为空，则需要从程序集加载
            if (servicePlugin == null)
            {
                var assemblyPath = options.AssemblyPath;
                if (_pluginMgr.GetOrLoad(assemblyPath) is not IServicePlugin plugin)
                    throw new InvalidCastException(
                        $"ServiceManager.CreateService: servicePlugin must be an IServicePlugin interface, " +
                        $"ServiceId:{serviceId}, ServiceName:{options.ServiceName}");
                servicePlugin = plugin;
            }

            // startup service
            try
            {
                var serviceCtx = new ServiceContext(serviceId, servicePlugin, options);
                await serviceCtx.StartAsync(cancellationToken);
                return serviceId;
            }
            catch (Exception e)
            {
                Logger.Error($"Error, ServiceId:{serviceId}, ServiceName:{options.ServiceName}, Exception: {e}");
                throw;
            }
        }
        finally
        {
            _creatingServices.Remove(serviceId, out _);
        }
    }
    
    /// 服务创建完毕加到列表
    internal void OnServiceCreated(ServiceContext serviceCtx)
    {
        _serviceCtxs[serviceCtx.ServiceId] = serviceCtx;
        RegisterServiceName(serviceCtx);
    }
    
    /// 尝试获取服务的上下文
    public bool TryGetServiceContext(short serviceId, [MaybeNullWhen(false)] out ServiceContext serviceCtx)
    {
        return _serviceCtxs.TryGetValue(serviceId, out serviceCtx);
    }
    
    /// 注册服务名
    private void RegisterServiceName(ServiceContext serviceCtx)
    {
        _serviceRwLocker.EnterWriteLock();
        try
        {
            if (!_serviceNameDict.TryGetValue(serviceCtx.ServiceName, out var serviceIds))
            {
                serviceIds = new List<short>();
                _serviceNameDict.Add(serviceCtx.ServiceName, serviceIds);
                _serviceNameList.Add(serviceCtx.ServiceName);
            }

            if (serviceIds.Contains(serviceCtx.ServiceId))
            {
                throw new InvalidOperationException($"ServiceManager.RegisterServiceName: ServiceId already exists, " +
                                                    $"ServiceName:{serviceCtx.ServiceName}, ServiceId:{serviceCtx.ServiceId}");
            }
                
            serviceIds.Add(serviceCtx.ServiceId);
        }
        finally
        {
            _serviceRwLocker.ExitWriteLock();
        }
    }
    
    /// 尝试通过服务名取服务ID列表
    public bool TryGetServiceIds(string serviceName, [MaybeNullWhen(false)] out List<short> serviceIds)
    {
        _serviceRwLocker.EnterReadLock();
        try
        {
            serviceIds = null;
            if (!_serviceNameDict.TryGetValue(serviceName, out var ids))
                return false;
            serviceIds = [..ids];
            return true;
        }
        finally
        {
            _serviceRwLocker.ExitReadLock();
        }
    }

    /// 尝试服务名取服务ID列表，取不到抛异常
    public List<short> GetServiceIds(string serviceName)
    {
        if (!TryGetServiceIds(serviceName, out var serviceIds) || serviceIds.Count == 0)
            throw new InvalidOperationException($"ServiceManager.GetServiceIds: Service {serviceName} not found");
        return serviceIds;
    }

    /// 尝试获取一个服务Id
    public bool TryGetServiceId(string serviceName, ServiceSelectPolicy policy, out short serviceId)
    {
        _serviceRwLocker.EnterReadLock();
        try
        {
            serviceId = 0;
            if (!_serviceNameDict.TryGetValue(serviceName, out var ids) || ids.Count == 0)
                return false;

            switch (policy)
            {
                case ServiceSelectPolicy.First:
                    serviceId = ids[0];
                    break;
                case ServiceSelectPolicy.Random:
                default:
                    var idx = Random.Shared.Next(0, ids.Count);
                    serviceId = ids[idx];
                    break;
            }
            return true;
        }
        finally
        {
            _serviceRwLocker.ExitReadLock();
        }
    }

    /// 获取一个服务Id
    public short GetServiceId(string serviceName, ServiceSelectPolicy policy)
    {
        if (!TryGetServiceId(serviceName, policy, out var serviceId))
            throw new InvalidOperationException($"ServiceManager.GetServiceId: Service {serviceName} not found");
        return serviceId;
    }

    /// 判断是否存在某个服务
    public bool HasService(string serviceName)
    {
        return TryGetServiceId(serviceName, ServiceSelectPolicy.First, out _);
    }
#endregion
}