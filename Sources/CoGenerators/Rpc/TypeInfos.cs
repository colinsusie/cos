// Written by Colin on 2024-9-26

using Microsoft.CodeAnalysis;

namespace CoGenerators.Rpc;

public struct ReturnInfo
{
    public bool IsVoid;
    public bool HasReturn;
    public string TypeName;
    public ITypeSymbol? Type;
}