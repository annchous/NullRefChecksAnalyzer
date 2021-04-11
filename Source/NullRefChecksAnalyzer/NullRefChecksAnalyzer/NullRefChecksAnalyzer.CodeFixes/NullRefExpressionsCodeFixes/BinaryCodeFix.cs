using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;
using NullRefChecksAnalyzer.NullRefExpressionsSimplifiers;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class BinaryCodeFix : BaseCodeFix
    {
        public BinaryCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            if (expression.IsLogicalNotParent() || expression.IsParenthesizedParent())
            {
                var notSimplifier = new LogicalNotSimplifier(expression);
                if (notSimplifier.Simplify())
                {
                    if ((expression.Kind() is SyntaxKind.EqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 == 0) ||
                        (expression.Kind() is SyntaxKind.NotEqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 != 0))
                    {
                        NewRoot = OldRoot?.ReplaceNode(notSimplifier.ResultExpression, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression).NormalizeWhitespace());
                    }
                    else
                    {
                        NewRoot = OldRoot?.ReplaceNode(notSimplifier.ResultExpression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression).NormalizeWhitespace());
                    }
                }
            }
            else if (expression.Kind() is SyntaxKind.EqualsExpression)
            {
                NewRoot = OldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
            }
            else if (expression.Kind() is SyntaxKind.NotEqualsExpression)
            {
                NewRoot = OldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
            }
            else if (expression is InvocationExpressionSyntax)
            {
                return new InvocationCodeFix(Document, OldRoot, NewRoot).GetFixedDocument(expression);
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}