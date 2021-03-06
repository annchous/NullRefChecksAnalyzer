using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace NullRefChecksAnalyzer.NullRefExpressionsCodeFixes
{
    public abstract class BaseCodeFix
    {
        protected readonly Document Document;
        protected readonly SyntaxNode OldRoot;
        protected SyntaxNode NewRoot;

        protected BaseCodeFix(Document document, SyntaxNode oldRoot, SyntaxNode newRoot)
        {
            Document = document;
            OldRoot = oldRoot;
            NewRoot = newRoot;
        }

        public virtual Document GetFixedDocument(SyntaxNode expression)
        {
            return null;
        }
    }
}