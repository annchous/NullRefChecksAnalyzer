using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class ConditionalAccessCodeFix : BaseCodeFix
    {
        public ConditionalAccessCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            var conditionalAccessExpression = expression as ConditionalAccessExpressionSyntax;
            var leftExpression = conditionalAccessExpression?.Expression.ToFullString();
            var rightExpression = conditionalAccessExpression?.WhenNotNull.ToFullString();

            NewRoot = OldRoot?.ReplaceNode(conditionalAccessExpression,
                SyntaxFactory.ParseExpression(leftExpression + rightExpression));

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}