using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzersExtensions
{
    public static class ExpressionsAnalyzerExtensions
    {
        public static IEnumerable<BinaryExpressionSyntax> FilterForEqualityCheck(this IEnumerable<BinaryExpressionSyntax> binaryExpressions) =>
            binaryExpressions.Where(binaryExpression => binaryExpression.Kind() is SyntaxKind.EqualsExpression ||
                                                        binaryExpression.Kind() is SyntaxKind.NotEqualsExpression);

        public static IEnumerable<PatternSyntax> FilterByPatterns(this IEnumerable<PatternSyntax> patternExpressions) =>
            patternExpressions.Where(patternExpression => patternExpression.Kind() is SyntaxKind.ConstantPattern ||
                                                          patternExpression.Kind() is SyntaxKind.RecursivePattern);

        public static IEnumerable<BinaryExpressionSyntax> FilterByCoalesce(this IEnumerable<BinaryExpressionSyntax> expressions) =>
            expressions.Where(expression => expression.Kind() is SyntaxKind.CoalesceExpression);

        public static IEnumerable<AssignmentExpressionSyntax> FilterByCoalesceAssignment(this IEnumerable<AssignmentExpressionSyntax> expressions) =>
            expressions.Where(expression => expression.Kind() is SyntaxKind.CoalesceAssignmentExpression);

        public static IdentifierNameSyntax GetParentIdentifierName(this SyntaxNode expression)
        {
            var parent = expression;
            var identifierName = expression.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            while (identifierName is null)
            {
                parent = parent?.Parent;
                identifierName = parent?.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            }

            return identifierName;
        }
    }
}