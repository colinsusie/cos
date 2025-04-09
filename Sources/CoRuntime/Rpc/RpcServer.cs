// Written by Colin on 2024-9-12

using System.Net;
using CoLib.Logging;
using CoRuntime.Net;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace CoRuntime.Rpc;

/// <summary>
/// RPC服务器，负责监听RPC客户端的连接并转发消息
/// </summary>
public class RpcServer: ChannelHandlerAdapter
{
    private static readonly AttributeKey<RpcServer> AttrKey = AttributeKey<RpcServer>.NewInstance(nameof(RpcServer));
    
    private readonly RpcManager _rpcMgr;
    private IChannel? _listenChannel;
    private readonly Logger _logger;

    public RpcServer(RpcManager rpcMgr)
    {
        _rpcMgr = rpcMgr;
        _logger = RuntimeEnv.LogMgr.GetLogger(nameof(RpcServer));
    }

    public async Task StartAsync()
    {
        _listenChannel = await InitAndListen();
    }

    public async Task StopAsync()
    {
        if (_listenChannel != null)
        {
            var channel = _listenChannel;
            _listenChannel = null;
            await channel.CloseAsync();
        }
    }

    private async Task<IChannel> InitAndListen()
    {
        var channel = new TcpServerSocketChannel();
        InitListenChannel(channel);
        await RegisterListenChannel(channel);
        await BindListenChannel(channel);
        return channel;
    }

    private void InitListenChannel(IChannel channel)
    {
        try
        {
            _logger.Info($"Init channel");
            var logMgr = RuntimeEnv.LogMgr;
            
            // Options
            channel.Configuration.SetOption(ChannelOption.SoReuseaddr, true);
        
            // Pipeline
            if (_rpcMgr.Options.EnableLogging)
            {
                channel.Pipeline.AddLast(new NetLoggingHandler(logMgr.GetLogger("RpcServer.Listen")));
            }
            channel.Pipeline.AddLast(new ActionChannelInitializer<IChannel>(ch =>
            {
                ch.Pipeline.AddLast(this);
            }));   
        }
        catch (Exception)
        {
            channel.Unsafe.CloseForcibly();
            throw;
        }
    }

    private async Task RegisterListenChannel(IChannel channel)
    {
        try
        {
            _logger.Info($"Register channel");
            var eventLoop = _rpcMgr.SelectEventLoop();
            await eventLoop.RegisterAsync(channel);
        }
        catch (Exception)
        {
            if (channel.Registered)
            {
                try
                {
                    await channel.CloseAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to close channel: {channel}, {ex}");
                }
            }
            else
            {
                channel.Unsafe.CloseForcibly();
            }
            throw;
        }
    }

    private async Task BindListenChannel(IChannel channel)
    {
        // This method is invoked before channelRegistered() is triggered.  Give user handlers a chance to set up
        // the pipeline in its channelRegistered() implementation.
        var endPoint = new IPEndPoint(IPAddress.Parse(_rpcMgr.Options.Host), _rpcMgr.Options.Port);
        _logger.Info($"Bind channel: {endPoint}");
        await channel.EventLoop.SubmitAsync<Task>(async () =>
        {
            try
            {
                await channel.BindAsync(endPoint);
            }
            catch (Exception)
            {
                channel.Unsafe.CloseForcibly();
                throw;
            }
        }).Unwrap();
    }
    
    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        var child = (IChannel)msg;
        if (InitConnectChannel(child))
        {
            _ = RegisterConnectChannel(child);
        }
    }

    private bool InitConnectChannel(IChannel channel)
    {
        try
        {
            var logMgr = RuntimeEnv.LogMgr;
            
            // Options
            channel.Configuration.SetOption(ChannelOption.TcpNodelay, true);
        
            // Attributes
            channel.GetAttribute(AttrKey).Set(this);
        
            // Pipeline
            if (_rpcMgr.Options.EnableLogging)
            {
                channel.Pipeline.AddLast(new NetLoggingHandler(logMgr.GetLogger("RpcServer.Connection")));
            }
            channel.Pipeline.AddLast(new ActionChannelInitializer<IChannel>(connChannel =>
            {
                IChannelPipeline pipeline = connChannel.Pipeline;
                // 编码：outbound
                pipeline.AddLast("packet-encoder", new RpcPacketEncoder(_rpcMgr.Options.MaxPacketSize, 
                    logMgr.GetLogger(nameof(RpcPacketEncoder))));
                // 解码: inbound
                pipeline.AddLast("packet-decoder", new RpcPacketDecoder(_rpcMgr.Options.MaxPacketSize, 
                    logMgr.GetLogger(nameof(RpcPacketDecoder))));
                // Rpc处理
                pipeline.AddLast(new RpcServerChannelHandler(_rpcMgr, connChannel,
                    logMgr.GetLogger(nameof(RpcServerChannelHandler))));
            }));
            return true;
        }
        catch (Exception e)
        {
            channel.Unsafe.CloseForcibly();
            _logger.Error($"Error: {e}");
            return false;
        }
    }

    private async Task RegisterConnectChannel(IChannel channel)
    {
        try
        {
            var eventLoop = _rpcMgr.SelectEventLoop();
            await eventLoop.RegisterAsync(channel);
        }
        catch (Exception e)
        {
            _logger.Error($"Error: {e}");
            if (channel.Registered)
            {
                try
                {
                    await channel.CloseAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to close channel, error:{ex}");
                }
            }
            else
            {
                channel.Unsafe.CloseForcibly();
            }
        }
    }
    
    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
    {
        IChannelConfiguration config = ctx.Channel.Configuration;
        if (config.AutoRead)
        {
            // stop accept new connections for 1 second to allow the channel to recover
            // See https://github.com/netty/netty/issues/1328
            config.AutoRead = false;
            ctx.Channel.EventLoop.ScheduleAsync(c => { ((IChannelConfiguration)c).AutoRead = true; }, config, TimeSpan.FromSeconds(1));
        }
        // still let the ExceptionCaught event flow through the pipeline to give the user
        // a chance to do something with it
        ctx.FireExceptionCaught(cause);
    }
}