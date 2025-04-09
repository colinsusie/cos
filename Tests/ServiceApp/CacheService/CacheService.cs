using CoRuntime.Services;
using CoRuntime.Services.Cluster;
using ServiceDefines;

namespace CacheService;

public partial class CacheService: Service, ICacheService
{
    private readonly Dictionary<string, string> _cache = new();
    
    public CacheService(ServiceContext serviceCtx) : base(serviceCtx)
    {
        Logger.Info($"{nameof(CacheService)} created");
    }
    
    protected override Task DoStartAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"{nameof(CacheService)} is Starting, register to cluster.");
        this.RegisterToCluster();
        return Task.CompletedTask;
    }

    protected override Task DoStopAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"{nameof(CacheService)} is Stopping.");
        return Task.CompletedTask;
    }

    public ValueTask<string?> GetValue(string key)
    {
        return _cache.TryGetValue(key, out var value) ? new ValueTask<string?>(value) : 
            ValueTask.FromResult<string?>(null);
    }

    public ValueTask SetValue(string key, string value)
    {
        _cache[key] = value;
        return ValueTask.CompletedTask;
    }

    public ValueTask<string> GetRequiredValue(string key)
    {
        if (_cache.TryGetValue(key, out var value))
            return new ValueTask<string>(value);
        throw new KeyNotFoundException($"key {key} not found");
    }

    public async ValueTask<string> GetValueTimeOut(string key)
    {
        await Task.Delay(4000);
        return "ok";
    }
}