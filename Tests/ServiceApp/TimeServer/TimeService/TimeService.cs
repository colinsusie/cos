using CoRuntime.Services;
using CoRuntime.Services.Cluster;
using CoRuntime.Services.ServiceDefines;

namespace TimeServer.TimeService;

public partial class TimeService: Service, ITimeService
{
    public TimeService(ServiceContext serviceCtx) : base(serviceCtx)
    {
        
    }

    protected override Task DoStartAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"{nameof(TimeService)} is Starting, register to cluster");
        this.RegisterToCluster();
        return Task.CompletedTask;
    }

    protected override Task DoStopAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"{nameof(TimeService)} is Stopping");
        return Task.CompletedTask;
    }

    public ValueTask<long> GetUtcTimestamp()
    {
        var now = DateTimeOffset.UtcNow;
        var timestamp = now.ToUnixTimeSeconds();
        Logger.Debug($"Current UTC timestamp:{timestamp}, Datetime:{now}");
        return ValueTask.FromResult(timestamp);
    }
}