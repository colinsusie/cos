// Written by Colin on 2024-9-26

using Microsoft.CodeAnalysis;

namespace CoGenerators.Rpc;

public static class DiagDescriptors
{
    public static DiagnosticDescriptor CommonError { get; } = new (
        id: "RpcGen1001",
        title: "发生错误",
        messageFormat: "发生错误：{0}",
        category: "RpcGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ArgsNumError { get; } = new (
        id: "RpcGen1002",
        title: "参数错误",
        messageFormat: "参数最多只能有{0}个，当前是{1}个",
        category: "RpcGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ReturnTypeError1 { get; } = new (
        id: "RpcGen2001",
        title: "返回值错误",
        messageFormat: "返回值必须是[void|ValueTask|ValueTask<T>]，当前返回值为:{0}",
        category: "RpcGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ReturnTypeError2 { get; } = new (
        id: "RpcGen2002",
        title: "返回值错误",
        messageFormat: "方法{0}返回的泛型参数类型{1}是类或结构，类型声明必须有MemoryPackable属性",
        category: "RpcGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ServiceError1 { get; } = new (
        id: "RpcGen3001",
        title: "类型错误",
        messageFormat: "{0}必须是partial类，以便生成服务派发代码",
        category: "RpcGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ServiceError2 { get; } = new (
        id: "RpcGen3002",
        title: "找不到服务接口",
        messageFormat: "{0}没有实现IRpcService的子类",
        category: "RpcGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}