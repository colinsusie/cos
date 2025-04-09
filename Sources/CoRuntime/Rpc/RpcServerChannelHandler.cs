// Written by Colin on 2024-8-24

using System.Threading.Channels;
using CoLib.Logging;
using CoRuntime.Services;
using DotNetty.Transport.Channels;

namespace CoRuntime.Rpc;

/// <summary>
/// RPC服务器连接处理
/// </summary>
public class RpcServerChannelHandler: ChannelHandlerAdapter
{
    private readonly RpcManager _rpcMgr;
    private readonly Logger _logger;

    private readonly IChannel _channel;
    private ChannelWriter<RpcMessage>? _channelWriter;

    internal RpcServerChannelHandler(RpcManager rpcMgr, IChannel channel, Logger logger)
    {
        _logger = logger;
        _rpcMgr = rpcMgr;
        _channel = channel;
    }
    
    public override void ChannelActive(IChannelHandlerContext context)
    {
        _logger.Info($"Channel:{context.Channel} Active");
        base.ChannelActive(context);

        if (_channelWriter != null)
        {
            _logger.Error($"This can't be happening");
            return;
        }
        
        var unboundedChannel = Channel.CreateUnbounded<RpcMessage>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true,
        });
        _channelWriter = unboundedChannel.Writer;
        _ = ReadMessages(unboundedChannel.Reader);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        _logger.Info($"Channel:{context.Channel} Inactive");
        base.ChannelInactive(context);
        _channelWriter?.TryComplete();
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is not RpcMessage rpcMsg)
        {
            throw new RpcException($"Channel:{context.Channel}, ChannelRead, message is not RpcMessage: {message}");
        }

        switch (rpcMsg)
        {
            case RpcNotifyMessage notifyMsg:
            {
                DispatchNotify(notifyMsg);
                break;
            }
            case RpcRequestMessage requestMsg:
            {
                DispatchRequest(requestMsg);
                break;
            }
            default:
            {
                rpcMsg.Dispose();
                throw new RpcException($"Channel:{context.Channel}, Unsupported rpc type: {rpcMsg}");
            }
        }
    }
    
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        _logger.Error($"Channel:{context.Channel}, Error:{exception}");
        context.CloseAsync();
    }

    // Chanel线程
    private void DispatchNotify(RpcNotifyMessage rpcMsg)
    {
        if (!RuntimeEnv.ServiceMgr.TryGetServiceContext(rpcMsg.ServiceId, out var dstContext))
        {
            _logger.Error($"Channel:{_channel}, Unable to find the target service, service:{rpcMsg.ServiceId}");
            rpcMsg.Dispose();
            return;
        }
        
        // Service线程
        dstContext.EventLoop.ExecuteEx(dstContext.Service, rpcMsg, static (service, msg) =>
        {
            try
            {
                if (service.IsStopped)
                {
                    service.Logger.Error($"The service has been stopped, Service:{service}");
                    return;
                }
                
                if (service is not IRpcDispatcher dispatcher)
                {
                    service.Logger.Error($"The Service has not implemented IRpcDispatcher");
                    return;
                }
                
                dispatcher.DispatchNotify(msg);
            }
            catch (Exception e)
            {
                service.Logger.Error($"Error: {e}");
            }
            finally
            {
                msg.Dispose();
            }
        });
    }
    
    // Chanel线程
    private void DispatchRequest(RpcRequestMessage rpcMsg)
    {
        // 先处理预定义的消息
        if (HandlePredefineRequest(rpcMsg))
            return;
        
        if (!RuntimeEnv.ServiceMgr.TryGetServiceContext(rpcMsg.ServiceId, out var dstContext))
        {
            _logger.Error($"Channel:{_channel}, Unable to find the target service: {rpcMsg.ServiceId}");
            ResponseException(rpcMsg.RequestId, new RpcException("Unable to find the target service"));
            rpcMsg.Dispose();
            return;
        }
        
        dstContext.EventLoop.ExecuteEx(this, dstContext.Service, rpcMsg, static (self, service, msg) =>
        {
            _ = self.DoDispatchRequest(service, msg);
        });
    }

    // 处理预定义的消息
    private bool HandlePredefineRequest(RpcRequestMessage msg)
    {
        switch (msg.MethodId)
        {
            case PredefineMethodId.PingPong:
            {
                ResponseResult(RpcResponseMessage.Create(msg.RequestId, null));
                return true;
            }
            default:
                return false;
        }
    }

    // Service线程
    private async Task DoDispatchRequest(Service service, RpcRequestMessage msg)
    {
        try
        {
            if (service.IsStopped)
            {
                throw new InvalidOperationException($"The service has been stopped, Service:{service}");
            }
            
            if (service is not IRpcDispatcher dispatcher)
            {
                service.Logger.Error($"The server has not implemented IRpcDispatcher");
                ResponseException(msg.RequestId, new RpcException("Server has not implemented IRpcService"));
                return;
            }
            
            var respMsg = await dispatcher.DispatchRequest(msg);
            ResponseResult(respMsg);
        }
        catch (Exception e)
        {
            service.Logger.Error($"Error: {e}");
            ResponseException(msg.RequestId, e);
        }
        finally
        {
            msg.Dispose();
        }
    }

    // 在Service线程
    private void ResponseResult(RpcResponseMessage rpcMsg)
    {
        _channelWriter?.TryWrite(rpcMsg);
    }
    
    // 在Channel/Service线程
    private void ResponseException(int requestId, Exception exception)
    {
        var rpcMsg = RpcMessageSerializer.SerializeException(requestId, exception.ToString());
        _channelWriter?.TryWrite(rpcMsg);
    }

    private async Task ReadMessages(ChannelReader<RpcMessage> reader)
    {
        try
        {
            while (await reader.WaitToReadAsync())
            {
                if (!_channel.Active)
                    break;
                
                var writeCount = 0;
                while (reader.TryRead(out var msg))
                {
                    _ = _channel.WriteAsync(msg);
                    writeCount++;
                    if (writeCount >= _rpcMgr.Options.WriteFlushCount)
                        break;
                }

                if (writeCount > 0)
                {
                    _channel.Flush();
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Channel:{_channel}, Error:{e}");
        }
    }
}