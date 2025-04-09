// Written by Colin on 2023-11-03

using CoLib.Logging.LogTargets;

namespace CoLib.Logging.OutputTargets;

/// <summary>
/// 什么也不输出
/// </summary>
public class LogNone: LogTarget
{
    public LogNone(LogManager logMgr, Func<LogItem, bool>? filterFunc = null) : 
        base(logMgr, LogLevel.Debug, filterFunc)
    {
    }
    
    public override void Output(in LogItem logItem)
    {
        // 什么也不做
    }
}