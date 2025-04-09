// Written by Colin on 2023-11-08

using System.Reflection;
using System.Runtime.Loader;
using CoLib.Logging;

namespace CoLib.Plugin;

/// <summary>
/// 插件加载上下文
/// </summary>
internal class PluginLoadContext:  AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly AssemblyLoadContext _defaultContext = GetLoadContext(Assembly.GetExecutingAssembly()) ?? Default;
    private readonly Logger _logger;
    
    public PluginLoadContext(Logger logger, string modulePath) : base(name: Path.GetFileName(modulePath))
    {
        _logger = logger;
        _resolver = new AssemblyDependencyResolver(modulePath);
    }
    
    protected override Assembly? Load(AssemblyName asmName)
    {
        var assembly = LoadDefault(asmName);
        if (assembly != null)
            return assembly;
        
        var asmPath = _resolver.ResolveAssemblyToPath(asmName);
        return asmPath == null ? null : LoadFromAssemblyPath(asmPath);
    }

    private Assembly? LoadDefault(AssemblyName asmName)
    {
        try
        {
            return _defaultContext.LoadFromAssemblyName(asmName);
        }
        catch (Exception e)
        {
            _logger.Error($"Load default assembly failed, AssemblyName:{asmName}, Exception:{e}");
            return null;
        }
    }
}