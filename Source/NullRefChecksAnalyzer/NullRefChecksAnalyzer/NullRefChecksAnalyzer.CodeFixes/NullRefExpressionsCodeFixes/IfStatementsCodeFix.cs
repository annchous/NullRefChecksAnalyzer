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
            if (expression.IsLogicalOrParent())
            {
                var orSimplifier = new LogicalOrSimplifier(expression);
                if (orSimplifier.Simplify())
                {
                    NewRoot = OldRoot?.ReplaceNode(orSimplifier.ParentExpression, orSimplifier.ResultExpression.NormalizeWhitespace());
                }
            }
            else if (expression.IsLogicalAndParent())
            {
                var andSimplifier = new LogicalAndSimplifier(expression);
                if (andSimplifier.Simplify())
                {
                    NewRoot = OldRoot?.ReplaceNode(andSimplifier.ParentExpression, andSimplifier.ResultExpression.NormalizeWhitespace());
                }
            }
            else if (expression.IsLogicalNotParent() || expression.IsParenthesizedParent())
            {
                var notSimplifier = new LogicalNotSimplifier(expression);
                if (notSimplifier.Simplify())
                {
                    if ((expression.Kind() is SyntaxKind.EqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 == 0) ||
                        (expression.Kind() is SyntaxKind.NotEqualsExpression && notSimplifier.LogicalNotExpressionsCount % 2 != 0))
                    {
                        var secondExpression = notSimplifier.ResultExpression?.Parent?.DescendantNodes().OfType<ExpressionSyntax>()
                            .FirstOrDefault(node => node.Parent == notSimplifier.ResultExpression.Parent && node != notSimplifier.ResultExpression);

                        if (secondExpression is null)
                        {
                            NewRoot = OldRoot?.RemoveNode(notSimplifier.ResultExpression.Parent, SyntaxRemoveOptions.KeepNoTrivia);
                            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
                        }

                        NewRoot = OldRoot?.ReplaceNode(notSimplifier.ResultExpression.Parent, secondExpression);
                    }
                    else
                    {
                        NewRoot = OldRoot?.ReplaceNode(notSimplifier.ResultExpression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression).NormalizeWhitespace());
                    }
                }
            }
            else if (expression.IsPattern())
            {
                var isPatternSimplifier = new IsPatternSimplifier(expression);
                if (isPatternSimplifier.Simplify())
                {
                    NewRoot = OldRoot?.ReplaceNode(isPatternSimplifier.ParentExpression,
                        isPatternSimplifier.ResultExpression.NormalizeWhitespace());
                }
            }
            else if (expression.IsConditionalAccessParent())
            {
                return new ConditionalAccessCodeFix(Document, OldRoot, NewRoot).GetFixedDocument(expression);
            }
            else if (expression.IsInvocation())
            {
                return new InvocationCodeFix(Document, OldRoot, NewRoot).GetFixedDocument(expression);
            }
            else if (expression.IsIfStatementParent())
            {
                var ifStatement = expression?.Parent;

                if (ifStatement is null)
                {
                    return Document;
                }

                if (expression.Kind() is SyntaxKind.EqualsExpression)
                {
                    NewRoot = OldRoot?.RemoveNode(ifStatement, SyntaxRemoveOptions.KeepNoTrivia);
                }
                else if (expression.Kind() is SyntaxKind.NotEqualsExpression)
                {
                    NewRoot = OldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
                }
            }

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}