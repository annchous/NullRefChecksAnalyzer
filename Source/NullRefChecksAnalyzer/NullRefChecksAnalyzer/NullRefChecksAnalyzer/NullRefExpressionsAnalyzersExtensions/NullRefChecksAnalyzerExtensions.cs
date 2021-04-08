using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NullRefChecksAnalyzer.NullRefExpressionsAnalyzersExtensions
{
    public static class NullRefChecksAnalyzerExtensions
    {
        public static IEnumerable<T> GetExpressions<T>(this SyntaxNode node) => node.DescendantNodes().OfType<T>();

        public static IEnumerable<ParameterSyntax> WhereIsReferenceTypeParameter(this SeparatedSyntaxList<ParameterSyntax> parameters, SemanticModel semanticModel) =>
            parameters.Where(parameter => semanticModel.GetDeclaredSymbol(parameter)?.Type != null)
                .Where(parameter => semanticModel.GetDeclaredSymbol(parameter).Type.IsReferenceType);

        public static IEnumerable<ParameterSyntax> GetReferenceTypeParameters(this BaseMethodDeclarationSyntax node,
            SemanticModel semanticModel) => node.ParameterList.Parameters.WhereIsReferenceTypeParameter(semanticModel);
        
        public static bool IsParameterIdentifier(this IdentifierNameSyntax identifierName, SemanticModel semanticModel,
            IEnumerable<ParameterSyntax> parameters) => parameters.Any(parameter =>
            ModelExtensions.GetDeclaredSymbol(semanticModel, parameter)?.Name.ToString() == identifierName.Identifier.ToString());

        public static bool IsNullOrDefault(this LiteralExpressionSyntax literalExpression) =>
            (literalExpression.Kind() is SyntaxKind.NullLiteralExpression ||
             literalExpression.Kind() is SyntaxKind.DefaultLiteralExpression);

        public static bool ContainsNullOrDefault(this SyntaxNode expression) => !(expression.DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .FirstOrDefault(literalExpression => literalExpression.IsNullOrDefault()) is null);
    }
}