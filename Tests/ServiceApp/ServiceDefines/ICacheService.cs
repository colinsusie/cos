using CoRuntime.Rpc;

namespace ServiceDefines;

/// <summary>
/// 定义缓存服务
/// </summary>
public interface ICacheService: IRpcService
{
    /// 取值
    ValueTask<string?> GetValue(string key);
    /// 设值
    ValueTask SetValue(string key, string value);
    /// 取一定有的值，如果没有会抛异常
    ValueTask<string> GetRequiredValue(string key);
    /// 测试超时
    ValueTask<string> GetValueTimeOut(string key);
}