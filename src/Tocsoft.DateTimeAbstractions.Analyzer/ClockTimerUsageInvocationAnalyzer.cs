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
    public class ClockTimerUsageInvocationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TOCSOFT0003";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(
                nameof(Resources.ClockTimerTitle),
                Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString InvocationMessageFormat = new LocalizableResourceString(
            nameof(Resources.ClockTimerAnalyzer_InvocationMessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString InvocationMessageFormatDirected = new LocalizableResourceString(
            nameof(Resources.ClockTimerAnalyzer_InvocationMessageFormat_DirectToClockTimer),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.ClockTimerAnalyzerDescription),
            Resources.ResourceManager,
            typeof(Resources));

        private const string Category = "Testability";

        private static DiagnosticDescriptor invocationRule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            InvocationMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        private static DiagnosticDescriptor invocationRuleDirected = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            InvocationMessageFormatDirected,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(invocationRule, invocationRuleDirected); }
        }

        private static readonly ImmutableArray<string> StopwatchMethods = new[]
            {
                "StartNew"
            }.ToImmutableArray();

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
                        IInvocationOperation invocation = (IInvocationOperation)operationContext.Operation;

                        if (invocation.Type == null || invocation.Type != stopwatchType)
                        {
                            return;
                        }

                        if (!StopwatchMethods.Contains(invocation.TargetMethod?.Name))
                        {
                            return;
                        }

                        operationContext.ReportDiagnostic(Diagnostic.Create(
                                  clockTimerType == null ? invocationRule : invocationRuleDirected,
                                  invocation.Syntax.GetLocation(),
                                  stopwatchType.Name,
                                  invocation.TargetMethod.Name,
                                      clockTimerType?.Name));
                    }, OperationKind.Invocation);
            });
        }
    }
}
