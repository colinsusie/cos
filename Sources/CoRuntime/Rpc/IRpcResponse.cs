// Written by Colin on 2024-9-24

namespace CoRuntime.Rpc;

/// <summary>
/// 代表一个请求的响应
/// </summary>
public interface IRpcResponse
{
    /// 请求开始时间
    public TimeSpan StartTime { get; }
    /// 设置请求结果
    void SetResult(RpcResponseMessage msg);
    /// 设置请求异常
    void SetException(Exception exception);
}