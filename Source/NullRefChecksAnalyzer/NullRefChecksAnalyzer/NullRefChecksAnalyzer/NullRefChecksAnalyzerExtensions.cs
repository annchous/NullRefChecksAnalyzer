using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace NullRefChecksAnalyzer
{
    public static class NullRefChecksAnalyzerExtensions
    {
        public static IEnumerable<ParameterSyntax> WhereIsReferenceTypeParameter(this SeparatedSyntaxList<ParameterSyntax> parameters, SemanticModel semanticModel) =>
            parameters.Where(parameter => semanticModel.GetDeclaredSymbol(parameter).Type != null)
                .Where(parameter => semanticModel.GetDeclaredSymbol(parameter).Type.IsReferenceType);

        public static IEnumerable<ParameterSyntax> GetReferenceTypeParameters(this BaseMethodDeclarationSyntax node,
            SemanticModel semanticModel) => node.ParameterList.Parameters.WhereIsReferenceTypeParameter(semanticModel);

        public static IEnumerable<ParameterSyntax> GetReferenceTypeParameters(this LocalFunctionStatementSyntax node,
            SemanticModel semanticModel) => node.ParameterList.Parameters.WhereIsReferenceTypeParameter(semanticModel);

        public static bool IsParameterIdentifier(this IdentifierNameSyntax identifierName, SemanticModel semanticModel,
            IEnumerable<ParameterSyntax> parameters) => parameters.Any(parameter =>
            ModelExtensions.GetDeclaredSymbol(semanticModel, parameter).Name.ToString() == identifierName.Identifier.ToString());

        public static bool IsNullOrDefault(this LiteralExpressionSyntax literalExpression) =>
            (literalExpression.Kind() is SyntaxKind.NullLiteralExpression ||
             literalExpression.Kind() is SyntaxKind.DefaultLiteralExpression);

        public static Location GetIsPatternExpressionLocation(this IsPatternExpressionSyntax patternExpression) =>
            patternExpression.Pattern is RecursivePatternSyntax
                ? patternExpression.GetLocation()
                : patternExpression.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .FirstOrDefault(literalExpression => literalExpression.IsNullOrDefault())?.Parent.GetLocation();
    }
}