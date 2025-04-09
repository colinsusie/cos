// Written by Colin on 2024-11-26

using System.Net;
using CoLib.Logging;
using CoRuntime.Net;
using CoRuntime.Rpc;
using CoRuntime.Services;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace CoRuntime.Gate;

public class TcpGateServer: ChannelHandlerAdapter, IGateServer
{
    private static readonly AttributeKey<TcpGateServer> AttrKeyServer = AttributeKey<TcpGateServer>.NewInstance(nameof(TcpGateServer));
    private static readonly AttributeKey<IGateConnection> AttrKeyConnection = AttributeKey<IGateConnection>.NewInstance(nameof(IGateConnection));
    
    private readonly IGateHandler _handler;
    private readonly GateServerOptions _options;
    private IChannel? _listenChannel;
    private readonly Logger _logger;
    
    public Service Service { get; }

    internal TcpGateServer(Service service, IGateHandler handler, GateServerOptions options)
    {
        _handler = handler;
        _options = options;
        Service = service;
        _logger = Service.GetLogger(nameof(TcpGateServer));
    }

    public async Task StartAsync()
    {
        _listenChannel = await InitAndListen();
    }

    public Task StopAsync()
    {
        throw new NotImplementedException();
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
            // Options
            channel.Configuration.SetOption(ChannelOption.SoReuseaddr, true);
        
            // Pipeline
            if (_options.EnableLogging)
            {
                channel.Pipeline.AddLast(new NetLoggingHandler(Service.GetLogger("TcpGateServer.Listen")));
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
            await Service.EventLoop.RegisterAsync(channel);
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
        try
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(_options.ListenHost), _options.ListenPort);
            _logger.Info($"Bind channel: {endPoint}");
            await channel.BindAsync(endPoint);
        }
        catch (Exception)
        {
            channel.Unsafe.CloseForcibly();
            throw;
        }
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
            // Options
            channel.Configuration.SetOption(ChannelOption.TcpNodelay, true);
        
            // Attributes
            channel.GetAttribute(AttrKeyServer).Set(this);
        
            // Pipeline
            if (_options.EnableLogging)
            {
                channel.Pipeline.AddLast(new NetLoggingHandler(Service.GetLogger("TcpGateServer.Connection")));
            }
            channel.Pipeline.AddLast(new ActionChannelInitializer<IChannel>(connChannel =>
            {
                IChannelPipeline pipeline = connChannel.Pipeline;
                // 编码：outbound
                pipeline.AddLast("packet-encoder", new GatePacketEncoder(_options.MaxPacketSize, 
                    Service.GetLogger(nameof(RpcPacketEncoder))));
                // 解码: inbound
                pipeline.AddLast("packet-decoder", new GatePacketDecoder(_options.MaxPacketSize, 
                    Service.GetLogger(nameof(RpcPacketDecoder))));
                // 消息处理
                pipeline.AddLast(new GateServerChannelHandler(connChannel,
                    Service.GetLogger(nameof(GateServerChannelHandler))));

                var conn = _handler.CreateConnection(this, channel);
                connChannel.GetAttribute(AttrKeyConnection).Set(conn);
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
            await Service.EventLoop.RegisterAsync(channel);
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