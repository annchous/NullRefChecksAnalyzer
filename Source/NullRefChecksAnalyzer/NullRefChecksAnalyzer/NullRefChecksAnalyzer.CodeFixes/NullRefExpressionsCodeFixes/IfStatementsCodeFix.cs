using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class IfStatementsCodeFix : BaseCodeFix
    {
        public IfStatementsCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            if (expression.IsIfStatementParent())
            {
                var ifStatement = expression?.Parent;
                if (ifStatement is null)
                {
                    return Document;
                }

                NewRoot = OldRoot?.RemoveNode(ifStatement, SyntaxRemoveOptions.KeepNoTrivia);
            }
            else if (expression.IsLogicalOrParent() || expression.IsLogicalAndParent())
            {
                if (expression?.Kind() is SyntaxKind.EqualsExpression)
                {
                    var secondExpression = expression.Parent?.DescendantNodes().OfType<ExpressionSyntax>()
                        .FirstOrDefault(node => node.Parent == expression.Parent && node != expression);
                    if (secondExpression is null)
                    {
                        return Document;
                    }

                    NewRoot = OldRoot?.ReplaceNode(expression.Parent, secondExpression);
                }
                else if (expression?.Kind() is SyntaxKind.NotEqualsExpression)
                {
                    NewRoot = OldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
                }
            }
            else if (expression.IsLogicalNotParent())
            {
                var parent = expression?.Ancestors().FirstOrDefault(node =>
                    node.Kind() is SyntaxKind.LogicalOrExpression || node.Kind() is SyntaxKind.LogicalAndExpression ||
                    node.Kind() is SyntaxKind.IfStatement);

                if (parent is null)
                {
                    return Document;
                }

                if (expression.Kind() is SyntaxKind.EqualsExpression)
                {
                    var parentExpression = expression.Ancestors().FirstOrDefault(node => node.Kind() is SyntaxKind.LogicalNotExpression);
                    var secondExpression = parentExpression?.Parent?.DescendantNodes().OfType<ExpressionSyntax>()
                        .FirstOrDefault(node => node.Parent == parentExpression.Parent && node != parentExpression);

                    if (secondExpression is null)
                    {
                        return Document;
                    }

                    NewRoot = OldRoot?.ReplaceNode(parent, secondExpression);
                }
                else if (expression.Kind() is SyntaxKind.NotEqualsExpression)
                {
                    var secondExpression = parent?.DescendantNodes().OfType<ExpressionSyntax>()
                        .FirstOrDefault(node => node.Parent == expression.Parent && node != expression);

                    if (expression.Parent is null || secondExpression is null)
                    {
                        return Document;
                    }

                    NewRoot = OldRoot?.ReplaceNode(expression.Parent, secondExpression);
                }
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}