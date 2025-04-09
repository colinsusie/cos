// Written by Colin on 2024-8-5

namespace CoRuntime.Rpc;

/// <summary>
/// 代表一个Rpc服务，服务需要实现它的子类，RPC调用可能抛出如下异常：
///     <see cref="TimeoutException"/> RPC超时，不能确定调用成功与否 <br/>
///     <see cref="RpcException"/> RPC请求异常 <br/>
/// </summary>
public interface IRpcService;

/// <summary>
/// Rpc消息派发器，服务需要实现该接口以完成消息派发
/// </summary>
public interface IRpcDispatcher
{
    /// 派发通知类消息
    void DispatchNotify(RpcNotifyMessage msg);
    /// 派发请求类消息
    ValueTask<RpcResponseMessage> DispatchRequest(RpcRequestMessage msg);
}