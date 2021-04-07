using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace NullRefChecksAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NullRefChecksAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NullRefChecksAnalyzer";
        
        private static readonly LocalizableString Title = "Redundant null reference check";
        private static readonly LocalizableString MessageFormat = "Redundant null reference check";
        private static readonly LocalizableString Description = "Redundant null reference check";
        private const string Category = "Syntax";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;
            var parameters = methodDeclaration.GetReferenceTypeParameters(context.SemanticModel).ToList();
            if (parameters.Any()) AnalyzeNodeParameters(parameters, context);
        }

        private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
        {
            var сonstructorDeclaration = (ConstructorDeclarationSyntax)context.Node;
            var parameters = сonstructorDeclaration.GetReferenceTypeParameters(context.SemanticModel).ToList();
            if (parameters.Any()) AnalyzeNodeParameters(parameters, context);
        }

        private static IEnumerable<T> GetExpressions<T>(SemanticModel semanticModel,
            CancellationToken cancellationToken) =>
            semanticModel.SyntaxTree.GetRoot(cancellationToken).DescendantNodes().OfType<T>();

        private static void AnalyzeNodeParameters(List<ParameterSyntax> parameters, SyntaxNodeAnalysisContext context)
        {
            var binaryExpressions = GetExpressions<BinaryExpressionSyntax>(context.SemanticModel, context.CancellationToken)
                .FilterForEqualityCheck()
                .ToList();
            //var isPatternExpressions = GetExpressions<IsPatternExpressionSyntax>(context.SemanticModel, context.CancellationToken)
            //    .ToList();
            var caseExpressions = GetExpressions<SwitchStatementSyntax>(context.SemanticModel, context.CancellationToken)
                .ToList();
            var switchExpressions = GetExpressions<SwitchExpressionSyntax>(context.SemanticModel, context.CancellationToken)
                .ToList();
            var conditionalAccessExpressions = GetExpressions<ConditionalAccessExpressionSyntax>(context.SemanticModel, context.CancellationToken)
                .ToList();
            //var patternExpressions = GetExpressions<PatternSyntax>(context.SemanticModel, context.CancellationToken)
            //    .FilterByPatterns()
            //    .ToList();
            var coalesceExpressions = GetExpressions<CSharpSyntaxNode>(context.SemanticModel, context.CancellationToken)
                .FilterByCoalesce()
                .ToList();

            binaryExpressions.ForEach(binaryExpression => ReportForNullRefChecks(binaryExpression, parameters, context));
            //isPatternExpressions.ForEach(isPatternExpression => ReportForNullRefChecks(isPatternExpression, parameters, context));
            caseExpressions.ForEach(caseExpression => ReportForNullRefChecks(caseExpression, parameters, context));
            switchExpressions.ForEach(switchExpression => ReportForNullRefChecks(switchExpression, parameters, context));
            conditionalAccessExpressions.ForEach(conditionalAccessExpression => ReportForNullRefChecks(conditionalAccessExpression, parameters, context));
            //patternExpressions.ForEach(patternExpression => ReportForNullRefChecks(patternExpression, parameters, context));
            coalesceExpressions.ForEach(coalesceExpression => ReportForNullRefChecks(coalesceExpression, parameters, context));
        }

        private static void ReportForNullRefChecks(CSharpSyntaxNode expression, List<ParameterSyntax> parameters, SyntaxNodeAnalysisContext context)
        {
            if (!expression.GetParentIdentifierName().IsParameterIdentifier(context.SemanticModel, parameters))
            {
                return;
            }

            Location location = Location.None;

            if (expression is BinaryExpressionSyntax binaryExpression)
            {
                if (expression.Kind() is SyntaxKind.CoalesceExpression)
                {
                    location = expression.DescendantTokens()
                        .FirstOrDefault(token => token.Kind() is SyntaxKind.QuestionQuestionToken).GetLocation();
                }
                else
                {
                    if (!binaryExpression.ContainsNullOrDefault())
                    {
                        return;
                    }

                    location = binaryExpression.GetLocation();
                }
            }

            if (expression is SwitchStatementSyntax switchStatement)
            {
                if (!switchStatement.ContainsNullOrDefault())
                {
                    return;
                }

                location = switchStatement.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .FirstOrDefault(literalExpression => literalExpression.IsNullOrDefault())?.Parent?.GetLocation();
            }

            if (expression is SwitchExpressionSyntax switchExpression)
            {
                if (!switchExpression.ContainsNullOrDefault())
                {
                    return;
                }

                location = switchExpression.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .FirstOrDefault(literalExpression => literalExpression.IsNullOrDefault())?.Parent?.Parent?.GetLocation();
            }

            if (expression is ConditionalAccessExpressionSyntax conditionalAccessExpression)
            {
                location = conditionalAccessExpression.DescendantTokens()
                    .FirstOrDefault(token => token.Kind() is SyntaxKind.QuestionToken).GetLocation();
            }

            if (expression.Kind() is SyntaxKind.CoalesceAssignmentExpression)
            {
                location = expression.DescendantTokens()
                    .FirstOrDefault(token => token.Kind() is SyntaxKind.QuestionQuestionEqualsToken).GetLocation();
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }
    }
}
