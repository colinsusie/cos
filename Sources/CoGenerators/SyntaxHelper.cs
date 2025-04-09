// // Written by Colin on 2023-12-31

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoGenerators;

public static class SyntaxHelper
{
    /// <summary>
    /// 判断该类型是否是Partial类
    /// </summary>
    public static bool IsPartialClass(ClassDeclarationSyntax classSyntax)
    {
        return classSyntax.Modifiers.Any(mod => mod.Text == "partial");
    }

    /// <summary>
    /// 判断类是否静态类
    /// </summary>
    public static bool IsStaticClass(ClassDeclarationSyntax classSyntax)
    {
        return classSyntax.Modifiers.Any(mod => mod.Text == "static");
    }
    
    /// <summary>
    /// 判断类型是否是partial类型
    /// </summary>
    public static bool IsPartialType(TypeDeclarationSyntax typeSyntax)
    {
        return typeSyntax.Modifiers.Any(mod => mod.Text == "partial");
    }
}