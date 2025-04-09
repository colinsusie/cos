// Written by Colin on 2024-10-20

using System;
using Microsoft.CodeAnalysis;

namespace CoGenerators.Serialize;

public class WellKnownSymbols
{
    public readonly INamedTypeSymbol ValueTuple1;
    public readonly INamedTypeSymbol ValueTuple2;
    public readonly INamedTypeSymbol ValueTuple3;
    public readonly INamedTypeSymbol ValueTuple4;
    public readonly INamedTypeSymbol ValueTuple5;
    public readonly INamedTypeSymbol ValueTuple6;
    public readonly INamedTypeSymbol ValueTuple7;
    public readonly INamedTypeSymbol ValueTuple8;
    public readonly INamedTypeSymbol List;
    public readonly INamedTypeSymbol HashSet;
    public readonly INamedTypeSymbol Queue;
    public readonly INamedTypeSymbol Stack;
    public readonly INamedTypeSymbol LinkedList;
    public readonly INamedTypeSymbol SortedSet;
    public readonly INamedTypeSymbol Dictionary;
    public readonly INamedTypeSymbol SortedDictionary;
    public readonly INamedTypeSymbol PriorityQueue;
    public readonly INamedTypeSymbol DateTimeOffset;
    
    
    public WellKnownSymbols(Compilation compilation)
    {
        ValueTuple1 = compilation.GetTypeByMetadataName("System.ValueTuple`1") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`1符号");
        ValueTuple2 = compilation.GetTypeByMetadataName("System.ValueTuple`2") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`2符号");
        ValueTuple3 = compilation.GetTypeByMetadataName("System.ValueTuple`3") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`3符号");
        ValueTuple4 = compilation.GetTypeByMetadataName("System.ValueTuple`4") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`4符号");
        ValueTuple5 = compilation.GetTypeByMetadataName("System.ValueTuple`5") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`5符号");
        ValueTuple6 = compilation.GetTypeByMetadataName("System.ValueTuple`6") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`6符号");
        ValueTuple7 = compilation.GetTypeByMetadataName("System.ValueTuple`7") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`7符号");
        ValueTuple8 = compilation.GetTypeByMetadataName("System.ValueTuple`8") ?? 
                      throw new InvalidOperationException($"找不到System.ValueTuple`8符号");
        
        List = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1") ?? 
               throw new InvalidOperationException($"找不到System.Collections.Generic.List`1符号");
        HashSet = compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1") ?? 
                  throw new InvalidOperationException($"找不到System.Collections.Generic.HashSet`1符号");
        Queue = compilation.GetTypeByMetadataName("System.Collections.Generic.Queue`1") ?? 
                throw new InvalidOperationException($"找不到System.Collections.Generic.Queue`1符号");
        Stack = compilation.GetTypeByMetadataName("System.Collections.Generic.Stack`1") ?? 
                throw new InvalidOperationException($"找不到System.Collections.Generic.Stack`1符号");
        LinkedList = compilation.GetTypeByMetadataName("System.Collections.Generic.LinkedList`1") ?? 
                     throw new InvalidOperationException($"找不到System.Collections.Generic.LinkedList`1符号");
        SortedSet = compilation.GetTypeByMetadataName("System.Collections.Generic.SortedSet`1") ?? 
                    throw new InvalidOperationException($"找不到System.Collections.Generic.SortedSet`1符号");
        Dictionary = compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2") ?? 
                     throw new InvalidOperationException($"找不到System.Collections.Generic.Dictionary`2符号");
        SortedDictionary = compilation.GetTypeByMetadataName("System.Collections.Generic.SortedDictionary`2") ?? 
                           throw new InvalidOperationException($"找不到System.Collections.Generic.SortedDictionary`2符号");
        PriorityQueue = compilation.GetTypeByMetadataName("System.Collections.Generic.PriorityQueue`2") ?? 
                        throw new InvalidOperationException($"找不到System.Collections.Generic.PriorityQueue`2符号");
        
        DateTimeOffset = compilation.GetTypeByMetadataName("System.DateTimeOffset") ?? 
                         throw new InvalidOperationException($"找不到System.DateTimeOffset符号");
    }
}