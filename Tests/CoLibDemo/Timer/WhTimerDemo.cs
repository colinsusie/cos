// Written by Colin on 2023-12-10

using CoLib.Container;
using CoLib.Logging;
using CoLib.Logging.OutputTargets;
using CoLib.Timer;

namespace CoLibDemo.Timer;

public static class WhTimerDemo
{
    private static Logger _logger = null!;
    private static ITimerScheduler _scheduler = null!;
    private static TimerGroup _group = new();
    
    public static void Start()
    {
        var logMgr = new LogManagerBuilder()
            .AddOutputConsole()
            .Build();
        
        _logger = logMgr.GetLogger("WhTimerDemo");
        _scheduler = TimerFactory.CreateWhTimerScheduler(new WhTimerOptions{
            TickInterval = 10
        }, logMgr.GetLogger("WhTimerScheduler"));

        StartTest();
        
        while (true)
        {
            _scheduler.Schedule();
            Thread.Sleep(1);
        }
    }

    public static void StartTest()
    {
        _logger.Debug($"Start");
        // 1tick, 0period
        // _scheduler.AddTimer(10, 0, _ =>
        // {
        //     _logger.Debug($"Hello 1 tick, 0 period");
        // });
        
        // _scheduler.AddTimer(100, 100, _ =>
        // {
        //     _logger.Debug($"Hello 1 tick, n period");
        // });
        
        // 在第2个轮
        // _scheduler.AddTimer(0x100, 0, _ =>
        // {
        //     _logger.Debug($"Hello 1 tick, 0 period");
        // });
        
        // 在第3个轮
        // _scheduler.AddTimer(0x4000, 0, _ =>
        // {
        //     _logger.Debug($"Hello 1 tick, 0 period");
        // });
        
        // _scheduler.AddTimer(18000, 1000, _ =>
        // {
        //     _logger.Debug($"Hello 1 tick, 0 period");
        // });
        _scheduler.AddTimer(0, 0, OnFirstTimer);
        
        // 0xB6D3
        var time = DateTime.Now;
        _scheduler.AddTimer(255, 255, _ =>
        {
            var now = DateTime.Now;
            var span = (now - time);
            time = now;
            // _logger.Debug($"Hello: {(int)span.TotalMilliseconds}");
        });
    }
    
    private static void OnFirstTimer(object? obj)
    {
        // _logger.Debug($"Hello");
        
        for (var i = 0; i < 10; ++i)
        {
            _scheduler.AddTimer(200, 300, OnLoopTimer);
        }
        
        for (var i = 0; i < 5; ++i)
        {
            var data = DataItem.Create();
            data.Set(_scheduler.AddTimer(5000, 5000, OnLoopTimer2, data));
        }
        
        for (var i = 0; i < 10; ++i)
        {
            _scheduler.AddTimer(_group, 3000, 3000, OnLoopTimer3, null);
        }

        _scheduler.AddTimer(1000, 1000, _ =>
        {
            _logger.Debug($"{_scheduler.GetDebugInfo(false)}");
        });
    }

    private static void OnLoopTimer(DataItem? data)
    {
        // _logger.Debug($"Hello");
    }

    private static void OnLoopTimer2(DataItem? data)
    {
        var item = data?.Get<long>();
        if (item != null)
        {
            _scheduler.RemoveTimer(item.Value);
        }
    }

    private static void OnLoopTimer3(DataItem? data)
    {
        // _logger.Debug($"group");
        _scheduler.RemoveGroupTimers(_group);
    }
}