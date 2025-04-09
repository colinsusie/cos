// Written by Colin on 2024-10-20

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoGenerators.Serialize;

/// <summary>
/// 生成序列化的代码
/// </summary>
[Generator]
public class SourceGen: IIncrementalGenerator
{
    private const string NameOfCoPackable = "CoLib.Serialize.CoPackableAttribute";
    private const string NameOfTag = "CoLib.Serialize.TagAttribute";
    private const string NameOfUnion = "CoLib.Serialize.CoPackUnionAttribute";
    private const string NameOfConstructor = "CoLib.Serialize.CoPackConstructorAttribute";
    private const string NameOfBeforeSerialize = "CoLib.Serialize.CoPackBeforeSerializeAttribute";
    private const string NameOfAfterSerialize = "CoLib.Serialize.CoPackAfterSerializeAttribute";
    private const string NameOfBeforeDeserialize = "CoLib.Serialize.CoPackBeforeDeserializeAttribute";
    private const string NameOfAfterDeserialize = "CoLib.Serialize.CoPackAfterDeserializeAttribute";
    private const string NameofPackFlags = "CoLib.Serialize.PackFlags";
    
    private WellKnownSymbols _wellKnownSymbols = null!;
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 扫出有CoPackableAttribute的类，结构
        IncrementalValuesProvider<(INamedTypeSymbol, TypeDeclarationSyntax)> targetList = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => CoPackableDeclarationSyntax(node),
                transform: static (ctx, _) => GetCoPackableSymbol(ctx))
            .Where(static m => (m is not null))
            .Select(static (m, _) => m!.Value);
        
        // 组合收集到的信息
        IncrementalValueProvider<(Compilation, ImmutableArray<(INamedTypeSymbol, TypeDeclarationSyntax)>)> targets =
            context.CompilationProvider.Combine(targetList.Collect());
        
        // 生成源码
        context.RegisterSourceOutput(targets, (spc, source) =>
            GenerateSource(spc, source.Item1, source.Item2));
    }

    // 类，结构声明
    private static bool CoPackableDeclarationSyntax(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax {AttributeLists.Count: > 0}
            or StructDeclarationSyntax {AttributeLists.Count: > 0}
            or RecordDeclarationSyntax {AttributeLists.Count: > 0};
    }

    // 收集有CoPackable的符号
    private static (INamedTypeSymbol, TypeDeclarationSyntax)? GetCoPackableSymbol(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeSyntax)
            return null;
        
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol typeSymbol)
            return null;

        return !SymbolHelper.HasAttribute(typeSymbol, NameOfCoPackable) ? null : 
            (typeSymbol, typeSyntax);
    }
    
    // 生成代码
    private void GenerateSource(SourceProductionContext context, Compilation compilation,
        ImmutableArray<(INamedTypeSymbol, TypeDeclarationSyntax)> targets)
    {
        try
        {
            _wellKnownSymbols = new(compilation);
            GenTypeFormatters(context, compilation, targets);
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

    // 生成Formatter注册代码
    private void GenTypeFormatters(SourceProductionContext context, Compilation compilation,
        ImmutableArray<(INamedTypeSymbol, TypeDeclarationSyntax)> targets)
    {
        var symbolSet = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var (typeSymbol, typeSyntax) in targets)
        {
            if (!symbolSet.Add(typeSymbol))
                continue;
            GenRegisterFormatter(context, compilation, typeSymbol, typeSyntax);
        }
    }

    private void GenRegisterFormatter(SourceProductionContext context, Compilation compilation,
        INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeSyntax)
    {
        if (!SyntaxHelper.IsPartialType(typeSyntax))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.PartialError, 
                SymbolHelper.GetLocation(typeSymbol), typeSymbol.Name));
            throw new GenInterruptException();
        }
        
        var nameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
        var typeName = typeSymbol.Name;
        var typeKeyword = GetTypeKeyword(typeSymbol);
        var typeInfo = ParseTypeInfo(context, compilation, typeSymbol);
        
        var code = $$"""
#nullable enable
#pragma warning disable CS0108 // hides inherited member
#pragma warning disable CS0162 // Unreachable code
#pragma warning disable CS0164 // This label has not been referenced
#pragma warning disable CS0219 // Variable assigned but never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment
#pragma warning disable CS8602
#pragma warning disable CS8604 // Possible null reference argument for parameter
#pragma warning disable CS8619
#pragma warning disable CS8620
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method
#pragma warning disable CS8765 // Nullability of type of parameter
#pragma warning disable CS9074 // The 'scoped' modifier of parameter doesn't match overridden or implemented member
#pragma warning disable CA1050 // Declare types in namespaces.

using System.Buffers;
using CoLib.Serialize;
namespace {{nameSpace}};

public partial {{typeKeyword}} {{typeName}}
{
    static partial void StaticConstructor();
    
    static {{typeName}}()
    {
{{GenRegisterFormatters(typeInfo)}}
        StaticConstructor();
    }
    
{{GenFormatter(typeInfo)}}
}
""";
        
        context.AddSource($"{nameSpace}.{typeName}.CoPackFormatter.g.cs", code);
    }
    
    private string GetTypeKeyword(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.TypeKind switch
        {
            TypeKind.Class => typeSymbol.IsRecord ? "record class" : "class",
            _ => typeSymbol.IsRecord ? "record struct": "struct",
        };
    }

    private CoPackableTypeInfo ParseTypeInfo(SourceProductionContext context, Compilation compilation,
        INamedTypeSymbol typeSymbol)
    {
        var typeInfo = new CoPackableTypeInfo(typeSymbol);
        ParseFieldInfos(context, compilation, typeInfo, typeSymbol);
        ParseUnions(context, compilation, typeInfo, typeSymbol);
        ParseMethods(context, compilation, typeInfo, typeSymbol);
        return typeInfo;
    }

    private void ParseFieldInfos(SourceProductionContext context, Compilation compilation,
        CoPackableTypeInfo typeInfo, INamedTypeSymbol typeSymbol)
    {
        var fields = SymbolHelper.GetAllMembers(typeSymbol)
            .Where(f => SymbolHelper.HasAttribute(f, NameOfTag));

        var intTagSet = new HashSet<int>();
        var strTagSet = new HashSet<string>();
        foreach (var field in fields)
        {
            switch (field)
            {
                case IFieldSymbol fieldSymbol:
                {
                    if (fieldSymbol.IsReadOnly)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.FieldRadOnlyError,
                            SymbolHelper.GetLocation(field), field.Name));
                        throw new GenInterruptException();
                    }

                    break;
                }
                case IPropertySymbol propertySymbol:
                {
                    if (propertySymbol.SetMethod == null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.FieldRadOnlyError, 
                            SymbolHelper.GetLocation(propertySymbol), propertySymbol.Name));
                        throw new GenInterruptException();
                    }

                    break;
                }
                default:
                    continue;
            }
            
            if (!SymbolHelper.TryGetAttribute(field, NameOfTag, out var attrData) ||
                attrData == null)
                continue;

            var fieldInfo = GetFieldInfo(context, compilation, field, attrData, intTagSet, strTagSet);
            typeInfo.TagFields.Add(fieldInfo);
        }
    }

    private FieldInfo GetFieldInfo(SourceProductionContext context, Compilation compilation,
        ISymbol symbol, AttributeData attrData,
        HashSet<int> intTagSet, HashSet<string> strTagSet)
    {
        if (attrData.ConstructorArguments.Length != 3)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.FieldTagLenError, 
                SymbolHelper.GetLocation(symbol), symbol.Name));
            throw new GenInterruptException();       
        }

        int intTag = 0;
        string strTag = string.Empty;
        bool isProperty = symbol is IPropertySymbol;
        bool packIfDefault;
        int flags;
        
        var typedConstant = attrData.ConstructorArguments[0];
        switch (typedConstant.Value)
        {
            case int intValue:
                intTag = intValue;
                if (!intTagSet.Add(intTag))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.FieldIntTagDupError, 
                        SymbolHelper.GetLocation(symbol), symbol.Name, intTag));
                    throw new GenInterruptException();      
                }
                break;
            case string strValue:
                strTag = strValue;
                if (string.IsNullOrEmpty(strTag))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.FieldStrTagEmptyError, 
                        SymbolHelper.GetLocation(symbol), symbol.Name));
                    throw new GenInterruptException();      
                }
                if (!strTagSet.Add(strTag))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.FieldStrTagDupError, 
                        SymbolHelper.GetLocation(symbol), symbol.Name, strTag));
                    throw new GenInterruptException();      
                }
                break;
            default:
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.FieldTagTypeError, 
                    SymbolHelper.GetLocation(symbol), symbol.Name));
                throw new GenInterruptException();       
        }

        typedConstant = attrData.ConstructorArguments[1];
        packIfDefault = Convert.ToBoolean(typedConstant.Value);
        typedConstant = attrData.ConstructorArguments[2];
        flags = Convert.ToInt32(typedConstant.Value);

        return new FieldInfo(symbol, isProperty, intTag, strTag, packIfDefault, flags);
    }

    private void ParseUnions(SourceProductionContext context, Compilation compilation,
        CoPackableTypeInfo typeInfo, INamedTypeSymbol typeSymbol)
    {
        foreach (var attrData in typeSymbol.GetAttributes())
        {
            if (attrData.AttributeClass?.ToDisplayString() != NameOfUnion)
                continue;

            int tag = Convert.ToInt32(attrData.ConstructorArguments[0].Value);
            INamedTypeSymbol? type = attrData.ConstructorArguments[1].Value as INamedTypeSymbol;
            if (type == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.UnionTypeError, 
                    SymbolHelper.GetLocation(typeSymbol), typeSymbol.Name, tag));
                throw new GenInterruptException();       
            }
            
            typeInfo.UnionInfos.Add(new UnionInfo(type, tag));
        }
    }

    private void ParseMethods(SourceProductionContext context, Compilation compilation,
        CoPackableTypeInfo typeInfo, INamedTypeSymbol typeSymbol)
    {
        foreach (var methodSymbol in typeSymbol.Constructors)
        {
            if (SymbolHelper.HasAttribute(methodSymbol, NameOfConstructor))
            {
                if (methodSymbol.Parameters.Length != 1 ||
                    methodSymbol.Parameters[0].Type.SpecialType != SpecialType.System_Object)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.ConstructorError, 
                        SymbolHelper.GetLocation(methodSymbol)));
                    throw new GenInterruptException();    
                }

                typeInfo.Constructor = methodSymbol;
            }
        }

        foreach (var mem in typeSymbol.GetMembers())
        {
            if (mem is not IMethodSymbol methodSymbol)
                continue;

            if (SymbolHelper.HasAttribute(methodSymbol, NameOfBeforeSerialize))
            {
                if (methodSymbol.Parameters.Length != 1 ||
                    methodSymbol.Parameters[0].Type.ToDisplayString() != NameofPackFlags)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.BeforeSerializeError, 
                        SymbolHelper.GetLocation(methodSymbol)));
                    throw new GenInterruptException();  
                }

                typeInfo.BeforeSerialize = methodSymbol;
            }
            else if (SymbolHelper.HasAttribute(methodSymbol, NameOfAfterSerialize))
            {
                if (methodSymbol.Parameters.Length != 1 ||
                    methodSymbol.Parameters[0].Type.ToDisplayString() != NameofPackFlags)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.AfterSerializeError, 
                        SymbolHelper.GetLocation(methodSymbol)));
                    throw new GenInterruptException();  
                }

                typeInfo.AfterSerialize = methodSymbol;
            }
            else if (SymbolHelper.HasAttribute(methodSymbol, NameOfBeforeDeserialize))
            {
                if (methodSymbol.Parameters.Length != 1 ||
                    methodSymbol.Parameters[0].Type.SpecialType != SpecialType.System_Object)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.BeforeDeserializeError, 
                        SymbolHelper.GetLocation(methodSymbol)));
                    throw new GenInterruptException();  
                }

                typeInfo.BeforeDeserialize = methodSymbol;
            }
            else if (SymbolHelper.HasAttribute(methodSymbol, NameOfAfterDeserialize))
            {
                if (methodSymbol.Parameters.Length != 1 ||
                    methodSymbol.Parameters[0].Type.SpecialType != SpecialType.System_Object)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagDescriptors.AfterDeserializeError, 
                        SymbolHelper.GetLocation(methodSymbol)));
                    throw new GenInterruptException();  
                }

                typeInfo.AfterDeserialize = methodSymbol;
            }
        }
    }

    private string GenRegisterFormatters(CoPackableTypeInfo typeInfo)
    {
        var sbr = new StringBuilder();
        
        if (typeInfo.TypeSymbol.TypeKind == TypeKind.Class)
        {
            sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<{{typeInfo.TypeSymbol.Name}}>())
        {
            CoPackFormatterProvider.Register(new {{typeInfo.TypeSymbol.Name}}Formatter());
        }
""");
        }
        else
        {
            // nullable
            sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<{{typeInfo.TypeSymbol.Name}}>())
        {
            CoPackFormatterProvider.Register(new {{typeInfo.TypeSymbol.Name}}Formatter());
            CoPackFormatterProvider.Register(new NullableFormatter<{{typeInfo.TypeSymbol.Name}}>());
        }
""");
        }
        
        foreach (var fieldInfo in typeInfo.TagFields)
        {
            if (fieldInfo.IsProperty)
            {
                var propSymbol = (fieldInfo.Symbol as IPropertySymbol)!;
                GenRegister(propSymbol.Type);
            }
            else
            {
                var fieldSymbol = (fieldInfo.Symbol as IFieldSymbol)!;
                GenRegister(fieldSymbol.Type);   
            }
        }
        return sbr.ToString();
        
        void GenRegister(ITypeSymbol fieldSymbol)
        {
            if (fieldSymbol is not INamedTypeSymbol namedTypeSymbol)
                return;
            
            // enum
            if (namedTypeSymbol.TypeKind == TypeKind.Enum)
            {
                var typeName = fieldSymbol.ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<{{typeName}}>())
        {
            CoPackFormatterProvider.Register(new EnumFormatter<{{typeName}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<{{typeName}}>());
        }
""");
                return;
            }
            
            // ValueTuple
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple1))
            {
                var typeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<ValueTuple<{{typeName}}>>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<ValueTuple<{{typeName}}>>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                return;
            }
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple2))
            {
                var typeName1 = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var typeName2 = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<({{typeName1}}, {{typeName2}})>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName1}}, {{typeName2}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<({{typeName1}}, {{typeName2}})>());
        }
""");
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                return;
            }
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple3))
            {
                var typeName1 = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var typeName2 = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                var typeName3 = namedTypeSymbol.TypeArguments[2].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<({{typeName1}}, {{typeName2}}, {{typeName3}})>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName1}}, {{typeName2}}, {{typeName3}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<({{typeName1}}, {{typeName2}}, {{typeName3}})>());
        }
