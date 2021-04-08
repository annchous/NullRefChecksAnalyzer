using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class AssignmentExpressionsAnalyzer : ExpressionsAnalyzer
    {
        private readonly AssignmentExpressionSyntax _assignmentExpression;

        public AssignmentExpressionsAnalyzer(AssignmentExpressionSyntax assignmentExpression)
        {
            _assignmentExpression = assignmentExpression;
        }

        public override void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            if (!(_assignmentExpression.Kind() is SyntaxKind.CoalesceAssignmentExpression))
            {
                return;
            }

            Location = _assignmentExpression.DescendantTokens()
                .FirstOrDefault(token => token.Kind() is SyntaxKind.QuestionQuestionEqualsToken).GetLocation();

            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location));
        }
    }
}