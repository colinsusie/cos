// Written by Colin on 2024-10-20

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CoGenerators.Serialize;

public record struct FieldInfo(
    ISymbol Symbol,
    bool IsProperty,
    int IntTag,
    string StrTag,
    bool PackIfDefault,
    int Flags);

public record struct UnionInfo(
    INamedTypeSymbol TypeSymbol,
    int Tag
);

public class CoPackableTypeInfo
{
    public readonly INamedTypeSymbol TypeSymbol;
    public readonly List<FieldInfo> TagFields = new();
    public readonly List<UnionInfo> UnionInfos = new();
    public IMethodSymbol? Constructor;
    public IMethodSymbol? BeforeSerialize;
    public IMethodSymbol? AfterSerialize;
    public IMethodSymbol? BeforeDeserialize;
    public IMethodSymbol? AfterDeserialize;

    public CoPackableTypeInfo(INamedTypeSymbol typeSymbol)
    {
        TypeSymbol = typeSymbol;
    }
}