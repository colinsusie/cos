// Written by Colin on 2023-11-02

using System.Collections.Concurrent;
using CoLib.Logging.LogTargets;

namespace CoLib.Logging;

/// <summary>
/// 日志管理器
/// </summary>
public class LogManager
{
    private readonly LogPipe? _logPipe;
    // 日志器
    private readonly ConcurrentDictionary<string, Logger> _loggers = new();
    // 日志输出目标
    internal readonly List<ILogTarget> Targets = new();
    // 日志被丢弃的回调
    private Action<LogManager, LogItem>? _logDropFunc;
    // 日志输出异常回调
    private Action<LogManager, ILogTarget, LogItem, Exception>? _logExceptionFunc;

    // 日志输出的最小等级，小于这个等级的日志不会输出
    private volatile LogLevel _minLogLevel;
    // 后台日志队列的最大容量，超过会阻塞
    internal int LogQueueCapacity { get; private set; }
    // 是否异步输出日志
    internal bool AsyncOutput { get; private set; }
    
    /// <summary>
    /// 取或设置日志最小等级
    /// </summary>
    public LogLevel MinLogLevel
    {
        get => _minLogLevel;
        set => _minLogLevel = value;
    }

    // 只能由LogManagerBuilder调用
    internal LogManager(LogManagerBuilder builder)
    {
        _minLogLevel = builder.MinLogLevel;
        AsyncOutput = builder.AsyncOutput;
        LogQueueCapacity = builder.QueueCapacity;
        _logDropFunc = builder.LogDropFunc;
        _logExceptionFunc = builder.LogExceptionFunc;
        
        foreach (var createFunc in builder.LogTargets)
        {
            Targets.Add(createFunc(this));
        }
        _logPipe = new LogPipe(this);
    }

    /// <summary>
    /// 停止日志库
    /// </summary>
    public void Stop()
    {
        _logPipe?.Stop();
        foreach (var target in Targets)
        {
            target.Dispose();
        }
    }

    /// <summary>
    /// 取日志输出器
    /// </summary>
    /// <param name="tag">自定义标签， 同一个标签的只有一份实例</param>
    /// <returns></returns>
    public Logger GetLogger(string tag)
    {
        return _loggers.GetOrAdd(tag, static (logTag, logMgr) => new Logger(logMgr, logTag), this);
    }

    // 检查日志等级,返回true将输出日志
    internal bool CheckLevel(LogLevel level)
    {
        // 日志级别符合会输出
        return _minLogLevel <= level;
    }
    
    // 输出日志
    internal void Output(in LogItem logItem)
    {
        _logPipe?.Send(logItem);
    }

    // 输出到目标
    internal void OutputToTarget(in LogItem logItem)
    {
        foreach (var target in Targets)
        {
            try
            {
                if (!target.Filter(logItem))
                {
                    target.Output(logItem);
                }
            }
            catch (Exception e)
            {
                OnLogException(target, logItem, e);
            }
        }
    }
    
    internal void OnLogDrop(LogItem item)
    {
        _logDropFunc?.Invoke(this, item);
    }

    internal void OnLogException(ILogTarget target, LogItem item, Exception ex)
    {
        _logExceptionFunc?.Invoke(this, target, item, ex);
    }
}