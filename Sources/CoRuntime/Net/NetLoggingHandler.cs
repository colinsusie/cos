// Written by Colin on 2024-8-24

using CoLib.Extensions;
using CoLib.Logging;

namespace CoRuntime.Net;

using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

/// <summary>
/// 网络消息日志输出器
/// </summary>
public class NetLoggingHandler : ChannelHandlerAdapter
{
    private readonly Logger _logger;
    public NetLoggingHandler(Logger logger)
    {
        _logger = logger;
    }

    public override bool IsSharable => true;

    public override void ChannelRegistered(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} REGISTERED");
        ctx.FireChannelRegistered();
    }

    public override void ChannelUnregistered(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} UNREGISTERED");
        ctx.FireChannelUnregistered();
    }

    public override void ChannelActive(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} ACTIVE");
        ctx.FireChannelActive();
    }

    public override void ChannelInactive(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} INACTIVE");
        ctx.FireChannelInactive();
    }

    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
    {
        _logger.Error($"{ctx.Channel} EXCEPTION:{cause}");
        ctx.FireExceptionCaught(cause);
    }

    public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
    {
        _logger.Info($"{ctx.Channel} USER_EVENT:{evt}");
        ctx.FireUserEventTriggered(evt);
    }

    public override Task BindAsync(IChannelHandlerContext ctx, EndPoint localAddress)
    {
        _logger.Info($"{ctx.Channel} BIND");
        return ctx.BindAsync(localAddress);
    }

    public override Task ConnectAsync(IChannelHandlerContext ctx, EndPoint remoteAddress, EndPoint localAddress)
    {
        _logger.Info($"{ctx.Channel} CONNECT:{remoteAddress},{localAddress}");
        return ctx.ConnectAsync(remoteAddress, localAddress);
    }

    public override Task DisconnectAsync(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} DISCONNECT");
        return ctx.DisconnectAsync();
    }

    public override Task CloseAsync(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} CLOSE");
        return ctx.CloseAsync();
    }

    public override Task DeregisterAsync(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} DEREGISTER");
        return ctx.DeregisterAsync();
    }

    public override void ChannelRead(IChannelHandlerContext ctx, object message)
    {
        _logger.Info($"{ctx.Channel} RECEIVED:{Format(message)}");
        ctx.FireChannelRead(message);
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} RECEIVED_COMPLETE");
        ctx.FireChannelReadComplete();
    }

    public override void ChannelWritabilityChanged(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} WRITABILITY:{ctx.Channel.IsWritable}");
        ctx.FireChannelWritabilityChanged();
    }

    public override void HandlerAdded(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} HANDLER_ADDED");
    }

    public override void HandlerRemoved(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} HANDLER_REMOVED");
    }

    public override void Read(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} READ");
        ctx.Read();
    }

    public override Task WriteAsync(IChannelHandlerContext ctx, object msg)
    {
        _logger.Info($"{ctx.Channel} WRITE:{Format(msg)}");
        return ctx.WriteAsync(msg);
    }

    public override void Flush(IChannelHandlerContext ctx)
    {
        _logger.Info($"{ctx.Channel} FLUSH");
        ctx.Flush();
    }

    private string Format(object arg)
    {
        if (arg is IByteBuffer buffer)
        {
            var len = buffer.ReadableBytes; 
            return len == 0 ? "0B" : $"{len}B\n{ByteBufferUtil.PrettyHexDump(buffer)}";
        }
        else if (arg is IByteBufferHolder holder)
        {
            buffer = holder.Content;
            var len = buffer.ReadableBytes; 
            return len == 0 ? "0B" : $"{len}B\n{ByteBufferUtil.PrettyHexDump(buffer)}";
        }
        else
        {
            var str = arg.ToString();
            return str ?? string.Empty;
        }
    }
}