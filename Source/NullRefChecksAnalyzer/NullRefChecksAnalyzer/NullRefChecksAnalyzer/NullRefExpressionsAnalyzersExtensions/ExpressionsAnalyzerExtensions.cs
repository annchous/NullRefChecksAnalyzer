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

        public static IEnumerable<InvocationExpressionSyntax> FilterByInvocations(this IEnumerable<InvocationExpressionSyntax> expressions) => expressions.Where(expression => 
            expression.DescendantNodes().OfType<IdentifierNameSyntax>().Any(identifierName => identifierName.Identifier.ToString() == "Equals") || 
            expression.DescendantNodes().OfType<IdentifierNameSyntax>().Any(identifierName => identifierName.Identifier.ToString() == "ReferenceEquals"));
    }
}