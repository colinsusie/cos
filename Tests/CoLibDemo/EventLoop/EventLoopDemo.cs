// Written by Colin on 2024-9-21

using CoLib.EventLoop;
using CoLib.Logging;

namespace CoLibDemo.EventLoop;

public class EventLoopDemo
{
    private static StEventLoop _loop;
    private static TaskCompletionSource? _tsc;
    
    public static void Start()
    {
        var logMgr = new LogManagerBuilder()
            .SetMinLogLevel(LogLevel.Info)
            .AddOutputConsole()
            .Build();

        _loop = new(logMgr.GetLogger("eventloop"));
        
        _loop.Execute(DoTest);
        
        Console.ReadLine();
    }

    public static void DoTest()
    {
        for (var i = 0; i < 1; ++i)
        {
            Console.WriteLine($"{Environment.CurrentManagedThreadId}: {i}");
            _ = WaitDelay(i);
        }
    }

    static Task DelayTest()
    {
        if (_tsc == null)
        {
            _tsc = new TaskCompletionSource();
            _ = DoDelay();
        }

        return _tsc.Task.WaitAsync(TimeSpan.FromMilliseconds(500));

        async Task DoDelay()
        {
            await Task.Delay(10000);
            _tsc?.TrySetResult();
        }
    }

    static async Task WaitDelay(int index)
    {
        await DelayTest();
        Console.WriteLine($"{Environment.CurrentManagedThreadId}, WaitDelay: {index}");
    }
}