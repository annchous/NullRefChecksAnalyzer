using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;
using NullRefChecksAnalyzer.NullRefExpressionsSimplifiers;

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
            else if (expression.AncestorsAndSelf().Any(node => node.Kind() is SyntaxKind.LogicalOrExpression))
            {
                var orSimplifier = new LogicalOrSimplifier(expression);
                if (orSimplifier.Simplify())
                {
                    NewRoot = OldRoot?.ReplaceNode(orSimplifier.ParentExpression, orSimplifier.ResultExpression.NormalizeWhitespace());
                }
            }
            else if (expression.AncestorsAndSelf().Any(node => node.Kind() is SyntaxKind.LogicalAndExpression))
            {
                var andSimplifier = new LogicalAndSimplifier(expression);
                if (andSimplifier.Simplify())
                {
                    NewRoot = OldRoot?.ReplaceNode(andSimplifier.ParentExpression, andSimplifier.ResultExpression.NormalizeWhitespace());
                }
            }
            else if (expression.IsLogicalNotParent() || expression.IsParenthesizedParent())
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