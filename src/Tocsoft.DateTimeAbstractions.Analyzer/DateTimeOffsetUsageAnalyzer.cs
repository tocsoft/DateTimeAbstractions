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
    public class DateTimeOffsetUsageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TOCSOFT0002";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(
                nameof(Resources.AnalyzerTitle),
                Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(Resources.AnalyzerMessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.AnalyzerDescription),
            Resources.ResourceManager,
            typeof(Resources));

        private const string Category = "Testability";

        private static DiagnosticDescriptor rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(rule); }
        }

        private static readonly ImmutableArray<string> PropertyNames = new[]
            {
                "Now",
                "UtcNow"
            }.ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                Compilation compilation = compilationStartContext.Compilation;
                INamedTypeSymbol dateTimeType = compilation.GetTypeByMetadataName("System.DateTimeOffset");

                compilationStartContext.RegisterOperationAction(
                    operationContext =>
                    {
                        IPropertyReferenceOperation invocation = (IPropertyReferenceOperation)operationContext.Operation;

                        if (invocation.Member == null || invocation.Member.ContainingType != dateTimeType)
                        {
                            return;
                        }

                        IPropertySymbol targetProperty = invocation.Property;

                        if (targetProperty == null || !PropertyNames.Contains(targetProperty.MetadataName))
                        {
                            return;
                        }

                        operationContext.ReportDiagnostic(Diagnostic.Create(
                                  rule,
                                  invocation.Syntax.GetLocation(),
                                  invocation.Member.ContainingType.Name,
                                  targetProperty.Name));
                    }, OperationKind.PropertyReference);
            });
        }
    }
}
