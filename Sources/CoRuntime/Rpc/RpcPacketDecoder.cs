// Written by Colin on 2024-8-24

using CoLib.Container;
using CoLib.Extensions;
using CoLib.Logging;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace CoRuntime.Rpc;

/// <summary>
/// RPC包解码器
/// </summary>
public class RpcPacketDecoder: ByteToMessageDecoder
{
    private const int PacketLengthBytes = 4;
    
    // 最大消息大小
    private readonly int _maxPacketLength;
    private readonly Logger _logger;
    
    public RpcPacketDecoder(int maxPacketLength, Logger logger)
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
        try
        {
            var rpcMsg = DoDecode(context, input);
            if (rpcMsg != null)
            {
                output.Add(rpcMsg);
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error {e}");
            context.CloseAsync();
        }
    }

    private RpcMessage? DoDecode(IChannelHandlerContext context, IByteBuffer input)
    {
        if (input.ReadableBytes < PacketLengthBytes)
            return null;

        var packetLen = input.GetIntLE(input.ReaderIndex);
        if (packetLen <= 0)
        {
            throw new DecoderException($"Negative pre-adjustment length field: {packetLen}, channel:{context.Channel}");
        }
        if (packetLen + PacketLengthBytes > _maxPacketLength)
        {
            throw new DecoderException($"Rpc packet length exceeds {_maxPacketLength}, size:{packetLen + PacketLengthBytes}, " +
                                       $"channel:{context.Channel}");
        }
        
        // 包还不完整
        var actualMsgLen = packetLen + PacketLengthBytes;
        if (input.ReadableBytes < actualMsgLen)
            return null;

        input.SkipBytes(PacketLengthBytes);
        var msgType = (RpcMessageType)input.ReadByte();
        int msgHeaderLen;
        RpcMessage? rpcMsg;
        switch (msgType)
        {
            case RpcMessageType.RmtNotify:
            {
                msgHeaderLen = RpcNotifyMessage.HeaderLen;
                CheckPacketLen(context, msgType, packetLen, msgHeaderLen);

                var serviceId = input.ReadShortLE();
                var methodId = input.ReadShortLE();
                var buffer = ExtractContent(input, input.ReaderIndex, packetLen - msgHeaderLen);
                rpcMsg = RpcNotifyMessage.Create(serviceId, methodId, buffer);
                break;
            }
            case RpcMessageType.RmtRequest:
            {
                msgHeaderLen = RpcRequestMessage.HeaderLen;
                CheckPacketLen(context, msgType, packetLen, msgHeaderLen);
                
                var serviceId = input.ReadShortLE();
                var methodId = input.ReadShortLE();
                var requestId = input.ReadIntLE();
                var buffer = ExtractContent(input, input.ReaderIndex, packetLen - msgHeaderLen);
                rpcMsg = RpcRequestMessage.Create(serviceId, methodId, requestId, buffer);
                break;
            }
            case RpcMessageType.RmtResponse:
            {
                msgHeaderLen = RpcResponseMessage.HeaderLen;
                CheckPacketLen(context, msgType, packetLen, msgHeaderLen);
                
                var requestId = input.ReadIntLE();
                var buffer = ExtractContent(input, input.ReaderIndex, packetLen - msgHeaderLen);
                rpcMsg = RpcResponseMessage.Create(requestId, buffer);
                break;
            }
            case RpcMessageType.RmtException:
            {
                msgHeaderLen = RpcExceptionMessage.HeaderLen;
                CheckPacketLen(context, msgType, packetLen, msgHeaderLen);
                
                var requestId = input.ReadIntLE();
                var buffer = ExtractContent(input, input.ReaderIndex, packetLen - msgHeaderLen);
                rpcMsg = RpcExceptionMessage.Create(requestId, buffer);
                break;
            }
            default:
                throw new DecoderException($"Rpc message type invalid: {msgType}, channel:{context.Channel}");
        }

        return rpcMsg;
    }

    private void CheckPacketLen(IChannelHandlerContext context, RpcMessageType msgType, int packetLen, int msgHeaderLen)
    {
        if (packetLen < msgHeaderLen)
        {
            throw new DecoderException(
                $"packet length less than message header, channel: {context.Channel}, MsgType:{msgType}, " +
                $"PacketLen:{packetLen}, MsgHeaderLen:{msgHeaderLen}");
        }
    }
    
    // 提取消息内容
    private PooledByteBufferWriter ExtractContent(IByteBuffer buffer, int index, int length)
    {
        var bufferWriter = PooledByteBufferWriter.Create(length);
        var segment = buffer.GetIoBuffer(index, length);
        segment.AsSpan().CopyTo(bufferWriter.GetSpan());
        bufferWriter.Advance(length);
        buffer.SkipBytes(length);
        return bufferWriter;
    }
}