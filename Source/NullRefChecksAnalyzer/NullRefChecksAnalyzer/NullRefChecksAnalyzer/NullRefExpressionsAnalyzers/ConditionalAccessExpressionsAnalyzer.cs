using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class ConditionalAccessExpressionsAnalyzer : ExpressionsAnalyzer
    {
        private readonly ConditionalAccessExpressionSyntax _conditionalAccessExpression;

        public ConditionalAccessExpressionsAnalyzer(ConditionalAccessExpressionSyntax conditionalAccessExpression)
        {
            _conditionalAccessExpression = conditionalAccessExpression;
        }

        public override void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            Location = _conditionalAccessExpression.DescendantTokens()
                .FirstOrDefault(token => token.Kind() is SyntaxKind.QuestionToken).GetLocation();

            if (Location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Location));
            }
        }
    }
}