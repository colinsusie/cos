// Written by Colin on 2024-11-28

using CoLib.Logging;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace CoRuntime.Gate;

public class GatePacketDecoder: ByteToMessageDecoder
{
    private readonly int _maxPacketLength;
    private readonly Logger _logger;

    public GatePacketDecoder(int maxPacketLength, Logger logger)
    {
        if (maxPacketLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPacketLength), "maxPacketLength must be a positive integer: " + maxPacketLength);
        }
        _maxPacketLength = maxPacketLength;
        _logger = logger;
    }
    
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        throw new NotImplementedException();
    }
}