""");
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                GenRegister(namedTypeSymbol.TypeArguments[2]);
                return;
            }
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple4))
            {
                var typeName1 = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var typeName2 = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                var typeName3 = namedTypeSymbol.TypeArguments[2].ToDisplayString();
                var typeName4 = namedTypeSymbol.TypeArguments[3].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}})>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}})>());
        }
""");
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                GenRegister(namedTypeSymbol.TypeArguments[2]);
                GenRegister(namedTypeSymbol.TypeArguments[3]);
                return;
            }
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple5))
            {
                var typeName1 = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var typeName2 = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                var typeName3 = namedTypeSymbol.TypeArguments[2].ToDisplayString();
                var typeName4 = namedTypeSymbol.TypeArguments[3].ToDisplayString();
                var typeName5 = namedTypeSymbol.TypeArguments[4].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}})>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}})>());
        }
""");
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                GenRegister(namedTypeSymbol.TypeArguments[2]);
                GenRegister(namedTypeSymbol.TypeArguments[3]);
                GenRegister(namedTypeSymbol.TypeArguments[4]);
                return;
            }
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple6))
            {
                var typeName1 = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var typeName2 = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                var typeName3 = namedTypeSymbol.TypeArguments[2].ToDisplayString();
                var typeName4 = namedTypeSymbol.TypeArguments[3].ToDisplayString();
                var typeName5 = namedTypeSymbol.TypeArguments[4].ToDisplayString();
                var typeName6 = namedTypeSymbol.TypeArguments[5].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}})>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}})>());
        }
""");
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                GenRegister(namedTypeSymbol.TypeArguments[2]);
                GenRegister(namedTypeSymbol.TypeArguments[3]);
                GenRegister(namedTypeSymbol.TypeArguments[4]);
                GenRegister(namedTypeSymbol.TypeArguments[5]);
                return;
            }
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple7))
            {
                var typeName1 = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var typeName2 = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                var typeName3 = namedTypeSymbol.TypeArguments[2].ToDisplayString();
                var typeName4 = namedTypeSymbol.TypeArguments[3].ToDisplayString();
                var typeName5 = namedTypeSymbol.TypeArguments[4].ToDisplayString();
                var typeName6 = namedTypeSymbol.TypeArguments[5].ToDisplayString();
                var typeName7 = namedTypeSymbol.TypeArguments[6].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}}, {{typeName7}})>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}}, {{typeName7}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}}, {{typeName7}})>());
        }
