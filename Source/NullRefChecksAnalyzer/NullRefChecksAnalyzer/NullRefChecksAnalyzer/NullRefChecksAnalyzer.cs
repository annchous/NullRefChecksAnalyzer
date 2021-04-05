using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace NullRefChecksAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NullRefChecksAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NullRefChecksAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = "Redundant null reference check";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Syntax";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeNullChecks, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeNullChecks(SyntaxNodeAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var syntaxTree = semanticModel.SyntaxTree;
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            foreach (var ifStatement in methodDeclaration.DescendantNodes().OfType<IfStatementSyntax>())
            {
                var literalExpressions = ifStatement.Condition.DescendantNodes().OfType<LiteralExpressionSyntax>().ToList();
                if (literalExpressions.Any())
                {
                    var nullLiteralExpressions = GetNullLiteralExpressions(literalExpressions);
                    nullLiteralExpressions.ForEach(literalExpression => context.ReportDiagnostic(Diagnostic.Create(Rule, literalExpression.Parent.GetLocation())));
                    var defaultLiteralExpressions = GetDefaultLiteralExpressions(literalExpressions);
                    defaultLiteralExpressions.ForEach(literalExpression => context.ReportDiagnostic(Diagnostic.Create(Rule, literalExpression.Parent.GetLocation())));
                }
            }

            foreach (var switchSection in methodDeclaration.DescendantNodes().OfType<SwitchSectionSyntax>())
            {
                var caseSwitchLabels = switchSection.Labels.OfType<CaseSwitchLabelSyntax>().ToList();
                if (caseSwitchLabels.Any())
                {
                    var nullLiteralSwitchCases = GetNullSwitchCaseLiteralExpressions(caseSwitchLabels);
                    nullLiteralSwitchCases.ForEach(nullLiteralSwitchCase => context.ReportDiagnostic(Diagnostic.Create(Rule, nullLiteralSwitchCase.GetLocation())));
                }
            }

            foreach (var switchExpression in methodDeclaration.DescendantNodes().OfType<SwitchExpressionSyntax>())
            {

            }
        }

        private static List<LiteralExpressionSyntax>
            GetNullLiteralExpressions(List<LiteralExpressionSyntax> literalExpressions) => literalExpressions
            .Where(literalExpression => literalExpression.Kind() is SyntaxKind.NullLiteralExpression).ToList();

        private static List<LiteralExpressionSyntax>
            GetDefaultLiteralExpressions(List<LiteralExpressionSyntax> literalExpressions) => literalExpressions
            .Where(literalExpression => literalExpression.Kind() is SyntaxKind.DefaultLiteralExpression).ToList();

        private static List<CaseSwitchLabelSyntax>
            GetNullSwitchCaseLiteralExpressions(List<CaseSwitchLabelSyntax> caseSwitchLabels) => caseSwitchLabels
            .Where(caseSwitchLabel => caseSwitchLabel.Value.Kind() is SyntaxKind.NullLiteralExpression).ToList();
    }
}
