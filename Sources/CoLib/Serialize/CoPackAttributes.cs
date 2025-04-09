// Written by Colin on 2024-10-19

namespace CoLib.Serialize;

/// <summary>
/// 指定类，结构或接口是可序列化的
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class CoPackableAttribute: Attribute;

/// <summary>
/// 用于支持继承
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class CoPackUnionAttribute : Attribute
{
    /// 唯一标识子类
    public int Tag { get; }
    /// 子类的类型
    public Type SubType { get; }

    public CoPackUnionAttribute(int tag, Type subType)
    {
        Tag = tag;
        SubType = subType;
    }
}

/// <summary>
/// 序列化标记位
/// </summary>
[Flags]
public enum PackFlags: byte
{
    Private = 1 << 0,
    Db = 1 << 1,
    
    All = 0xFF,
}

/// <summary>
/// 指定字段属性是序列化字段
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TagAttribute : Attribute
{
    /// 字符串Tag，如果为空就用intTag，否则优先用StrTag
    public string? StrTag { get; }
    /// 整型Tag
    public int IntTag { get; }
    /// 如果字段等于默认值是否序列化
    public bool PackIfDefault { get; }
    /// 序列化标志
    public PackFlags Flags { get; }

    public TagAttribute(int intTag, bool packIfDefault = false, PackFlags flags = PackFlags.All)
    {
        IntTag = intTag;
        PackIfDefault = packIfDefault;
        Flags = flags;
    }

    public TagAttribute(string strTag, bool packIfDefault = false, PackFlags flags = PackFlags.All)
    {
        StrTag = strTag;
        PackIfDefault = packIfDefault;
        Flags = flags;
    }
}

/// 指定构造函数，函数签名必须是：Constructor(object? state)
[AttributeUsage(AttributeTargets.Constructor)]
public class CoPackConstructorAttribute : Attribute;

/// 序列化流程：BeforeSerialize -> Serialize -> AfterSerialize
/// 反序列化流程：BeforeDeserialize -> Deserialize -> AfterDeserialize

/// <summary>
/// 指定该方法在对象序列化之前调用
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CoPackBeforeSerializeAttribute: Attribute;

/// <summary>
/// 指定该方法在对象序列化之后调用
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CoPackAfterSerializeAttribute: Attribute;

/// <summary>
/// 指定该方法在对象反序列化之前调用
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CoPackBeforeDeserializeAttribute: Attribute;

/// <summary>
/// 指定该方法在对象反序列化之后调用
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CoPackAfterDeserializeAttribute: Attribute;