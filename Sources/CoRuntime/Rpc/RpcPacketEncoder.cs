
// Written by Colin on 2024-8-24

using CoLib.Extensions;
using CoLib.Logging;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace CoRuntime.Rpc;

/// <summary>
/// RPC包编码器
/// </summary>
public class RpcPacketEncoder: MessageToByteEncoder<RpcMessage>
{
    private const int PacketLengthBytes = 4;
    
    private readonly int _maxPacketLength;
    private readonly Logger _logger;

    public RpcPacketEncoder(int maxPacketLength, Logger logger)
    {
        if (maxPacketLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPacketLength), "maxPacketLength must be a positive integer: " + maxPacketLength);
        }
        _maxPacketLength = maxPacketLength;
        _logger = logger;
    }
    
    protected override void Encode(IChannelHandlerContext context, RpcMessage rpcMsg, IByteBuffer output)
    {
        try
        {
            var buffer = rpcMsg.Buffer;
            switch (rpcMsg)
            {
                case RpcNotifyMessage notifyMsg:
                {
                    if (buffer != null)
                    {
                        var content = buffer.WrittenMemory;
                        var packetLen = RpcNotifyMessage.HeaderLen + content.Length;
                        CheckPacketLen(context, notifyMsg, packetLen);

                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtNotify);
                        output.WriteShortLE(notifyMsg.ServiceId);
                        output.WriteShortLE(notifyMsg.MethodId);
                        content.CopyTo(output.GetIoBuffer(output.WriterIndex, content.Length));
                        output.SetWriterIndex(output.WriterIndex + content.Length);
                    }
                    else
                    {
                        var packetLen = RpcNotifyMessage.HeaderLen;
                        CheckPacketLen(context, notifyMsg, packetLen);
                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtNotify);
                        output.WriteShortLE(notifyMsg.ServiceId);
                        output.WriteShortLE(notifyMsg.MethodId);
                    }

                    break;
                }
                case RpcRequestMessage requestMsg:
                {
                    if (buffer != null)
                    {
                        var content = buffer.WrittenMemory;
                        var packetLen = RpcRequestMessage.HeaderLen + content.Length;
                        CheckPacketLen(context, requestMsg, packetLen);

                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtRequest);
                        output.WriteShortLE(requestMsg.ServiceId);
                        output.WriteShortLE(requestMsg.MethodId);
                        output.WriteIntLE(requestMsg.RequestId);
                        content.CopyTo(output.GetIoBuffer(output.WriterIndex, content.Length));
                        output.SetWriterIndex(output.WriterIndex + content.Length);
                    }
                    else
                    {
                        var packetLen = RpcRequestMessage.HeaderLen;
                        CheckPacketLen(context, requestMsg, packetLen);

                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtRequest);
                        output.WriteShortLE(requestMsg.ServiceId);
                        output.WriteShortLE(requestMsg.MethodId);
                        output.WriteIntLE(requestMsg.RequestId);
                    }

                    break;
                }
                case RpcResponseMessage responseMsg:
                {
                    if (buffer != null)
                    {
                        var content = buffer.WrittenMemory;
                        var packetLen = RpcResponseMessage.HeaderLen + content.Length;
                        CheckPacketLen(context, responseMsg, packetLen);

                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtResponse);
                        output.WriteIntLE(responseMsg.RequestId);
                        content.CopyTo(output.GetIoBuffer(output.WriterIndex, content.Length));
                        output.SetWriterIndex(output.WriterIndex + content.Length);
                    }
                    else
                    {
                        var packetLen = RpcResponseMessage.HeaderLen;
                        CheckPacketLen(context, responseMsg, packetLen);

                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtResponse);
                        output.WriteIntLE(responseMsg.RequestId);
                    }

                    break;
                }
                case RpcExceptionMessage exceptionMsg:
                {
                    if (buffer != null)
                    {
                        var content = buffer.WrittenMemory;
                        var packetLen = RpcExceptionMessage.HeaderLen + content.Length;
                        CheckPacketLen(context, exceptionMsg, packetLen);

                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtException);
                        output.WriteIntLE(exceptionMsg.RequestId);
                        content.CopyTo(output.GetIoBuffer(output.WriterIndex, content.Length));
                        output.SetWriterIndex(output.WriterIndex + content.Length);
                    }
                    else
                    {
                        var packetLen = RpcExceptionMessage.HeaderLen;
                        CheckPacketLen(context, exceptionMsg, packetLen);

                        output.EnsureWritable(PacketLengthBytes + packetLen);
                        output.WriteIntLE(packetLen);
                        output.WriteByte((byte) RpcMessageType.RmtException);
                        output.WriteIntLE(exceptionMsg.RequestId);
                    }

                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(rpcMsg));
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error: {e}");
            throw;
        }
        finally
        {
            rpcMsg.Dispose();
        }
    }

    private void CheckPacketLen(IChannelHandlerContext context, RpcMessage rpcMsg, int packetLen)
    {
        if (packetLen + PacketLengthBytes > _maxPacketLength)
        {
            throw new EncoderException($"Rpc packet length exceeds {_maxPacketLength}, " +
                                       $"size:{packetLen + PacketLengthBytes}, " +
                                       $"channel:{context.Channel}" +
                                       $"RpcMsg:{rpcMsg}");
        }
    }
}