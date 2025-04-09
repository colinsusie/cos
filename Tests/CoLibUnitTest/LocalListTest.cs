// Written by Colin on 2023-11-26

using CoLib.Container;

namespace CoLibUnitTest;

public class LocalListTest
{
    [Fact]
    public void TestValue()
    {
        using LocalList<int> list = new(4);
        for (var i = 0; i < 20; ++i)
        {
            list.Add(i);
        }

        using LocalList<int> list2 = new LocalList<int>();
        
        Assert.True(list.Count == 20);
        Assert.True(list.Capacity == 32);
        
        Assert.True(list[1] == 1);
        Assert.True(list.Get(2) == 2);
        list.Set(2, 100);
        Assert.True(list.Get(2) == 100);

        list.Remove(0);
        list.Remove(10);
        list.RemoveAt(list.Count-1);
        Assert.True(list.Count == 17);
        Assert.True(list[0] == 1);
        
        list.Clear();
        Assert.True(list.Count == 0);

        Assert.ThrowsAny<Exception>(() =>
        {
            list.Set(-1, 100);
        });
        
        Assert.ThrowsAny<Exception>(() =>
        {
            list.Set(10, 100);
        });
    }

    [Fact]
    public void TestEnumerator()
    {
        using LocalList<int> list = new();
        foreach (var v in list)
        {
        }
        
        for (var i = 0; i < 10; ++i)
            list.Add(i);

        var v2 = 0;
        foreach (var v in list)
        {
            Assert.True(v == v2++);
        }
    }

    [Fact]
    public void TestReference()
    {
        using LocalList<string> list = new();
        for (var i = 0; i < 10; ++i)
            list.Add(i.ToString());
        
        Assert.True(list[4] == "4");
        list.RemoveAt(4);
        Assert.True(list[4] == "5");
    }
}