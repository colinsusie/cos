// Written by Colin on 2025-1-23

using CoLib.Logging;
using CoRuntime.Common;
using CoRuntime.EventLoop;
using CoRuntime.Rpc;
using CoRuntime.Services;
using CoRuntime.Services.Cluster;
using CoRuntime.Services.Uid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Winton.Extensions.Configuration.Consul;

namespace CoRuntime;

/// <summary>
/// 服务管理器启动器
/// </summary>
public static class RuntimeBootstrap
{
    /// 程序名，表示这一类程序
    private const string AppName = "AppName";
    /// 节点名，唯一标识这个节点
    private const string NodeName = "NodeName";
    /// 节点ID，唯一标识这个节点
    private const string NodeId = "NodeId";
    /// 本地配置目录
    private const string ConfigDir = "ConfigDir";
    /// 本地配置文件列表
    private const string Configs = "Configs";
    /// Consul地址
    private const string ConsulAddress = "ConsulAddress";
    /// Consul配置中心的配置
    private const string ConsulConfigs = "ConsulConfigs";

    /// 服务管理器启动
    public static void Startup(IRuntimeHandler handler, IServiceCollection services, 
        ConfigurationManager configMgr, string[] args)
    {
        // 初始化配置
        InitConfigs(configMgr, args);
        // 初始化日志
        InitLog(configMgr);
        // 绑定选项
        InitOptions(services, handler);
        // 启动运行时
        services.AddHostedService<RuntimeHostedService>(provider => 
            new RuntimeHostedService(provider, handler));
    }
    
    private static void InitConfigs(ConfigurationManager configMgr, string[] args)
    {
        // 初始化必要的全局变量
        var appName = configMgr.GetValue<string>(AppName);
        if (appName == null)
            throw new InvalidOperationException($"RuntimeBootstrap.Startup: Please set `{AppName}` configuration");
        RuntimeEnv.Config.AppName = appName;

        var nodeName = configMgr.GetValue<string>(NodeName);
        if (nodeName == null)
            throw new InvalidOperationException($"RuntimeBootstrap.Startup: Please set `{NodeName}` configuration");
        RuntimeEnv.Config.NodeName = nodeName;

        var nodeId = configMgr.GetValue<short>(NodeId);
        if (nodeId <= 0)
            throw new InvalidOperationException($"RuntimeBootstrap.Startup: The range value of `{NodeId}` id (0, 32767]");
        RuntimeEnv.Config.NodeId = nodeId;
        
        var consulAddress = configMgr.GetValue<string>(ConsulAddress);
        RuntimeEnv.Config.ConsulAddress = consulAddress ?? string.Empty;
        
        // 设置根路径
        var configDir = configMgr.GetValue<string>(ConfigDir);
        if (configDir == null)
            throw new InvalidOperationException($"InitOptions: Please set `{ConfigDir}` configuration");
        configMgr.SetBasePath(Path.GetFullPath(configDir));

        // 配置文件
        var configsStr = configMgr.GetValue<string>(Configs);
        if (configsStr == null)
            throw new InvalidOperationException($"InitOptions: Please set `{Configs}` configuration");
        var configs = configsStr.Split(';');
        
        // 加载Yml配置文件
        foreach (var config in configs)
        {
            configMgr.AddYamlFile(config, false, true);
        }
        
        // 使用Consul配置中心
        var consulConfigsStr = configMgr.GetValue<string>(ConsulConfigs);
        if (consulConfigsStr != null && consulAddress != null)
        {
            var consulConfigs = consulConfigsStr.Split(';');
            foreach (var config in consulConfigs)
            {
                configMgr.AddConsul(
                    config,
                    options =>
                    {
                        options.ConsulConfigurationOptions = cco =>
                        {
                            cco.Address = new Uri(consulAddress); // Consul 服务器地址
                        };
                        options.Optional = true; // 是否允许找不到配置
                        options.ReloadOnChange = true; // 是否监听 Consul 配置变更
                        options.PollWaitTime = TimeSpan.FromSeconds(60); // 轮询时间间隔
                        options.Parser = new YamlConfigurationParser();  // 支持Yaml
                        options.OnLoadException = e =>
                        {
                            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                            RuntimeEnv.Logger?.Error($"OnLoadException, e:{e}");
                        };
                        options.OnWatchException = e =>
                        {
                            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                            RuntimeEnv.Logger?.Error($"OnWatchException, e:{e}");
                            return TimeSpan.FromSeconds(10);
                        };
                    });
            }        
        }
        
        // 添加环境变量和命令行的配置
        configMgr.AddEnvironmentVariables();
        configMgr.AddCommandLine(args);
    }

    private static void InitOptions(IServiceCollection services, IRuntimeHandler handler)
    {
        RuntimeEnv.OptionsMgr = new(services);
        
        // 绑定各种选项
        RuntimeEnv.OptionsMgr.AddOptions<EventLoopOptions>(nameof(EventLoopOptions));
        RuntimeEnv.OptionsMgr.AddOptions<ServiceManagerOptions>(nameof(ServiceManagerOptions));
        RuntimeEnv.OptionsMgr.AddOptions<ServiceGroupsOptions>(nameof(ServiceGroupsOptions), false);
        RuntimeEnv.OptionsMgr.AddOptions<ClusterOptions>(nameof(ClusterOptions));
        RuntimeEnv.OptionsMgr.AddOptions<RpcOptions>(nameof(RpcOptions));
        RuntimeEnv.OptionsMgr.AddOptions<UidServiceOptions>(nameof(UidServiceOptions));
        
        // 绑定自定义的选项
        handler.InitOptions(RuntimeEnv.OptionsMgr);
    }

    // 初始化日志
    private static void InitLog(ConfigurationManager configMgr)
    {
        var logOptions = configMgr.GetSection(nameof(LogOptions)).Get<LogOptions>();
        if (logOptions == null)
            throw new InvalidOperationException($"InitLog: Please set `{nameof(LogOptions)}` configuration");
        
        var logBuilder = new LogManagerBuilder();
        logBuilder.SetMinLogLevel(logOptions.MinLogLevel);
        logBuilder.SetAsyncOutput(logOptions.AsyncOutput, logOptions.AsyncQueueCapacity);
        if (logOptions.LogConsoleOptions != null)
        {
            logBuilder.AddOutputConsole(logOptions.LogConsoleOptions.MinLogLevel);
        }

        foreach (var fileOptions in logOptions.LogFileOptions)
        {
            logBuilder.AddOutputFile(fileOptions.LogPath,
                fileOptions.MinLogLevel,
                null, fileOptions.RollOnFileSize);
        }

        var logMgr = logBuilder.Build();
        RuntimeEnv.LogMgr = logMgr;
        RuntimeEnv.Logger = logMgr.GetLogger(RuntimeEnv.Config.AppName);
    }
}