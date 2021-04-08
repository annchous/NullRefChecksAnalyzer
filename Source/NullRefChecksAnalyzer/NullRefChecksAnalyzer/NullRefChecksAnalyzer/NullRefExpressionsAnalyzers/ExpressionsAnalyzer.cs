using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class ExpressionsAnalyzer
    {
        protected Location Location { get; set; }

        public ExpressionsAnalyzer()
        {
            Location = Location.None;
        }

        public virtual void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor) { }
    }
}