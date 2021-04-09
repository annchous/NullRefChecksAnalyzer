using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NullRefChecksAnalyzer.NullRefExpressionsCodeFixesExtensions;

namespace NullRefChecksAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullRefChecksAnalyzerCodeFixProvider)), Shared]
    public class NullRefChecksAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NullRefChecksAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var expression = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().ToList()
                .FindExpression();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove redundant check",
                    createChangedDocument: c => RemoveRedundantNullRefCheckAsync(context.Document, expression, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private async Task<Document> RemoveRedundantNullRefCheckAsync(Document document, SyntaxNode expression, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = null;

            if (expression?.Parent is IfStatementSyntax ifStatement)
            {
                newRoot = oldRoot?.RemoveNode(ifStatement, SyntaxRemoveOptions.KeepNoTrivia);
            }

            if (expression != null && expression.IsLogicalOrParent())
            {
                if (expression.Kind() is SyntaxKind.EqualsExpression)
                {
                    var secondExpression = expression?.Parent?.DescendantNodes().OfType<ExpressionSyntax>().FirstOrDefault(node => node?.Parent == expression.Parent && node != expression);
                    newRoot = oldRoot?.ReplaceNode(expression.Parent, secondExpression);
                    //newRoot = oldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
                }

                if (expression.Kind() is SyntaxKind.NotEqualsExpression)
                {
                    newRoot = oldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
                }
            }

            if (expression?.Parent is EqualsValueClauseSyntax)
            {
                if (expression.Kind() is SyntaxKind.EqualsExpression)
                {
                    newRoot = oldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
                }

                if (expression.Kind() is SyntaxKind.NotEqualsExpression)
                {
                    newRoot = oldRoot?.ReplaceNode(expression, SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
                }
            }

            return newRoot == null ? document : document.WithSyntaxRoot(newRoot);
        }
    }
}
