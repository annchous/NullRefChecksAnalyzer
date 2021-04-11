using System;
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
            else
            {
                var leftPart = expression.ToFullString().Split(new string[] { "??" }, StringSplitOptions.None)[0].Trim();
                if (string.IsNullOrEmpty(leftPart))
                {
                    return Document;
                }

                var newExpression = SyntaxFactory.ParseExpression(leftPart);
                NewRoot = OldRoot?.ReplaceNode(expression, newExpression);
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}