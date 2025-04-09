// Written by Colin on 2024-10-19

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using CoLib.Common;
using CoLib.Container;
using CoLib.Serialize;
using MemoryPack;

namespace CoLibDemo.Serialize;

public class SerializeDemo
{
    public static void Start()
    {
        {
            SeObject? obj = null;
            var buffer = new PooledByteBufferWriter();
            CoPackSerializer.Serialize(buffer, obj);
            var obj2 = CoPackSerializer.Deserialize<SeObject>(buffer.WrittenSpan);
            Console.WriteLine($">>>>>>1{obj2}");
        }

        {
            Struct? struct1 = null;
            var buffer = new PooledByteBufferWriter();
            CoPackSerializer.Serialize(buffer, struct1);
            var obj2 = CoPackSerializer.Deserialize<Struct?>(buffer.WrittenSpan);
            Console.WriteLine($">>>>>>2{obj2}");
        }

        {
            List<Struct> structs = new()
            {
                new Struct()
                {
                    IntField = 1,
                },
                new Struct()
                {
                    IntField = 2,
                }
            };
            var buffer = new PooledByteBufferWriter();
            CoPackSerializer.Serialize(buffer, structs);
            var obj2 = CoPackSerializer.Deserialize<List<Struct>>(buffer.WrittenSpan);
            Console.WriteLine($">>>>>>2{obj2}");
        }
        
        // var obj = new SeObject(null);
        //
        // var buffer = new PooledByteBufferWriter();
        // CoPackSerializer.Serialize(buffer, obj);
        //
        // var json = CoPackSerializer.ToJson(buffer.WrittenSpan);
        // Console.WriteLine(json);
        //
        // var obj2 = CoPackSerializer.Deserialize<SeObject>(buffer.WrittenSpan, "hello");

        MemoryPackStruct? packStruct = null;
        var pck = MemoryPackSerializer.Serialize(packStruct);
    }
}

[MemoryPackable]
public partial struct MemoryPackStruct
{
    public int Field { get; set; }
    public string Field2 { get; set; }
}

//=============================================================================
// 类型声明

[CoPackable]
public partial class SeObject
{
    static partial void StaticConstructor()
    {
        Console.WriteLine("SeObject Static Constructor");
    }
    
    [Tag(1)]
    public int IntField = 1000;
    [Tag(2, true)]
    public int? IntField2 { get; set; } = null;
    [Tag(3, flags: PackFlags.Db)]
    public bool BoolField { get; set; } = true;
    [Tag(4)]
    public float FloatField { get; set; } = 1.234567f;
    [Tag("sf")]
    public string StringField { get; set; } = "hello 世界";
    [Tag("lf")]
    public List<long> LongFields { get; set; } = [1, 2, 3];
    [Tag(5)]
    public List<SeSubObject> SubObjects { get; set; } =
    [
        new()
        {
            IntField = 10000212,
            BoolField = false,
        },
        new()
        {
            IntField = 20000212,
            BoolField = true,
        },
    ];
    [Tag(6)]
    public Dictionary<string, double> DictField { get; set; } = new() 
    {
        {"hello", 1.0},
        {"强大", 100.0},
    };
    [Tag(7)]
    public SeSubObject ObjectField { get; set; } = new SeSubObject() 
    {
        IntField = 20000212,
        BoolField = true,
    };
    [Tag(8)]
    public SeSubObject? ObjectField2 { get; set; }
    [Tag(9)]
    public BaseObject UnionField { get; set; } = new DerivedObject() 
    {
        Field1 = true,
        Field2 = "yes",
    };
    [Tag(10)]
    public DateTimeOffset Time { get; set; } = new DateTimeOffset(new DateTime(2018, 9, 10));
    [Tag(11)]
    public Vector2? Pos { get; set; } = new Vector2(100, 200);
    [Tag(12)]
    public Color Color { get; set; } = Color.Red;
    [Tag(13)]
    public Timestamp Timestamp { get; set; } = Timestamp.UtcNow;
    [Tag(14)]
    public ValueTuple<int> Tuple1 { get; set; }
    [Tag(15)]
    public MyStructObject MySturct { get; set; }

    
    [CoPackConstructor]
    public SeObject(object? state)
    {
        string? str = (state as string);
        Console.WriteLine($"Create: {str}");
    }

    [CoPackBeforeSerialize]
    public void OnBeforeSerialize(PackFlags flags)
    {
        Console.WriteLine("OnBeforeSerialize");
    }

    [CoPackAfterSerialize]
    public void OnAfterSerialize(PackFlags flags)
    {
        Console.WriteLine("OnAfterSerialize");
    }

