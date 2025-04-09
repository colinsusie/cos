// Written by Colin on 2023-11-24

using BenchmarkDotNet.Attributes;
using CoLib.ObjectPools;
using DotNetty.Common;
using Microsoft.Extensions.ObjectPool;

namespace CoLibBenchmark.ObjectPool;

public class SharedPooledObject
{
    private class Policy : IPooledObjectPolicy<SharedPooledObject>
    {
        SharedPooledObject IPooledObjectPolicy<SharedPooledObject>.Create() => new();
        bool IPooledObjectPolicy<SharedPooledObject>.Return(SharedPooledObject obj) => true;
    }
    
    private static readonly ObjectPool<SharedPooledObject> ObjectPool =
        new DefaultObjectPool<SharedPooledObject>(new Policy());

    public static SharedPooledObject Create()
    {
        return ObjectPool.Get();
    }

    public void Release()
    {
        ObjectPool.Return(this);
    }
}

public class TlsPooledObject: ICleanable
{
    [ThreadStatic]
    private static StObjectPool<TlsPooledObject>? _objectPool;

    public static TlsPooledObject Create()
    {
        _objectPool ??= new StObjectPool<TlsPooledObject>(100, () => new());
        return _objectPool.Get();
    }

    public void Release()
    {
        _objectPool ??= new StObjectPool<TlsPooledObject>(100, () => new());
        _objectPool.Return(this);
    }

    public void Cleanup()
    {
    }
}

public class NettyPooledObject
{
    static readonly ThreadLocalPool<NettyPooledObject> Pool = new(h => new NettyPooledObject(h));
    
    readonly ThreadLocalPool.Handle _handle;

    public NettyPooledObject(ThreadLocalPool.Handle handle)
    {
        _handle = handle;
    }

    public static NettyPooledObject Create()
    {
        return Pool.Take();
    }

    public void Release()
    {
        _handle.Release(this);
    }
}

[MemoryDiagnoser()]
public class ObjectPoolBenchmark
{
    [Benchmark]
    public void TestSharedObjectPool()
    {
        var tasks = new List<Task>(16);
        for (var i = 0; i < 16; ++i)
        {
            var task = Task.Run(() =>
            {
                for (var j = 0; j < 1000; ++j)
                {
                    var objs = new List<SharedPooledObject>();
                    for (var k = 0; k < 100; ++k)
                    {
                        objs.Add(SharedPooledObject.Create());
                    }

                    foreach (var obj in objs)
                    {
                        obj.Release();
                    }
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());
    }
    
    [Benchmark]
    public void TestTlsObjectPool()
    {
        var tasks = new List<Task>(16);
        for (var i = 0; i < 16; ++i)
        {
            var task = Task.Run(() =>
            {
                for (var j = 0; j < 1000; ++j)
                {
                    var objs = new List<TlsPooledObject>();
                    for (var k = 0; k < 100; ++k)
                    {
                        objs.Add(TlsPooledObject.Create());
                    }

                    foreach (var obj in objs)
                    {
                        obj.Release();
                    }
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());
    }
    
    [Benchmark]
    public void TestNettyPoolObject()
    {
        var tasks = new List<Task>(16);
        for (var i = 0; i < 16; ++i)
        {
            var task = Task.Run(() =>
            {
                for (var j = 0; j < 1000; ++j)
                {
                    var objs = new List<NettyPooledObject>();
                    for (var k = 0; k < 100; ++k)
                    {
                        objs.Add(NettyPooledObject.Create());
                    }

                    foreach (var obj in objs)
                    {
                        obj.Release();
                    }
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());
    }
    
    [Benchmark]
    public void TestNotPool()
    {
        var tasks = new List<Task>(16);
        for (var i = 0; i < 16; ++i)
        {
            var task = Task.Run(() =>
            {
                for (var j = 0; j < 1000; ++j)
                {
                    var objs = new List<TlsPooledObject>();
                    for (var k = 0; k < 100; ++k)
                    {
                        objs.Add(new TlsPooledObject());
                    }

                    objs.Clear();
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());
    }
}