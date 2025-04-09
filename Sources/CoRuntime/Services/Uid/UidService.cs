// Written by Colin on 2025-02-20

using CoLib.UniqueId;

namespace CoRuntime.Services.Uid;

public partial class UidService: Service, IUidTimeProvider
{
    private UidManager _uidManager = null!;
    
    public UidService(ServiceContext serviceCtx) : base(serviceCtx)
    {
    }
    
    protected override async Task DoStartAsync(CancellationToken cancellationToken)
    {
        var options = RuntimeEnv.OptionsMgr.GetOptions<UidServiceOptions>();
        if (options.UseTimeService)
        {
            // 使用时间服务
            Logger.Info($"Use TimeService");
            await GetTimestampFromTimeService(cancellationToken);
        }
        else
        {
            // 使用本地时间
            Logger.Info($"Use local timestamp");
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
        _uidManager = new(RuntimeEnv.LogMgr.GetLogger(nameof(UidManager)), this,
            new UidOptions
            {
                BaseTimestamp = options.BaseTimestamp,
                IncIdBits = options.IncIdBits,
                NodeId = RuntimeEnv.Config.NodeId,
                NodeIdBits = options.NodeIdBits,
                TimestampBits = options.TimestampBits,
            });
        RuntimeEnv.GObjectMgr.Register(_uidManager);
        
        ExecTask(StartTick);
    }

    // 开始Tick更新UID
    private async Task StartTick(CancellationToken cancellationToken)
    {
        while (!IsStoppingOrStopped)
        {
            _uidManager.Tick();
            await Task.Delay(1000, cancellationToken);
        }
    }

    public long Timestamp { get; private set; }
    
    private async Task GetTimestampFromTimeService(CancellationToken cancellationToken)
    {
        // 先取时间服务的地址
        ServiceAddr addr;
        while (true)
        {
            if (!ServiceHelper.TryGetClusterServiceAddr("TimeService", ServiceSelectPolicy.First, out addr))
            {
                Logger.Warn($"Can not get TimeService address, wait a moment");
                await Task.Delay(3000, cancellationToken);
                continue;
            }
            break;
        }

        // 尝试取时间服务的时间
        var timeService = TimeServiceFactory.Create(addr);
        long? timestamp = null;
        for (var i = 0; i < 5; ++i)
        {
            try
            {
                timestamp = await timeService.GetUtcTimestamp(cancellationToken);
                break;
            }
            catch (Exception e)
            {
                // 抛出异常，等一会再重试
                Logger.Error($"GetUtcTimestamp raise an exception: {e}");
                await Task.Delay(3000, cancellationToken);
            }
        }
        
        if (!timestamp.HasValue)
            throw new Exception("UidService.DoStartAsync: Can not get TimeService timestamp");
        
        Logger.Info($"Get timestamp from TimeService done, Timestamp: {timestamp.Value}, DateTime: {DateTimeOffset.FromUnixTimeSeconds(timestamp.Value)}");
        Timestamp = timestamp.Value;
    }
}