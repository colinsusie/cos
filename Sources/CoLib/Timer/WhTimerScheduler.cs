// Written by Colin on 2023-12-09

using System.Runtime.CompilerServices;
using System.Text;
using CoLib.Common;
using CoLib.Container;
using CoLib.Extensions;
using CoLib.Logging;
using CoLib.ObjectPools;

namespace CoLib.Timer;


// 基于时间轮的定时器
public class WhTimerScheduler: ITimerScheduler
{
    private const int FirstBits = 8;                    // 第一个轮占用的位
    private const int RestBits = 6;                     // 后面轮占用的位
    private const int FirstSize = (1 << FirstBits);     // 第一个轮大小
    private const int RestSize = (1 << RestBits);       // 后面轮大小
    private const int FirstMask = (FirstSize - 1);
    private const int RestMask = (RestSize - 1);
    private const int MaxTickLimit = 6000;              // 一次调度最多执行的Tick数

    private sealed class TimerLinkList
    {
        public TimerNode? Head;
        public TimerNode? Tail;

        // 清理链表内容，并返回原来的链表头
        public TimerNode? Clear()
        {
            var node = Head;
            Head = null;
            Tail = null;
            return node;
        }

        public int Count()
        {
            var count = 0;
            var node = Head;
            while (node != null)
            {
                if (node.TimerFunc != null)
                    count++;
                node = node.Next;
            }

            return count;
        }
    }

    private sealed class TimerNode: ICleanable
    {
        // 链表下一个节点 
        public TimerNode? Next;
        // 对外的唯一Id
        public long Id;
        // 到期的Tick
        public uint ExpireTick;
        // 周期触发的Tick
        public uint PeriodTick;
        // 回调函数
        public Action<DataItem?>? TimerFunc;
        // 回调函数的状态
        public DataItem? DataItem;
        // 所属的时间组
        public ITimerGroup? Group;

        internal void Initialize(long id, uint periodTick, Action<DataItem?> timerFunc, DataItem? dataItem)
        {
            Id = id;
            PeriodTick = periodTick;
            TimerFunc = timerFunc;
            DataItem = dataItem;
        }

        public void Cleanup()
        {
            CleanData();
            Next = null;
        }

        public void CleanData()
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

    // 每个Tick的间隔(毫秒)
    private readonly uint _interval;
    // 第几个Tick，从0开始
    private uint _tick;
    // 最近一次记录的Tick点
    private long _lastTick;
    // 时间轮向量
    private readonly TimerLinkList[] _timerVector1 = new TimerLinkList[FirstSize];
    private readonly TimerLinkList[] _timerVector2 = new TimerLinkList[RestSize];
    private readonly TimerLinkList[] _timerVector3 = new TimerLinkList[RestSize];
    private readonly TimerLinkList[] _timerVector4 = new TimerLinkList[RestSize];
    private readonly TimerLinkList[] _timerVector5 = new TimerLinkList[RestSize];
    private readonly List<TimerLinkList[]> _timerVectors = new();
    // 下一个Id
    private long _nextId = 1;
    // 时间节点
    private readonly Dictionary<long, TimerNode> _timerNodes = new();
    // 分组节点
    private readonly Dictionary<ITimerGroup, HashSet<TimerNode>> _groupNodes = new();
    // 正在回调的定时器节点
    private TimerNode? _callingNode;
    private readonly Logger _logger;
    // 定时器节点池
    private readonly StObjectPool<TimerNode> _pool = new(4096, () => new TimerNode());
    
    internal WhTimerScheduler(WhTimerOptions options, Logger logger)
    {
        _logger = logger;
        VerifyOptions(options);
        _interval = options.TickInterval;
        _lastTick = GetCurrentTick();

        for (var i = 0; i < FirstSize; ++i)
            _timerVector1[i] = new();
        for (var i = 0; i < RestSize; ++i)
            _timerVector2[i] = new();
        for (var i = 0; i < RestSize; ++i)
            _timerVector3[i] = new();
        for (var i = 0; i < RestSize; ++i)
            _timerVector4[i] = new();
        for (var i = 0; i < RestSize; ++i)
            _timerVector5[i] = new();
        _timerVectors.Add(_timerVector1);
        _timerVectors.Add(_timerVector2);
        _timerVectors.Add(_timerVector3);
        _timerVectors.Add(_timerVector4);
        _timerVectors.Add(_timerVector5);
    }
    
    public long AddTimer(ITimerGroup? group, uint firstTime, uint periodTime, Action<DataItem?> timerFunc, DataItem? dataItem)
    {
        var id = GenerateId();
        var periodTick = (periodTime + _interval - 1) / _interval;
        var node = CreateTimerNode(id, periodTick, timerFunc, dataItem);

        _timerNodes[id] = node;
        if (group != null) 
            AddGroupNode(group, node);

        // 最少1个Tick
        firstTime = Math.Max(1, firstTime);
        var expires = (firstTime + _interval - 1) / _interval;
        node.ExpireTick = _tick + expires;
        AddToTimerVector(node);
        return id;
    }

    public bool RemoveTimer(long timerId)
    {
        if (!_timerNodes.Remove(timerId, out var node))
            return false;
        if (node.Group != null)
            RemoveGroupNode(node.Group, node);
        node.CleanData();
        
        return true;
    }

