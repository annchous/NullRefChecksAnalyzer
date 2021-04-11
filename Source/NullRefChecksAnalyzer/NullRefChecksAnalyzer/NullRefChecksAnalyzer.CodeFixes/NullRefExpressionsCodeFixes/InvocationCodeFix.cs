using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;
using NullRefChecksAnalyzer.NullRefExpressionsSimplifiers;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class InvocationCodeFix : BaseCodeFix
    {
        public InvocationCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            var simplifier = new LogicalNotSimplifier(expression);
            if (simplifier.Simplify())
            {
                NewRoot = OldRoot?.ReplaceNode(simplifier.ResultExpression,
                    SyntaxFactory.LiteralExpression(simplifier.LogicalNotExpressionsCount % 2 == 0
                        ? SyntaxKind.FalseLiteralExpression
                        : SyntaxKind.TrueLiteralExpression));
            }
            else if (expression.IsNoParent())
            {
                var parentSyntax = expression.Parent;
                if (parentSyntax is null)
                {
                    return Document;
                }

                NewRoot = OldRoot?.RemoveNode(parentSyntax, SyntaxRemoveOptions.KeepNoTrivia);
            }
            else
            {
                NewRoot = OldRoot?.ReplaceNode(expression,
                    SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}