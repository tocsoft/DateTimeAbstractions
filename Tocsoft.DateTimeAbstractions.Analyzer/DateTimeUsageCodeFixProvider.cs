using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;

namespace Tocsoft.DateTimeAbstractions.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DateTimeUsageCodeFixProvider)), Shared]
    public class DateTimeUsageCodeFixProvider : CodeFixProvider
    {
        private const string title = "Replace DateTime with Clock";

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
            var diagnostic = context.Diagnostics.First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceWithCallToClock(context, c),
                    equivalenceKey: title),
                diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> ReplaceWithCallToClock(CodeFixContext context, CancellationToken cancellationToken)
        {
            var document = context.Document;
            
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // this is us accessing the property on datetime i.e. the call to 'DateTime.Now'

            root = await ReplaceMemberCall(context, root).ConfigureAwait(false);
            root = ApplyUsings(root);

            return document.WithSyntaxRoot(root);
        }

        private static async Task<SyntaxNode> ReplaceMemberCall(CodeFixContext context, SyntaxNode root)
        {
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var memberAccess =
                root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<MemberAccessExpressionSyntax>().First();

            var operation = (IPropertyReferenceOperation)model.GetOperation(memberAccess);
            var property = operation.Property.Name;

            var expression = SyntaxFactory.MemberAccessExpression(
                                                     SyntaxKind.SimpleMemberAccessExpression,
                                                     SyntaxFactory.MemberAccessExpression(
                                                         SyntaxKind.SimpleMemberAccessExpression,
                                                         SyntaxFactory.MemberAccessExpression(
                                                             SyntaxKind.SimpleMemberAccessExpression,
                                                             SyntaxFactory.IdentifierName("Tocsoft"),
                                                             SyntaxFactory.IdentifierName("DateTimeAbstractions")),
                                                         SyntaxFactory.IdentifierName("Clock"))
                                                         .WithAdditionalAnnotations(Simplifier.Annotation),
                                                     SyntaxFactory.IdentifierName(property));

            root = root.ReplaceNode(memberAccess, expression);
            return root;
        }

        private static SyntaxNode ApplyUsings(SyntaxNode root)
        {
            var compilation =
                root as CompilationUnitSyntax;

            var abstractionsUsingStatement =
          SyntaxFactory.UsingDirective(
              SyntaxFactory.QualifiedName(
                  SyntaxFactory.IdentifierName("Tocsoft"),
                  SyntaxFactory.IdentifierName("DateTimeAbstractions")));

            if (null == compilation)
            {
                root =
                root.InsertNodesBefore(
                    root.ChildNodes().First(),
                    new[] { abstractionsUsingStatement });
            }
            else if (compilation.Usings.All(u => u.Name.GetText().ToString() != "Tocsoft.DateTimeAbstractions"))
            {
                root =
                    root.InsertNodesAfter(compilation.Usings.Last(),
                    new[] { abstractionsUsingStatement });
            }

            return root;
        }
    }
}

