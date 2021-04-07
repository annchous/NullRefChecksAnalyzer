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
    }
}