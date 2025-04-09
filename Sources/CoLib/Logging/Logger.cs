// Written by Colin on 2023-11-02

using System.Runtime.CompilerServices;
using CoLib.Logging.LogTargets;

namespace CoLib.Logging;

/// <summary>
/// 日志输出器
/// </summary>
public class Logger
{
    // 所属的管理器
    public LogManager LogMgr { get; }
    // 日志标签
    public string Tag { get; }
    
    internal Logger(LogManager logMgr, string tag)
    {
        LogMgr = logMgr;
        Tag = tag;
    }

    /// <summary>
    /// 输出Debug日志，比如： logger.Debug($"hello {Name}");
    /// </summary>
    public void Debug([InterpolatedStringHandlerArgument("")] ref DebugInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            LogMgr.Output(new LogItem(handler.ToStringAndClear(), LogLevel.Debug, Tag));
        }
    } 
    
    /// <summary>
    /// 输出Info日志，比如： logger.Info($"hello {Name}");
    /// </summary>
    public void Info([InterpolatedStringHandlerArgument("")] ref InfoInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            LogMgr.Output(new LogItem(handler.ToStringAndClear(), LogLevel.Info, Tag));
        }
    }
    
    /// <summary>
    /// 输出Warn日志，比如： logger.Warn($"hello {Name}");
    /// </summary>
    public void Warn([InterpolatedStringHandlerArgument("")] ref WarnInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            LogMgr.Output(new LogItem(handler.ToStringAndClear(), LogLevel.Warn, Tag));
        }
    }
    
    /// <summary>
    /// 输出Error日志，比如： logger.Error($"hello {Name}");
    /// </summary>
    public void Error([InterpolatedStringHandlerArgument("")] ref ErrorInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            LogMgr.Output(new LogItem(handler.ToStringAndClear(), LogLevel.Error, Tag));
        }
    }
}