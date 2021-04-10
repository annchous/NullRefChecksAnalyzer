using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class EqualsValueClauseCodeFix : BaseCodeFix
    {
        public EqualsValueClauseCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            if (expression.Kind() is SyntaxKind.EqualsExpression)
            {
                NewRoot = OldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
            }

            if (expression.Kind() is SyntaxKind.NotEqualsExpression)
            {
                NewRoot = OldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}