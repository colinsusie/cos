// Written by Colin on ${2023}-12-16

using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace CoLibBenchmark.Misc;

public class RwLockBench
{
    private const int MaxServiceId = 256;
    
    private readonly ReaderWriterLock _rwLock = new();
    private readonly int[] _services1 = new int[MaxServiceId];
    private readonly ConcurrentDictionary<int, int> _services2 = new();

    [Params(4, 8, 16)]
    public int ThreadCount;

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < MaxServiceId; ++i)
            _services2[i] = i;
    }

    // [Benchmark]
    // public void TestRwLock()
    // {
    //     var tasks = new Task[ThreadCount];
    //     for (var i = 0; i < ThreadCount; ++i)
    //     {
    //         var task = Task.Factory.StartNew(() =>
    //         {
    //             for (var k = 0; k < 10240; k++)
    //                 TryGet1(k, out var value);
    //         });
    //         tasks[i] = task;
    //     }
    //
    //     Task.WaitAll(tasks);
    // }
    //
    // [Benchmark]
    // public void TestCoDict()
    // {
    //     var tasks = new Task[ThreadCount];
    //     for (var i = 0; i < ThreadCount; ++i)
    //     {
    //         var task = Task.Factory.StartNew(() =>
    //         {
    //             for (var k = 0; k < 10240; k++)
    //                 TryGet2(k, out var value);
    //         });
    //         tasks[i] = task;
    //     }
    //
    //     Task.WaitAll(tasks);
    // }
    
    [Benchmark]
    public void Test()
    {
        var tasks = new Task[ThreadCount];
        for (var i = 0; i < ThreadCount; ++i)
        {
            var task = Task.Factory.StartNew(() =>
            {
                for (var k = 0; k < 10240; k++)
                    TryGet3(k, out var value);
            });
            tasks[i] = task;
        }

        Task.WaitAll(tasks);
    }
    
    bool TryGet1(int serviceId, out int value)
    {
        if (serviceId is <= 0 or >= MaxServiceId)
        {
            value = 0;
            return false;
        }
        
        _rwLock.AcquireReaderLock(Timeout.Infinite);
        try
        {
            value = _services1[serviceId];
            return true;
        }
        finally
        {
            _rwLock.ReleaseReaderLock();
        }
    }

    bool TryGet2(int serviceId, out int value)
    {
        return _services2.TryGetValue(serviceId, out value);
    }
    
    bool TryGet3(int serviceId, out int value)
    {
        if (serviceId is <= 0 or >= MaxServiceId)
        {
            value = 0;
            return false;
        }
        
        value = _services1[serviceId];
        return true;
    }
}