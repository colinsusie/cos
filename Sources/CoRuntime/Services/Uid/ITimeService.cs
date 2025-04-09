// Written by Colin on 2025-02-07

using CoRuntime.Rpc;

namespace CoRuntime.Services.ServiceDefines;

/// <summary>
/// 时间服务
/// </summary>
public interface ITimeService: IRpcService
{
    /// 取当前UTC时间戳
    ValueTask<long> GetUtcTimestamp();
}