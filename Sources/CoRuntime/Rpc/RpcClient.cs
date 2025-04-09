// Written by Colin on 2024-9-11

using System.Net;
using System.Threading.Channels;
using CoLib.Container;
using CoLib.EventLoop;
using CoLib.Extensions;
using CoLib.Logging;
using CoRuntime.Net;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace CoRuntime.Rpc;

/// 状态 
internal enum RpcClientStatus
{
    // 未连接
    NoConnect,
    // 正在连接
    Connecting,
    // 已连接
    Connected,
    // 关闭，不会再连接
    Close,
}

/// <summary>
/// RPC远程客户端
/// </summary>
public class RpcClient: ChannelHandlerAdapter, IRpcClient
{
    private readonly RpcManager _rpcMgr;
    
    private string _connectHost;
    private int _connectPort;
    private readonly StEventLoop _eventLoop;
    private IChannel? _channel;
    
    private int _nextRequestId;
    private readonly Logger _logger;

    private Task? _closeTask;
    private readonly CancellationTokenSource _cancellation = new();
    
    private Task<IChannel?>? _connectingTask;
    private readonly Dictionary<int, IRpcResponse> _responses = new();
    private readonly ChannelWriter<RpcMessage> _channelWriter;
    
    private readonly Action _tickFunc;
    private TimeSpan _connectTime = TimeSpanExt.FromStart();
    // 心跳机制：
    private TimeSpan _lastPingTime = TimeSpanExt.FromStart();
    private TimeSpan _lastPongTime = TimeSpanExt.FromStart();
    

