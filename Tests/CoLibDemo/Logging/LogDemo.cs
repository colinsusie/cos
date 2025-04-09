// Written by Colin on 2023-11-02

using CoLib.Logging;
using CoLib.Logging.OutputTargets;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;

namespace CoLibDemo.Logging;

internal class LogEventEnricher : ILogEventEnricher
{
    private const string Tag = "Tag";
    private const string File = "File";
    private const string Method = "Method";
    private const string Line = "Line";

    private readonly string _file;
    private readonly string _method;
    private readonly int _line;
    private readonly string _sourceContext;

    public LogEventEnricher(string file, string method, int line, string sourceContext)
    {
        _file = file;
        _method = method;
        _line = line;
        _sourceContext = sourceContext;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(Tag, _sourceContext));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(File, Path.GetFileName(_file)));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(Method, _method));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(Line, _line));
    }
}

public static class LogDemo
{
    public static void Start()
    {
        TestSimple();
        // TestMultiThread();
        // TestSerilogMultiThread();
    }

    private static void TestSimple()
    {
        // 配置日志库
        var logMgr = new LogManagerBuilder()
            .SetMinLogLevel(LogLevel.Info)
            .AddOutputConsole()
            .AddOutputFile("{ProcessName}_{Date}.log", LogLevel.Debug, null, 200)
            .Build();
        
        var logger = logMgr.GetLogger("App");
        logger.Debug($"hello {100} world: {true}, {DateTimeOffset.Now:yy-MM-dd HH:mm:ss.fff}");
        logger.Info($"hello {100} world: {true}, {DateTimeOffset.Now:yy-MM-dd HH:mm:ss.fff}");
        logger.Warn($"hello {100} world: {true}, {DateTimeOffset.Now:yy-MM-dd HH:mm:ss.fff}");
        logger.Error($"hello {100} world: {true}, {DateTimeOffset.Now:yy-MM-dd HH:mm:ss.fff}");

        // 停止日志库
        logMgr.Stop();
    }

    private static void TestMultiThread()
    {
        var logMgr = new LogManagerBuilder()
            .SetAsyncOutput(false, 0)
            .AddOutputConsole()
            .AddOutputFile("{ProcessName}_{ProcessId}_{Date}.log")
            .AddOutputFile("{ProcessName}_{ProcessId}_{Date}_error.log", LogLevel.Error)
            .Build();

        var tasks = new List<Task>();
        for (var i = 0; i < 8; ++i)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var k = 0; k < 100; ++k)
                {
                    var id = 107030862;
                    var testId = 139000025;
                    var subTestId = 155;
                    long uniqueId = 24;
                    var theName = "DoSomeThine";
        
                    var logger = logMgr.GetLogger("MultiThread");
                    logger.Debug($"[测试模块]{id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{testId}_{subTestId} UniqueId:{uniqueId} Index:1 TheName:{theName}]");
                    logger.Info($"[测试模块]{id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{testId}_{subTestId} UniqueId:{uniqueId} Index:1 TheName:{theName}]");
                    logger.Warn($"[测试模块]{id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{testId}_{subTestId} UniqueId:{uniqueId} Index:1 TheName:{theName}]");
                    logger.Error($"[测试模块]{id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{testId}_{subTestId} UniqueId:{uniqueId} Index:1 TheName:{theName}]");
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
        
        logMgr.Stop();
    }

    private static void TestSerilogMultiThread()
    {
        var enricher = new LogEventEnricher("D:\\mydev\\cos\\Tests\\CoLibBenchmark\\LogDemo.cs", "TestSerilogMultiThread",
            107, "MultiThread");

        const string logTemp = "[{@t:yy-MM-dd HH:mm:ss.fff zzz}][{@l:u3}]" +
                               "[{ThreadId}]" +
                               "{#if Tag is not null}[{Tag}]{#end}" +
                               "{#if Method is not null}[{File}@{Method}:{Line}]{#end}" +
                               "{@m}\n{#if @x is not null}Exception: {@x}{#end}";

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .MinimumLevel.Debug()
            .WriteTo.Console(new ExpressionTemplate(logTemp))
            .CreateLogger();
        
        var tasks = new List<Task>();
        for (var i = 0; i < 8; ++i)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var k = 0; k < 100; ++k)
                {
                    var id = 107030862;
                    var testId = 139000025;
                    var subTestId = 155;
                    long uniqueId = 24;
                    var theName = "DoSomeThine";
        
                    using (LogContext.Push(enricher))
                    {
                        Log.Debug("[测试模块]{Id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{TestId}_{SubTestId} UniqueId:{UniqueId} Index:1 TheName:{TheName}]",
                            id, testId, subTestId, uniqueId, theName);
                        Log.Information("[测试模块]{Id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{TestId}_{SubTestId} UniqueId:{UniqueId} Index:1 TheName:{TheName}]",
                            id, testId, subTestId, uniqueId, theName);
                        Log.Warning("[测试模块]{Id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{TestId}_{SubTestId} UniqueId:{UniqueId} Index:1 TheName:{TheName}]",
                            id, testId, subTestId, uniqueId, theName);
                        Log.Error("[测试模块]{Id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{TestId}_{SubTestId} UniqueId:{UniqueId} Index:1 TheName:{TheName}]",
                            id, testId, subTestId, uniqueId, theName);
                    }
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
        
        Log.CloseAndFlush();
    }
}