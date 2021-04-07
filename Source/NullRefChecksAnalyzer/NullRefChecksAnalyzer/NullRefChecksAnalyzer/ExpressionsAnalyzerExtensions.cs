using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer
{
    public static class ExpressionsAnalyzerExtensions
    {
        public static IEnumerable<BinaryExpressionSyntax> FilterForEqualityCheck(this IEnumerable<BinaryExpressionSyntax> binaryExpressions) =>
            binaryExpressions.Where(binaryExpression => binaryExpression.Kind() is SyntaxKind.EqualsExpression ||
                                                        binaryExpression.Kind() is SyntaxKind.NotEqualsExpression ||
                                                        binaryExpression.Kind() is SyntaxKind.IsExpression);

        public static IEnumerable<PatternSyntax> FilterByPatterns(this IEnumerable<PatternSyntax> patternExpressions) =>
            patternExpressions.Where(patternExpression => patternExpression.Kind() is SyntaxKind.ConstantPattern ||
                                                          patternExpression.Kind() is SyntaxKind.RecursivePattern);

        public static IdentifierNameSyntax GetParentIdentifierName(this CSharpSyntaxNode expression)
        {
            IdentifierNameSyntax identifierName = expression.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            while (identifierName is null)
            {
                identifierName = expression.Parent?.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            }

            return identifierName;
        }
    }
}