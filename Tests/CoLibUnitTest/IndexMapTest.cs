// Written by Colin on 2023-11-24

using CoLib.Container;
using Xunit.Abstractions;

namespace CoLibUnitTest;

public class IndexMapTest
{
    private readonly ITestOutputHelper _output;

    public IndexMapTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestLongIndexId()
    {
        var id1 = new LongIndexId(1001, 1);
        var id2 = new LongIndexId(1001, 2);
        Assert.True(id1.CompareTo(id2) == -1);
        Assert.False(id1.Equals(id2));

        uint version = 0xFFFFFFFE;
        Assert.True((int)version == -2);
        Assert.True(id1.ToString() == "4299262263297");

        var id3 = id1;
        Assert.True(id1 == id3);
        Assert.True(id1 != id2);
        Assert.True(id1 < id2);
        Assert.True(id1 <= id2);
        Assert.False(id1 > id2);
        Assert.False(id1 >= id2);

        var str = $"{id1}";
        Assert.True(str == "4299262263297");
    }
    
    [Fact]
    public void TestIntIndexId()
    {
        var id1 = new IntIndexId(1001, 1);
        var id2 = new IntIndexId(1001, 2);
        Assert.True(id1.CompareTo(id2) == -1);
        Assert.False(id1.Equals(id2));
        Assert.True(id1.ToString() == "65601537");

        var id3 = id1;
        Assert.True(id1 == id3);
        Assert.True(id1 != id2);
        Assert.True(id1 < id2);
        Assert.True(id1 <= id2);
        Assert.False(id1 > id2);
        Assert.False(id1 >= id2);

        var str = $"{id1}";
        Assert.True(str == "65601537");
    }

    [Fact]
    public void TestLongIndexMap()
    {
        var indexMap = new LongIndexMap<TestObject>(16, 16);

        var obj1 = new TestObject();
        var obj2 = new TestObject();
        var obj3 = new TestObject();
        var obj4 = new TestObject();
        
        var id1 = indexMap.Add(obj1);
        var id2 = indexMap.Add(obj2);
        var id3 = indexMap.Add(obj3);
        var id4 = indexMap.Add(obj4);
        
        Assert.True(indexMap.TryGet(id2, out var obj));
        Assert.Equal(obj, obj2);
        
        // 扩充大小
        for (var i = 0; i < 28; ++i)
        {
            indexMap.Add(new TestObject());
        }
        
        // 删除缓存并重用
        indexMap.TryRemove(id1);
        indexMap.TryRemove(id2);
        indexMap.TryRemove(id3);
        var newId = indexMap.Add(new TestObject());
        Assert.True(newId.Index == id1.Index);
        Assert.True(newId.Version == 2);
        
        Assert.False(indexMap.TryGet(new LongIndexId(32, 1), out _));
    }
    
    [Fact]
    public void TestIntIndexMap()
    {
        var indexMap = new IntIndexMap<TestObject>(16);

        var obj1 = new TestObject();
        var obj2 = new TestObject();
        var obj3 = new TestObject();
        var obj4 = new TestObject();
        
        var id1 = indexMap.Add(obj1);
        var id2 = indexMap.Add(obj2);
        var id3 = indexMap.Add(obj3);
        var id4 = indexMap.Add(obj4);
        
        Assert.True(indexMap.TryGet(id2, out var obj));
        Assert.Equal(obj, obj2);
        
        // 扩充大小
        for (var i = 0; i < 28; ++i)
        {
            indexMap.Add(new TestObject());
        }
        
        // 删除缓存并重用
        indexMap.TryRemove(id1);
        indexMap.TryRemove(id2);
        indexMap.TryRemove(id3);
        var newId = indexMap.Add(new TestObject());
        Assert.True(newId.Index == id1.Index);
        Assert.True(newId.Version == 2);
        
        Assert.False(indexMap.TryGet(new IntIndexId(32, 1), out _));
    }
}

file class TestObject
{
    
}