// Written by Colin on 2024-10-11

using System.Buffers;
using System.Runtime.InteropServices;

namespace CoLib.Serialize;

public sealed class ListFormatter<T>: ICoPackFormatter<List<T?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in List<T?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteListHeader(value.Count);
        
        var formatter = CoPackFormatterProvider.GetFormatter<T>();
        var listSpan = CollectionsMarshal.AsSpan(value);
        for (var i = 0; i < listSpan.Length; i++)
        {
            formatter.Write(ref writer, in listSpan[i], flags);
        }
    }

    public List<T?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadListHeader();
        var list = new List<T?>(len);
        for (var i = 0; i < len; ++i)
        {
            list.Add(reader.ReadValue<T>(state));
        }

        return list;
    }
}

public sealed class HashSetFormatter<T> : ICoPackFormatter<HashSet<T?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in HashSet<T?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteListHeader(value.Count);
        
        var formatter = CoPackFormatterProvider.GetFormatter<T>();
        foreach (var item in value)
        {
            formatter.Write(ref writer, in item, flags);
        }
    }

    public HashSet<T?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadListHeader();
        var set = new HashSet<T?>(len);
        for (var i = 0; i < len; ++i)
        {
            set.Add(reader.ReadValue<T>(state));
        }

        return set;
    }
}

public sealed class QueueFormatter<T>: ICoPackFormatter<Queue<T?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in Queue<T?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteListHeader(value.Count);
        
        var formatter = CoPackFormatterProvider.GetFormatter<T>();
        foreach (var item in value)
        {
            formatter.Write(ref writer, in item, flags);
        }
    }

    public Queue<T?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadListHeader();
        var queue = new Queue<T?>(len);
        for (var i = 0; i < len; ++i)
        {
            queue.Enqueue(reader.ReadValue<T>(state));
        }

        return queue;
    }
}

public sealed class StackFormatter<T>: ICoPackFormatter<Stack<T?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in Stack<T?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteListHeader(value.Count);
        
        var formatter = CoPackFormatterProvider.GetFormatter<T>();
        foreach (var item in value.Reverse())
        {
            formatter.Write(ref writer, in item, flags);
        }
    }

    public Stack<T?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadListHeader();
        var stack = new Stack<T?>(len);
        for (var i = 0; i < len; ++i)
        {
            stack.Push(reader.ReadValue<T>(state));
        }

        return stack;
    }
}

public sealed class LinkedListFormatter<T>: ICoPackFormatter<LinkedList<T?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in LinkedList<T?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteListHeader(value.Count);
        
        var formatter = CoPackFormatterProvider.GetFormatter<T>();
        foreach (var item in value)
        {
            formatter.Write(ref writer, in item, flags);
        }
    }

    public LinkedList<T?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadListHeader();
        var list = new LinkedList<T?>();
        for (var i = 0; i < len; ++i)
        {
            list.AddLast(reader.ReadValue<T>(state));
        }

        return list;
    }
}

public sealed class SortedSetFormatter<T>: ICoPackFormatter<SortedSet<T?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in SortedSet<T?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteListHeader(value.Count);
        
        var formatter = CoPackFormatterProvider.GetFormatter<T>();
        foreach (var item in value)
        {
            formatter.Write(ref writer, in item, flags);
        }
    }

    public SortedSet<T?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadListHeader();
        var set = new SortedSet<T?>();
        for (var i = 0; i < len; ++i)
        {
            set.Add(reader.ReadValue<T>(state));
        }

        return set;
    }
}

public sealed class DictionaryFormatter<TK, TV>: ICoPackFormatter<Dictionary<TK, TV?>>
    where TK : notnull
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in Dictionary<TK, TV?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteMapHeader(value.Count);
        var keyFormatter = CoPackFormatterProvider.GetFormatter<TK>();
        var valueFormatter = CoPackFormatterProvider.GetFormatter<TV>();
        foreach (var (k, v) in value)
        {
            keyFormatter.Write(ref writer, in k, flags);
            valueFormatter.Write(ref writer, in v, flags);
        }
    }

    public Dictionary<TK, TV?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadMapHeader();
        var dict = new Dictionary<TK, TV?>();
        var keyFormatter = CoPackFormatterProvider.GetFormatter<TK>();
        var valueFormatter = CoPackFormatterProvider.GetFormatter<TV>();
        for (var i = 0; i < len; ++i)
        {
            var key = keyFormatter.Read(ref reader, state);
            var value = valueFormatter.Read(ref reader, state);
            dict.Add(key!, value);
        }

        return dict;
    }
}

public sealed class SortedDictionaryFormatter<TK, TV>: ICoPackFormatter<SortedDictionary<TK, TV?>>
    where TK : notnull
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in SortedDictionary<TK, TV?>? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        writer.WriteMapHeader(value.Count);
        var keyFormatter = CoPackFormatterProvider.GetFormatter<TK>();
        var valueFormatter = CoPackFormatterProvider.GetFormatter<TV>();
        foreach (var (k, v) in value)
        {
            keyFormatter.Write(ref writer, in k, flags);
            valueFormatter.Write(ref writer, in v, flags);
        }
    }

    public SortedDictionary<TK, TV?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadMapHeader();
        var dict = new SortedDictionary<TK, TV?>();
        var keyFormatter = CoPackFormatterProvider.GetFormatter<TK>();
        var valueFormatter = CoPackFormatterProvider.GetFormatter<TV>();
        for (var i = 0; i < len; ++i)
        {
            var key = keyFormatter.Read(ref reader, state);
            var value = valueFormatter.Read(ref reader, state);
            dict.Add(key!, value);
        }

        return dict;
    }
}

public sealed class PriorityQueueFormatter<TE, TP> : ICoPackFormatter<PriorityQueue<TE?, TP?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in PriorityQueue<TE?, TP?>? value, PackFlags flags)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteMapHeader(value.Count);

        var eleFormatter = CoPackFormatterProvider.GetFormatter<TE>();
        var priFormatter = CoPackFormatterProvider.GetFormatter<TP>();
        foreach (var item in value.UnorderedItems)
        {
            eleFormatter.Write(ref writer, in item.Element, flags);
            priFormatter.Write(ref writer, in item.Priority, flags);
        }
    }

    public PriorityQueue<TE?, TP?>? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var len = reader.ReadMapHeader();
        var priQueue = new PriorityQueue<TE?, TP?>();
        var keyFormatter = CoPackFormatterProvider.GetFormatter<TE>();
        var valueFormatter = CoPackFormatterProvider.GetFormatter<TP>();
        for (var i = 0; i < len; ++i)
        {
            var key = keyFormatter.Read(ref reader, state);
            var value = valueFormatter.Read(ref reader, state);
            priQueue.Enqueue(key, value);
        }

        return priQueue;
    }
}