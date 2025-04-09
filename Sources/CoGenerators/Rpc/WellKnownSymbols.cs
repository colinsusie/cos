// Written by Colin on 2024-9-26

using System;
using Microsoft.CodeAnalysis;

namespace CoGenerators.Rpc;

public class WellKnownSymbols
{
    public readonly INamedTypeSymbol ValueTaskSymbol;
    public readonly INamedTypeSymbol ValueTaskOfTSymbol;

    public WellKnownSymbols(Compilation compilation)
    {
        ValueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask") ?? 
                          throw new InvalidOperationException($"找不到System.Threading.Tasks.ValueTask符号");
        ValueTaskOfTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1") ?? 
                             throw new InvalidOperationException($"找不到System.Threading.Tasks.ValueTask`1符号");
    }
}