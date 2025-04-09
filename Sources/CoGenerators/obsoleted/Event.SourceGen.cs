// Written by Colin on 2024-1-14

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoGenerators.Event;

/// <summary>
/// 生成服务消息索引和处理器
/// </summary>
[Generator]
public class SourceGen : IIncrementalGenerator
{
    private const string EventTypeAttribute = "CoLib.Event.EventTypeAttribute";
    private const string EventHandlerAttribute = "CoLib.Event.EventHandlerAttribute";
    private const string EventDispatcherAttribute = "CoLib.Event.EventDispatcherAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // // 得到事件类型列表
        // IncrementalValuesProvider<string> msgTypes = context.SyntaxProvider.CreateSyntaxProvider(
        //         predicate: static (node, _) => IsMessageSyntaxTarget(node),
        //         transform: static (ctx, _) => GetMessageSemanticTarget(ctx))
        //     .Where(static m => m is not null)
        //     .Select(static (m, _) => m!);
        //
        // // 得到事件处理器语法节点列表
        // IncrementalValuesProvider<ClassDeclarationSyntax> dispatcherSyntaxList = context.SyntaxProvider.CreateSyntaxProvider(
        //         predicate: static (node, _) => IsHandlerSyntaxTarget(node),
        //         transform: static (ctx, _) => GetHandlerSemanticTarget(ctx))
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
    
    // 声明有EventTypeAttribute属性的语法
    private static string? GetMessageSemanticTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeSyntax)
            return null;

        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeSyntax); 
        if (typeSymbol is not ITypeSymbol)
            return null;
        return SymbolHelper.HasAttribute(typeSymbol, EventTypeAttribute) ? typeSymbol.ToDisplayString() : null;
    }
    
    // 类且带有属性
    private static bool IsHandlerSyntaxTarget(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classSyntax)
            return false;

        return classSyntax.AttributeLists.Count > 0;
    }
    
    // 声明EventDispatcher属性的类
    private static ClassDeclarationSyntax? GetHandlerSemanticTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
            return null;

        return SymbolHelper.HasAttribute(classSymbol, EventDispatcherAttribute) ? classSyntax : null;
    }
    
    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<string> evTypes,
        ImmutableArray<ClassDeclarationSyntax> dispatchers)
    {
        // 生成消息类型索引映射
        Dictionary<string, int> evIndexes = new Dictionary<string, int>();
        var evIdx = 1;
        foreach (var type in evTypes)
        {
            if (!evIndexes.ContainsKey(type))
                evIndexes[type] = evIdx++;
        }
        GenEventIndexer(context, compilation, evIndexes);
        
        // 生成事件派发
        IEnumerable<ClassDeclarationSyntax> distinctDispatchers = dispatchers.Distinct();
        foreach (var dispatcherSyntax in distinctDispatchers)
        {
            var dispatcherClass = ParseDispatcherClass(context, compilation, dispatcherSyntax, evIndexes);
            if (dispatcherClass != null)
                GenEventDispatch(context, dispatcherClass);
        }
    }
    
    // 生成索引器
    private static void GenEventIndexer(SourceProductionContext context, Compilation compilation, 
        Dictionary<string, int> evIndexes)
    {
        // 生成索引器
        var className = "EventIndexer";
        var nameSpace = compilation.Assembly.Name;
        
        var code =
            $$"""
              namespace {{nameSpace}};

              /// <summary>
              /// 事件索引器
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
            foreach (var item in evIndexes)
            {
                sbr.AppendLine($"        IndexWrapper<{item.Key}>.Index = {item.Value};");
            }
            return sbr.ToString();
        }
        
        context.AddSource($"{className}.g.cs", code);
    }
    
    // 解析服务类
    private static DispatcherClass? ParseDispatcherClass(SourceProductionContext context, Compilation compilation, 
        ClassDeclarationSyntax dispatcherSyntax, Dictionary<string, int> evIndexes)
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
            
            if (!SymbolHelper.HasAttribute(methodSymbol, EventHandlerAttribute))
                continue;
            
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
            
            dispatcherClass.Handlers.Add(handler);
        }

        HandlerMethod? CheckAndCreateHandler(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Parameters.Length != 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ParamNumberError, 
                    SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                return null;
            }

            var paramSymbol = methodSymbol.Parameters[0].Type;
            if (!SymbolHelper.HasAttribute(paramSymbol, EventTypeAttribute))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ParamAttrError, 
                    SymbolHelper.GetLocation(methodSymbol), dispatcherSymbol.Name, methodSymbol.Name));
                return null;
            }

            var paramName = paramSymbol.ToDisplayString();
            if (!evIndexes.TryGetValue(paramName, out var index))
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
        
        dispatcherClass.Handlers.Sort((info1, info2) => info1.Index - info2.Index);
        return dispatcherClass;
    }

    // 生成消息派发器
    private static void GenEventDispatch(SourceProductionContext context, DispatcherClass dispatcherClass)
    {
        var code = $$"""
using System.Runtime.CompilerServices;
namespace {{dispatcherClass.NameSpace}};

public partial class {{dispatcherClass.ClassName}}
{
    void DoDispatchEvent<TEvent>(ref TEvent evt)
    {
        var idx = EventIndexer.GetIndex<TEvent>();
        switch (idx)
        {
{{GenSwitchCase()}}
        }
    }
}
""";

        string GenSwitchCase()
        {
            var indent = "            ";
            var sbr = new StringBuilder();
            foreach (var info in dispatcherClass.Handlers)
            {
                sbr.AppendLine($"{indent}case {info.Index} : {info.MethodName}(Unsafe.As<TEvent, {info.ParamType}>(ref evt)); break;");
            }

            return sbr.ToString();
        }
        
        context.AddSource($"{dispatcherClass.ClassName}.Dispatch.g.cs", code);
    }
}

internal static class DiagDescriptors
{
    public static DiagnosticDescriptor RequestPartialClass { get; } = new (
        id: "EvtDisp1001",
        title: "必须是partial类",
        messageFormat: "{0}必须声明为partial类",
        category: "EventDispatcher",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ParamNumberError { get; } = new (
        id: "EvtDisp1002",
        title: "事件处理器参数错误",
        messageFormat: "事件处理器的参数必须是1个，第1个为声明EventType属性的类型: {0}.{1}",
        category: "EventDispatcher",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ParamAttrError { get; } = new (
        id: "EvtDisp1003",
        title: "事件处理器参数的类型必须声明EventType属性",
        messageFormat: "事件处理器的参数类型必须声明EventType属性: {0}.{1}",
        category: "EventDispatcher",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor VoidReturnError { get; } = new (
        id: "EvtDisp1004",
        title: "事件处理器返回值错误",
        messageFormat: "事件处理器的返回值必须是void: {0}.{1}",
        category: "EventDispatcher",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static DiagnosticDescriptor ParamIndexError { get; } = new (
        id: "EvtDisp1005",
        title: "事件处理器参数找不到索引",
        messageFormat: "事件处理器参数找不到索引: {0}.{1}",
        category: "EventDispatcher",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

internal class DispatcherClass
{
    public string ClassName = string.Empty;
    public string NameSpace = string.Empty;
    public readonly List<HandlerMethod> Handlers = new();
}

internal class HandlerMethod
{
    public string MethodName = string.Empty;
    public string ParamType = string.Empty;
    public int Index;
}