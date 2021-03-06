﻿// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Analyzer.Test
{
    public class ClockTimerAnalyzerTest : CodeFixVerifier
    {
        [Fact]
        public void TestMethod1()
        {
            string test = string.Empty;

            this.VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void TestMethodAnalyzerExpected()
        {
            string test = @"
    using System;
    using System.Diagnostics;
    namespace Tocsoft.DateTimeAbstractions
    {
        class TypeName
        {   
            public TypeName(){
                Stopwatch time = Stopwatch.StartNew();
            }
        }
    }";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = ClockTimerUsageInvocationAnalyzer.DiagnosticId,
                Message = "Do not call Stopwatch.StartNew as they are not testable, use an abstraction instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                   new[]
                     {
                            new DiagnosticResultLocation("Test0.cs", 9, 34)
                       }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void TestMethodAnalyzerExpected_WithClockTimerMessage()
        {
            string test = @"
    using System;
    using System.Diagnostics;
    namespace Tocsoft.DateTimeAbstractions
    {
        class TypeName
        {   
            public TypeName(){
                Stopwatch time = Stopwatch.StartNew();
            }
        }
        public class ClockTimer
        {  }
    }";

            DiagnosticResult expected = new DiagnosticResult
            {
                Id = ClockTimerUsageInvocationAnalyzer.DiagnosticId,
                Message = "Do not call Stopwatch.StartNew as they are not testable, use an abstraction like the available ClockTimer class instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                   new[]
                     {
                            new DiagnosticResultLocation("Test0.cs", 9, 34)
                       }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void TestMethodAnalyzerExpectedForVar()
        {
            string test = @"
    namespace Tocsoft.DateTimeAbstractions
    {
        class TypeName
        {   
            public TypeName(){
                var time = System.Diagnostics.Stopwatch.StartNew();
            }
        }
    }";
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = ClockTimerUsageInvocationAnalyzer.DiagnosticId,
                Message = "Do not call Stopwatch.StartNew as they are not testable, use an abstraction instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                   new[]
                     {
                            new DiagnosticResultLocation("Test0.cs", 7, 28)
                       }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void TestMethodAnalyzerExpectedForVar_WithClockTimerMessage()
        {
            string test = @"
    namespace Tocsoft.DateTimeAbstractions
    {
        class TypeName
        {   
            public TypeName(){
                var time = System.Diagnostics.Stopwatch.StartNew();
            }
        }
        public class ClockTimer
        {  }
    }";
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = ClockTimerUsageInvocationAnalyzer.DiagnosticId,
                Message = "Do not call Stopwatch.StartNew as they are not testable, use an abstraction like the available ClockTimer class instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                   new[]
                     {
                            new DiagnosticResultLocation("Test0.cs", 7, 28)
                       }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ClockTimerUsageInvocationAnalyzer();
        }
    }
}
