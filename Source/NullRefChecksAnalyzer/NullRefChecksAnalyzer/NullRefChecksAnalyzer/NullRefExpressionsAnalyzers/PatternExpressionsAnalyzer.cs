using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class PatternExpressionsAnalyzer : ExpressionsAnalyzer
    {
        private readonly PatternSyntax _patternExpression;

        public PatternExpressionsAnalyzer(PatternSyntax patternExpression)
        {
            _patternExpression = patternExpression;
        }

        public override void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            var parent = _patternExpression.Parent;
            while (parent?.GetType() != typeof(IsPatternExpressionSyntax))
            {
                parent = parent?.Parent;
            }

            Location = parent.GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location));
        }
    }
}