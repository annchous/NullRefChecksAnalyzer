using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsSimplifiers
{
    public class LogicalOrSimplifier : BaseSimplifier
    {
        public SyntaxNode ParentExpression { get; set; }

        public LogicalOrSimplifier(SyntaxNode expression) : base(expression)
        {
            ParentExpression = expression;
        }

        public override bool Simplify()
        {
            var notSimplifier = new LogicalNotSimplifier(Expression);
            if (notSimplifier.Simplify())
            {
                ResultExpression = notSimplifier.ResultExpression;
            }

            while (ParentExpression?.Kind() != SyntaxKind.LogicalOrExpression)
            {
                ParentExpression = ParentExpression?.Parent;
            }

            if ((Expression?.Kind() is SyntaxKind.EqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 == 0) ||
                (Expression?.Kind() is SyntaxKind.NotEqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 != 0))
            {
                var secondExpression = GetSecondInOrExpression();
                if (secondExpression is null)
                {
                    return false;
                }

                ResultExpression = secondExpression;
            }
            else if (Expression?.Kind() is SyntaxKind.IsPatternExpression)
            {
                var isPatternSimplifier = new IsPatternSimplifier(Expression);
                if (isPatternSimplifier.Simplify())
                {
                    ResultExpression = isPatternSimplifier.ResultExpression;
                }
            }
            else
            {
                ResultExpression = ReplaceWithTrueExpression();
            }

            return Expression != ResultExpression;
        }

        private SyntaxNode GetSecondInOrExpression() => ResultExpression?.Parent?.DescendantNodes().OfType<ExpressionSyntax>()
            .FirstOrDefault(node => node.Parent == ResultExpression.Parent && node != ResultExpression);

        private SyntaxNode ReplaceWithTrueExpression() => ParentExpression?.ReplaceNode(ResultExpression,
            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
    }
}