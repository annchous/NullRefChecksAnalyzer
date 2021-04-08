using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NullRefChecksAnalyzer.NullRefExpressionsAnalyzersExtensions;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzers
{
    public class InvocationExpressionsAnalyzer : ExpressionsAnalyzer
    {
        private readonly InvocationExpressionSyntax _invocationExpression;
        private readonly List<ParameterSyntax> _parameters;

        public InvocationExpressionsAnalyzer(InvocationExpressionSyntax invocationExpression, List<ParameterSyntax> parameters)
        {
            _invocationExpression = invocationExpression;
            _parameters = parameters;
        }

        public override void ReportForNullRefChecks(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            if (_invocationExpression.DescendantNodes().OfType<IdentifierNameSyntax>()
                .Any(identifierName => identifierName.Identifier.ToString() == "Equals"))
            {
                if (!_invocationExpression.ArgumentList.ContainsNullOrDefault())
                {
                    return;
                }

                var identifierName = _invocationExpression.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .FirstOrDefault(name => name.Identifier.ToString() != "Equals");

                if (!identifierName.IsParameterIdentifier(context.SemanticModel, _parameters))
                {
                    return;
                }

                Location = _invocationExpression.GetLocation();
            }

            if (_invocationExpression.DescendantNodes().OfType<IdentifierNameSyntax>()
                .Any(identifierName => identifierName.Identifier.ToString() == "ReferenceEquals"))
            {
                if (!_invocationExpression.ArgumentList.ContainsNullOrDefault())
                {
                    return;
                }

                var identifierNames =
                    _invocationExpression.ArgumentList.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();

                if (!identifierNames.Any(name => name.IsParameterIdentifier(context.SemanticModel, _parameters)))
                {
                    return;
                }

                Location = _invocationExpression.GetLocation();
            }

            if (Location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Location));
            }
        }
    }
}