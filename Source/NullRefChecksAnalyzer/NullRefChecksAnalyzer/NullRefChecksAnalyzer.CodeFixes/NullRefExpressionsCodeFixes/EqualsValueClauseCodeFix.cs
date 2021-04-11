using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;

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
            else if (expression.Kind() is SyntaxKind.NotEqualsExpression)
            {
                NewRoot = OldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
            }
            else if (expression.Kind() is SyntaxKind.CoalesceAssignmentExpression)
            {
                return new CoalesceAssignmentCodeFix(Document, OldRoot, null).GetFixedDocument(expression);
            }
            else if (expression.IsInvocation())
            {
                return new InvocationCodeFix(Document, OldRoot, NewRoot).GetFixedDocument(expression);
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}