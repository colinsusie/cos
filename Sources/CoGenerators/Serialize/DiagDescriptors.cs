// Written by Colin on 2024-10-20

using Microsoft.CodeAnalysis;

namespace CoGenerators.Serialize;

public static class DiagDescriptors
{
    public static DiagnosticDescriptor CommonError { get; } = new (
        id: "SeriGen1001",
        title: "发生错误",
        messageFormat: "发生错误：{0}",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor PartialError { get; } = new (
        id: "SeriGen1002",
        title: "类型错误",
        messageFormat: "{0}必须是partial类，以便生成Formatter注册代码",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor FieldRadOnlyError { get; } = new (
        id: "SeriGen2001",
        title: "字段错误",
        messageFormat: "字段{0}不可以是只读，否则CoPack将无法序列化",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor FieldTagLenError { get; } = new (
        id: "SeriGen2002",
        title: "字段错误",
        messageFormat: "字段{0},Tag属性参数数量不对",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor FieldTagTypeError { get; } = new (
        id: "SeriGen2003",
        title: "字段错误",
        messageFormat: "字段{0},Tag属性参数类型不对",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor FieldStrTagEmptyError { get; } = new (
        id: "SeriGen2004",
        title: "字段错误",
        messageFormat: "字段{0},字符串Tag不可以为空",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor FieldIntTagDupError { get; } = new (
        id: "SeriGen2005",
        title: "字段错误",
        messageFormat: "字段{0},整型Tag不可以重复:{1}",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor FieldStrTagDupError { get; } = new (
        id: "SeriGen2006",
        title: "字段错误",
        messageFormat: "字段{0}, 字符串Tag不可以重复:{1}",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor UnionTypeError { get; } = new (
        id: "SeriGen3001",
        title: "联合错误",
        messageFormat: "类型{0}的联合{1}找不到类型",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ConstructorError { get; } = new (
        id: "SeriGen4001",
        title: "方法错误",
        messageFormat: "CoPack构造函数的签名不正确，必须是 (object? state)",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor BeforeSerializeError { get; } = new (
        id: "SeriGen4003",
        title: "方法错误",
        messageFormat: "BeforeSerialize的签名不正确，必须是 (PackFlags flags)",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor AfterSerializeError { get; } = new (
        id: "SeriGen4004",
        title: "方法错误",
        messageFormat: "AfterSerialize的签名不正确，必须是 (PackFlags flags)",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor BeforeDeserializeError { get; } = new (
        id: "SeriGen4005",
        title: "方法错误",
        messageFormat: "BeforeDeserialize的签名不正确，必须是 (object? state)",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor AfterDeserializeError { get; } = new (
        id: "SeriGen4006",
        title: "方法错误",
        messageFormat: "AfterDeserialize的签名不正确，必须是 (object? state)",
        category: "SeriGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}