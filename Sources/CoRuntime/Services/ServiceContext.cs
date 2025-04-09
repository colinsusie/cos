// Written by Colin on 2023-12-16

using CoLib.Container;
using CoLib.EventLoop;
using CoLib.Extensions;
using CoLib.Logging;

namespace CoRuntime.Services;

/// <summary>
/// 服务上下文
/// </summary>
public class ServiceContext: RuntimeObject
{
    private readonly IServicePlugin _plugin;
    
    private Service? _service;
    public Service Service => _service.NotNull();
    
    public readonly short ServiceId;
    public string ServiceName => Options.ServiceName;
    public readonly ServiceOptions Options;
    public readonly StEventLoop EventLoop;

    internal ServiceContext(short serviceId, IServicePlugin plugin, ServiceOptions options)
    {
        ServiceId = serviceId;
        Options = options;
        
        EventLoop = RuntimeEnv.EventLoopMgr.SelectEventLoop(options.EventLoopGroup);
        _plugin = plugin;
    }
    
    // 启动服务
    protected override async Task DoStartAsync(CancellationToken cancellationToken)
    {
        await EventLoop.SubmitAsync(async () =>
        {
            // EventLoop
            _service = _plugin.CreateService(this);
            RuntimeEnv.ServiceMgr.OnServiceCreated(this);
            
            // 启动服务
            Logger.Info($"Start service, ServiceId:{ServiceId}, ServiceName:{Options.ServiceName}");
            await _service.StartAsync(cancellationToken);
            return true;
        }, cancellationToken).Unwrap();
    }

    // 停止服务
    protected override async Task DoStopAsync(CancellationToken cancellationToken)
    {
        await EventLoop.SubmitAsync(async () =>
        {
            if (_service != null)
            {
                await _service.StopAsync(cancellationToken);
            }
        }, cancellationToken).Unwrap();
    }

    // App启动完毕
    public void OnAppStarted()
    {
        EventLoop.Execute(() =>
        {
            if (_service is {IsStoppingOrStopped: false})
                _service.OnAppStarted();
        });
    }

    /// 取一个日志器
    public Logger GetLogger(string tag)
    {
        return RuntimeEnv.LogMgr.GetLogger($"{tag}:{ServiceId}");
    }
}