using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsSimplifiers
{
    public class LogicalNotSimplifier : BaseSimplifier
    {
        public int LogicalNotExpressionsCount { get; set; }

        public LogicalNotSimplifier(SyntaxNode expression) : base(expression)
        {
        }

        public override bool Simplify()
        {
            LogicalNotExpressionsCount = 0;
            while ((ResultExpression.IsLogicalNotParent() || ResultExpression.IsParenthesizedParent()) &&
                   !ResultExpression.IsNoParent())
            {
                ResultExpression = ResultExpression?.Parent;
                if (ResultExpression?.Kind() is SyntaxKind.LogicalNotExpression)
                {
                    LogicalNotExpressionsCount += 1;
                }
            }

            return Expression != ResultExpression;
        }
    }
}