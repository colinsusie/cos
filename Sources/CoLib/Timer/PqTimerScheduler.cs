// Written by Colin on 2023-12-09

using System.Text;
using CoLib.Container;
using CoLib.Extensions;
using CoLib.Logging;
using CoLib.ObjectPools;

namespace CoLib.Timer;

/// <summary>
/// 基于优先队列的定时器调度器
/// </summary>
public class PqTimerScheduler: ITimerScheduler
{
    private sealed class TimerNode: ICleanable
    {
        // 对外的唯一Id
        public long Id;
        // 周期触发的时间(毫秒)
        public uint Period;
        // 回调函数
        public Action<DataItem?>? TimerFunc;
        // 回调函数的状态
        public DataItem? DataItem;
        // 所属的时间组
        public ITimerGroup? Group;

        internal void Initialize(long id, uint period, Action<DataItem?> timerFunc, DataItem? dataItem)
        {
            Id = id;
            Period = period;
            TimerFunc = timerFunc;
            DataItem = dataItem;
        }

        public void Cleanup()
        {
            TimerFunc = null;
            Group = null;
            if (DataItem != null)
            {
                DataItem.Dispose();
                DataItem = null;
            }
        }
    }

    // 优先队列
    private readonly PriorityQueue<TimerNode, TimeSpan> _timerQueue = new();
    // 下一个Id
    private long _nextId = 1;
    // 时间节点
    private readonly Dictionary<long, TimerNode> _timerNodes = new();
    // 分组节点
    private readonly Dictionary<ITimerGroup, HashSet<TimerNode>> _groupNodes = new();
    // 正在执行的时间节点
    private readonly Queue<TimerNode> _callingNodes = new();
    private readonly Logger _logger;
    // 定时器节点池
    private readonly StObjectPool<TimerNode> _pool = new(4096, () => new TimerNode());

    internal PqTimerScheduler(Logger logger)
    {
        _logger = logger;
    }
    
    public long AddTimer(ITimerGroup? group, uint firstTime, uint periodTime, Action<DataItem?> timerFunc, DataItem? dataItem)
    {
        var id = GenerateId();
        var node = CreateTimerNode(id, periodTime, timerFunc, dataItem);
        
        _timerNodes[id] = node;
        _timerQueue.Enqueue(node, TimeSpanExt.FromStart(firstTime));
        if (group != null)
            AddGroupNode(group, node);
        
        return id;
    }

    public bool RemoveTimer(long timerId)
    {
        if (!_timerNodes.Remove(timerId, out var node))
            return false;
        if (node.Group != null)
            RemoveGroupNode(node.Group, node);
        node.Cleanup();
        
        return true;
    }

    public bool RemoveGroupTimers(ITimerGroup group)
    {
        if (!_groupNodes.Remove(group, out var nodeSet))
            return false;

        foreach (var node in nodeSet)
        {
            _timerNodes.Remove(node.Id);
            node.Cleanup();
        }

        return true;
    }

    public void Schedule()
    {
        if (_callingNodes.Count > 0)
        {
            throw new InvalidOperationException($"Cannot schedule when calling timer");
        }
        
        if (_timerQueue.Count == 0)
            return;
        
        var now = TimeSpanExt.FromStart();
        while (_timerQueue.TryPeek(out var node, out var expires))
        {
            if (expires > now)
                break;

            _timerQueue.Dequeue();
            _callingNodes.Enqueue(node);
        }

        while (_callingNodes.TryPeek(out var node))
        {
            // 已被删除
            if (node.TimerFunc == null)
            {
                _callingNodes.Dequeue();
                ReturnTimerNode(node);
                continue;
            }
            
            // 执行回调
            SafeExecute(node);
            _callingNodes.Dequeue();
            
            // 回调完自己被删除
            if (node.TimerFunc == null)
            {
                ReturnTimerNode(node);
                continue;
            }
            
            // 一次性定时器
            if (node.Period <= 0)
            {
                RemoveTimer(node.Id);
                ReturnTimerNode(node);
                continue;
            }
            
            // 周期性定时器
            _timerQueue.Enqueue(node, TimeSpanExt.FromStart(node.Period));
        }
    }

    public string GetDebugInfo(bool detail)
    {
        StringBuilder builder = new();
        builder.AppendLine();
        builder.AppendLine($"Total Timer: {_timerNodes.Count}");
        builder.AppendLine($"Total group: {_groupNodes.Count}");
        foreach (var (group, nodeSet) in _groupNodes)
        {
            builder.AppendLine($"    Group:{group.GetType()}={nodeSet.Count}");
        }

        builder.AppendLine($"Calling Timer: {_callingNodes.Count}");
        return builder.ToString();
    }

    private TimerNode CreateTimerNode(long id, uint period, Action<DataItem?> timerFunc, DataItem? dataItem)
    {
        var node = _pool.Get();
        node.Initialize(id, period, timerFunc, dataItem);
        return node;
    }

    private void ReturnTimerNode(TimerNode node)
    {
        _pool.Return(node);
    }

    private long GenerateId()
    {
        var id = _nextId;
        if (_nextId == long.MaxValue)
            _nextId = 1;
        else
            _nextId += 1;
        return id;
    }

    private void AddGroupNode(ITimerGroup group, TimerNode node)
    {
        if (!_groupNodes.TryGetValue(group, out var nodeSet))
        {
            nodeSet = new();
            _groupNodes[group] = nodeSet;
        }

        node.Group = group;
        nodeSet.Add(node);
    }

    private void RemoveGroupNode(ITimerGroup group, TimerNode node)
    {
        if (!_groupNodes.TryGetValue(group, out var nodeSet)) 
            return;

        node.Group = null;
        nodeSet.Remove(node);
        if (nodeSet.Count == 0)
            _groupNodes.Remove(group);
    }

    private void SafeExecute(TimerNode node)
    {
        try
        {
            node.TimerFunc!.Invoke(node.DataItem);
        }
        catch (Exception e)
        {
            _logger.Error($"error:{e}");
        }
    }
}