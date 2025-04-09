// Written by Colin on 2023-11-24

using BenchmarkDotNet.Attributes;
using CoLib.Container;

namespace CoLibBenchmark.Container;

[MemoryDiagnoser]
public class LongIndexMapBenchmark
{
    private const int Capacity = 4096;
    private Dictionary<long, TestValue> _dict = new(Capacity);
    private List<long> _dictKeys = new();
    
    private LongIndexMap<TestValue> _indexMap = new(Capacity, 0);
    private List<LongIndexId> _indexKeys = new();
    
    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < Capacity; ++i)
        {
            var key = Random.Shared.NextInt64();
            _dictKeys.Add(key);
            _dict.Add(key, new TestValue());
            
            var idxKey = _indexMap.Add(new TestValue());
            _indexKeys.Add(idxKey);
        }
    }
    
    [Benchmark]
    public void DictionaryGet()
    {
        foreach (var key in _dictKeys)
        {
            _dict.TryGetValue(key, out _);
        }
    }

    [Benchmark]
    public void IndexMapGet()
    {
        foreach (var key in _indexKeys)
        {
            _indexMap.TryGet(key, out _);
        }
    }
    
    [Benchmark]
    public void DictionaryAdd()
    {
        var dict = new Dictionary<long, TestValue>(Capacity);
        for (var i = 0; i < Capacity; ++i)
        {
            dict.Add(i, new TestValue());
        }
    }

    [Benchmark]
    public void IndexMapAdd()
    {
        var map = new LongIndexMap<TestValue>(Capacity, 0);
        for (var i = 0; i < Capacity; ++i)
        {
            map.Add(new TestValue());
        }
    }
}

[MemoryDiagnoser]
public class IntIndexMapBenchmark
{
    private const int Capacity = 4096;
    private Dictionary<int, TestValue> _dict = new(Capacity);
    private List<int> _dictKeys = new();
    
    private IntIndexMap<TestValue> _indexMap = new(Capacity, 0);
    private List<IntIndexId> _indexKeys = new();
    
    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < Capacity; ++i)
        {
            var key = Random.Shared.Next();
            _dictKeys.Add(key);
            _dict.Add(key, new TestValue());
            
            var idxKey = _indexMap.Add(new TestValue());
            _indexKeys.Add(idxKey);
        }
    }
    
    [Benchmark]
    public void DictionaryGet()
    {
        foreach (var key in _dictKeys)
        {
            _dict.TryGetValue(key, out _);
        }
    }

    [Benchmark]
    public void IndexMapGet()
    {
        foreach (var key in _indexKeys)
        {
            _indexMap.TryGet(key, out _);
        }
    }
    
    [Benchmark]
    public void DictionaryAdd()
    {
        var dict = new Dictionary<int, TestValue>(Capacity);
        for (var i = 0; i < Capacity; ++i)
        {
            dict.Add(i, new TestValue());
        }
    }

    [Benchmark]
    public void IndexMapAdd()
    {
        var map = new IntIndexMap<TestValue>(Capacity, 0);
        for (var i = 0; i < Capacity; ++i)
        {
            map.Add(new TestValue());
        }
    }
}

internal class TestValue
{
}