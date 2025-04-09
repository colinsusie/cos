// Written by Colin on 2023-11-03

namespace CoLib.Logging.LogTargets;

/// <summary>
/// 待输出的日志项
/// </summary>
public record struct LogItem(string Message, LogLevel Level, string Tag);


/// <summary>
/// 日志输出目标接口
/// </summary>
public interface ILogTarget: IDisposable
{
    // 设置输出的最小等级
    public LogLevel MinLogLevel { get; set; }
    // 过滤日志，返回true将不输出
    public bool Filter(in LogItem logItem);
    // 输出一条日志项
    public void Output(in LogItem logItem);
}