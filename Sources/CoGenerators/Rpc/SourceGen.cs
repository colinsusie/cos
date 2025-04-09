// Written by Colin on 2024-9-26

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoGenerators.Rpc;

/// <summary>
/// 生成RPC相关的代码
/// </summary>
[Generator]
public class SourceGen : IIncrementalGenerator
{
    private const string NameOfIRpcService = "CoRuntime.Rpc.IRpcService";
    private const string NameOfIRpcDispatcher = "CoRuntime.Rpc.IRpcDispatcher";
    private const string NameOfService = "CoRuntime.Services.Service";

    private WellKnownSymbols _wellKnownSymbols = null!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 扫描直接继承自IRpcService的接口。
        IncrementalValuesProvider<INamedTypeSymbol> interfaceSyntaxList = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is InterfaceDeclarationSyntax,
                transform: (ctx, _) => GetRpcInterfaceTarget(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // 扫描继承自Service且实现IRpcService子类的服务类
        IncrementalValuesProvider<(INamedTypeSymbol, ClassDeclarationSyntax)> classSyntaxList = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => GetRpcServiceTarget(ctx))
            .Where(static m => (m.Item1 is not null) && (m.Item2 is not null))
            .Select(static (m, _) => (m.Item1!, m.Item2!));

        // 组合收集到的信息
        IncrementalValueProvider<(Compilation, ImmutableArray<INamedTypeSymbol>)> target1 =
            context.CompilationProvider.Combine(interfaceSyntaxList.Collect());
        var target2 = target1.Combine(classSyntaxList.Collect());

