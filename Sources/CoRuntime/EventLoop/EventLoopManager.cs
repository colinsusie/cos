using System.Diagnostics.CodeAnalysis;
using CoLib.EventLoop;
using CoLib.Logging;
using CoRuntime.Services;

namespace CoRuntime.EventLoop;

/// <summary>
/// 事件循环管理器
/// </summary>
public class EventLoopManager: RuntimeObject
{
    /// 事件循环
    private readonly Dictionary<string, EventLoopGroup> _eventLoopGroups = new();
    
    /// 事件循环分类
    private class EventLoopGroup
    {
        /// 当前已分配的索引
        public int CurrIndex;
        /// 事件循环列表 
        public readonly List<StEventLoop> Loops = new();
    }
    
    public EventLoopManager()
    {
        var options = RuntimeEnv.OptionsMgr.GetOptions<EventLoopOptions>();
        var globalGroup = new EventLoopGroup();
        for (var i = 0; i < options.Count; ++i)
        {
            var eventLoop = new StEventLoop(RuntimeEnv.LogMgr.GetLogger(nameof(StEventLoop)));
            globalGroup.Loops.Add(eventLoop);
        }
        _eventLoopGroups[string.Empty] = globalGroup; 

        if (options.Groups != null)
        {
            int index = 0;
            foreach (var (group, count) in options.Groups)
            {
                if (group == string.Empty)
                {
                    throw new ArgumentException("EventLoopManager: EventLoop group can no be empty string");
                }
                if (index + count > globalGroup.Loops.Count)
                {
                    throw new ArgumentOutOfRangeException($"EventLoopManager: EventLoop group count out of range, index:{index}, count:{count}");
                }

                var eventLoopGroup = new EventLoopGroup();
                for (var i = 0; i < count; ++i)
                {
                    eventLoopGroup.Loops.Add(globalGroup.Loops[index++]);
                }

                _eventLoopGroups[group] = eventLoopGroup;
            }
        }
    }

    protected override async Task DoStopAsync(CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        if (_eventLoopGroups.TryGetValue(string.Empty, out var category))
        {
            foreach (var eventLoop in category.Loops)
            {
                tasks.Add(eventLoop.ShutdownGracefullyAsync());
            }

            await Task.WhenAll(tasks);
        }
    }
    
    /// 选择一个事件循环，category 为事件类型，空字符串表示从全局事件循环中选择
    public StEventLoop SelectEventLoop(string category)
    {
        if (!_eventLoopGroups.TryGetValue(category, out var loopCategory))
        {
            throw new InvalidOperationException($"ServiceManager.SelectEventLoop: Unable to find eventLoops by category: {category}");
        }

        var idx = Math.Abs(Interlocked.Increment(ref loopCategory.CurrIndex) % loopCategory.Loops.Count);
        return loopCategory.Loops[idx];
    }

    /// 获取一个事件循环组
    internal bool TryGetEventLoops(string category, [MaybeNullWhen(false)] out List<StEventLoop> eventLoops)
    {
        if (!_eventLoopGroups.TryGetValue(category, out var categories))
        {
            eventLoops = null;
            return false;
        }

        eventLoops = categories.Loops;
        return true;
    }
}