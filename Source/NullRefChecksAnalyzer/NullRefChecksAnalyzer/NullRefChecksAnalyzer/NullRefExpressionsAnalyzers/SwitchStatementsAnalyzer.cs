using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NullRefChecksAnalyzer.NullRefExpressionsAnalyzersExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class SwitchStatementsAnalyzer : ExpressionsAnalyzer
    {
        private readonly SwitchStatementSyntax _switchStatement;

        public SwitchStatementsAnalyzer(SwitchStatementSyntax switchStatement)
        {
            _switchStatement = switchStatement;
        }

        public override void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            if (!_switchStatement.ContainsNullOrDefault())
            {
                return;
            }

            Location = _switchStatement.DescendantNodes().OfType<LiteralExpressionSyntax>()
                .FirstOrDefault(literalExpression => literalExpression.IsNullOrDefault())?.Parent?.GetLocation();

            if (Location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Location));
            }
        }
    }
}