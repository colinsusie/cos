// Written by Colin on ${CurrentDate.Year}-${CurrentDate.Month}-${CurrentDate.Day}

using System.Text;
using CoLib.Extensions;
using CoLib.Logging;
using CoLib.Logging.LogTargets;
using Serilog;
using Serilog.Configuration;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Templates;

namespace CoLibDemo.Logging;

// 测试日志的吞吐量
public class LogTpsDemo
{
    private const int WriteThread = 4;
    public static void Start()
    {
        TestCoLog();
        // TestSerilog();
    }

    static void TestCoLog()
    {
        var logMgr = new LogManagerBuilder()
            .AddOutputTarget(_ => new LogTargetStat())
            .Build();

        var logger = logMgr.GetLogger("LogTps");
        for (var i = 0; i < WriteThread; ++i)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var id = 107030862;
                    var testId = 139000025;
                    var subTestId = 155;
                    long uniqueId = 24;
                    var theName = "DoSomeThine";
        
                    logger.Warn($"[测试模块]{id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{testId}_{subTestId} UniqueId:{uniqueId} Index:1 TheName:{theName}]");
                }
            }, TaskCreationOptions.LongRunning);
        }
        
        Stat.Start();
        Console.ReadLine();
    }

    static void TestSerilog()
    {
        var enricher = new LogEventEnricher("D:\\mydev\\cos\\Tests\\CoLibBenchmark\\LogBenchmark.cs", "TestSerilog",
            107, "LogBenchmark");

        const string logTemp = "[{@t:yy-MM-dd HH:mm:ss.fff zzz}][{@l:u3}]" +
                               "[{ThreadId}]" +
                               "{#if Tag is not null}[{Tag}]{#end}" +
                               "{#if Method is not null}[{File}@{Method}:{Line}]{#end}" +
                               "{@m}\n{#if @x is not null}Exception: {@x}{#end}";

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .WriteTo.None(new ExpressionTemplate(logTemp))
            .CreateLogger();
        
        for (var i = 0; i < WriteThread; ++i)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var id = 107030862;
                    var testId = 139000025;
                    var subTestId = 155;
                    long uniqueId = 24;
                    var theName = "DoSomeThine";
        
                    using (LogContext.Push(enricher))
                    {
                        Log.Warning(
                            "[测试模块]{Id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{TestId}_{SubTestId} UniqueId:{UniqueId} Index:1 TheName:{TheName}]",
                            id, testId, subTestId, uniqueId, theName);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        
        Stat.Start();
        Console.ReadLine();
    }
}

public class Stat
{
    public static long _count;

    public static void IncCount()
    {
        Interlocked.Increment(ref _count);
    }
    
    public static void Start()
    {
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                var span = TimeSpanExt.FromStart();
                Thread.Sleep(1000);
                span = TimeSpanExt.FromStart() - span;

                var count = Interlocked.Exchange(ref _count, 0);
                var countPerSec = (int)(count / span.TotalSeconds);
                Console.WriteLine($"LogOutput = {countPerSec}/S");
            }
        }, TaskCreationOptions.LongRunning);
    }
}

public class LogTargetStat: ILogTarget
{
    public LogLevel MinLogLevel { get; set; }

    public bool Filter(in LogItem logItem)
    {
        return false;
    }

    public void Output(in LogItem logItem)
    {
        Stat.IncCount();
    }

    public void Dispose()
    {
    }
}

public static class NoneLoggerConfigurationExtensions
{
    private const string DefaultConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration None(
        this LoggerSinkConfiguration sinkConfiguration,
        ITextFormatter formatter)
    {
        if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));
        return sinkConfiguration.Sink(new StatSink(formatter));
    }
}

class StatSink : ILogEventSink
{
    readonly ITextFormatter _formatter;

    const int DefaultWriteBufferCapacity = 256;

    public StatSink(ITextFormatter formatter)
    {
        _formatter = formatter;
    }

    public void Emit(LogEvent logEvent)
    {
        var buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));
        _formatter.Format(logEvent, buffer);
        var str = buffer.ToString();
        
        Stat.IncCount();
    }
}