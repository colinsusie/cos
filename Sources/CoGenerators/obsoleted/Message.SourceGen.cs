// // Written by Colin on 2023-12-30

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoGenerators.Message;

/// <summary>
/// 生成服务消息索引和处理器
/// </summary>
[Generator]
public class SourceGen : IIncrementalGenerator
{
    private const string MessageTypeAttribute = "CoRuntime.Services.MessageTypeAttribute";
    private const string MessageSendAttribute = "CoRuntime.Services.MessageSendAttribute";
    private const string MessageCallAttribute = "CoRuntime.Services.MessageCallAttribute";
    private const string MessageInvokeAttribute = "CoRuntime.Services.MessageInvokeAttribute";
    private const string MessageDispatcherAttribute = "CoRuntime.Services.MessageDispatcherAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // // 得到服务消息类型列表
        // IncrementalValuesProvider<string> msgTypes = context.SyntaxProvider.CreateSyntaxProvider(
        //         predicate: static (node, _) => IsMessageSyntaxTarget(node),
        //         transform: static (ctx, _) => GetMessageSemanticTarget(ctx))
        //     .Where(static m => m is not null)
        //     .Select(static (m, _) => m!);
        //
        // // 得到服务处理器语法节点列表
        // IncrementalValuesProvider<ClassDeclarationSyntax> dispatcherSyntaxList = context.SyntaxProvider.CreateSyntaxProvider(
        //     predicate: static (node, _) => IsHandlerSyntaxTarget(node),
        //     transform: static (ctx, _) => GetHandlerSemanticTarget(ctx))
        //     .Where(static m => m is not null)
        //     .Select(static (m, _) => m!);
        //
        // // 组合收集到的信息
        // IncrementalValueProvider<(Compilation, ImmutableArray<string>)> target1 = context.CompilationProvider.Combine(msgTypes.Collect());
        // var target2 = target1.Combine(dispatcherSyntaxList.Collect()); 
        //
        // // 生成源码
        // context.RegisterSourceOutput(target2, static (spc, source) => Execute(spc, source.Left.Item1, source.Left.Item2, source.Right));
    }

    // 类，结构，或记录的语法
    private static bool IsMessageSyntaxTarget(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax {AttributeLists.Count: > 0} 
            or StructDeclarationSyntax {AttributeLists.Count: > 0}
            or RecordDeclarationSyntax {AttributeLists.Count: > 0};
    }

    // 声明有MessageTypeAttribute属性的语法
    private static string? GetMessageSemanticTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeSyntax)
            return null;

        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeSyntax); 
        if (typeSymbol is not ITypeSymbol)
            return null;
        return SymbolHelper.HasAttribute(typeSymbol, MessageTypeAttribute) ? typeSymbol.ToDisplayString() : null;
    }

    // 类
    private static bool IsHandlerSyntaxTarget(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classSyntax)
            return false;

        return classSyntax.AttributeLists.Count > 0;
    }

    // 声明MessageDispatcher属性的类
    private static ClassDeclarationSyntax? GetHandlerSemanticTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
            return null;

        return SymbolHelper.HasAttribute(classSymbol, MessageDispatcherAttribute) ? classSyntax : null;
    }

    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<string> msgTypes,
        ImmutableArray<ClassDeclarationSyntax> dispatchers)
    {
        // 生成消息类型索引映射
        Dictionary<string, int> msgIndexes = new Dictionary<string, int>();
        var msgIdx = 1;
        foreach (var type in msgTypes)
        {
            if (!msgIndexes.ContainsKey(type))
                msgIndexes[type] = msgIdx++;
        }
        GenMessageIndexer(context, compilation, msgIndexes);
        
        // 预先收集一些符号
        WellKnownSymbols wkSymbols = new WellKnownSymbols(compilation);

        // 生成消息派发
        IEnumerable<ClassDeclarationSyntax> distinctDispatchers = dispatchers.Distinct();
        foreach (var dispatcherSyntax in distinctDispatchers)
        {
            var dispatcherClass = ParseDispatcherClass(context, compilation, wkSymbols, dispatcherSyntax, msgIndexes);
            if (dispatcherClass != null)
                GenMessageDispatch(context, dispatcherClass);
        }
    }

    // 生成消息索引器
    private static void GenMessageIndexer(SourceProductionContext context, Compilation compilation, 
        Dictionary<string, int> msgIndexes)
    {
        // 生成消息索引器
        var className = "MessageIndexer";
        var nameSpace = compilation.Assembly.Name;
        
        var code =
            $$"""
              namespace {{nameSpace}};

              /// <summary>
              /// 服务消息索引器
              /// </summary>
              internal static class {{className}}
              {
                  public static int GetIndex<TArgs>()
                  {
                      return IndexWrapper<TArgs>.Index;
                  }
                  
                  // ReSharper disable once UnusedTypeParameter
                  struct IndexWrapper<TArgs>
                  {
                      public static int Index;
                  }
                  
                  static {{className}}()
                  {
              {{GenerateIndexes()}}
                  }
              }
              """;
        
        string GenerateIndexes()
        {
            var sbr = new StringBuilder();
            foreach (var item in msgIndexes)
            {
                sbr.AppendLine($"        IndexWrapper<{item.Key}>.Index = {item.Value};");
            }
            return sbr.ToString();
        }
        
        context.AddSource($"{className}.g.cs", code);
    }

    // 解析服务类
    private static DispatcherClass? ParseDispatcherClass(SourceProductionContext context, Compilation compilation, 
        WellKnownSymbols wkSymbols, ClassDeclarationSyntax dispatcherSyntax, 
        Dictionary<string, int> msgIndexes)
    {
        SemanticModel sm = compilation.GetSemanticModel(dispatcherSyntax.SyntaxTree);
        if (sm.GetDeclaredSymbol(dispatcherSyntax) is not INamedTypeSymbol dispatcherSymbol)
            return null;
     
        // 检查这个类是不是partial
        if (!SyntaxHelper.IsPartialClass(dispatcherSyntax))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.RequestPartialClass, 
                SymbolHelper.GetLocation(dispatcherSymbol), dispatcherSymbol.Name));
            return null;
        }
        
        var dispatcherClass = new DispatcherClass
        {
            NameSpace = dispatcherSymbol.ContainingNamespace.ToDisplayString(),
            ClassName = dispatcherSymbol.Name,
        };
            
        foreach (var symbol in dispatcherSymbol.GetMembers())
        {
            if (symbol is not IMethodSymbol methodSymbol)
                continue;
            
            // 必须是普通方法
            if (methodSymbol.IsStatic || methodSymbol.MethodKind != MethodKind.Ordinary)
                continue;
            
            if (SymbolHelper.HasAttribute(methodSymbol, MessageSendAttribute))
            {
                // 参数检查
                if (!methodSymbol.ReturnsVoid)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.VoidReturnError, 
                        SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                    return null;
                }

                var handler = CheckAndCreateHandler(methodSymbol);
                if (handler == null)
                    return null;
                
                dispatcherClass.SendHandlers.Add(handler);
            }
            else if (SymbolHelper.HasAttribute(methodSymbol, MessageInvokeAttribute))
            {
                var returnSymbol = methodSymbol.ReturnType;
                if (!SymbolHelper.IsIdentity(returnSymbol, wkSymbols.ValueTaskSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ValueTaskReturnError, 
                        SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                    return null;
                }
                
                var handler = CheckAndCreateHandler(methodSymbol);
                if (handler == null)
                    return null;
                
                dispatcherClass.InvokeHandlers.Add(handler);
            }
            else if (SymbolHelper.HasAttribute(methodSymbol, MessageCallAttribute))
            {
                var returnSymbol = methodSymbol.ReturnType;
                if (!SymbolHelper.IsIdentity(returnSymbol.OriginalDefinition, wkSymbols.ValueTaskOfTSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ValueTaskOfTReturnError, 
                        SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                    return null;
                }
                
                var handler = CheckAndCreateHandler(methodSymbol);
                if (handler == null)
                    return null;
                
                dispatcherClass.CallHandlers.Add(handler);
            }
        }

        HandlerMethod? CheckAndCreateHandler(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Parameters.Length != 2)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ParamNumberError, 
                    SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                return null;
            }

            var paramSymbol = methodSymbol.Parameters[0].Type;
            if (!SymbolHelper.IsIdentity(paramSymbol, wkSymbols.IntSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ParamDispatcherError, 
                    SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                return null;
            }
            
            paramSymbol = methodSymbol.Parameters[1].Type;
            if (!SymbolHelper.HasAttribute(paramSymbol, MessageTypeAttribute))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ParamAttrError, 
                    SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                return null;
            }

            var paramName = paramSymbol.ToDisplayString();
            if (!msgIndexes.TryGetValue(paramName, out var index))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ParamIndexError, 
                    SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                return null;
            }

            var handler = new HandlerMethod
            {
                MethodName = methodSymbol.Name,
                ParamType = paramName,
                Index = index,
            };

            return handler;
        }
        
        dispatcherClass.SendHandlers.Sort((info1, info2) => info1.Index - info2.Index);
        dispatcherClass.CallHandlers.Sort((info1, info2) => info1.Index - info2.Index);
        dispatcherClass.InvokeHandlers.Sort((info1, info2) => info1.Index - info2.Index);
        return dispatcherClass;
    }

    // 生成消息派发器
    private static void GenMessageDispatch(SourceProductionContext context, DispatcherClass dispatcherClass)
    {
        var code = $$"""
using System.Runtime.CompilerServices;
namespace {{dispatcherClass.NameSpace}};

public partial class {{dispatcherClass.ClassName}}
{
    public void DoDispatchSend<TMsg>(int srcService, ref TMsg msg)
    {
        var idx = MessageIndexer.GetIndex<TMsg>();
        switch (idx)
        {
{{GenSwitchCaseOfSend()}}
        }
    }
    
    public ValueTask DoDispatchInvoke<TMsg>(int srcService, ref TMsg msg)
    {
        var idx = MessageIndexer.GetIndex<TMsg>();
        switch (idx)
        {
{{GenSwitchCaseOfInvoke()}}
        }
    }
    
    public ValueTask<TResult> DoDispatchCall<TMsg, TResult>(int srcService, ref TMsg msg)
    {
        var idx = MessageIndexer.GetIndex<TMsg>();
        switch (idx)
        {
{{GenSwitchCaseOfCall()}}
        }
    }
}
""";

        string GenSwitchCaseOfSend()
        {
            var indent = "            ";
            var sbr = new StringBuilder();
            foreach (var info in dispatcherClass.SendHandlers)
            {
                sbr.AppendLine($"{indent}case {info.Index} : {info.MethodName}(srcService, Unsafe.As<TMsg, {info.ParamType}>(ref msg)); break;");
            }
            return sbr.ToString();
        }
        
        string GenSwitchCaseOfInvoke()
        {
            var indent = "            ";
            var sbr = new StringBuilder();
            foreach (var info in dispatcherClass.InvokeHandlers)
            {
                sbr.AppendLine($"{indent}case {info.Index} : return {info.MethodName}(srcService, Unsafe.As<TMsg, {info.ParamType}>(ref msg));");
            }

            sbr.Append($"{indent}default: throw new InvalidOperationException($\"没有消息处理器: {dispatcherClass.ClassName} - {{typeof(TMsg)}}\");");
            return sbr.ToString();
        }
        
        string GenSwitchCaseOfCall()
        {
            var indent = "            ";
            var sbr = new StringBuilder();
            foreach (var info in dispatcherClass.CallHandlers)
            {
                sbr.AppendLine($"{indent}case {info.Index} :");
                sbr.AppendLine($"{indent}{{");
                sbr.AppendLine($"{indent}    var ret = {info.MethodName}(srcService, Unsafe.As<TMsg, {info.ParamType}>(ref msg));");
                sbr.AppendLine($"{indent}    if (ret is ValueTask<TResult> result) return result;");
                sbr.AppendLine($"{indent}    throw new InvalidCastException($\"{dispatcherClass.ClassName}.{info.MethodName}: 无法将返回值类型从{{ret.GetType()}}转换为{{typeof(ValueTask<TResult>)}}\");");
                sbr.AppendLine($"{indent}}}");
            }

            sbr.Append($"{indent}default: throw new InvalidOperationException($\"没有消息处理器: {dispatcherClass.ClassName} - {{typeof(TMsg)}}\");");
            return sbr.ToString();
        }
        
        context.AddSource($"{dispatcherClass.ClassName}.Dispatch.g.cs", code);
    }
}

