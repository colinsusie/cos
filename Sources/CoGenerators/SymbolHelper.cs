// // Written by Colin on 2023-12-31

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

namespace CoGenerators;

public static class SymbolHelper
{
    /// <summary>
    /// 检查一个类是否继承自baseClass
    /// </summary>
    /// <param name="symbol">类的符号</param>
    /// <param name="baseClass">基类全命：namespace.class </param>
    /// <param name="checkAncestor">是否检查祖先类，否则必须是直接基类</param>
    /// <returns></returns>
    public static bool IsInheritedFrom(INamedTypeSymbol symbol, string baseClass, bool checkAncestor = true)
    {
        if (!checkAncestor)
            return symbol.BaseType?.ToDisplayString() == baseClass;

        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString() == baseClass)
                return true;
            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// 检查一个类型是否实现了某个接口
    /// </summary>
    /// <param name="symbol">类型的符号</param>
    /// <param name="interfaceName">接口的名字：namespace.interface</param>
    /// <param name="checkBase">是否检查基类是否实现该接口</param>
    /// <returns></returns>
    public static bool IsImplementInterface(INamedTypeSymbol symbol, string interfaceName, bool checkBase = true)
    {
        ImmutableArray<INamedTypeSymbol> interfaces;
        interfaces = checkBase ? symbol.AllInterfaces : symbol.Interfaces;
        foreach (var interfaceSymbol in interfaces)
        {
            if (interfaceSymbol.ToDisplayString() == interfaceName)
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// 判断一个符号是否有某个属性
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="attrName"></param>
    /// <returns></returns>
    public static bool HasAttribute(ISymbol symbol, string attrName)
    {
        foreach (var attrData in symbol.GetAttributes())
        {
            if (attrData.AttributeClass?.ToDisplayString() != attrName)
                continue;
            return true;
        }

        return false;
    }

    public static bool TryGetAttribute(ISymbol symbol, string attrName, 
        out AttributeData? attrData)
    {
        foreach (var adata in symbol.GetAttributes())
        {
            if (adata.AttributeClass?.ToDisplayString() != attrName)
                continue;
            attrData = adata;
            return true;
        }

        attrData = null;
        return false;
    }

    /// <summary>
    /// 取这个符号声明的位置，如果有多个位置返回第1个
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static Location? GetLocation(ISymbol symbol)
    {
        if (symbol.Locations.Length == 0)
            return null;
        return symbol.Locations[0];
    }

    /// <summary>
    /// 判断两相类型符号是否相等
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static bool IsIdentity(ITypeSymbol source, ITypeSymbol dest)
    {
        return SymbolEqualityComparer.Default.Equals(source, dest);
    }

    /// <summary>
    /// 获得一个类型及其父类的所有成员
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public static List<ISymbol> GetAllMembers(INamedTypeSymbol typeSymbol)
    {
        var members = new List<ISymbol>();
        DoGetMembers(typeSymbol, members);
        return members;

        void DoGetMembers(INamedTypeSymbol aTypeSymbol, List<ISymbol> aMembers)
        {
            if (aTypeSymbol.BaseType != null)
            {
                DoGetMembers(aTypeSymbol.BaseType, aMembers);
            }
            aMembers.AddRange(aTypeSymbol.GetMembers());
        }
    }
}