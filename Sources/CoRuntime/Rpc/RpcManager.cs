// Written by Colin on 2024-9-11

using System.Diagnostics.CodeAnalysis;
using CoLib.EventLoop;
using CoRuntime.Services.Cluster;
using ServiceAddr = CoRuntime.Services.ServiceAddr;

namespace CoRuntime.Rpc;

/// <summary>
/// Rpc管理器
/// </summary>
public class RpcManager: RuntimeObject
{
    public readonly RpcOptions Options;
    
    private readonly List<StEventLoop> _eventLoops;
    private int _eventLoopIndex;
    private RpcServer? _server;
    private readonly object _clientLock = new();
    private readonly Dictionary<string, RpcClient> _clients = new();
    
    public RpcManager()
    {
        Options = RuntimeEnv.OptionsMgr.GetOptions<RpcOptions>();
        if (!RuntimeEnv.EventLoopMgr.TryGetEventLoops(Options.EventLoopGroup, out var eventLoops))
        {
            throw new InvalidOperationException($"RpcManager: Unable to find event loop category: {Options.EventLoopGroup}");
        }
        _eventLoops = eventLoops;
    }

    /// 启动RPC管理器
    protected override Task DoStartAsync(CancellationToken cancellationToken)
    {
        _server = new RpcServer(this);
        return _server.StartAsync();
    }

    /// 停止RPC管理器
    protected override async Task DoStopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_server != null)
            {
                await _server.StopAsync();
            }
            
            var tasks = new List<Task>();
            lock (_clientLock)
            {
                foreach (var (_, client) in _clients)
                {
                    var task = client.CloseAsync();
                    tasks.Add(task);
                }
            }
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            Logger.Error($"Error: {e}");
        }
    }
    
    /// 选择一下EventLoop
    internal StEventLoop SelectEventLoop()
    {
        var idx = Math.Abs(Interlocked.Increment(ref _eventLoopIndex) % _eventLoops.Count);
        return _eventLoops[idx];
    }

    /// 更新客户端列表
    public void UpdateClients(Dictionary<string, ClusterNodeInfo> nodeInfos, ClusterManager clusterMgr)
    {
        try
        {
            lock (_clientLock)
            {
                foreach (var (nodeName, nodeInfo) in nodeInfos)
                {
                    // 本节点不创建
                    if (nodeName == RuntimeEnv.Config.NodeName)
                        continue;

                    if (_clients.TryGetValue(nodeName, out var client))
                    {
                        // 更新地址
                        client.UpdateConnectionInfo(nodeInfo.Host, nodeInfo.Port);
                    }
                    else
                    {
                        // 新Client
                        client = new RpcClient(this, nodeName, nodeInfo.Host, nodeInfo.Port);
                        _clients[nodeName] = client;
                    }
                }

                // 删除旧的
                var removeClients = new List<string>(); 
                foreach (var (nodeName, _) in _clients)
                {
                    if (clusterMgr.ContainsNode(nodeName))
                        continue;
                    removeClients.Add(nodeName);
                }
                foreach (var nodeName in removeClients)
                {
                    if (!_clients.Remove(nodeName, out var client))
                        continue;
                    client.CloseAsync();
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error: {e}");
        }
    }

    /// 通过服务地址取Rpc客户端
    public bool TryGetClient(ServiceAddr serviceAddr, [MaybeNullWhen(false)] out IRpcClient client)
    {
        // 本地客户端
        if (serviceAddr.NodeName == RuntimeEnv.Config.NodeName)
        {
            client = new RpcLocalClient(serviceAddr.ServiceId);
            return true;
        }

        // 远程客户端
        lock (_clientLock)
        {
            if (!_clients.TryGetValue(serviceAddr.NodeName, out var remoteClient))
            {
                client = null;
                return false;
            }
            client = remoteClient;
        }
        
        return true;
    }
}