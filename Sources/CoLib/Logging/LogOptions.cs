// Written by Colin on 2025-1-25

namespace CoLib.Logging;

/// <summary>
/// 日志选项
/// </summary>
public sealed class LogOptions
{
    public LogLevel MinLogLevel { get; set; } = LogLevel.Debug;
    public bool AsyncOutput { get; set; } = true;
    public int AsyncQueueCapacity { get; set; } = 100000;
    public LogConsoleOptions? LogConsoleOptions { get; set; }
    public List<LogFileOptions> LogFileOptions { get; set; } = new();
}

/// <summary>
/// 日志控制台选项
/// </summary>
public sealed class LogConsoleOptions
{
    public LogLevel MinLogLevel { get; set; } = LogLevel.Debug;
}

/// <summary>
/// 日志文件选项
/// </summary>
public sealed class LogFileOptions
{
    public string LogPath { get; set; } = string.Empty;
    public LogLevel MinLogLevel { get; set; } = LogLevel.Debug;
    public long RollOnFileSize { get; set; } = 50 * 1024 * 1024;
}



