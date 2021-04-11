using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsSimplifiers
{
    public class IsPatternSimplifier : BaseSimplifier
    {
        public SyntaxNode ParentExpression { get; set; }

        public IsPatternSimplifier(SyntaxNode expression) : base(expression)
        {
            ParentExpression = expression;
        }

        public override bool Simplify()
        {
            while (ParentExpression?.Kind() != SyntaxKind.IsPatternExpression)
            {
                ParentExpression = ParentExpression?.Parent;

                if (ParentExpression is null)
                {
                    return false;
                }
            }

            if (Expression == ParentExpression && !ContainsNotPatterns() && !ContainsOrAndPatterns())
            {
                ResultExpression = ParentExpression.ReplaceNode(ParentExpression,
                    SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
            }
            else if (Expression == ParentExpression && ContainsNotPatterns() && !ContainsOrAndPatterns())
            {
                ResultExpression = ParentExpression.ReplaceNode(ParentExpression,
                    SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
            }
            else
            {
                return false;
            }

            return Expression != ResultExpression;
        }

        private bool ContainsNotPatterns()
        {
            var notPatterns = ParentExpression.DescendantNodes().OfType<PatternSyntax>()
                .Where(pattern => pattern.Kind() is SyntaxKind.NotPattern).ToList();

            return notPatterns.Count > 0;
        }

        private bool ContainsOrAndPatterns()
        {
            var notPatterns = ParentExpression.DescendantNodes().OfType<PatternSyntax>()
                .Where(pattern => pattern.Kind() is SyntaxKind.OrPattern || pattern.Kind() is SyntaxKind.AndPattern).ToList();

            return notPatterns.Count > 0;
        }
    }
}