internal static class DiagDescriptors
{
    public static DiagnosticDescriptor RequestPartialClass { get; } = new (
        id: "SrvMsg1001",
        title: "必须是partial类",
        messageFormat: "{0}必须声明为partial类",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ParamNumberError { get; } = new (
        id: "SrvMsg1002",
        title: "消息处理器参数错误",
        messageFormat: "消息处理器的参数必须是2个，第1个为源服务Id，第2个为声明DispatcherMessage属性的类型: {0}.{1}",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ParamAttrError { get; } = new (
        id: "SrvMsg1003",
        title: "消息处理器参数的类型必须声明DispatcherMessage",
        messageFormat: "消息处理器的参数类型必须声明DispatcherMessage: {0}.{1}",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ParamDispatcherError { get; } = new (
        id: "SrvMsg1003",
        title: "消息处理器第1个参数必须是int，代表源服务Id",
        messageFormat: "消息处理器第1个参数必须是int，代表源服务Id: {0}.{1}",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor VoidReturnError { get; } = new (
        id: "SrvMsg1004",
        title: "Send消息处理器返回值错误",
        messageFormat: "Send消息处理器的返回值必须是void: {0}.{1}",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ParamIndexError { get; } = new (
        id: "SrvMsg1005",
        title: "消息处理器参数找不到索引",
        messageFormat: "消息处理器参数找不到索引: {0}.{1}",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ValueTaskReturnError { get; } = new (
        id: "SrvMsg1006",
        title: "Invoke消息处理器返回值错误",
        messageFormat: "Invoke消息处理器的返回值必须是ValueTask: {0}.{1}",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ValueTaskOfTReturnError { get; } = new (
        id: "SrvMsg1007",
        title: "Call消息处理器返回值错误",
        messageFormat: "Call消息处理器的返回值必须是ValueTask<T>: {0}.{1}",
        category: "DispatcherMessage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

internal class WellKnownSymbols
{
    public readonly INamedTypeSymbol IntSymbol;
    public readonly INamedTypeSymbol ValueTaskSymbol;
    public readonly INamedTypeSymbol ValueTaskOfTSymbol;

    public WellKnownSymbols(Compilation compilation)
    {
        IntSymbol = compilation.GetSpecialType(SpecialType.System_Int32);
        ValueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask") ?? 
                          throw new InvalidOperationException($"找不到System.Threading.Tasks.ValueTask符号");
        ValueTaskOfTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1") ?? 
                           throw new InvalidOperationException($"找不到System.Threading.Tasks.ValueTask`1符号");
    }
}

internal class DispatcherClass
{
    public string ClassName = string.Empty;
    public string NameSpace = string.Empty;
    public readonly List<HandlerMethod> SendHandlers = new();
    public readonly List<HandlerMethod> CallHandlers = new();
    public readonly List<HandlerMethod> InvokeHandlers = new();
}

internal class HandlerMethod
{
    public string MethodName = string.Empty;
    public string ParamType = string.Empty;
    public int Index;
}