// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Tocsoft.DateTimeAbstractions.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DateTimeUsageCodeFixProvider))]
    [Shared]
    public class DateTimeUsageCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Replace with Clock";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DateTimeUsageAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.Where(x => x.Id == DateTimeUsageAnalyzer.DiagnosticId).FirstOrDefault();

            if (diagnostic != null)
            {
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => this.ReplaceWithCallToClock(context, c),
                    equivalenceKey: Title),
                diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> ReplaceWithCallToClock(CodeFixContext context, CancellationToken cancellationToken)
        {
            Document document = context.Document;

            SyntaxNode root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // this is us accessing the property on datetime i.e. the call to 'DateTime.Now'
            root = await ReplaceMemberCall(context, root).ConfigureAwait(false);
            root = ApplyUsings(root);

            return document.WithSyntaxRoot(root);
        }

        private static async Task<SyntaxNode> ReplaceMemberCall(CodeFixContext context, SyntaxNode root)
        {
            SemanticModel model = await context.Document.GetSemanticModelAsync(context.CancellationToken);

            Diagnostic diagnostic = context.Diagnostics.Where(x => x.Id == DateTimeUsageAnalyzer.DiagnosticId).FirstOrDefault();
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);
            MemberAccessExpressionSyntax memberAccess = node.DescendantNodesAndSelf(x => !(x is MemberAccessExpressionSyntax))
                                                            .OfType<MemberAccessExpressionSyntax>()
                                                            .First();

            string propertyName = memberAccess.Name.ToString();
            SyntaxTriviaList trivia = node.GetTrailingTrivia();

            MemberAccessExpressionSyntax expression = SyntaxFactory.MemberAccessExpression(
                                                     SyntaxKind.SimpleMemberAccessExpression,
                                                     SyntaxFactory.MemberAccessExpression(
                                                         SyntaxKind.SimpleMemberAccessExpression,
                                                         SyntaxFactory.MemberAccessExpression(
                                                             SyntaxKind.SimpleMemberAccessExpression,
                                                             SyntaxFactory.IdentifierName("Tocsoft"),
                                                             SyntaxFactory.IdentifierName("DateTimeAbstractions")),
                                                         SyntaxFactory.IdentifierName("Clock"))
                                                         .WithAdditionalAnnotations(Simplifier.Annotation),
                                                     SyntaxFactory.IdentifierName(propertyName))
                                                     .WithTrailingTrivia(trivia);
            root = root.ReplaceNode(memberAccess, expression);
            return root;
        }

        private static SyntaxNode ApplyUsings(SyntaxNode root)
        {
            CompilationUnitSyntax compilation =
                root as CompilationUnitSyntax;
            if (compilation == null)
            {
                return root;
            }

            if (compilation.Usings.Any(x => x.Name.GetText().ToString() == "Tocsoft.DateTimeAbstractions"))
            {
                return root;
            }

            UsingDirectiveSyntax abstractionsUsingStatement =
              SyntaxFactory.UsingDirective(
                  SyntaxFactory.QualifiedName(
                      SyntaxFactory.IdentifierName("Tocsoft"),
                      SyntaxFactory.IdentifierName("DateTimeAbstractions")));

            return compilation.AddUsings(abstractionsUsingStatement);
        }
    }
}