    public RpcClient(RpcManager rpcMgr, string nodeName, string connectHost, int connectPort)
    {
        _rpcMgr = rpcMgr;
        _logger = RuntimeEnv.LogMgr.GetLogger(nameof(RpcClient));
        NodeName = nodeName;
        _connectHost = connectHost;
        _connectPort = connectPort;

        _tickFunc = Tick;
        _eventLoop = rpcMgr.SelectEventLoop();
        _eventLoop.Schedule(_tickFunc, TimeSpan.FromSeconds(1));
        
        var channel = Channel.CreateUnbounded<RpcMessage>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true,
        });
        _channelWriter = channel.Writer;
        _eventLoop.Execute(() =>
        {
            _ = ReadMessages(channel.Reader);
        });
    }
    
    public string NodeName { get; }
    
    // 可以多次作为Handler加进Pipeline
    public override bool IsSharable => true;

    private RpcClientStatus Status
    {
        get
        {
            if (_closeTask != null)
                return RpcClientStatus.Close;
            if (_channel is {Active: true})
                return RpcClientStatus.Connected;
            if (_connectingTask is {IsCompleted: false})
                return RpcClientStatus.Connecting;
            return RpcClientStatus.NoConnect;
        }
    }

    public override string ToString()
    {
        return $"{NodeName}-{_connectHost}:{_connectPort}";
    }

    private async ValueTask ReadMessages(ChannelReader<RpcMessage> reader)
    {
        try
        {
            while (await reader.WaitToReadAsync(_cancellation.Token))
            {
                var channel = await GetOrCreateChannel();
                var writeCount = 0;
                while (reader.TryRead(out var msg))
                {
                    if (msg is RpcNotifyMessage ntMsg)
                    {
                        if (channel == null)
                        {
                            _logger.Error($"Client:{this}, Channel is inactive");
                            ntMsg.Dispose();
                            continue;
                        }

                        _ = channel.WriteAsync(ntMsg);
                        writeCount++;
                    }
                    else if (msg is RpcRequestMessage reqMsg)
                    {
                        var response = reqMsg.Response!;
                        if (channel == null)
                        {
                            _logger.Error($"Client:{this}, Channel is inactive");
                            response.SetException(new RpcException("Channel is inactive"));
                            reqMsg.Dispose();
                            continue;
                        }

                        if (!_responses.TryAdd(reqMsg.RequestId, response))
                        {
                            _logger.Error($"Client:{this}, Request id already exists:{reqMsg.RequestId}");
                            response.SetException(new RpcException($"Request id already exists:{reqMsg.RequestId}"));
                            reqMsg.Dispose();
                            continue;
                        }

                        _ = channel.WriteAsync(msg);
                        writeCount++;
                    }

                    if (writeCount >= _rpcMgr.Options.WriteFlushCount)
                        break;
                }

                if (writeCount > 0)
                {
                    channel?.Flush();
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Client:{this}, Error:{e}");
        }
    }

    // 创建通道：一定在EventLoop执行
    private ValueTask<IChannel?> GetOrCreateChannel()
    {
        // 通道已激活
        if (_channel is {Active: true})
            return ValueTask.FromResult<IChannel?>(_channel);

        // 还在连接中
        if (_connectingTask is {IsCompleted: false})
            return new ValueTask<IChannel?>(_connectingTask);

        // 启动连接
        _connectingTask = DoCreateChannel();
        return new ValueTask<IChannel?>(_connectingTask);
            
        async Task<IChannel?> DoCreateChannel()
        {
            try
            {
                var channel = new TcpSocketChannel();
                
                // init
                InitChannel(channel);
            
                // register
                await RegisterChannel(channel);
            
                // connect
                await ConnectChannel(channel);
                
                return channel;
            }
            catch (Exception e)
            {
                _logger.Error($"Client:{this}, Error: {e}");
                return null;
            }    
        }
    }
    
    private void InitChannel(IChannel channel)
    {
        try
        {
            var logMgr = RuntimeEnv.LogMgr;
            
            // Options
            channel.Configuration.SetOption(ChannelOption.TcpNodelay, true);
            channel.Configuration.SetOption(ChannelOption.ConnectTimeout, _rpcMgr.Options.ConnectTimeout);
            
            // Pipeline
            if (_rpcMgr.Options.EnableLogging)
            {
                channel.Pipeline.AddLast(new NetLoggingHandler(logMgr.GetLogger("RpcClient.Connection")));
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
                pipeline.AddLast(this);
            }));
        }
        catch (Exception)
        {
            channel.Unsafe.CloseForcibly();
            throw;
        }
    }

    private async Task RegisterChannel(IChannel channel)
    {
        try
        {
            await _eventLoop.RegisterAsync(channel);
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
                    _logger.Error($"Client:{this}, Failed to close channel, error:{ex}");
                }
            }
            else
            {
                channel.Unsafe.CloseForcibly();
            }
            throw;
        }
    }

    private async Task ConnectChannel(IChannel channel)
    {
        try
        {
            await channel.ConnectAsync(new IPEndPoint(IPAddress.Parse(_connectHost), _connectPort));
        }
        catch (Exception)
        {
            try
            {
                await channel.CloseAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"Client:{this}, Failed to close channel, Error:{e}");
            }
            throw;
        }
    }
    
    public override void ChannelActive(IChannelHandlerContext context)
    {
        _logger.Info($"Client:{this}, Channel active, Channel:{context.Channel}");
        _channel = context.Channel;
        _connectingTask = null;
        _lastPingTime = _lastPongTime = TimeSpanExt.FromStart();
        base.ChannelActive(context);
    }
    
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        _logger.Info($"Client:{this}, Channel inactive, Channel:{context.Channel}");
        _channel = null;
        _connectingTask = null;
        base.ChannelInactive(context);
        
        ClearResponses(new RpcException("Channel is inactive"));
    }
    
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        _logger.Error($"Client:{this}, Channel exception, Channel:{context.Channel}, Error:{exception}");
        context.CloseAsync();
    }
    
    /// 更新连接地址
    public void UpdateConnectionInfo(string connectHost, int connectPort)
    {
        if (_connectHost == connectHost && _connectPort == connectPort)
            return;

        _eventLoop.ExecuteEx(this, connectHost, connectPort, (self, host, port) =>
        {
            self._connectHost = host;
            self._connectPort = port;
            if (self._channel is {Active: true})
            {
                self._channel.CloseAsync();
            }
        });
    }

    /// 关闭客户端
    internal Task CloseAsync()
    {
        if (_closeTask is {IsCompleted: false})
            return _closeTask;
        
        _closeTask = _eventLoop.SubmitAsync<Task>(async () =>
        {
            _channelWriter.TryComplete();
            await _cancellation.CancelAsync();
            if (_channel is {Active: true})
            {
                await _channel.CloseAsync();
            }
        }).Unwrap();
        
        return _closeTask;
    }
    
    /// IRpcClient： 生成请求ID
    public int GenerateRequestId()
    {
        return Interlocked.Increment(ref _nextRequestId);
    }

    /// 派发通知消息
    public void DispatchNotify(RpcNotifyMessage msg)
    {
        if (!_channelWriter.TryWrite(msg))
        {
            _logger.Error($"Client:{this}, Rpc client is close");
            msg.Dispose();
        }
    }
    
    /// 派发请求消息
    public ValueTask DispatchRequest(RpcRequestMessage msg, CancellationToken token)
    {
        var response = RpcResponse.Create(token);
        msg.Response = response;
        if (!_channelWriter.TryWrite(msg))
        {
            _logger.Error($"Client:{this}, Rpc client is close");
            response.SetException(new RpcException("Rpc client is close"));
            msg.Dispose();
        }
        return response.ValueTask;
    }
    
    /// 派发请求消息
    public ValueTask<TResponse> DispatchRequest<TResponse>(RpcRequestMessage msg, CancellationToken token)
    {
        var response = RpcResponse<TResponse>.Create(token);
        msg.Response = response;
        if (!_channelWriter.TryWrite(msg))
        {
            _logger.Error($"Client:{this}, Rpc client is close");
            response.SetException(new RpcException("Rpc client is close"));
            msg.Dispose();
        }
        return response.ValueTask;
    }
    
    // 断开连接时清理请求响应
    private void ClearResponses(Exception e)
    {
        foreach (var requestId in _responses.Keys.ToList())
        {
            if (!_responses.Remove(requestId, out var response))
                continue;
            response.SetException(e);
        }
    }
    
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is not RpcMessage rpcMsg)
        {
            _logger.Error($"Client:{this}, ChannelRead, message is not RpcMessage: {message}");
            return;
        }
        
        switch (rpcMsg)
        {
            case RpcResponseMessage rspMsg:
            {
                ResponseResult(rspMsg);
                break;
            }
            case RpcExceptionMessage exMsg:
            {
                ResponseException(exMsg);
                break;
            }
            default:
            {
                _logger.Error($"Client:{this}, ChannelRead, Unsupported rpc type: {rpcMsg}");
                rpcMsg.Dispose();
                break;
            }
        }
    }
    
    private void ResponseResult(RpcResponseMessage respMsg)
    {
        if (!_responses.Remove(respMsg.RequestId, out var response))
        {
            _logger.Error($"Client:{this}, Unable to find rpc response: {respMsg}");
            respMsg.Dispose();
            return;
        }

        try
        {
            response.SetResult(respMsg);
        }
        finally
        {
            respMsg.Dispose();
        }
    }

    private void ResponseException(RpcExceptionMessage rpcMsg)
    {
        if (!_responses.Remove(rpcMsg.RequestId, out var response))   
        {
            _logger.Error($"Client:{this}, Unable to find rpc response: {rpcMsg}");
            rpcMsg.Dispose();
            return;
        }

        try
        {
            var errMsg = RpcMessageSerializer.Deserialize1<string>(rpcMsg);
            response.SetException(new RpcException(errMsg ?? string.Empty));
        }
        catch (Exception e)
        {
            response.SetException(e);
        }
        finally
        {
            rpcMsg.Dispose();
        }
    }

    private void Tick()
    {
        try
        {
            OnTick();
        }
        catch (Exception e)
        {
            _logger.Error($"Client:{this}, Error:{e}");
        }
        finally
        {
            _eventLoop.Schedule(_tickFunc, TimeSpan.FromSeconds(1));   
        }
    }

    private void OnTick()
    {
        if (Status == RpcClientStatus.Close)
            return;
        
        HandleResponseTimeOut();
        HandleReconnect();
        HandleHeartbeat();
    }

    private void HandleResponseTimeOut()
    {
        if (_responses.Count == 0)
            return;
        
        var now = TimeSpanExt.FromStart();
        using LocalList<int> timeOutList = new(); 
        foreach (var (requestId, response) in _responses)
        {
            if (now - response.StartTime >= _rpcMgr.Options.RequestTimeOut)
            {
                timeOutList.Add(requestId);
            }
        }

        if (timeOutList.Count == 0)
            return;
        
        foreach (var requestId in timeOutList)
        {
            if (!_responses.Remove(requestId, out var response))
                continue;
                
            response.SetException(new TimeoutException());
        }
    }

    private void HandleReconnect()
    {
        if (Status != RpcClientStatus.NoConnect)
            return;

        var now = TimeSpanExt.FromStart();
        if (now - _connectTime >= _rpcMgr.Options.ReconnectInterval)
        {
            _connectTime = now;
            GetOrCreateChannel();
        }
    }

    private void HandleHeartbeat()
    {
        if (!_rpcMgr.Options.EnableHeartbeat)
            return;
        if (Status != RpcClientStatus.Connected)
            return;
        
        var channel = _channel!;
        var now = TimeSpanExt.FromStart();
        if (now - _lastPongTime >= _rpcMgr.Options.HeartbeatTimeOut)
        {
            _logger.Error($"Client:{this}, Heartbeat timeout");
            channel.CloseAsync();
            return;
        }
        if (now - _lastPingTime >= _rpcMgr.Options.HeartbeatInterval)
        {
            _ = Ping();
        }
    }

    private async Task Ping()
    {
        try
        {
            await DispatchRequest(RpcRequestMessage.Create(0, PredefineMethodId.PingPong, GenerateRequestId(), null), CancellationToken.None);
            _lastPongTime = TimeSpanExt.FromStart();
        }
        catch (Exception e)
        {
            _logger.Error($"Client:{this}, Error:{e}");
        }
    }
}