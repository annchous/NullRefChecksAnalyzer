using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class CoalesceCodeFix : BaseCodeFix
    {
        public CoalesceCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            if (expression.IsNoParent())
            {
                if (expression?.Parent is null)
                {
                    return Document;
                }

                NewRoot = OldRoot?.RemoveNode(expression.Parent, SyntaxRemoveOptions.KeepNoTrivia);
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}