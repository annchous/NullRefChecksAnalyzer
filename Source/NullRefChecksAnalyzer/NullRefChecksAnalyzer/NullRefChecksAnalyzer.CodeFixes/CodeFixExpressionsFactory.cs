using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer
{
    public static class CodeFixExpressionsFactory
    {
        public static SyntaxNode FindExpression(this List<SyntaxNode> expressions)
        {
            if (expressions.OfType<BinaryExpressionSyntax>().FirstOrDefault() != null)
            {
                return expressions.OfType<BinaryExpressionSyntax>().First();
            }

            if (expressions.OfType<IsPatternExpressionSyntax>().FirstOrDefault() != null)
            {
                return expressions.OfType<IsPatternExpressionSyntax>().First();
            }

            if (expressions.OfType<ConditionalAccessExpressionSyntax>().FirstOrDefault() != null)
            {
                return expressions.OfType<ConditionalAccessExpressionSyntax>().First();
            }

            if (expressions.OfType<InvocationExpressionSyntax>().FirstOrDefault() != null)
            {
                return expressions.OfType<InvocationExpressionSyntax>().First();
            }

            if (expressions.OfType<CaseSwitchLabelSyntax>().FirstOrDefault() != null)
            {
                return expressions.OfType<CaseSwitchLabelSyntax>().First();
            }

            if (expressions.OfType<AssignmentExpressionSyntax>().FirstOrDefault() != null)
            {
                return expressions.OfType<AssignmentExpressionSyntax>().First();
            }

            return null;
        }
    }
}