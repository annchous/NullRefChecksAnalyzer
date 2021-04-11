using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public class SwitchStatementCodeFix : BaseCodeFix
    {
        public SwitchStatementCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot) : base(document, oldRoot, newRoot)
        {
        }

        public override Document GetFixedDocument(SyntaxNode expression)
        {
            var caseSwitchLabel = expression as CaseSwitchLabelSyntax;
            var switchSection = caseSwitchLabel?.Parent;
            if (switchSection is null)
            {
                return Document;
            }

            NewRoot = OldRoot?.RemoveNode(switchSection, SyntaxRemoveOptions.KeepNoTrivia);

            return NewRoot == null ? Document : Document.WithSyntaxRoot(NewRoot);
        }
    }
}