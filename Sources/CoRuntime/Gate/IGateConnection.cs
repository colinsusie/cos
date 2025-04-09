// Written by Colin on 2024-11-26

namespace CoRuntime.Gate;

/// <summary>
/// 代表一个网关连接
/// </summary>
public interface IGateConnection
{
    // 派发通知消息
    void DispatchNotify(GateNotifyMessage msg);
    // 派发请求消息
    ValueTask<GateResponseMessage> DispatchRequest(GateRequestMessage msg);
}