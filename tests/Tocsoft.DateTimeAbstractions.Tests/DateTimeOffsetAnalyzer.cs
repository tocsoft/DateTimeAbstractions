// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Analyzer.Test
{
    public class DateTimeOffsetAnalyzer : CodeFixVerifier
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
    namespace Tocsoft.DateTimeAbstractions
    {
        class TypeName
        {   
            public TypeName(){
                DateTimeOffset time = DateTimeOffset.Now;
            }
        }

        public static class ClockOffset { public static DateTimeOffset Now { get; set; } }
    }";
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = DateTimeOffsetUsageAnalyzer.DiagnosticId,
                Message = "Do not call DateTimeOffset.Now as they are not testable, use an abstraction instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                  new[]
                    {
                            new DiagnosticResultLocation("Test0.cs", 8, 39)
                      }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void TestMethodAnalyzerExpectedForSystemDatetime()
        {
            string test = @"
    namespace Tocsoft.DateTimeAbstractions
    {
        class TypeName
        {   
            public TypeName(){
                System.DateTimeOffset time = System.DateTimeOffset.Now;
            }
        }

        public static class ClockOffset { public static DateTimeOffset Now { get; set; } }
    }";
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = DateTimeOffsetUsageAnalyzer.DiagnosticId,
                Message = "Do not call DateTimeOffset.Now as they are not testable, use an abstraction instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                  new[]
                    {
                            new DiagnosticResultLocation("Test0.cs", 7, 46, 25)
                      }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void CodeFixReducesProperly()
        {
            string test = @"
using System;
namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            DateTimeOffset time = DateTimeOffset.Now;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class ClockOffset { public static DateTimeOffset Now { get; set; } }
}"
            };

            string fixtest = @"
using System;
using Tocsoft.DateTimeAbstractions;

namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            DateTimeOffset time = ClockOffset.Now;
        }
    }
}";
            this.VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void DateTimeUtcNowMappsToClockUtcNow()
        {
            string test = @"
using Tocsoft.DateTimeAbstractions;
using System;

namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            DateTimeOffset time = DateTimeOffset.UtcNow;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class ClockOffset { public static DateTimeOffset UtcNow { get; set; } }
}"
            };

            string fixtest = @"
using Tocsoft.DateTimeAbstractions;
using System;

namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            DateTimeOffset time = ClockOffset.UtcNow;
        }
    }
}";
            this.VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void DateTimeUtcNowMappsToClockUtcNowDotHour()
        {
            string test = @"
using System;
namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            DateTimeOffset time = DateTimeOffset.UtcNow.Hour;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class ClockOffset { public static DateTimeOffset UtcNow { get; set; } }
}"
            };

            string fixtest = @"
using System;
using Tocsoft.DateTimeAbstractions;

namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            DateTimeOffset time = ClockOffset.UtcNow.Hour;
        }
    }
}";
            this.VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void SystemDateTimeUtcNowMappsToClockUtcNow()
        {
            string test = @"
namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            System.DateTimeOffset time = System.DateTimeOffset.UtcNow;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class ClockOffset { public static DateTimeOffset UtcNow { get; set; } }
}"
            };

            string fixtest = @"using Tocsoft.DateTimeAbstractions;

namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            System.DateTimeOffset time = ClockOffset.UtcNow;
        }
    }
}";
            this.VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DateTimeOffsetUsageCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DateTimeOffsetUsageAnalyzer();
        }
    }
}
