// Written by Colin on 2024-10-1

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CoAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CoAnalyzer: DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule1001 = new DiagnosticDescriptor(
        id: "CoA1001", 
        title: "检查LocalList", 
        messageFormat: "类型为LocalList<T>的局部变量{0}应使用using关键字声明，以确保该变量正确释放缓存", 
        category: "代码规范",
        DiagnosticSeverity.Error, 
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule1001);

    public override void Initialize(AnalysisContext context)
    {
        // 不分析生成的代码
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        // 允许并行执行
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var localListType = compilationContext.Compilation.GetTypeByMetadataName("CoLib.Container.LocalList`1");
            
            // 声明 LocalList 局部变量 必须使用using关键字
            compilationContext.RegisterSyntaxNodeAction(symbolContext =>
            {
                if (symbolContext.Node is not LocalDeclarationStatementSyntax localDeclaration)
                    return;

                if (localDeclaration.Declaration.Variables.Count == 0)
                    return;
                
                var varType = symbolContext.SemanticModel.GetTypeInfo(localDeclaration.Declaration.Type).Type;
                if (varType == null || !SymbolEqualityComparer.Default.Equals(varType.OriginalDefinition, localListType))
                    return;

                if (!localDeclaration.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
                {
                    var variableName = localDeclaration.Declaration.Variables.First().Identifier.Text;
                    var diagnostic = Diagnostic.Create(Rule1001, localDeclaration.GetLocation(), variableName);
                    symbolContext.ReportDiagnostic(diagnostic);
                }
            }, SyntaxKind.LocalDeclarationStatement);
        });
        
        
    }
}