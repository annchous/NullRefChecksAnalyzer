using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NullRefChecksAnalyzer.NullRefExpressionsAnalyzersExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class SwitchExpressionsAnalyzer : ExpressionsAnalyzer
    {
        private readonly SwitchExpressionSyntax _switchExpression;

        public SwitchExpressionsAnalyzer(SwitchExpressionSyntax switchExpression)
        {
            _switchExpression = switchExpression;
        }

        public override void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            if (!_switchExpression.ContainsNullOrDefault())
            {
                return;
            }

            Location = _switchExpression.DescendantNodes().OfType<ConstantPatternSyntax>()
                .FirstOrDefault(pattern => pattern.ContainsNullOrDefault())?.Parent?.GetLocation();

            if (Location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Location));
            }
        }
    }
}