// Written by Colin on 2024-11-28

using CoLib.Logging;
using DotNetty.Transport.Channels;

namespace CoRuntime.Gate;

public class GateServerChannelHandler: ChannelHandlerAdapter
{
    private readonly Logger _logger;
    private readonly IChannel _channel;

    internal GateServerChannelHandler(IChannel channel, Logger logger)
    {
        _logger = logger;
        _channel = channel;
    }
    
    
}