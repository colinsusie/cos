using CoLib.ObjectPools;
using Microsoft.Extensions.ObjectPool;
using Xunit.Abstractions;

namespace CoLibUnitTest;

file class SimpleObject: ICleanable
{
    public string Name = string.Empty;
    public int Level;
    
    public void Cleanup()
    {
        Name = string.Empty;
        Level = 0;
    }
}

public class ObjectPoolTest
{
    private readonly ITestOutputHelper _output;

    public ObjectPoolTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void StObjectPool_PoolSize()
    {
        var objPool = new StObjectPool<SimpleObject>(10, () => new SimpleObject());
        var num = 5;
        for (var i = 0; i < num; ++i)
        {
            objPool.Return(new SimpleObject());
        }
        
        Assert.Equal(num, objPool.Count);
    }
    
    [Fact]
    public void StObjectPool_MaxCapacity()
    {
        var maxCapacity = 10;
        var objPool = new StObjectPool<SimpleObject>(maxCapacity, () => new SimpleObject());
        for (var i = 0; i < 20; ++i)
        {
            objPool.Return(new SimpleObject());
        }
        
        Assert.Equal(maxCapacity, objPool.Count);
    }
    
    [Fact]
    public void StObjectPool_Reset()
    {
        var objPool = new StObjectPool<SimpleObject>(10, () => new SimpleObject());
        var obj = objPool.Get();
        obj.Name = "Hello";
        obj.Level = 10;

        objPool.Return(obj);

        obj = objPool.Get();
        
        Assert.True(obj is {Level: 0, Name: ""});
    }
    
    [Fact]
    public void MtObjectPool_PoolSize()
    {
        var objPool = new MtObjectPool<SimpleObject>(5000, () => new SimpleObject());
        
        var list = new List<Task>();
        for (var i = 0; i < 5; ++i)
        {
            list.Add(Task.Run(() =>
            {
                for (var k = 0; k < 1000; ++k)
                    objPool.Return(new SimpleObject());
            }));
        }

        Task.WaitAll(list.ToArray());
        
        Assert.Equal(5000, objPool.Count);
    }
    
    [Fact]
    public void MtObjectPool_MaxCapacity()
    {
        var objPool = new MtObjectPool<SimpleObject>(100, () => new SimpleObject());
        
        var list = new List<Task>();
        for (var i = 0; i < 5; ++i)
        {
            list.Add(Task.Run(() =>
            {
                for (var k = 0; k < 1000; ++k)
                    objPool.Return(new SimpleObject());
            }));
        }

        Task.WaitAll(list.ToArray());
        
        Assert.Equal(100, objPool.Count);
    }
    
    [Fact]
    public void MtObjectPool_MultiOp()
    {
        var objPool = new MtObjectPool<SimpleObject>(1000, () => new SimpleObject());
        
        var list = new List<Task>();
        for (var i = 0; i < 5; ++i)
        {
            list.Add(Task.Run(() =>
            {
                for (var k = 0; k < 1000; ++k)
                {
                    objPool.Return(new SimpleObject());
                }
                for (var k = 0; k < 1000; ++k)
                {
                    var obj = objPool.Get();
                    objPool.Return(obj);
                }
            }));
        }

        Task.WaitAll(list.ToArray());
    }
}