    public bool RemoveGroupTimers(ITimerGroup group)
    {
        if (!_groupNodes.Remove(group, out var nodeSet))
            return false;

        foreach (var node in nodeSet)
        {
            _timerNodes.Remove(node.Id);
            node.CleanData();
        }

        return true;
    }

    public void Schedule()
    {
        if (_callingNode != null)
        {
            throw new InvalidOperationException($"Cannot schedule when calling timer: {_callingNode.Id}");
        }
        
        var currTick = GetCurrentTick();
        if (currTick <= _lastTick)
            return;

        var ticks = currTick - _lastTick;
        _lastTick = currTick;
        if (ticks > MaxTickLimit)
        {
            _logger.Warn($"the call to schedule is too long, limiting the number of ticks {ticks} to {MaxTickLimit}");
            ticks = MaxTickLimit;
        }
        
        for (var i = 0; i < ticks; ++i)
        {
            OneTick();
        }
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
    
    private void OneTick()
    {
        var tick = ++_tick;
        var firstIdx = tick & FirstMask;
        if (firstIdx == 0)
        {
            var idx = (tick >> (FirstBits + 0 * RestBits)) & RestMask;
            CascadeNodes(_timerVector2[idx]);
            if (idx == 0)
            {
                idx = (tick >> (FirstBits + 1 * RestBits)) & RestMask;
                CascadeNodes(_timerVector3[idx]);
                if (idx == 0)
                {
                    idx = (tick >> (FirstBits + 2 * RestBits)) & RestMask;
                    CascadeNodes(_timerVector4[idx]);
                    if (idx == 0)
                    {
                        idx = (tick >> (FirstBits + 3 * RestBits)) & RestMask;
                        CascadeNodes(_timerVector5[idx]);
                    }
                }
            }
        }

        var node = _timerVector1[firstIdx].Clear();
        while (node != null)
        {
            var currNode = node;
            node = node.Next;
            
            // 已被删除
            if (currNode.TimerFunc == null)
            {
                ReturnTimerNode(currNode);
                continue;
            }

            // 执行回调
            SafeExecute(currNode);
            
            // 回调完自己被删除
            if (currNode.TimerFunc == null)
            {
                ReturnTimerNode(currNode);
                continue;
            }
            
            // 只触发一次
            if (currNode.PeriodTick <= 0)
            {
                RemoveTimer(currNode.Id);
                ReturnTimerNode(currNode);
                continue;
            }

            // 周期性定时器
            currNode.ExpireTick = tick + currNode.PeriodTick;
            AddToTimerVector(currNode);
        }
    }

    // 重排节点
    private void CascadeNodes(TimerLinkList list)
    {
        var node = list.Clear();
        while (node != null)
        {
            var currNode = node;
            node = node.Next;
            AddToTimerVector(currNode);
        }
    }

    private void VerifyOptions(WhTimerOptions options)
    {
        if (options.TickInterval <= 0)
        {
            throw new OptionsException($"TickInterval must be greater than 0: {options.TickInterval}");
        }
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
            _callingNode = node;
            node.TimerFunc!.Invoke(node.DataItem);
        }
        catch (Exception e)
        {
            _logger.Error($"error:{e}");
        }
        finally
        {
            _callingNode = null;
        }
    }

    private void AddToTimerVector(TimerNode node)
    {
        var expires = node.ExpireTick; 
        uint idx = expires - _tick;
        uint index;
        switch (idx)
        {
            case < FirstSize:
                index = expires & FirstMask;
                AddToLinkList(_timerVector1[index], node);
                break;
            case < 1 << (FirstBits + RestBits * 1):
                index = (expires >> (FirstBits + RestBits * 0)) & RestMask;
                AddToLinkList(_timerVector2[index], node);
                break;
            case < 1 << (FirstBits + RestBits * 2):
                index = (expires >> (FirstBits + RestBits * 1)) & RestMask;
                AddToLinkList(_timerVector3[index], node);
                break;
            case < 1 << (FirstBits + RestBits * 3):
                index = (expires >> (FirstBits + RestBits * 2)) & RestMask;
                AddToLinkList(_timerVector4[index], node);
                break;
            default:
                index = (expires >> (FirstBits + RestBits * 3)) & RestMask;
                AddToLinkList(_timerVector5[index], node);
                break;
        }
    }

    private void AddToLinkList(TimerLinkList list, TimerNode node)
    {
        if (list.Tail == null)
        {
            list.Head = node;
            list.Tail = node;
        }
        else
        {
            list.Tail.Next = node;
            list.Tail = node;
        }
        node.Next = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long GetCurrentTick()
    {
        return (long) TimeSpanExt.FromStart().TotalMilliseconds / _interval;
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

        var idx = 1;
        foreach (var vector in _timerVectors)
        {
            builder.Append($"Timer Vector {idx}: ");
            if (idx == 1 && _callingNode != null)
            {
                var count = 0;
                var node = _callingNode;
                while (node != null)
                {
                    if (node.TimerFunc != null)
                        count++;
                    node = node.Next;
                }
                if (count > 0)
                    builder.Append($"[Calling]={count} ");
            }
            
            for (var i = 0; i < vector.Length; ++i)
            {
                var list = vector[i];
                var count = list.Count();
                if (count > 0)
                    builder.Append($"[{i}]={count} ");
            }

            builder.AppendLine();
            ++idx;
        }

        return builder.ToString();
    }
}