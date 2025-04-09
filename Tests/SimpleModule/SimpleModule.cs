using CoLib.Logging;
using CoLibDemo.Module;
using CoRuntime.Services;

namespace SimpleModule;

public class SimpleModule: ISimpleServiceModule
{
    public ISimpleService CreateService()
    {
        return new SimpleService();
    }
}

public class SimpleService : ISimpleService
{
    private static readonly Logger Logger = ServiceEnv.CurrContext.GetLogger(nameof(SimpleService));
    
    public void Start()
    {
        Logger.Debug($"service start!!!");
    }

    public void Stop()
    {
        Logger.Debug($"service stop!!!");
    }
}

