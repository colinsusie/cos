// Written by Colin on 2023-11-03

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using CoLib.Logging;
using CoLib.Logging.OutputTargets;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;
using Logger = CoLib.Logging.Logger;

namespace CoLibBenchmark.Logging;


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

[HideColumns(Column.Error, Column.StdDev, Column.StdErr)]
[MemoryDiagnoser]
public class LogBenchmark
{
    private LogManager _logMgr = null!;
    private Logger _logger = null!;
    private LogEventEnricher _enricher = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        // CoLog
        {
            // 配置日志库
            _logMgr = new LogManagerBuilder()
                .SetAsyncOutput(false, 0)
                .AddOutputTarget(logMgr => new LogNone(logMgr))
                .Build();
            _logger = _logMgr.GetLogger("LogBenchmark");
        }

        // Serilog
        {
            _enricher = new LogEventEnricher("D:\\mydev\\cos\\Tests\\CoLibBenchmark\\LogBenchmark.cs", "TestSerilog",
                107, "LogBenchmark");

            const string logTemp = "[{@t:yy-MM-dd HH:mm:ss.fff zzz}][{@l:u3}]" +
                                   "[{ThreadId}]" +
                                   "{#if Tag is not null}[{Tag}]{#end}" +
                                   "{#if Method is not null}[{File}@{Line}:{Method}]{#end}" +
                                   "{@m}\n{#if @x is not null}Exception: {@x}{#end}";

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.None(new ExpressionTemplate(logTemp))
                .CreateLogger();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _logMgr.Stop();
        Log.CloseAndFlush();
    }
    
    [Benchmark]
    public void TestSerilog()
    {
        var id = 107030862;
        var testId = 139000025;
        var subTestId = 155;
        long uniqueId = 24;
        var theName = "DoSomeThine";
        
        using (LogContext.Push(_enricher))
        {
            Log.Warning(
                "[测试模块]{Id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{TestId}_{SubTestId} UniqueId:{UniqueId} Index:1 TheName:{TheName}]",
                id, testId, subTestId, uniqueId, theName);
        }
    }

    [Benchmark(Baseline = true)]
    public void TestCoLog()
    {
        var id = 107030862;
        var testId = 139000025;
        var subTestId = 155;
        long uniqueId = 24;
        var theName = "DoSomeThine";
        
        _logger.Warn($"[测试模块]{id}[完成测试行为成功 Path:TestAction_281474976710752_Test_{testId}_{subTestId} UniqueId:{uniqueId} Index:1 TheName:{theName}]");
    }
}