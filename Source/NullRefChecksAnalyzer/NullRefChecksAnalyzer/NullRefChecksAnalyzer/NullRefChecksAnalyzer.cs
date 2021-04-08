using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using NullRefChecksAnalyzer.NullRefExpressionsAnalyzers;
using NullRefChecksAnalyzer.NullRefExpressionsAnalyzersExtensions;

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
            if (parameters.Any()) AnalyzeNodeParameters(parameters, methodDeclaration, context);
        }

        private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax) context.Node;
            var parameters = constructorDeclaration.GetReferenceTypeParameters(context.SemanticModel).ToList();
            if (parameters.Any()) AnalyzeNodeParameters(parameters, constructorDeclaration, context);
        }

        private static void AnalyzeNodeParameters(List<ParameterSyntax> parameters, SyntaxNode node, SyntaxNodeAnalysisContext context)
        {
            var binaryExpressions = node.GetExpressions<BinaryExpressionSyntax>().ToList();
            var caseExpressions = node.GetExpressions<SwitchStatementSyntax>().ToList();
            var switchExpressions = node.GetExpressions<SwitchExpressionSyntax>().ToList();
            var conditionalAccessExpressions = node.GetExpressions<ConditionalAccessExpressionSyntax>().ToList();
            var patternExpressions = node.GetExpressions<PatternSyntax>().FilterByPatterns().ToList();
            var coalesceAssignmentExpressions = node.GetExpressions<AssignmentExpressionSyntax>().FilterByCoalesceAssignment().ToList();

            var equalityExpressions = binaryExpressions.FilterForEqualityCheck().ToList();
            var coalesceExpressions = binaryExpressions.FilterByCoalesce().ToList();

            equalityExpressions.ForEach(equalityExpression => ReportForNullRefChecks(equalityExpression, parameters, context));
            coalesceExpressions.ForEach(coalesceExpression => ReportForNullRefChecks(coalesceExpression, parameters, context));
            caseExpressions.ForEach(caseExpression => ReportForNullRefChecks(caseExpression, parameters, context));
            switchExpressions.ForEach(switchExpression => ReportForNullRefChecks(switchExpression, parameters, context));
            conditionalAccessExpressions.ForEach(conditionalAccessExpression => ReportForNullRefChecks(conditionalAccessExpression, parameters, context));
            patternExpressions.ForEach(patternExpression => ReportForNullRefChecks(patternExpression, parameters, context));
            coalesceAssignmentExpressions.ForEach(coalesceAssignmentExpression => ReportForNullRefChecks(coalesceAssignmentExpression, parameters, context));
        }

        private static void ReportForNullRefChecks(SyntaxNode expression, List<ParameterSyntax> parameters, SyntaxNodeAnalysisContext context)
        {
            if (!expression.GetParentIdentifierName().IsParameterIdentifier(context.SemanticModel, parameters))
            {
                return;
            }

            if (expression is BinaryExpressionSyntax binaryExpression)
            {
                new BinaryExpressionsAnalyzer(binaryExpression).ReportForNullRefChecks(context, Rule);
            }

            if (expression is SwitchStatementSyntax switchStatement)
            {
                new SwitchStatementsAnalyzer(switchStatement).ReportForNullRefChecks(context, Rule);
            }

            if (expression is SwitchExpressionSyntax switchExpression)
            {
                new SwitchExpressionsAnalyzer(switchExpression).ReportForNullRefChecks(context, Rule);
            }

            if (expression is ConditionalAccessExpressionSyntax conditionalAccessExpression)
            {
                new ConditionalAccessExpressionsAnalyzer(conditionalAccessExpression).ReportForNullRefChecks(context, Rule);
            }

            if (expression is AssignmentExpressionSyntax assignmentExpression)
            {
                new AssignmentExpressionsAnalyzer(assignmentExpression).ReportForNullRefChecks(context, Rule);
            }

            if (expression is PatternSyntax patternExpression)
            {
                new PatternExpressionsAnalyzer(patternExpression).ReportForNullRefChecks(context, Rule);
            }
        }
    }
}
