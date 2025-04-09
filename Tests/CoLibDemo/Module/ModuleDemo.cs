// Written by Colin on 2023-11-08

using CoLib.Extensions;
using CoLib.Logging;
using CoLib.Logging.OutputTargets;
using CoLib.Plugin;

namespace CoLibDemo.Module;

public interface ISimpleServicePlugin : IPlugin
{
    public ISimpleService CreateService();
}

public interface ISimpleService
{
    public void Start();
    public void Stop();
}



public static class ModuleDemo
{
    public static void Start()
    {
        var logMgr = new LogManagerBuilder()
            .AddOutputConsole()
            .Build();
        var logger = logMgr.GetLogger("ModuleDemo");
        
        try
        {
            var loadMgr = new PluginManager(@"..\..\..\..\SimpleModule\bin\Debug\net8.0", logMgr.GetLogger(nameof(PluginManager)));
            var module = loadMgr.GetOrLoad("SimpleModule.dll") as ISimpleServicePlugin;
            if (module == null)
            {
                throw new Exception("Get ISimpleServiceModule failed");
            }
            
            var service = module.CreateService();
            service.Start();
            service.Stop();
        }
        catch (Exception e)
        {
            logger.Error($"{e.GetFullStacktrace()}");
        }
        finally
        {
            logger.Debug($"Bye!");
            logMgr.Stop();
        }
    }
}