// Written by Colin on 2023-11-08

using System.Data;
using CoLib.Logging;

namespace CoLib.Plugin;

/// <summary>
/// 插件管理器
/// </summary>
public class PluginManager
{
    private readonly string _basePath;
    private readonly Dictionary<string, IPlugin> _plugins = new();
    private readonly Logger _logger;

    public PluginManager(string basePath, Logger logger)
    {
        _basePath = Path.GetFullPath(basePath);
        _logger = logger;
    }
    
    /// <summary>
    /// 通过程序集名取插件接口，如果未加载尝试先加载
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IPlugin GetOrLoad(string assemblyPath)
    {
        lock (this)
        {
            if (_plugins.TryGetValue(assemblyPath, out var plugin))
            {
                return plugin;
            }

            var assemblyFullPath = Path.Combine(_basePath, assemblyPath);
            var loader = new PluginLoadContext(_logger, assemblyFullPath);
            var assembly = loader.LoadFromAssemblyPath(assemblyFullPath);

            var assemblyTypes = assembly.ExportedTypes.Where(t => typeof(IPlugin).IsAssignableFrom(t)).ToList();
            switch (assemblyTypes.Count)
            {
                case 0:
                    throw new InvalidConstraintException(
                        $"Assembly must have a type that implements the IPlugin interface, AssemblyPath:{assemblyFullPath}");
                case > 1:
                    throw new InvalidConstraintException("Assembly can only have one type that implements " +
                                                         $"the IPlugin interface, AssemblyPath:{assemblyFullPath}, Types:{string.Join(",", assemblyTypes)}");
            }

            var assemblyType = assemblyTypes.First();
            plugin = Activator.CreateInstance(assemblyType) as IPlugin;
            if (plugin == null)
            {
                throw new InvalidOperationException(
                    $"Plugin construction failed, AssemblyPath:{assemblyFullPath}, Type:{assemblyType}");
            }

            _logger.Info($"Plugin load succeed, AssemblyPath:{assemblyFullPath}, Type:{assemblyType}");
            _plugins[assemblyPath] = plugin;
            return plugin;
        }
    }
}