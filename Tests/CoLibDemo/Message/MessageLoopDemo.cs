// Written by Colin on 2023-12-15

using CoLib.Extensions;
using CoLib.Logging;
using CoLib.Logging.OutputTargets;
using CoLib.Message;
using CoLib.ObjectPools;

namespace CoLibDemo.Message;

public class TestMessage: IMessage, ICleanable
{
    [ThreadStatic] private static StObjectPool<TestMessage>? _pool;
    private static StObjectPool<TestMessage> Pool => _pool ??= new StObjectPool<TestMessage>(128, () => new TestMessage());
    
    public static TestMessage Create()
    {
        return Pool.Get();
    }
    
    public void Process()
    {
        Stat.IncCount();
    }
    
    public void Cleanup()
    {
    }

    public void Dispose()
    {
        Pool.Return(this);
    }
}

public class CallMessage: IMessage
{
    private readonly Action _action;
    
    public CallMessage(Action action)
    {
        _action = action;
    }
    
    public void Process()
    {
        _action();
    }
    public void Dispose()
    {
        
    }
}

public class MessageLoopDemo
{
    public static Task Start()
    {
        return TestAsyncAction();
        // Throughout();
        // return Task.CompletedTask;
    }

    static async Task TestAsyncAction()
    {
        var logMgr = new LogManagerBuilder()
            .AddOutputConsole()
            .Build();

        var logger = logMgr.GetLogger("Demo");
        logger.Info($"Ready");
        
        var loop1 = new MessageLoop(logMgr.GetLogger(nameof(MessageLoop)));
        await loop1.StartAsync();
        logger.Info($"Start");
        
        
        loop1.Post(state =>
        {
            logger.Info($"Execute: {state}");
        }, 100);
        Thread.Sleep(100);
        
        await loop1.ExecuteAsync(state =>
        {
            logger.Info($"ExecuteAsync: {state}");
            Thread.Sleep(10);
            return ValueTask.CompletedTask;
        }, new object());
        logger.Info($"ExecuteAsync Done");

        await loop1.ExecuteAsync(state =>
        {
            logger.Info($"ExecuteAsync2: {state}");
            return ValueTask.CompletedTask;
        }, 100);
        logger.Info($"ExecuteAsync2 Done");

        await loop1.ExecuteAsync(state =>
        {
            logger.Info($"ExecuteAsync3: {state}");
            Thread.Sleep(10);
            return ValueTask.CompletedTask;
        }, new object());
        logger.Info($"ExecuteAsync3 Done");
        
        await loop1.ExecuteAsync(async state =>
        {
            await Task.Delay(2000);
            logger.Info($"ExecuteAsync4: {state}");
        }, new object());
        logger.Info($"ExecuteAsync4 Done");
        
        var result = await loop1.ExecuteAsync(async state =>
        {
            await Task.Delay(2000);
            logger.Info($"ExecuteAsync5: {state}");
            return state;
        }, true);
        logger.Info($"ExecuteAsync5 Done: {result}");

        logger.Info($"Execute Done");
        
        await loop1.StopAsync();
        logger.Info($"stop");
        logMgr.Stop();
    }
    
    public static async Task TestSimple()
    {
        var logMgr = new LogManagerBuilder()
            .AddOutputConsole()
            .Build();

        var logger = logMgr.GetLogger("Demo");
        logger.Info($"Ready");
        
        var loop = new MessageLoop(logMgr.GetLogger(nameof(MessageLoop)));
        await loop.StartAsync();
        logger.Info($"Start");

        for (var i = 0; i < 10; ++i)
        {
            var i1 = i;
            loop.Post(new CallMessage(() =>
            {
                logger.Info($"hello: {i1}");
            }));
        }

        await loop.StopAsync();
        logger.Info($"stop");
        logMgr.Stop();
    }

    public static void Throughout()
    {
        var logMgr = new LogManagerBuilder()
            .AddOutputConsole()
            .Build();
        
        var loop = new MessageLoop(logMgr.GetLogger(nameof(MessageLoop)));
        loop.StartAsync().Wait();

        for (var i = 0; i < 4; ++i)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    loop.Post(TestMessage.Create());
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
                Console.WriteLine($"EventLoop = {countPerSec}/S");
            }
        }, TaskCreationOptions.LongRunning);
    }
}