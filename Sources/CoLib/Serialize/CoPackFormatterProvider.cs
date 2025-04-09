// Written by Colin on 2024-10-10

using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CoLib.Common;

namespace CoLib.Serialize;

public static class CoPackFormatterProvider
{
    static readonly Dictionary<Type, Type> TupleFormatters = new Dictionary<Type, Type>(8)
    {
        { typeof(ValueTuple<>), typeof(ValueTupleFormatter<>) },
        { typeof(ValueTuple<,>), typeof(ValueTupleFormatter<,>) },
        { typeof(ValueTuple<,,>), typeof(ValueTupleFormatter<,,>) },
        { typeof(ValueTuple<,,,>), typeof(ValueTupleFormatter<,,,>) },
        { typeof(ValueTuple<,,,,>), typeof(ValueTupleFormatter<,,,,>) },
        { typeof(ValueTuple<,,,,,>), typeof(ValueTupleFormatter<,,,,,>) },
        { typeof(ValueTuple<,,,,,,>), typeof(ValueTupleFormatter<,,,,,,>) },
        { typeof(ValueTuple<,,,,,,,>), typeof(ValueTupleFormatter<,,,,,,,>) },
    };
    
    // generics known types
    static readonly Dictionary<Type, Type> KnownGenericTypeFormatters = new Dictionary<Type, Type>(3)
    {
        { typeof(Nullable<>), typeof(NullableFormatter<>) },
    };
    
    static readonly Dictionary<Type, Type> CollectionFormatters = new Dictionary<Type, Type>(18)
    {
        { typeof(List<>), typeof(ListFormatter<>) },
        { typeof(Stack<>), typeof(StackFormatter<>) },
        { typeof(Queue<>), typeof(QueueFormatter<>) },
        { typeof(LinkedList<>), typeof(LinkedListFormatter<>) },
        { typeof(HashSet<>), typeof(HashSetFormatter<>) },
        { typeof(SortedSet<>), typeof(SortedSetFormatter<>) },
        { typeof(PriorityQueue<,>), typeof(PriorityQueueFormatter<,>) },
        { typeof(Dictionary<,>), typeof(DictionaryFormatter<,>) },
        { typeof(SortedDictionary<,>), typeof(SortedDictionaryFormatter<,>) },
    };
    
    static CoPackFormatterProvider()
    {
        RegisterWellKnownFormatters();
    }

    private static void RegisterWellKnownFormatters()
    {
        Register(new Int8Formatter());
        Register(new NullableFormatter<sbyte>());
        Register(new UInt8Formatter());
        Register(new NullableFormatter<byte>());
        Register(new Int16Formatter());
        Register(new NullableFormatter<short>());
        Register(new UInt16Formatter());
        Register(new NullableFormatter<ushort>());
        Register(new Int32Formatter());
        Register(new NullableFormatter<int>());
        Register(new UInt32Formatter());
        Register(new NullableFormatter<uint>());
        Register(new Int64Formatter());
        Register(new NullableFormatter<long>());
        Register(new UInt64Formatter());
        Register(new NullableFormatter<ulong>());
        Register(new FloatFormatter());
        Register(new NullableFormatter<float>());
        Register(new DoubleFormatter());
        Register(new NullableFormatter<double>());
        Register(new BoolFormatter());
        Register(new NullableFormatter<bool>());
        Register(new StringFormatter());
        Register(new BytesFormatter());
        
        Register(new DateTimeOffsetFormatter());
        Register(new NullableFormatter<DateTimeOffset>());
        Register(new DateTimeFormatter());
        Register(new NullableFormatter<DateTime>());
        Register(new TimestampFormatter());
        Register(new NullableFormatter<Timestamp>());
        Register(new TimeSpanFormatter());
        Register(new NullableFormatter<TimeSpan>());
        Register(new Vector2Formatter());
        Register(new NullableFormatter<Vector2>());
        Register(new Vector3Formatter());
        Register(new NullableFormatter<Vector3>());
        Register(new Vector3Formatter());
        Register(new NullableFormatter<Vector3>());
        Register(new Vector4Formatter());
        Register(new NullableFormatter<Vector4>());
    }

    /// 该类型的序列化器是否已经注册
    public static bool IsRegistered<T>() => Check<T>.Registered;
    
    /// 注册一个类型的序列化器
    public static void Register<T>(ICoPackFormatter<T> formatter)
    {
        Check<T>.Registered = true;
        Cache<T>.Formatter = formatter;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ICoPackFormatter<T> GetFormatter<T>()
    {
        return Cache<T>.Formatter!;
    }
    
    private static class Check<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static bool Registered;
    }

    private static class Cache<T>
    {
        public static ICoPackFormatter<T>? Formatter = null;

        static Cache()
        {
            if (Check<T>.Registered) return;
            
            // 第1次尝试：强制触发类型的静态构造函数
            var type = typeof(T);
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            if (Formatter != null) return;
            
            // 第2次尝试：动态构造格式化器
            TryCreateGenericFormatter(type);
            if (Formatter != null) return;
            
            // 失败
            Formatter = new ErrorFormatter<T>();
            Check<T>.Registered = true;
        }
        
        private static void TryCreateGenericFormatter(Type type)
        {
            var formatterType = TryCreateEnumFormatter(type);
            if (formatterType != null) goto CREATE;
            
            formatterType = TryCreateGenericFormatterType(type, TupleFormatters);
            if (formatterType != null) goto CREATE;
            
            formatterType = TryCreateGenericFormatterType(type, KnownGenericTypeFormatters);
            if (formatterType != null) goto CREATE;
            
            formatterType = TryCreateGenericFormatterType(type, CollectionFormatters);
            if (formatterType != null) goto CREATE;
            
            return;
            CREATE:
            var formatObj = Activator.CreateInstance(formatterType);
            Formatter = formatObj as ICoPackFormatter<T>;
        }

        static Type? TryCreateEnumFormatter(Type type)
        {
            return type.IsEnum ? typeof(EnumFormatter<>).MakeGenericType(type) : null;
        }
        
        static Type? TryCreateGenericFormatterType(Type type, IDictionary<Type, Type> knownTypes)
        {
            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();
                if (knownTypes.TryGetValue(genericDefinition, out var formatterType))
                {
                    return formatterType.MakeGenericType(type.GetGenericArguments());
                }
            }

            return null;
        }
    }
}