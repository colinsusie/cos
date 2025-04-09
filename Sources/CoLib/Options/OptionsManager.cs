// Written by Colin on 2025-1-23

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoLib.Options;

/// <summary>
/// 选项管理器
/// </summary>
public class OptionsManager
{
    private IServiceCollection? _serviceCol;
    private IServiceProvider? _provider;

    public OptionsManager(IServiceCollection serviceCol)
    {
        _serviceCol = serviceCol;
    }

    public void SetServiceProvider(IServiceProvider provider)
    {
        _serviceCol = null;
        _provider = provider;
    }

    /// 增加一个选项，configName是与选项关联的配置名，requireConfig表示配置必须存在
    /// 如果不存在在取配置时会报错
    /// 返回OptionsBuilder，外部可以再进行数据正确性验证
    public OptionsBuilder<T> AddOptions<T>(string configName, bool requireConfig = true) where T : class
    {
        if (_serviceCol is null)
            throw new InvalidOperationException($"Options initialization has been completed");
        
        var builder = _serviceCol.AddOptions<T>();
        builder.Configure<IConfiguration>((opts, config) =>
        {
            IConfigurationSection section = config.GetSection(configName);
            if (requireConfig && !section.Exists())
            {
                throw new InvalidOperationException($"Missing configuration: {configName}");
            }
            section.Bind(opts, null);
        });
        return builder;
    }
    
    /// 尝试取一个选项
    public bool TryGetOptions<T>([MaybeNullWhen(false )] out T option) where T: class
    {
        if (_provider is null)
            throw new InvalidOperationException($"Options initialization is not yet completed");
        
        if (_provider.GetService(typeof(IOptions<T>)) is not IOptions<T> opt)
        {
            option = null;
            return false;
        }
        option = opt.Value;
        return true;
    }
    
    /// 取选项
    public T GetOptions<T>() where T: class
    {
        if (_provider is null)
            throw new InvalidOperationException($"Options initialization is not yet completed");
        
        var opt = _provider.GetRequiredService<IOptions<T>>();
        return opt.Value;
    }
    
    /// 尝试取选项监控器
    public bool TryGetOptionsMonitor<T>([MaybeNullWhen(false )] out IOptionsMonitor<T> monitor) where T: class
    {
        if (_provider is null)
            throw new InvalidOperationException($"Options initialization is not yet completed");
        
        monitor = _provider.GetService(typeof(IOptionsMonitor<T>)) as IOptionsMonitor<T>;
        return monitor != null;
    }

    /// 取选项监控器
    public IOptionsMonitor<T> GetOptionsMonitor<T>() where T: class
    {
        if (_provider is null)
            throw new InvalidOperationException($"Options initialization is not yet completed");
        
        return _provider.GetRequiredService<IOptionsMonitor<T>>();
    }
}