    [CoPackBeforeDeserialize]
    public void OnBeforeDeserialize(object? state)
    {
        Console.WriteLine("OnBeforeDeserialize");
    }

    [CoPackAfterDeserialize]
    public void OnAfterDeserialize(object? state)
    {
        Console.WriteLine("OnAfterDeserialize");
    }
}

public enum Color
{
    White,
    Blue,
    Red
};

[CoPackable]
public partial class SeSubObject
{
    [Tag(1)]
    public int IntField { get; set; }
    [Tag(2)]
    public bool BoolField { get; set; }

    [Tag(3)] 
    public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;
    [Tag(4)]
    public DateTime DateTime { get; set; }
    [Tag(5)]
    public Vector3 Pos;
    [Tag(6)]
    public Dictionary<int, List<long>> Field5 = new();
}

[CoPackable]
public partial struct Struct
{
    [Tag(1)]
    public int IntField { get; set; }
}


[CoPackUnion(1, typeof(DerivedObject))]
[CoPackable]
public partial class BaseObject
{
    [Tag(1)]
    public bool Field1 { get; set; }
}

[CoPackable]
public partial class DerivedObject : BaseObject
{
    [Tag(2)] // 子类的tag一定不能和父类相同
    public string? Field2 { get; set; }
}


[CoPackable]
public partial record struct MyStructObject
{
    [Tag(1)]
    private int Field1 = 0;
    [Tag(2)]
    public string Field2 = string.Empty;

    public MyStructObject()
    {
    }
}

