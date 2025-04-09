// Written by Colin on 2024-7-17

using CoLib.Logging.LogTargets;

namespace CoLib.Logging.OutputTargets;

/// <summary>
/// 日志输出基类
/// </summary>
public abstract class LogTarget: ILogTarget
{
    protected readonly LogManager LogMgr;
    protected readonly Func<LogItem, bool>? FilterFunc;
    private volatile LogLevel _minLogLevel;

    protected LogTarget(LogManager logMgr, LogLevel minLogLevel, Func<LogItem, bool>? filterFunc)
    {
        LogMgr = logMgr;
        FilterFunc = filterFunc;
        _minLogLevel = minLogLevel;
    }
    
    public LogLevel MinLogLevel
    {
        get => _minLogLevel;
        set => _minLogLevel = value;
    }

    public virtual void Dispose()
    {
    }

    public virtual bool Filter(in LogItem logItem)
    {
        var minLevel = _minLogLevel;
        if (logItem.Level < minLevel)
            return true;
        return FilterFunc != null && FilterFunc(logItem);
    }

    // 子类实现
    public abstract void Output(in LogItem logItem);
}