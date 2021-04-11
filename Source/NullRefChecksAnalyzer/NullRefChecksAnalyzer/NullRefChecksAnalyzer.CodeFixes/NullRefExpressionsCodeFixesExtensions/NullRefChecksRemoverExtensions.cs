using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions
{
    public static class NullRefChecksRemoverExtensions
    {
        public static bool IsIfStatementParent(this SyntaxNode node) =>
            node?.Parent is IfStatementSyntax;

        public static bool IsLogicalOrParent(this SyntaxNode node) =>
            node?.Parent?.Kind() is SyntaxKind.LogicalOrExpression;

        public static bool IsLogicalAndParent(this SyntaxNode node) =>
            node?.Parent?.Kind() is SyntaxKind.LogicalAndExpression;

        public static bool IsLogicalNotParent(this SyntaxNode node) =>
            node?.Parent?.Kind() is SyntaxKind.LogicalNotExpression;

        public static bool IsParenthesizedParent(this SyntaxNode node) =>
            node?.Parent?.Kind() is SyntaxKind.ParenthesizedExpression;

        public static bool IsConditionalAccessParent(this SyntaxNode node) =>
            node?.Parent is ConditionalAccessExpressionSyntax;

        public static bool IsCoalesceAssignmentParent(this SyntaxNode node) =>
            node?.Parent?.Kind() is SyntaxKind.CoalesceAssignmentExpression;

        public static bool IsEqualsValueClauseParent(this SyntaxNode node) =>
            node?.Parent?.Kind() is SyntaxKind.EqualsValueClause;

        public static bool IsNoParent(this SyntaxNode node) =>
            node?.Parent?.Kind() is SyntaxKind.ExpressionStatement;

        public static bool IsInvocation(this SyntaxNode node) => node.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Any(identifierName => identifierName.Identifier.ToString() == "ReferenceEquals");

        public static bool IsPattern(this SyntaxNode node) => node.Kind() is SyntaxKind.IsPatternExpression;
    }
}