// //=============================================================================
// // 代码生成
//
// public partial class SeObject
// {
//     // static SeObject()
//     // {
//     //     if (!CoPackFormatterProvider.IsRegistered<SeObject>())
//     //     {
//     //         CoPackFormatterProvider.Register(new SeObjectFormatter());
//     //     }
//     //     if (!CoPackFormatterProvider.IsRegistered<List<long>>())
//     //     {
//     //         CoPackFormatterProvider.Register(new ListFormatter<long>());
//     //     }
//     //     if (!CoPackFormatterProvider.IsRegistered<List<SeSubObject>>())
//     //     {
//     //         CoPackFormatterProvider.Register(new ListFormatter<SeSubObject>());
//     //     }
//     //     if (!CoPackFormatterProvider.IsRegistered<Dictionary<string, double>>())
//     //     {
//     //         CoPackFormatterProvider.Register(new DictionaryFormatter<string, double>());
//     //     }
//     //     if (!CoPackFormatterProvider.IsRegistered<Color>())
//     //     {
//     //         CoPackFormatterProvider.Register(new EnumFormatter<Color>());
//     //     }
//     // }
// }
//
// // public class SeObjectFormatter : ICoPackFormatter<SeObject>
// // {
// //     public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in SeObject? value, PackFlags flags) 
// //         where TBufferWriter : IBufferWriter<byte>
// //     {
// //         if (value == null)
// //         {
// //             writer.WriteNull();
// //             return;
// //         }
// //         
// //         value.OnBeforeSerialize(flags);
// //         
// //         writer.WriteObjectHeader();
// //
// //         if (value.IntField != 0 && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(1);
// //             writer.WriteVarInt(value.IntField);
// //         }
// //         
// //         if ((flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(2);
// //             writer.WriteValue(value.IntField2);
// //         }
// //
// //         if (value.BoolField && (flags & PackFlags.Db) != 0)
// //         {
// //             writer.WriteVarInt(3);
// //             writer.WriteBool(value.BoolField);
// //         }
// //
// //         if (value.FloatField != 0 && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(4);
// //             writer.WriteFloat(value.FloatField);
// //         }
// //         
// //         if (value.StringField != null && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteString("sf");
// //             writer.WriteString(value.StringField);
// //         }
// //
// //         if (value.LongFields != null && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteString("lf");
// //             writer.WriteValue(value.LongFields, flags);
// //         }
// //
// //         if (value.SubObjects != null && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(5);
// //             writer.WriteValue(value.SubObjects, flags);
// //         }
// //         
// //         if (value.DictField != null && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(6);
// //             writer.WriteValue(value.DictField, flags);
// //         }
// //         
// //         if (value.ObjectField != null && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(7);
// //             writer.WriteValue(value.ObjectField, flags);
// //         }
// //         
// //         if (value.ObjectField2 != null && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(8);
// //             writer.WriteValue(value.ObjectField2, flags);
// //         }
// //         
// //         if (value.UnionField != null && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(9);
// //             writer.WriteValue(value.UnionField, flags);
// //         }
// //         
// //         if ((flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(10);
// //             writer.WriteValue(value.Time, flags);
// //         }
// //         
// //         if ((flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(11);
// //             writer.WriteValue(value.Pos, flags);
// //         }
// //
// //         if (value.Color != Color.White && (flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(12);
// //             writer.WriteValue(value.Color, flags);
// //         }
// //
// //         if ((flags & PackFlags.All) != 0)
// //         {
// //             writer.WriteVarInt(13);
// //             writer.WriteValue(value.Timestamp, flags);
// //         }
// //         
// //         writer.WriteNull();
// //         
// //         value.OnAfterSerialize(flags);
// //     }
// //
// //     public SeObject? Read(ref CoPackReader reader, object? state)
// //     {
// //         if (reader.TryReadNull())
// //             return null;
// //         
// //         var value = new SeObject(state);
// //         
// //         value.OnBeforeDeserialize(state);
// //         
// //         reader.ReadObjectHeader();
// //         while (!reader.TryReadNull())
// //         {
// //             if (reader.TryReadInt32(out var intTag))
// //             {
// //                 switch (intTag)
// //                 {
// //                     case 1:
// //                         value.IntField = reader.ReadInt32();
// //                         break;
// //                     case 2:
// //                         value.IntField2 = reader.ReadValue<int?>(state);
// //                         break;
// //                     case 3:
// //                         value.BoolField = reader.ReadBool();
// //                         break;
// //                     case 4:
// //                         value.FloatField = reader.ReadFloat();
// //                         break;
// //                     case 5:
// //                         value.SubObjects = reader.ReadValue<List<SeSubObject>>(state);
// //                         break;
// //                     case 6:
// //                         value.DictField = reader.ReadValue<Dictionary<string, double>>(state);
// //                         break;
// //                     case 7:
// //                         value.ObjectField = reader.ReadValue<SeSubObject>(state);
// //                         break;
// //                     case 8:
// //                         value.ObjectField2 = reader.ReadValue<SeSubObject>(state);
// //                         break;
// //                     case 9:
// //                         value.UnionField = reader.ReadValue<BaseObject>(state);
// //                         break;
// //                     case 10:
// //                         value.Time = reader.ReadValue<DateTimeOffset>(state);
// //                         break;
// //                     case 11:
// //                         value.Pos = reader.ReadValue<Vector2>(state);
// //                         break;
// //                     case 12:
// //                         value.Color = reader.ReadValue<Color>(state);
// //                         break;
// //                     case 13:
// //                         value.Timestamp = reader.ReadValue<Timestamp>(state);
// //                         break;
// //                     default:
// //                         CoPackException.ThrowReadUnexpectedObjectTag(intTag);
// //                         break;
// //                 }
// //             }
// //             else
// //             {
// //                 var strTag = reader.ReadString();
// //                 switch (strTag)
// //                 {
// //                     case "sf":
// //                         value.StringField = reader.ReadString();
// //                         break;
// //                     case "lf":
// //                         value.LongFields = reader.ReadValue<List<long>>(state);
// //                         break;
// //                     default:
// //                         CoPackException.ThrowReadUnexpectedObjectTag(strTag);
// //                         break;
// //                 }
// //             }
// //         }
// //         
// //         value.OnAfterDeserialize(state);
// //         return value;
// //     }
// // }
//
// public partial class SeSubObject
// {
//     // static SeSubObject()
//     // {
//     //     if (!CoPackFormatterProvider.IsRegistered<SeSubObject>())
//     //     {
//     //         CoPackFormatterProvider.Register(new SeSubObjectFormatter());
//     //     }
//     // }
// }
//
// public sealed class SeSubObjectFormatter : ICoPackFormatter<SeSubObject?>
// {
//     public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in SeSubObject? value, PackFlags flags)
//         where TBufferWriter : IBufferWriter<byte>
//     {
//         if (value == null)
//         {
//             writer.WriteNull();
//             return;
//         }
//
//         writer.WriteObjectHeader();
//
//         if (value.IntField != 0 && (flags & PackFlags.All) != 0)
//         {
//             writer.WriteVarInt(1);
//             writer.WriteVarInt(value.IntField);
//         }
//
//         if (value.BoolField && (flags & PackFlags.All) != 0)
//         {
//             writer.WriteVarInt(2);
//             writer.WriteBool(value.BoolField);
//         }
//
//         writer.WriteNull();
//     }
//
//     public SeSubObject? Read(ref CoPackReader reader, object? state)
//     {
//         if (reader.TryReadNull())
//             return null;
//
//         var value = new SeSubObject();
//         reader.ReadObjectHeader();
//         while (!reader.TryReadNull())
//         {
//             var intTag = reader.ReadInt32();
//             switch (intTag)
//             {
//                 case 1:
//                     value.IntField = reader.ReadInt32();
//                     break;
//                 case 2:
//                     value.BoolField = reader.ReadBool();
//                     break;
//                 default:
//                     CoPackException.ThrowReadUnexpectedObjectTag(intTag);
//                     break;
//             }
//         }
//
//         return value;
//     }
// }
//
// public partial class BaseObject
// {
//     // static BaseObject()
//     // {
//     //     if (!CoPackFormatterProvider.IsRegistered<BaseObject>())
//     //     {
//     //         CoPackFormatterProvider.Register(new BaseObjectFormatter());
//     //     }
//     // }
// }
//
// public sealed class BaseObjectFormatter : ICoPackFormatter<BaseObject?>
// {
//     public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in BaseObject? value, PackFlags flags)
//         where TBufferWriter : IBufferWriter<byte>
//     {
//         if (value == null)
//         {
//             writer.WriteNull();
//             return;
//         }
//
//         switch (value)
//         {
//             case DerivedObject derivedObject:
//                 writer.WriteUnion(1, derivedObject, flags);
//                 break;
//             case { } thisObject:
//                 DoWrite(ref writer, thisObject, flags);
//                 break;
//             default:
//                 CoPackException.ThrowNotFoundInUnionType(value.GetType(), typeof(BaseObject));
//                 break;
//         }
//     }
//
//     public BaseObject? Read(ref CoPackReader reader, object? state)
//     {
//         if (reader.TryReadNull())
//             return null;
//
//         if (reader.TryReadObjectHeader())
//         {
//             return DoRead(ref reader, state);
//         }
//         
//         var tag = reader.ReadUnionHeader();
//         switch (tag)
//         {
//             case 1:
//                 return reader.ReadValue<DerivedObject>();
//             default:
//                 CoPackException.ThrowReadUnexpectedUnionTag(tag);
//                 return null;
//         }
//     }
//
//     private void DoWrite<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in BaseObject value,
//         PackFlags flags)
//         where TBufferWriter : IBufferWriter<byte>
//     {
//         writer.WriteObjectHeader();
//         if (value.Field1 && (flags & PackFlags.All) != 0)
//         {
//             writer.WriteVarInt(1);
//             writer.WriteBool(value.Field1);
//         }
//
//         writer.WriteNull();
//     }
//
//     private BaseObject DoRead(ref CoPackReader reader, object? state)
//     {
//         var value = new BaseObject();
//         while (!reader.TryReadNull())
//         {
//             var intTag = reader.ReadInt32();
//             switch (intTag)
//             {
//                 case 1:
//                     value.Field1 = reader.ReadBool();
//                     break;
//                 default:
//                     CoPackException.ThrowReadUnexpectedObjectTag(intTag);
//                     break;
//             }
//         }
//
//         return value;
//     }
// }
//
// public partial class DerivedObject
// {
//     // static DerivedObject()
//     // {
//     //     if (!CoPackFormatterProvider.IsRegistered<DerivedObject>())
//     //     {
//     //         CoPackFormatterProvider.Register(new DerivedObjectFormatter());
//     //     }
//     // }
// }
//
// public sealed class DerivedObjectFormatter : ICoPackFormatter<DerivedObject?>
// {
//     public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in DerivedObject? value, PackFlags flags)
//         where TBufferWriter : IBufferWriter<byte>
//     {
//         if (value == null)
//         {
//             writer.WriteNull();
//             return;
//         }
//
//         writer.WriteObjectHeader();
//         if (value.Field1 && (flags & PackFlags.All) != 0)
//         {
//             writer.WriteVarInt(1);
//             writer.WriteBool(value.Field1);
//         }
//
//         if (value.Field2 != null && (flags & PackFlags.All) != 0)
//         {
//             writer.WriteVarInt(2);
//             writer.WriteString(value.Field2);
//         }
//
//         writer.WriteNull();
//     }
//
//     public DerivedObject? Read(ref CoPackReader reader, object? state)
//     {
//         if (reader.TryReadNull())
//             return null;
//         
//         reader.ReadObjectHeader();
//         var value = new DerivedObject();
//         while (!reader.TryReadNull())
//         {
//             var intTag = reader.ReadInt32();
//             switch (intTag)
//             {
//                 case 1:
//                     value.Field1 = reader.ReadBool();
//                     break;
//                 case 2:
//                     value.Field2 = reader.ReadString();
//                     break;
//                 default:
//                     CoPackException.ThrowReadUnexpectedObjectTag(intTag);
//                     break;
//             }
//         }
//
//         return value;
//     }
// }


