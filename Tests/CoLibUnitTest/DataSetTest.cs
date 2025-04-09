// // Written by Colin on 2024-1-6

using CoLib.Container;
using Xunit.Abstractions;

namespace CoLibUnitTest;

public class DataSetTest
{
    private readonly ITestOutputHelper _output;

    public DataSetTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestDataSet()
    {
        using ( var container = DataSet.Create())
        {
            container.Add(new Data1 {A = 1, B = 2,});
            container.Add(new Data2 {A = true, B = "abc",});

            Assert.True(container.TryGet<Data1>(out var item1));
            Assert.Equal(1, item1.A);
            Assert.Equal(2, item1.B);
            Assert.True(container.TryGet<Data2>(out var item2));
            Assert.True(item2.A);
            Assert.Equal("abc", item2.B);
        }

        using (var container = DataSet.Create())
        {
            container.Add(new Data1 {A = 1, B = 2,});
            container.Add(new Data2 {A = true, B = "hello",});

            Assert.True(container.TryGet<Data1>(out var item1));
            Assert.Equal(1, item1.A);
            Assert.Equal(2, item1.B);
            Assert.True(container.TryGet<Data2>(out var item2));
            Assert.True(item2.A);
            Assert.Equal("hello", item2.B);
        }
    }

    [Fact]
    public void TestData()
    {
        using (var item = DataItem.Create())
        {
            item.Set(new Data2 {A = true, B = "aaaa"});
            var data = item.Get<Data2>();
            Assert.True(data.A);
            Assert.True(data.B == "aaaa");
        }       
        
        using (var item = DataItem.Create())
        {
            item.Set(new Data2 {A = false, B = "bbb"});
            var data = item.Get<Data2>();
            Assert.False(data.A);
            Assert.True(data.B == "bbb");
        }
    }
}

struct Data1
{
    public int A;
    public int B;
}

struct Data2
{
    public bool A;
    public string B;
}