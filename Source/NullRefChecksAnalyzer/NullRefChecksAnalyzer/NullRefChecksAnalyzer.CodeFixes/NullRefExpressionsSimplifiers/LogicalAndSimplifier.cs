using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsSimplifiers
{
    public class LogicalAndSimplifier : BaseSimplifier
    {
        public SyntaxNode ParentExpression { get; set; }

        public LogicalAndSimplifier(SyntaxNode expression) : base(expression)
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

            while (ParentExpression?.Kind() != SyntaxKind.LogicalAndExpression)
            {
                ParentExpression = ParentExpression?.Parent;
            }

            if ((Expression?.Kind() is SyntaxKind.EqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 == 0) ||
                (Expression?.Kind() is SyntaxKind.NotEqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 != 0))
            {
                ResultExpression = ReplaceWithFalseExpression();
            }
            else
            {
                var secondExpression = GetSecondInOrExpression();
                if (secondExpression is null)
                {
                    return false;
                }

                ResultExpression = secondExpression;
            }

            return Expression != ResultExpression;
        }

        private SyntaxNode GetSecondInOrExpression() => ResultExpression?.Parent?.DescendantNodes().OfType<ExpressionSyntax>()
            .FirstOrDefault(node => node.Parent == ResultExpression.Parent && node != ResultExpression);

        private SyntaxNode ReplaceWithFalseExpression() => ParentExpression?.ReplaceNode(ParentExpression,
            SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
    }
}