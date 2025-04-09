// Written by Colin on 2024-10-11

using System.Buffers;

namespace CoLib.Serialize;

public sealed class ValueTupleFormatter<T1> : ICoPackFormatter<ValueTuple<T1?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 1);
        writer.WriteValue(value.Item1, flags);
    }

    public ValueTuple<T1?> Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 1)
            CoPackException.ThrowReadUnexpectedLength(len, 1);

        return new ValueTuple<T1?>(reader.ReadValue<T1>(state));
    }
}

public sealed class ValueTupleFormatter<T1, T2> : ICoPackFormatter<ValueTuple<T1?, T2?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?, T2?> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 2);
        writer.WriteValue(value.Item1, flags);
        writer.WriteValue(value.Item2, flags);
    }

    public (T1?, T2?) Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 2)
            CoPackException.ThrowReadUnexpectedLength(len, 2);

        return new ValueTuple<T1?, T2?>(
            reader.ReadValue<T1>(state),
            reader.ReadValue<T2>(state));
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3> : ICoPackFormatter<ValueTuple<T1?, T2?, T3?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?, T2?, T3?> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 3);
        writer.WriteValue(value.Item1, flags);
        writer.WriteValue(value.Item2, flags);
        writer.WriteValue(value.Item3, flags);
    }

    public (T1?, T2?, T3?) Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 3)
            CoPackException.ThrowReadUnexpectedLength(len, 3);

        return new ValueTuple<T1?, T2?, T3?>(
            reader.ReadValue<T1>(state),
            reader.ReadValue<T2>(state),
            reader.ReadValue<T3>(state)
        );
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4> : ICoPackFormatter<ValueTuple<T1?, T2?, T3?, T4?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?, T2?, T3?, T4?> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 4);
        writer.WriteValue(value.Item1, flags);
        writer.WriteValue(value.Item2, flags);
        writer.WriteValue(value.Item3, flags);
        writer.WriteValue(value.Item4, flags);
    }

    public (T1?, T2?, T3?, T4?) Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 4)
            CoPackException.ThrowReadUnexpectedLength(len, 4);

        return new ValueTuple<T1?, T2?, T3?, T4?>(
            reader.ReadValue<T1>(state),
            reader.ReadValue<T2>(state),
            reader.ReadValue<T3>(state),
            reader.ReadValue<T4>(state)
        );
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5> : ICoPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?, T2?, T3?, T4?, T5?> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 5);
        writer.WriteValue(value.Item1, flags);
        writer.WriteValue(value.Item2, flags);
        writer.WriteValue(value.Item3, flags);
        writer.WriteValue(value.Item4, flags);
        writer.WriteValue(value.Item5, flags);
    }

    public (T1?, T2?, T3?, T4?, T5?) Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 5)
            CoPackException.ThrowReadUnexpectedLength(len, 5);

        return new ValueTuple<T1?, T2?, T3?, T4?, T5?>(
            reader.ReadValue<T1>(state),
            reader.ReadValue<T2>(state),
            reader.ReadValue<T3>(state),
            reader.ReadValue<T4>(state),
            reader.ReadValue<T5>(state)
        );
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6> : ICoPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 6);
        writer.WriteValue(value.Item1, flags);
        writer.WriteValue(value.Item2, flags);
        writer.WriteValue(value.Item3, flags);
        writer.WriteValue(value.Item4, flags);
        writer.WriteValue(value.Item5, flags);
        writer.WriteValue(value.Item6, flags);
    }

    public (T1?, T2?, T3?, T4?, T5?, T6?) Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 6)
            CoPackException.ThrowReadUnexpectedLength(len, 6);

        return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.ReadValue<T1>(state),
            reader.ReadValue<T2>(state),
            reader.ReadValue<T3>(state),
            reader.ReadValue<T4>(state),
            reader.ReadValue<T5>(state),
            reader.ReadValue<T6>(state)
        );
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : ICoPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 7);
        writer.WriteValue(value.Item1, flags);
        writer.WriteValue(value.Item2, flags);
        writer.WriteValue(value.Item3, flags);
        writer.WriteValue(value.Item4, flags);
        writer.WriteValue(value.Item5, flags);
        writer.WriteValue(value.Item6, flags);
        writer.WriteValue(value.Item7, flags);
    }

    public (T1?, T2?, T3?, T4?, T5?, T6?, T7?) Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 7)
            CoPackException.ThrowReadUnexpectedLength(len, 7);

        return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.ReadValue<T1>(state),
            reader.ReadValue<T2>(state),
            reader.ReadValue<T3>(state),
            reader.ReadValue<T4>(state),
            reader.ReadValue<T5>(state),
            reader.ReadValue<T6>(state),
            reader.ReadValue<T7>(state)
        );
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : ICoPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
    where TRest : struct
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteVarIntWithType(CoPackType.List, 7);
        writer.WriteValue(value.Item1, flags);
        writer.WriteValue(value.Item2, flags);
        writer.WriteValue(value.Item3, flags);
        writer.WriteValue(value.Item4, flags);
        writer.WriteValue(value.Item5, flags);
        writer.WriteValue(value.Item6, flags);
        writer.WriteValue(value.Item7, flags);
        writer.WriteValue(value.Rest, flags);
    }

    public ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> Read(ref CoPackReader reader, object? state)
    {
        var len = reader.ReadListHeader();
        if (len != 8)
            CoPackException.ThrowReadUnexpectedLength(len, 8);

        return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            reader.ReadValue<T1>(state),
            reader.ReadValue<T2>(state),
            reader.ReadValue<T3>(state),
            reader.ReadValue<T4>(state),
            reader.ReadValue<T5>(state),
            reader.ReadValue<T6>(state),
            reader.ReadValue<T7>(state),
            reader.ReadValue<TRest>(state)
        );
    }
}