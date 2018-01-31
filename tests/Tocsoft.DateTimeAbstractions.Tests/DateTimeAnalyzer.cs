// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Analyzer.Test
{
    public class DateTimeAnalyzer : CodeFixVerifier
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
                DateTime time = DateTime.Now;
            }
        }

        public static class Clock { public static DateTime Now { get; set; } }
    }";
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = DateTimeUsageAnalyzer.DiagnosticId,
                Message = "Do not call DateTime.Now as they are not testable, use an abstraction instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                  new[]
                    {
                            new DiagnosticResultLocation("Test0.cs", 8, 33)
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
                System.DateTime time = System.DateTime.Now;
            }
        }

        public static class Clock { public static DateTime Now { get; set; } }
    }";
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = DateTimeUsageAnalyzer.DiagnosticId,
                Message = "Do not call DateTime.Now as they are not testable, use an abstraction instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                  new[]
                    {
                            new DiagnosticResultLocation("Test0.cs", 7, 40, 19)
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
            DateTime time = DateTime.Now;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime Now { get; set; } }
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
            DateTime time = Clock.Now;
        }
    }
}";
            this.VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void IncompleteCodeCanBeFixed()
        {
            string test = @"
using System;
namespace TestApplication
{
    class TypeName
    {   
        DateTime prop;
        public TypeName()
        {
            var r = DateTime.Now
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime Now { get; set; } }
}"
            };

            string fixtest = @"
using System;
using Tocsoft.DateTimeAbstractions;

namespace TestApplication
{
    class TypeName
    {   
        DateTime prop;
        public TypeName()
        {
            var r = Clock.Now
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
            DateTime time = DateTime.UtcNow;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime UtcNow { get; set; } }
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
            DateTime time = Clock.UtcNow;
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
            DateTime time = DateTime.UtcNow.Hour;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime UtcNow { get; set; } }
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
            DateTime time = Clock.UtcNow.Hour;
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
            System.DateTime time = System.DateTime.UtcNow;
        }
    }
}";
            this.AdditionalCodeFiles = new[]
            {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime UtcNow { get; set; } }
}"
            };

            string fixtest = @"using Tocsoft.DateTimeAbstractions;

namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            System.DateTime time = Clock.UtcNow;
        }
    }
}";
            this.VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DateTimeUsageCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DateTimeUsageAnalyzer();
        }
    }
}
