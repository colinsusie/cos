// Written by Colin on 2023-11-03

using CoLib.Logging.LogTargets;

namespace CoLib.Logging.OutputTargets;

/// <summary>
/// 输出到控制台
/// </summary>
public class LogConsole: LogTarget
{
    private readonly object _syncObj = new();

    public LogConsole(LogManager logMgr, LogLevel minLogLevel, Func<LogItem, bool>? filterFunc = null): 
        base(logMgr, minLogLevel, filterFunc)
    {
    }
    
    public override void Output(in LogItem logItem)
    {
        var writer = logItem.Level >= LogLevel.Error ? Console.Error : Console.Out;

        if (LogMgr.AsyncOutput)     // 异步模式有一个专用的线程，不用锁
        {
            writer.WriteLine(logItem.Message);
        }
        else
        {
            lock (_syncObj)
            {
                writer.WriteLine(logItem.Message);
            }   
        }
    }

    public override void Dispose()
    {
        Console.Out.Flush();
        Console.Error.Flush();
    }
}