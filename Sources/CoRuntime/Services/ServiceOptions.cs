// Written by Colin on ${2023}-12-17

namespace CoRuntime.Services;

/// <summary>
/// 服务管理器选项
/// </summary>
public sealed class ServiceManagerOptions
{
    /// 服务插件的基础路径
    public string ServiceBasePath { get; set; }= string.Empty;
}

/// 要运行的服务组列表
public sealed class ServiceGroupsOptions
{
    public List<ServiceGroupOptions> Groups { get; set; } = new();
}

/// 服务组配置
public sealed class ServiceGroupOptions
{
    /// 服务名，需要在服务组中列表中唯一，通过服务名可以获得服务组的ID列表
    public string Name { get; set; } = string.Empty;
    /// 服务程序集路径
    public string AssemblyPath {get; set;} = string.Empty;
    /// 服务组数量
    public int Count { get; set; }
    /// 服务组所属的事件循环组
    public string EventLoopGroup { get; set; } = string.Empty;
}

/// <summary>
/// 服务配置
/// </summary>
public sealed class ServiceOptions
{
    /// 服务名，代表一个服务组的唯一名字
    public string ServiceName { get; set; } = string.Empty;
    /// 服务所属的程序集路径
    public string AssemblyPath { get; set; } = string.Empty;
    /// 所属的事件循环组，为空字符串表示在全部事件循环组里随机选一个
    public string EventLoopGroup { get; set; } = string.Empty;
}