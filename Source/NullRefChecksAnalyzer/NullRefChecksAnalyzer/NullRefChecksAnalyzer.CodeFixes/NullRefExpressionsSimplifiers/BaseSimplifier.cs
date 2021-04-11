using Microsoft.CodeAnalysis;

namespace NullRefChecksAnalyzer.NullRefExpressionsSimplifiers
{
    public class BaseSimplifier
    {
        public SyntaxNode Expression { get; }
        public SyntaxNode ResultExpression { get; set; }

        public BaseSimplifier(SyntaxNode expression)
        {
            Expression = expression;
            ResultExpression = expression;
        }

        public virtual bool Simplify()
        {
            return false;
        }
    }
}