""");
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                GenRegister(namedTypeSymbol.TypeArguments[2]);
                GenRegister(namedTypeSymbol.TypeArguments[3]);
                GenRegister(namedTypeSymbol.TypeArguments[4]);
                GenRegister(namedTypeSymbol.TypeArguments[5]);
                GenRegister(namedTypeSymbol.TypeArguments[6]);
                return;
            }
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.ValueTuple8))
            {
                var typeName1 = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var typeName2 = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                var typeName3 = namedTypeSymbol.TypeArguments[2].ToDisplayString();
                var typeName4 = namedTypeSymbol.TypeArguments[3].ToDisplayString();
                var typeName5 = namedTypeSymbol.TypeArguments[4].ToDisplayString();
                var typeName6 = namedTypeSymbol.TypeArguments[5].ToDisplayString();
                var typeName7 = namedTypeSymbol.TypeArguments[6].ToDisplayString();
                var typeName8 = namedTypeSymbol.TypeArguments[7].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}}, {{typeName7}}, {{typeName8}})>())
        {
            CoPackFormatterProvider.Register(new ValueTupleFormatter<{{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}}, {{typeName7}}, {{typeName8}}>());
            CoPackFormatterProvider.Register(new NullableFormatter<({{typeName1}}, {{typeName2}}, {{typeName3}}, {{typeName4}}, {{typeName5}}, {{typeName6}}, {{typeName7}}, {{typeName8}})>());
        }
""");
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                GenRegister(namedTypeSymbol.TypeArguments[2]);
                GenRegister(namedTypeSymbol.TypeArguments[3]);
                GenRegister(namedTypeSymbol.TypeArguments[4]);
                GenRegister(namedTypeSymbol.TypeArguments[5]);
                GenRegister(namedTypeSymbol.TypeArguments[6]);
                GenRegister(namedTypeSymbol.TypeArguments[7]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.List))
            {
                var typeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<List<{{typeName}}>>())
        {
            CoPackFormatterProvider.Register(new ListFormatter<{{typeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.HashSet))
            {
                var typeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<HashSet<{{typeName}}>>())
        {
            CoPackFormatterProvider.Register(new HashSetFormatter<{{typeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.Queue))
            {
                var typeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<Queue<{{typeName}}>>())
        {
            CoPackFormatterProvider.Register(new QueueFormatter<{{typeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.Stack))
            {
                var typeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<Stack<{{typeName}}>>())
        {
            CoPackFormatterProvider.Register(new StackFormatter<{{typeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.LinkedList))
            {
                var typeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<LinkedList<{{typeName}}>>())
        {
            CoPackFormatterProvider.Register(new LinkedListFormatter<{{typeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.SortedSet))
            {
                var typeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<SortedSet<{{typeName}}>>())
        {
            CoPackFormatterProvider.Register(new SortedSetFormatter<{{typeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.Dictionary))
            {
                var keyTypeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var valueTypeName = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<Dictionary<{{keyTypeName}}, {{valueTypeName}}>>())
        {
            CoPackFormatterProvider.Register(new DictionaryFormatter<{{keyTypeName}}, {{valueTypeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.SortedDictionary))
            {
                var keyTypeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var valueTypeName = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<SortedDictionary<{{keyTypeName}}, {{valueTypeName}}>>())
        {
            CoPackFormatterProvider.Register(new SortedDictionaryFormatter<{{keyTypeName}}, {{valueTypeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                return;
            }
            
            if (SymbolHelper.IsIdentity(namedTypeSymbol.OriginalDefinition, _wellKnownSymbols.PriorityQueue))
            {
                var keyTypeName = namedTypeSymbol.TypeArguments[0].ToDisplayString();
                var valueTypeName = namedTypeSymbol.TypeArguments[1].ToDisplayString();
                sbr.AppendLine($$"""
        if (!CoPackFormatterProvider.IsRegistered<PriorityQueue<{{keyTypeName}}, {{valueTypeName}}>>())
        {
            CoPackFormatterProvider.Register(new PriorityQueueFormatter<{{keyTypeName}}, {{valueTypeName}}>());
        }
""");
                // 递归生成注册代码
                GenRegister(namedTypeSymbol.TypeArguments[0]);
                GenRegister(namedTypeSymbol.TypeArguments[1]);
                return;
            }
        }
    }
    
    private string GenFormatter(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.UnionInfos.Count == 0)
        {
            return GenNormalFormatter(typeInfo);
        }
        else
        {
            return GenUnionFormatter(typeInfo);
        }
    }

    private string GenNormalFormatter(CoPackableTypeInfo typeInfo)
    {
        var typeName = typeInfo.TypeSymbol.Name;
        var typeNameOfParams = typeInfo.TypeSymbol.TypeKind == TypeKind.Struct ? typeName : $"{typeName}?";
        
        string code = $$"""
    public class {{typeName}}Formatter : ICoPackFormatter<{{typeName}}>
    {
        public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in {{typeNameOfParams}} value, PackFlags flags) 
            where TBufferWriter : IBufferWriter<byte>
        {
{{TryGenWriterNull(typeInfo)}}
{{TryGenBeforeSerialize(typeInfo)}}
            writer.WriteObjectHeader();
{{GenWriteFields(typeInfo)}}
            writer.WriteNull();
{{TryGenAfterSerialize(typeInfo)}}
        }
        
        public {{typeNameOfParams}} Read(ref CoPackReader reader, object? state)
        {
{{TryGenReadNull(typeInfo)}}
{{GenReadConstructor(typeInfo)}}
{{TryGenBeforeDeserialize(typeInfo)}}
            reader.ReadObjectHeader();
            while (!reader.TryReadNull())
            {
                if (reader.TryReadInt32(out var intTag))
                {
                    switch (intTag)
                    {
{{GenIntCases(typeInfo)}}
                        default:
                            CoPackException.ThrowReadUnexpectedObjectTag(intTag);
                            break;
                    }
                }
                else
                {
                    var strTag = reader.ReadString();
                    switch (strTag)
                    {
{{GenStrCases(typeInfo)}}
                        default:
                            CoPackException.ThrowReadUnexpectedObjectTag(strTag);
                            break;
                    }
                }
            }
{{TryGenAfterDeserialize(typeInfo)}}
            return value;
        }
    }
""";
        return code;
    }
    
    private string GenUnionFormatter(CoPackableTypeInfo typeInfo)
    {
        var typeName = typeInfo.TypeSymbol.Name;
        var typeNameOfParams = typeInfo.TypeSymbol.TypeKind == TypeKind.Struct ? typeName : $"{typeName}?";
        
        string code = $$"""
    public class {{typeName}}Formatter : ICoPackFormatter<{{typeName}}>
    {
        public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in {{typeNameOfParams}} value, PackFlags flags) 
            where TBufferWriter : IBufferWriter<byte>
        {
{{TryGenWriterNull(typeInfo)}}
            switch (value)
            {
{{GenWriteUnionCases(typeInfo)}}
                case { } thisObject:
                    DoWrite(ref writer, thisObject, flags);
                    break;
                default:
                    CoPackException.ThrowNotFoundInUnionType(value.GetType(), typeof({{typeName}}));
                    break;
            }
        }
        
        public {{typeNameOfParams}} Read(ref CoPackReader reader, object? state)
        {
{{TryGenReadNull(typeInfo)}}
            if (reader.TryReadObjectHeader())
            {
                return DoRead(ref reader, state);
            }

            var tag = reader.ReadUnionHeader();
            switch (tag)
            {
{{GenReadUnionCases(typeInfo)}}
                default:
                    CoPackException.ThrowReadUnexpectedUnionTag(tag);
                    return null;
            }
        }
        
        private void DoWrite<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in {{typeName}} value, PackFlags flags) 
            where TBufferWriter : IBufferWriter<byte>
        {
{{TryGenBeforeSerialize(typeInfo)}}
            writer.WriteObjectHeader();
{{GenWriteFields(typeInfo)}}
            writer.WriteNull();
{{TryGenAfterSerialize(typeInfo)}}
        }

        private {{typeName}} DoRead(ref CoPackReader reader, object? state)
        {
{{GenReadConstructor(typeInfo)}}
{{TryGenBeforeDeserialize(typeInfo)}}
            while (!reader.TryReadNull())
            {
                if (reader.TryReadInt32(out var intTag))
                {
                    switch (intTag)
                    {
{{GenIntCases(typeInfo)}}
                        default:
                            CoPackException.ThrowReadUnexpectedObjectTag(intTag);
                            break;
                    }
                }
                else
                {
                    var strTag = reader.ReadString();
                    switch (strTag)
                    {
{{GenStrCases(typeInfo)}}
                        default:
                            CoPackException.ThrowReadUnexpectedObjectTag(strTag);
                            break;
                    }
                }
            }
{{TryGenAfterDeserialize(typeInfo)}}
            return value;
        }
        
    }
""";
        return code;
    }
    
    string TryGenWriterNull(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.TypeSymbol.IsReferenceType)
        {
            return """
            if (value == null)
            {
                writer.WriteNull();
                return;
            }                       
""";
        }

        return string.Empty;
    }

    string TryGenBeforeSerialize(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.BeforeSerialize != null)
            return $$"""
            value.{{typeInfo.BeforeSerialize.Name}}(flags);                         
""";
        return string.Empty;
    }

    string TryGenAfterSerialize(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.AfterSerialize != null)
            return $$"""
            value.{{typeInfo.AfterSerialize.Name}}(flags);                         
""";
        return string.Empty;
    }

    string GenWriteFields(CoPackableTypeInfo typeInfo)
    {
        var sbr = new StringBuilder();
        foreach (var fieldInfo in typeInfo.TagFields)
        {
            var typeSymbol = fieldInfo.IsProperty
                ? (fieldInfo.Symbol as IPropertySymbol)!.Type
                : (fieldInfo.Symbol as IFieldSymbol)!.Type;
            var fieldName = fieldInfo.IsProperty
                ? (fieldInfo.Symbol as IPropertySymbol)!.Name
                : (fieldInfo.Symbol as IFieldSymbol)!.Name;
            sbr.AppendLine(GenWriteField(typeSymbol, fieldInfo, fieldName));
        }

        return sbr.ToString();
    }

    string GenWriteField(ITypeSymbol fieldTypeSymbol, FieldInfo fieldInfo, string fieldName)
    {
        if (fieldTypeSymbol.IsReferenceType)
        {
            if (fieldTypeSymbol.SpecialType == SpecialType.System_String)
            {
                return $$"""
            if ({{GenCheckReferenceDefault(fieldInfo, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteString(value.{{fieldName}});
            }
""";
            }
            else if (fieldTypeSymbol is IArrayTypeSymbol {SpecialType: SpecialType.System_Byte})
            {
                return $$"""
            if ({{GenCheckReferenceDefault(fieldInfo, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteBytes(value.{{fieldName}});
            }
""";
            }
            else
            {
                return $$"""
            if ({{GenCheckReferenceDefault(fieldInfo, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteValue(value.{{fieldName}});
            }
""";
            }
        }
        else if (fieldTypeSymbol.IsValueType)
        {
            if (fieldTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return $$"""
            if ({{GenCheckReferenceDefault(fieldInfo, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteValue(value.{{fieldName}});
            }
""";
            }

            switch (fieldTypeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return $$"""
            if ({{GenCheckValueDefault(fieldInfo, fieldTypeSymbol, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteBool(value.{{fieldName}});
            }
""";
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    return $$"""
            if ({{GenCheckValueDefault(fieldInfo, fieldTypeSymbol, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteVarInt(value.{{fieldName}});
            }
""";
                case SpecialType.System_Single:
                    return $$"""
            if ({{GenCheckValueDefault(fieldInfo, fieldTypeSymbol, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteFloat(value.{{fieldName}});
            }
""";
                case SpecialType.System_Double:
                    return $$"""
            if ({{GenCheckValueDefault(fieldInfo, fieldTypeSymbol, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteDouble(value.{{fieldName}});
            }
""";
                default:
                    return $$"""
            if ({{GenCheckValueDefault(fieldInfo, fieldTypeSymbol, fieldName)}}((int)flags & {{fieldInfo.Flags}}) != 0)
            {
{{GenWriteTag(fieldInfo)}}
                writer.WriteValue(value.{{fieldName}});
            }
""";
            }
        }

        return string.Empty;
    }

    string GenCheckReferenceDefault(FieldInfo fieldInfo, string fieldName)
    {
        if (fieldInfo.PackIfDefault)
            return "";
        return $"value.{fieldName} != null && ";
    }
    
    string GenCheckValueDefault(FieldInfo fieldInfo, ITypeSymbol fieldTypeSymbol, string fieldName)
    {
        if (fieldInfo.PackIfDefault)
            return "";
        switch (fieldTypeSymbol.SpecialType)
        {
            case SpecialType.System_Enum:
                return $"(int)value.{fieldName} != 0 && ";
            case SpecialType.System_Boolean:
                return $"value.{fieldName} != false && ";
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
                return $"value.{fieldName} != 0 && ";
            case SpecialType.System_DateTime:
                return $"value.{fieldName} != System.DateTime.MinValue && ";
            default:
                if (SymbolHelper.IsIdentity(fieldTypeSymbol, _wellKnownSymbols.DateTimeOffset))
                    return $"value.{fieldName} != System.DateTimeOffset.MinValue && ";
                break;
        }
        
        return string.Empty;
    }

    string GenWriteTag(FieldInfo fieldInfo)
    {
        if (string.IsNullOrEmpty(fieldInfo.StrTag))
            return $$"""
                writer.WriteVarInt({{fieldInfo.IntTag}});
""";
        else
            return $$"""
                writer.WriteString("{{fieldInfo.StrTag}}");
""";
    }

    string TryGenReadNull(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.TypeSymbol.IsReferenceType)
        {
            return $$"""
            if (reader.TryReadNull())
                return null;                     
""";
        }

        return string.Empty;
    }

    string GenReadConstructor(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.Constructor == null)
        {
            return $$"""
            var value = new {{typeInfo.TypeSymbol.Name}}();                                             
""";
        }
        else {
            return $$"""
            var value = new {{typeInfo.TypeSymbol.Name}}(state);                                             
""";
        }
    }
    
    string TryGenBeforeDeserialize(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.BeforeDeserialize != null)
            return $$"""
            value.{{typeInfo.BeforeDeserialize.Name}}(state);                         
""";
        return string.Empty;
    }
    
    string TryGenAfterDeserialize(CoPackableTypeInfo typeInfo)
    {
        if (typeInfo.AfterDeserialize != null)
            return $$"""
            value.{{typeInfo.AfterDeserialize.Name}}(state);                         
""";
        return string.Empty;
    }

    string GenIntCases(CoPackableTypeInfo typeInfo)
    {
        var sbr = new StringBuilder();
        foreach (var fieldInfo in typeInfo.TagFields)
        {
            if (!string.IsNullOrEmpty(fieldInfo.StrTag))
                continue;
            
            var typeSymbol = fieldInfo.IsProperty
                ? (fieldInfo.Symbol as IPropertySymbol)!.Type
                : (fieldInfo.Symbol as IFieldSymbol)!.Type;
            var fieldName = fieldInfo.IsProperty
                ? (fieldInfo.Symbol as IPropertySymbol)!.Name
                : (fieldInfo.Symbol as IFieldSymbol)!.Name;
            
            sbr.AppendLine($$"""
                        case {{fieldInfo.IntTag}}:
                            value.{{fieldName}} = {{GenReadField(typeSymbol, fieldInfo, fieldName)}};
                            break;
""");
        }

        var s = sbr.ToString();
        return s;
    }

    string GenStrCases(CoPackableTypeInfo typeInfo)
    {
        var sbr = new StringBuilder();
        foreach (var fieldInfo in typeInfo.TagFields)
        {
            if (string.IsNullOrEmpty(fieldInfo.StrTag))
                continue;
            
            var typeSymbol = fieldInfo.IsProperty
                ? (fieldInfo.Symbol as IPropertySymbol)!.Type
                : (fieldInfo.Symbol as IFieldSymbol)!.Type;
            var fieldName = fieldInfo.IsProperty
                ? (fieldInfo.Symbol as IPropertySymbol)!.Name
                : (fieldInfo.Symbol as IFieldSymbol)!.Name;
            
            sbr.AppendLine($$"""
                        case "{{fieldInfo.StrTag}}":
                            value.{{fieldName}} = {{GenReadField(typeSymbol, fieldInfo, fieldName)}};
                            break;
""");
        }

        var s = sbr.ToString();
        return s;
    }

    string GenReadField(ITypeSymbol fieldTypeSymbol, FieldInfo fieldInfo, string fieldName)
    {
        if (fieldTypeSymbol.IsReferenceType)
        {
            if (fieldTypeSymbol.SpecialType == SpecialType.System_String)
            {
                return "reader.ReadString()";
            }
            else if (fieldTypeSymbol is IArrayTypeSymbol {SpecialType: SpecialType.System_Byte})
            {
                return "reader.ReadBytes()";
            }
            else
            {
                return $"reader.ReadValue<{fieldTypeSymbol.ToDisplayString()}>(state)";
            }
        }
        else if (fieldTypeSymbol.IsValueType)
        {
            if (fieldTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return $"reader.ReadValue<{fieldTypeSymbol.ToDisplayString()}>(state)";
            }

            switch (fieldTypeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return "reader.ReadBool()";
                case SpecialType.System_SByte:
                    return "reader.ReadInt8()";
                case SpecialType.System_Byte:
                    return "reader.ReadUInt8()";
                case SpecialType.System_Int16:
                    return "reader.ReadInt16()";
                case SpecialType.System_UInt16:
                    return "reader.ReadUInt16()";
                case SpecialType.System_Int32:
                    return "reader.ReadInt32()";
                case SpecialType.System_UInt32:
                    return "reader.ReadUInt32()";
                case SpecialType.System_Int64:
                    return "reader.ReadInt64()";
                case SpecialType.System_UInt64:
                    return "reader.ReadUInt64()";
                case SpecialType.System_Single:
                    return "reader.ReadFloat()";
                case SpecialType.System_Double:
                    return "reader.ReadDouble()";
                default:
                    return $"reader.ReadValue<{fieldTypeSymbol.ToDisplayString()}>(state)";
            }
        }

        return string.Empty;
    }

    string GenWriteUnionCases(CoPackableTypeInfo typeInfo)
    {
        var sbr = new StringBuilder();
        foreach (var unionInfo in typeInfo.UnionInfos)
        {
            var typeName = unionInfo.TypeSymbol.ToDisplayString();
            var name = unionInfo.TypeSymbol.Name;
            sbr.Append($$"""
                case {{typeName}} a{{name}}:
                    writer.WriteUnion({{unionInfo.Tag}}, a{{name}}, flags);
                    break;
""");
        }

        var s = sbr.ToString();
        return s;
    }

    string GenReadUnionCases(CoPackableTypeInfo typeInfo)
    {
        var sbr = new StringBuilder();
        foreach (var unionInfo in typeInfo.UnionInfos)
        {
            var typeName = unionInfo.TypeSymbol.ToDisplayString();
            sbr.Append($$"""
                case {{unionInfo.Tag}}:
                    return reader.ReadValue<{{typeName}}>();
""");
        }

        var s = sbr.ToString();
        return s;
    }
}