        // 生成源码
        context.RegisterSourceOutput(target2, (spc, source) =>
            GenerateSource(spc, source.Left.Item1, source.Left.Item2, source.Right));
    }

    // 直接继承自IRpcService
    private INamedTypeSymbol? GetRpcInterfaceTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not InterfaceDeclarationSyntax interfaceSyntax)
            return null;
        
        if (context.SemanticModel.GetDeclaredSymbol(interfaceSyntax) is not INamedTypeSymbol interfaceSymbol)
            return null;

        return SymbolHelper.IsImplementInterface(interfaceSymbol, NameOfIRpcService, false) ? interfaceSymbol : null;
    }
    
    private (INamedTypeSymbol?, ClassDeclarationSyntax?) GetRpcServiceTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax)
            return (null, null);

        if (context.SemanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
            return (null, null);

        // 直接继承自Service
        if (!SymbolHelper.IsInheritedFrom(classSymbol, NameOfService, false))
            return (null, null);
        
        // 已经实现了IRpcDispatcher
        if (SymbolHelper.IsImplementInterface(classSymbol, NameOfIRpcDispatcher))
            return (null, null);
        
        // 实现IRpcService的子类
        foreach (var aInterface in classSymbol.Interfaces)
        {
            if (SymbolHelper.IsImplementInterface(aInterface, NameOfIRpcService, false))
                return (classSymbol, classSyntax);    
        }

        return (null, null);
    }

    // 生成代码
    private void GenerateSource(SourceProductionContext context, Compilation compilation,
        ImmutableArray<INamedTypeSymbol> rpcInterfaces, 
        ImmutableArray<(INamedTypeSymbol, ClassDeclarationSyntax)> rpcServices)
    {
        try
        {
            _wellKnownSymbols = new(compilation);
            GenRpcServiceFactories(context, compilation, rpcInterfaces);
            GenRpcServiceProxies(context, compilation, rpcInterfaces);
            GenRpcDispatchers(context, rpcServices);
        }
        catch (GenInterruptException e)
        {
            Debug.WriteLine($"{e}");
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.CommonError, null, e.ToString()));
        }
    }

    private void GenRpcServiceFactories(SourceProductionContext context, Compilation compilation,
        ImmutableArray<INamedTypeSymbol> rpcInterfaces)
    {
        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var aInterface in rpcInterfaces)
        {
            if (!set.Add(aInterface)) continue;
            GenRpcServiceFactory(context, compilation, aInterface);
        }
    }

    private void GenRpcServiceFactory(SourceProductionContext context, Compilation compilation, 
        INamedTypeSymbol rpcInterface)
    {
        var interfaceNameSpace = rpcInterface.ContainingNamespace.ToDisplayString();
        var nameSpace = compilation.Assembly.Name;
        var interfaceName = rpcInterface.Name;
        var serviceClassName = interfaceName.StartsWith("I") ? interfaceName.Substring(1) : interfaceName;
        var factoryClassName = $"{serviceClassName}Factory";
        var proxyClassName = $"{serviceClassName}Proxy";
        
        var code = $$"""
using CoRuntime;
using CoRuntime.Services;
using CoRuntime.Rpc;
using {{interfaceNameSpace}};

namespace {{nameSpace}};


public static class {{factoryClassName}}
{
    public static {{proxyClassName}} Create(ServiceAddr serviceAddr)
    {
        var rpcMgr = RuntimeEnv.RpcMgr;
        if (!rpcMgr.TryGetClient(serviceAddr, out var client))
        {
            throw new InvalidOperationException($"RpcClient is null, ServiceAddr:{serviceAddr}");
        }

        return new {{serviceClassName}}Proxy(client, serviceAddr.ServiceId);
    }
}
""";
        
        context.AddSource($"{factoryClassName}.g.cs", code);
    }

    private void GenRpcServiceProxies(SourceProductionContext context, Compilation compilation,
        ImmutableArray<INamedTypeSymbol> interfaceSymbols)
    {
        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var interfaceSymbol in interfaceSymbols)
        {
            // 排除掉重复的符号
            if (!set.Add(interfaceSymbol)) continue;
            GenRpcServiceProxy(context, compilation, interfaceSymbol);
        }
    }

    private void GenRpcServiceProxy(SourceProductionContext context, Compilation compilation,
        INamedTypeSymbol interfaceSymbol)
    {
        var interfaceNameSpace = interfaceSymbol.ContainingNamespace.ToDisplayString();
        var nameSpace = compilation.Assembly.Name;
        var interfaceName = interfaceSymbol.Name;
        var serviceClassName = interfaceName.StartsWith("I") ? interfaceName.Substring(1) : interfaceName; 
        var proxyClassName = $"{serviceClassName}Proxy";
        
        var code = $$"""
using {{interfaceNameSpace}};
using CoRuntime.Rpc;

namespace {{nameSpace}};

#nullable enable

public struct {{proxyClassName}}
{
    private readonly IRpcClient _rpcClient;
    private readonly short _serviceId;

    public {{proxyClassName}}(IRpcClient rpcClient, short serviceId)
    {
        _rpcClient = rpcClient;
        _serviceId = serviceId;
    }
 
{{GenRpcServiceProxyMethods(context, interfaceSymbol)}}
}
""";
        
        context.AddSource($"{proxyClassName}.g.cs", code);
    }

    private string GenRpcServiceProxyMethods(SourceProductionContext context,
        INamedTypeSymbol interfaceSymbol)
    {
        var sbr = new StringBuilder();

        short methodId = 0;
        foreach (var memberSymbol in interfaceSymbol.GetMembers())
        {
            if (memberSymbol is not IMethodSymbol methodSymbol)
                continue;
            GenRpcServiceProxyMethod(context, methodSymbol, methodId++, sbr);
        }

        var str = sbr.ToString();
        return str;
    }

    private void GenRpcServiceProxyMethod(SourceProductionContext context, IMethodSymbol methodSymbol, 
        short methodId, StringBuilder sbr)
    {
        var returnInfo = GenReturn(context, methodSymbol);
        var methodName = methodSymbol.Name;
        var methodArgNum = methodSymbol.Parameters.Length;
        
        // 参数个数
        const int maxMethodNum = 8;
        if (methodArgNum > maxMethodNum)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ArgsNumError, 
                SymbolHelper.GetLocation(methodSymbol), maxMethodNum, methodArgNum));
            throw new GenInterruptException();
        }

        if (returnInfo.IsVoid)      // Notify
        {
            sbr.AppendLine($$"""
    public {{returnInfo.TypeName}} {{methodName}}({{GetParamsDesc(methodSymbol)}})
    {
        _rpcClient.DispatchNotify(RpcMessageSerializer.SerializeNotify{{methodArgNum}}(_serviceId, {{methodId}}{{GetParams(methodSymbol, true)}}));
    }

""");
        }
        else if (returnInfo.HasReturn)  // request with return
        {
            sbr.AppendLine($$"""
    public ValueTask<{{returnInfo.TypeName}}> {{methodName}}({{GetParamsDesc(methodSymbol, "CancellationToken token")}})
    {
        var requestId = _rpcClient.GenerateRequestId();
        return _rpcClient.DispatchRequest<{{returnInfo.TypeName}}>(RpcMessageSerializer.SerializeRequest{{methodArgNum}}(_serviceId, {{methodId}}, requestId{{GetParams(methodSymbol, true)}}), token);
    }

""");
        }
        else // request no return
        {
            sbr.AppendLine($$"""
    public ValueTask {{methodName}}({{GetParamsDesc(methodSymbol, "CancellationToken token")}})
    {
        var requestId = _rpcClient.GenerateRequestId();
        return _rpcClient.DispatchRequest(RpcMessageSerializer.SerializeRequest{{methodArgNum}}(_serviceId, {{methodId}}, requestId{{GetParams(methodSymbol, true)}}), token);
    }

""");
        }
    }

    // 生成RPC派发类
    private void GenRpcDispatchers(SourceProductionContext context,
        ImmutableArray<(INamedTypeSymbol, ClassDeclarationSyntax)> rpcServices)
    {
        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var serviceSymbol in rpcServices)
        {
            if (!set.Add(serviceSymbol.Item1)) continue;
            GenRpcDispatcher(context, serviceSymbol);
        }
    }

    private void GenRpcDispatcher(SourceProductionContext context,
        (INamedTypeSymbol, ClassDeclarationSyntax) service)
    {
        var (serviceSymbol, serviceSyntax) = service;
        if (!SyntaxHelper.IsPartialClass(serviceSyntax))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ServiceError1, 
                SymbolHelper.GetLocation(serviceSymbol), serviceSymbol.Name));
            throw new GenInterruptException();
        }

        INamedTypeSymbol? interfaceSymbol = null;
        foreach (var aInterface in serviceSymbol.Interfaces)
        {
            if (SymbolHelper.IsImplementInterface(aInterface, NameOfIRpcService, false))
            {
                interfaceSymbol = aInterface;
                break;
            }
        }

        if (interfaceSymbol == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ServiceError2, 
                SymbolHelper.GetLocation(serviceSymbol), serviceSymbol.Name));
            throw new GenInterruptException();
        }

        var nameSpace = serviceSymbol.ContainingNamespace.ToDisplayString();
        var serviceName = serviceSymbol.Name;
        
        var code = $$"""
using CoRuntime.Rpc;

namespace {{nameSpace}};

public partial class {{serviceName}}: IRpcDispatcher
{
    public void DispatchNotify(RpcNotifyMessage msg)
    {
{{GenDispatchNotify(interfaceSymbol)}}
    }
    
    public async ValueTask<RpcResponseMessage> DispatchRequest(RpcRequestMessage msg)
    {
{{GenDispatchRequest(context, interfaceSymbol)}}
    }
}
""";
        
        context.AddSource($"{serviceName}.RpcDispatcher.g.cs", code);
    }
    
    private string GenDispatchNotify(INamedTypeSymbol interfaceSymbol)
    {
        var switchCode = $$"""
        switch (msg.MethodId)
        {
{{GenCases()}}
            default:
            {
                Logger.Error($"Unable to find service method associated with MethodId: {msg.MethodId}, msg:{msg}");
                break;
            }
        }
""";
        return switchCode;

        string GenCases()
        {
            var sbr = new StringBuilder();

            short methodId = 0;
            foreach (var symbol in interfaceSymbol.GetMembers())
            {
                if (symbol is not IMethodSymbol methodSymbol)
                    continue;
                var currMethodId = methodId++;
                if (!methodSymbol.ReturnsVoid)
                    continue;
                GenCase(sbr, currMethodId, methodSymbol);
            }

            var str = sbr.ToString();
            return str;
        }

        void GenCase(StringBuilder sbr, int methodId, IMethodSymbol methodSymbol)
        {
            sbr.AppendLine($$"""
            case {{methodId}}:
            {
{{GenArgsSerialize(methodSymbol)}}
{{GenNotify(methodSymbol)}}
                break;
            }
""");
        }

        string GenNotify(IMethodSymbol methodSymbol)
        {
            return $"                {methodSymbol.Name}({GenArgs(methodSymbol)});";
        }
    }

    private string GenDispatchRequest(SourceProductionContext context, INamedTypeSymbol interfaceSymbol)
    {
        var switchCode = $$"""
        switch (msg.MethodId)
        {
{{GenCases()}}
            default:
            {
                throw new RpcException($"Unable to find service method associated with MethodId: {msg.MethodId}, msg:{msg}");
            }
        }
""";
        return switchCode;

        string GenCases()
        {
            var sbr = new StringBuilder();

            short methodId = 0;
            foreach (var symbol in interfaceSymbol.GetMembers())
            {
                if (symbol is not IMethodSymbol methodSymbol)
                    continue;
                var currMethodId = methodId++;
                if (methodSymbol.ReturnsVoid)
                    continue;
                GenCase(sbr, currMethodId, methodSymbol);
            }

            var str = sbr.ToString();
            return str;
        }

        void GenCase(StringBuilder sbr, int methodId, IMethodSymbol methodSymbol)
        {
            var returnInfo = GenReturn(context, methodSymbol);
            sbr.AppendLine($$"""
            case {{methodId}}:
            {
{{GenArgsSerialize(methodSymbol)}}
{{GenRequest(methodSymbol, returnInfo)}}
{{GenResponse(returnInfo)}}
            }
""");
        }

        string GenRequest(IMethodSymbol methodSymbol, ReturnInfo returnInfo)
        {
            
            return !returnInfo.HasReturn ? 
                $"                await {methodSymbol.Name}({GenArgs(methodSymbol)});" : 
                $"                var result = await {methodSymbol.Name}({GenArgs(methodSymbol)});"; 
        }

        string GenResponse(ReturnInfo returnInfo)
        {
            return !returnInfo.HasReturn ? 
                $"                return RpcResponseMessage.Create(msg.RequestId, null);" : 
                $"                return RpcMessageSerializer.SerializeResponse(msg.RequestId, result);";
        }
    }
    
    string GenArgsSerialize(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Parameters.Length == 0)
            return "";
            
        var sbr = new StringBuilder();
        var num = methodSymbol.Parameters.Length; 
        if (num == 1)
        {
            sbr.AppendLine($"                var a1 = RpcMessageSerializer.Deserialize{num}<{GetParamsType(methodSymbol)}>(msg);");
        }
        else
        {
            sbr.AppendLine($"                var ({GenArgs(methodSymbol)}) = RpcMessageSerializer.Deserialize{num}<{GetParamsType(methodSymbol)}>(msg);");
        }
            
        var idx = 1;
        foreach (var paramSymbol in methodSymbol.Parameters)
        {
            if (paramSymbol is {NullableAnnotation: NullableAnnotation.NotAnnotated, Type.IsValueType: false})
                sbr.AppendLine($"                if (a{idx} == null) throw new RpcException($\"{idx}th parameter of {methodSymbol.Name} is {paramSymbol.Type.Name} and cannot be null, msg:{{msg}}\");");
            idx++;
        }
        return sbr.ToString();
    }
    
    // 取形参声明：(Type name1, Type name2)
    string GetParamsDesc(IMethodSymbol methodSymbol, string? postParam = null)
    {
        List<string> args = new();
        foreach (var paramSymbol in methodSymbol.Parameters)
        {
            args.Add($"{paramSymbol.Type.ToDisplayString()} {paramSymbol.Name}");
        }
        if (postParam != null)
            args.Add(postParam);

        return string.Join(", ", args);
    }
    
    // 取形参传递：(name1, name2)
    // preComma 是否根据参数个数加一个前置逗号
    string GetParams(IMethodSymbol methodSymbol, bool preComma)
    {
        List<string> args = new();
        foreach (var paramSymbol in methodSymbol.Parameters)
        {
            args.Add($"{paramSymbol.Name}");
        }

        var pre = "";
        if (preComma && args.Count > 0)
            pre = ", ";
        return $"{pre}{string.Join(", ", args)}";
    }

    // 取形参类型 <Type1, Type2>
    string GetParamsType(IMethodSymbol methodSymbol)
    {
        List<string> args = new();
        foreach (var paramSymbol in methodSymbol.Parameters)
        {
            args.Add($"{paramSymbol.Type.ToDisplayString()}");
        }

        return string.Join(", ", args);
    }
    
    string GenArgs(IMethodSymbol methodSymbol)
    {
        List<string> args = new();
        var idx = 1;
        foreach (var _ in methodSymbol.Parameters)
        {
            args.Add($"a{idx}");
            idx++;
        }

        return string.Join(", ", args);
    }
    
    ReturnInfo GenReturn(SourceProductionContext context, IMethodSymbol methodSymbol)
    {
        if (methodSymbol.ReturnsVoid)
        {
            return new ReturnInfo()
            {
                IsVoid = true,
                HasReturn = false,
                Type = null,
                TypeName = "void",
            };
        }

        var returnSymbol = methodSymbol.ReturnType;
        if (SymbolHelper.IsIdentity(returnSymbol, _wellKnownSymbols.ValueTaskSymbol))
        {
            return new ReturnInfo()
            {
                IsVoid = false,
                HasReturn = false,
                TypeName = "",
                Type = null,
            };
        }

        if (!SymbolHelper.IsIdentity(returnSymbol.OriginalDefinition, _wellKnownSymbols.ValueTaskOfTSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ReturnTypeError1, 
                SymbolHelper.GetLocation(methodSymbol), returnSymbol.Name));
            throw new GenInterruptException();    
        }
                
        var nameReturnSymbol = (returnSymbol as INamedTypeSymbol)!;
        ITypeSymbol returnType = nameReturnSymbol.TypeArguments[0];

        return new ReturnInfo()
        {
            IsVoid = false,
            HasReturn = true,
            Type = returnType,
            TypeName = returnType.ToDisplayString()
        };
    }
}