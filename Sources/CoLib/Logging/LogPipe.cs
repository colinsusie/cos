// Written by Colin on 2023-11-02

using System.Collections.Concurrent;
using System.Threading.Channels;
using CoLib.Logging.LogTargets;

namespace CoLib.Logging;

/// <summary>
/// 日志管道类
/// </summary>
internal class LogPipe
{
    private readonly LogManager _logMgr;
    private readonly ChannelWriter<LogItem>? _writer;
    private readonly Task? _completeTask;


    public LogPipe(LogManager logMgr)
    {
        _logMgr = logMgr;
        if (_logMgr.AsyncOutput)
        {
            var channel = Channel.CreateBounded<LogItem>(new BoundedChannelOptions(_logMgr.LogQueueCapacity)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropWrite
            }, _logMgr.OnLogDrop);
            _writer = channel.Writer;
            _completeTask = Consume(channel.Reader);
        }
    }

    // 发送日志
    public void Send(in LogItem logItem)
    {
        // 同步模式
        if (_writer == null)
        {
            _logMgr.OutputToTarget(logItem);
            return;
        }

        _writer.TryWrite(logItem);
    }

    // 读取日志并输出
    private async Task Consume(ChannelReader<LogItem> reader)
    {
        await Task.Yield();
        
        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (reader.TryRead(out var logItem))
            {
                OutputToTargets(logItem);
            }
        }
    }
    
    // 输出到目标
    private void OutputToTargets(in LogItem logItem)
    {
        foreach (var target in _logMgr.Targets)
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
                _logMgr.OnLogException(target, logItem, e);
            }
        }
    }

    public void Stop()
    {
        _writer?.TryComplete();
        _completeTask?.Wait();
    }
}