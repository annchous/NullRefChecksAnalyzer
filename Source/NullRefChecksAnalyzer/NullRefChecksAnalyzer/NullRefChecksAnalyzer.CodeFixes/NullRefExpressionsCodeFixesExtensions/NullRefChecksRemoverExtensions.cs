﻿using Microsoft.CodeAnalysis;
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
            node?.Parent?.Kind() is SyntaxKind.ParenthesizedExpression &&
            node?.Parent?.Parent?.Kind() is SyntaxKind.LogicalNotExpression;

        public static bool IsConditionalAccessParent(this SyntaxNode node) =>
            node?.Parent is ConditionalAccessExpressionSyntax;
    }
}