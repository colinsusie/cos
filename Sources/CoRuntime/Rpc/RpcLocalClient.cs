// Written by Colin on 2024-9-3

using CoRuntime.Services;

namespace CoRuntime.Rpc;

/// <summary>
/// 本地Rpc客户端
/// </summary>
public class RpcLocalClient: IRpcClient
{
    private readonly ServiceContext _dstContext;

    public RpcLocalClient(short serviceId)
    {
        if (!RuntimeEnv.ServiceMgr.TryGetServiceContext(serviceId, out var context))
        {
            throw new InvalidOperationException($"Unable to find target service: {serviceId}");
        }

        _dstContext = context;
    }

    public string NodeName => RuntimeEnv.Config.NodeName;

    public override string ToString()
    {
        return $"RpcLocalClient:{_dstContext.ServiceName}:{_dstContext.ServiceId}";
    }

    public int GenerateRequestId()
    {
        // 本地的永远是0
        return 0;
    }

    public void DispatchNotify(RpcNotifyMessage msg)
    {
        _dstContext.EventLoop.Execute((ctx, state) =>
        {
            var theService = (Service)ctx;
            var theMsg = (RpcNotifyMessage)state;
            try
            {
                if (theService.IsStopped)
                    return;
                if (theService is not IRpcDispatcher dispatcher)
                {
                    theService.Logger.Error($"Client:{this}, The server has not implemented IRpcDispatcher");
                    return;
                }
                dispatcher.DispatchNotify(theMsg);
            }
            finally
            {
                theMsg.Dispose();
            }
        }, _dstContext.Service, msg);
    }

    public ValueTask DispatchRequest(RpcRequestMessage msg, CancellationToken token)
    {
        var task =  _dstContext.EventLoop.SubmitAsync(async (ctx, state) =>
        {
            var service = (Service)ctx;
            var aMsg = (RpcRequestMessage)state;
            try
            {
                if (service.IsStopped)
                    throw new InvalidOperationException($"The service has been stopped, Service:{service}");
                
                if (service is not IRpcDispatcher dispatcher)
                {
                    service.Logger.Error($"The server has not implemented IRpcDispatcher");
                    throw new RpcException("The Server has not implemented IRpcDispatcher");
                }
                
                var rspMsg = await dispatcher.DispatchRequest(aMsg);
                rspMsg.Dispose();
            }
            finally
            {
                aMsg.Dispose();
            }
        }, _dstContext.Service, msg, token).Unwrap().WaitAsync(token);
        
        return new ValueTask(task);
    }

    public ValueTask<TResponse> DispatchRequest<TResponse>(RpcRequestMessage msg, CancellationToken token)
    {
        Task<TResponse> task = _dstContext.EventLoop.SubmitAsync(async (ctx, state) =>
        {
            var service = (Service)ctx;
            var aMsg = (RpcRequestMessage)state;
            RpcResponseMessage? rspMsg = null;
            try
            {
                if (service.IsStopped)
                    throw new InvalidOperationException($"The service has been stopped, Service:{service}");
                
                if (service is not IRpcDispatcher dispatcher)
                {
                    service.Logger.Error($"The server has not implemented IRpcDispatcher");
                    throw new RpcException("The Server has not implemented IRpcDispatcher");
                }
                
                rspMsg = await dispatcher.DispatchRequest(aMsg);
                var rsp = RpcMessageSerializer.Deserialize1<TResponse>(rspMsg);
                return rsp!;
            }
            finally
            {
                aMsg.Dispose();
                rspMsg?.Dispose();
            }
        }, _dstContext.Service, msg, token).Unwrap().WaitAsync(token);

        return new ValueTask<TResponse>(task);
    }
}