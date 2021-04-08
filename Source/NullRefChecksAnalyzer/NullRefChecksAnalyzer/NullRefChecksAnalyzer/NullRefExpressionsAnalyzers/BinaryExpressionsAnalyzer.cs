using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NullRefChecksAnalyzer.NullRefExpressionsAnalyzersExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class BinaryExpressionsAnalyzer : ExpressionsAnalyzer
    {
        private readonly BinaryExpressionSyntax _binaryExpression;

        public BinaryExpressionsAnalyzer(BinaryExpressionSyntax binaryExpression)
        {
            Location = Location.None;
            _binaryExpression = binaryExpression;
        }

        public override void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            if (_binaryExpression.Kind() is SyntaxKind.CoalesceExpression)
            {
                Location = _binaryExpression.DescendantTokens()
                    .FirstOrDefault(token => token.Kind() is SyntaxKind.QuestionQuestionToken).GetLocation();
            }
            else
            {
                if (!_binaryExpression.ContainsNullOrDefault())
                {
                    return;
                }

                Location = _binaryExpression.GetLocation();
            }

            if (Location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Location));
            }
        }
    }
}