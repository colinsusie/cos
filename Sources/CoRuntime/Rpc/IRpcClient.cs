// Written by Colin on 2024-9-11

namespace CoRuntime.Rpc;

/// <summary>
/// 定义Rpc客户端接口
/// </summary>
public interface IRpcClient
{
    string NodeName { get; }
    
    /// 生成请求Id
    int GenerateRequestId();
    
    /// 派发通知消息
    void DispatchNotify(RpcNotifyMessage msg);

    /// <summary>
    /// 派发请求消息
    /// </summary>
    /// <returns>返回响应消息</returns>
    ValueTask<TResponse> DispatchRequest<TResponse>(RpcRequestMessage msg, CancellationToken token);
    ValueTask DispatchRequest(RpcRequestMessage msg, CancellationToken token);
}