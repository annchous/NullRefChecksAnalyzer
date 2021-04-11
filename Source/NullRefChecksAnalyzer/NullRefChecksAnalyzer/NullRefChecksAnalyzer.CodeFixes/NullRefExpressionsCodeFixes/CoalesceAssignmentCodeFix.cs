using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class CoalesceAssignmentCodeFix : BaseCodeFix
    {
        public CoalesceAssignmentCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            var assignmentExpression = expression as AssignmentExpressionSyntax;
            if (assignmentExpression.IsNoParent())
            {
                var parentExpression = assignmentExpression?.Parent;
                if (parentExpression is null)
                {
                    return Document;
                }

                NewRoot = OldRoot?.RemoveNode(parentExpression, SyntaxRemoveOptions.KeepNoTrivia);
            }
            else
            {
                var leftPart = assignmentExpression?.Left.ToFullString().Trim();
                if (leftPart is null)
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