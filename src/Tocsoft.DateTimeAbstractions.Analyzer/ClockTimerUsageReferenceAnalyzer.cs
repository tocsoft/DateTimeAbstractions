// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Tocsoft.DateTimeAbstractions.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClockTimerUsageReferenceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TOCSOFT0003";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(
                nameof(Resources.ClockTimerTitle),
                Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString ReferenceMessageFormat = new LocalizableResourceString(
            nameof(Resources.ClockTimerTypeAnalyzer_ReferenceMessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString ReferenceMessageFormatDirected = new LocalizableResourceString(
            nameof(Resources.ClockTimerTypeAnalyzer_ReferenceMessageFormat_DirectToClockTimer),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.ClockTimerAnalyzerDescription),
            Resources.ResourceManager,
            typeof(Resources));

        private const string Category = "Testability";

        private static DiagnosticDescriptor referenceRule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            ReferenceMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        private static DiagnosticDescriptor referenceRuleDirected = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            ReferenceMessageFormatDirected,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(referenceRule, referenceRuleDirected); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                Compilation compilation = compilationStartContext.Compilation;
                INamedTypeSymbol stopwatchType = compilation.GetTypeByMetadataName("System.Diagnostics.Stopwatch");
                INamedTypeSymbol clockTimerType = compilation.GetTypeByMetadataName("Tocsoft.DateTimeAbstractions.ClockTimer");

                compilationStartContext.RegisterOperationAction(
                    operationContext =>
                    {
                        IVariableDeclarationOperation invocation = (IVariableDeclarationOperation)operationContext.Operation;

                        foreach (var declarator in invocation.Declarators)
                        {
                            if (declarator.Symbol.Type == null || declarator.Symbol.Type != stopwatchType)
                            {
                                return;
                            }
                            var location = (invocation.Syntax as VariableDeclarationSyntax).Type.GetLocation();

                            operationContext.ReportDiagnostic(Diagnostic.Create(
                                      clockTimerType == null ? referenceRule : referenceRuleDirected,
                                      location,
                                      declarator.Symbol.Type.Name,
                                      clockTimerType?.Name));
                        }

                    }, OperationKind.VariableDeclaration);
            });
        }
    }
}
