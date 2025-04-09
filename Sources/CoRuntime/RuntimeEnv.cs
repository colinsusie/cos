// Written by Colin on 2025-1-24

using CoLib.Logging;
using CoLib.Options;
using CoRuntime.EventLoop;
using CoRuntime.GObjects;
using CoRuntime.Rpc;
using CoRuntime.Services;

namespace CoRuntime;

/// <summary>
/// 运行时全局变量
/// </summary>
public static class RuntimeEnv
{
    /// 运行时配置
    public static RunTimeConfig Config { get; } = new();
    /// 日志管理器
    public static LogManager LogMgr { get; internal set; } = null!;
    /// 全局日志器
    public static Logger Logger { get; internal set; } = null!;
    /// 通用主机的服务提供者
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;
    /// 选项管理器
    public static OptionsManager OptionsMgr { get; internal set; } = null!;
    /// 事件循环管理器
    public static EventLoopManager EventLoopMgr { get; internal set; } = null!;
    /// RPC管理器
    public static RpcManager RpcMgr { get; internal set; } = null!;
    /// 服务管理器
    public static ServiceManager ServiceMgr { get; internal set; } = null!;
    /// 运行时对象管理器
    public static GObjectManager GObjectMgr { get; internal set; } = null!;
}