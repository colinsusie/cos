// Written by Colin on 2024-7-31

using CoLib.Logging.LogTargets;
using CoLib.Logging.OutputTargets;

namespace CoLib.Logging;

/// <summary>
/// 日志管理器构建
/// TODO: 通过配置生成日志管理器
/// </summary>
public class LogManagerBuilder
{
    private const int DefaultQueueCapacity = 80000;
    
    // 日志的最小等级
    internal LogLevel MinLogLevel { get; private set; } = LogLevel.Debug;
    // 是否使用异步日志输出
    internal bool AsyncOutput { get; private set; } = true;
    // 使用异步输出时，后台日志队列的容量
    internal int QueueCapacity { get; private set; } = DefaultQueueCapacity;
    // 日志被丢弃的回调
    internal Action<LogManager, LogItem>? LogDropFunc { get; private set; }
    // 日志输出异常回调
    internal Action<LogManager, ILogTarget, LogItem, Exception>? LogExceptionFunc { get; private set; }
    // 日志输出目标
    internal readonly List<Func<LogManager, ILogTarget>> LogTargets = new();
    
    /// 设置最小等级
    public LogManagerBuilder SetMinLogLevel(LogLevel minLevel)
    {
        MinLogLevel = minLevel;
        return this;
    }

    /// 设置是否异步输出
    public LogManagerBuilder SetAsyncOutput(bool value, int queueCapacity)
    {
        AsyncOutput = value;
        QueueCapacity = queueCapacity;
        if (AsyncOutput && queueCapacity <= 0)
        {
            throw new ArgumentException($"queueCapacity must be greater than 0");
        }
        return this;
    }

    /// 当日志超出队列长度时丢弃，丢弃回调
    public LogManagerBuilder SetLogDropFunc(Action<LogManager, LogItem> func)
    {
        LogDropFunc = func;
        return this;
    }

    /// 当日志发生异常时，异常回调
    public LogManagerBuilder SetLogExceptionFunc(Action<LogManager, ILogTarget, LogItem, Exception> func)
    {
        LogExceptionFunc = func;
        return this;
    }

    /// 增加控制台输出
    public LogManagerBuilder AddOutputConsole(LogLevel minLevel = LogLevel.Debug)
    {
        LogTargets.Add(logMgr => new LogConsole(logMgr, minLevel));
        return this;
    }

    /// 增加文件输出
    public LogManagerBuilder AddOutputFile(string path,
        LogLevel minLevel = LogLevel.Debug,
        Func<LogItem, bool>? filterFunc = null,
        long rollOnFileSize = LogFile.DefaultRollOnFileSize)
    {
        LogTargets.Add(logMgr => new LogFile(logMgr, path, minLevel, filterFunc, rollOnFileSize));
        return this;
    }

    /// 增加自定义输出目标
    public LogManagerBuilder AddOutputTarget(Func<LogManager, ILogTarget> createFunc)
    {
        LogTargets.Add(createFunc);
        return this;
    }

    /// 构建日志管理器
    public LogManager Build()
    {
        var logMgr = new LogManager(this);
        return logMgr;
    }
}