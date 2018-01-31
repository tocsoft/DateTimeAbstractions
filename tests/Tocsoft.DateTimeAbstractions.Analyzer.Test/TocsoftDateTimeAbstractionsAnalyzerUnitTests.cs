using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Tocsoft.DateTimeAbstractions.Analyzer;

namespace Tocsoft.DateTimeAbstractions.Analyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }



        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethodAnalyzerExpected()
        {
            var test = @"
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
            var expected = new DiagnosticResult
            {
                Id = DateTimeUsageAnalyzer.DiagnosticId,
                Message = "Do not call DateTime.Now",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                  new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 33)
                      }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethodAnalyzerExpectedForSystemDatetime()
        {
            var test = @"
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
            var expected = new DiagnosticResult
            {
                Id = DateTimeUsageAnalyzer.DiagnosticId,
                Message = "Do not call DateTime.Now",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                  new[] {
                            new DiagnosticResultLocation("Test0.cs", 7, 40, 19)
                      }
            };

            VerifyCSharpDiagnostic(test, expected);
        }



        //No diagnostics expected to show up
        [TestMethod]
        public void CodeFixReducesProperly()
        {
            var test = @"
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
            AdditionalCodeFiles = new[] {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime Now { get; set; } }
}"
            };

            var fixtest = @"
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
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void DateTimeUtcNowMappsToClockUtcNow()
        {
            var test = @"
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
            AdditionalCodeFiles = new[] {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime UtcNow { get; set; } }
}"
            };

            var fixtest = @"
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
            VerifyCSharpFix(test, fixtest);
        }
        [TestMethod]
        public void DateTimeUtcNowMappsToClockUtcNowDotHour()
        {
            var test = @"
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
            AdditionalCodeFiles = new[] {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime UtcNow { get; set; } }
}"
            };

            var fixtest = @"
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
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void SystemDateTimeUtcNowMappsToClockUtcNow()
        {
            var test = @"
namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            System.DateTime time = System.DateTime.UtcNow;
        }
    }
}";
            AdditionalCodeFiles = new[] {
                @"
using System;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock { public static DateTime UtcNow { get; set; } }
}"
            };

            var fixtest = @"using Tocsoft.DateTimeAbstractions;

namespace TestApplication
{
    class TypeName
    {   
        public TypeName(){
            System.DateTime time = Clock.UtcNow;
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
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
