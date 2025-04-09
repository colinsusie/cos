using CoLib.UniqueId;
using CoRuntime;
using CoRuntime.Services;
using ServiceDefines;

namespace ClientApp.ClientService;

public class ClientService: Service
{
    public ClientService(ServiceContext serviceCtx) : base(serviceCtx)
    {
    }

    protected override Task DoStartAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"Hello world");
        PostTask(StartTest);
        return Task.CompletedTask;
    }

    protected override Task DoStopAsync(CancellationToken cancellationToken)
    {
        Logger.Info($"Goodbye world");
        return Task.CompletedTask;
    }

    private async Task StartTest(CancellationToken cancellationToken)
    {
        var addr = ServiceHelper.GetClusterServiceAddr("CacheService");
        var cacheService = CacheServiceFactory.Create(addr);
        
        RuntimeEnv.GObjectMgr.TryGet<UidManager>(out var uidManager);
        
        // while (!cancellationToken.IsCancellationRequested)
        {
            var key = "hello";
            var value = "world";
            Logger.Info($"CacheService.SetValue: {key} {value}");
            await cacheService.SetValue(key, value, cancellationToken);

            var value1 = await cacheService.GetValue(key, cancellationToken);
            Logger.Info($"CacheService.GetValue, key:{key}, value:{value1}");

            var key2 = "nothing";
            var value2 = await cacheService.GetValue(key2, cancellationToken);
            Logger.Info($"CacheService.GetValue, key:{key2}, value:{value2}");

            var value3 = await cacheService.GetRequiredValue(key, cancellationToken);
            Logger.Info($"CacheService.GetRequiredValue, key:{key}, value:{value3}");

            try
            {
                var value4 = await cacheService.GetRequiredValue(key2, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.Info($"CacheService.GetRequiredValue, key:{key2}, exception:{e}");
            }

            if (uidManager != null)
            {
                var uid = uidManager.GenerateUid();
                Logger.Info($"uid:{uid}");
            }
            // try
            // {
            //     var value4 = await cacheService.GetValueTimeOut(key2);
            // }
            // catch (Exception e)
            // {
            //     Logger.Info($"CacheService.GetValueTimeOut, key:{key2}, exception:{e}");
            // }
            
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}