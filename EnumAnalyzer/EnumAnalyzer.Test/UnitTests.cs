using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace net.ajennings.EnumAnalyzer.Test
{
    [TestClass]
    public class UnitTest : DiagnosticVerifier
    {
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
namespace net.ajennings
{
    class EnumNotExhaustedException<T> : System.Exception
    {
    }
}

namespace EnumTests
{
    class Program
    {
        enum MyColor { Red, Green, Blue };

        static int TestOne(MyColor c)
        {
            if (c == MyColor.Red)
            {
                return 0;
            }
            else if (c == MyColor.Green)
            {
                return 1;
            }
            else if (c == MyColor.Blue)
            {
                return 2;
            }
            // This should NOT generate a compiler warning/error because we handled all enum values.
            throw new net.ajennings.EnumNotExhaustedException<MyColor>();
        }
    }
}
";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var test = @"
namespace net.ajennings
{
    class EnumNotExhaustedException<T> : System.Exception
    {
    }
}

namespace EnumTests
{
    class Program
    {
        enum MyColor { Red, Green, Blue };

        static int TestTwo(MyColor c)
        {
            if (c == MyColor.Red)
            {
                return 0;
            }
            else if (c == MyColor.Green)
            {
                return 1;
            }
            // This SHOULD generate a compiler warning/error because we didn't handle the MyColor.blue case.
            throw new net.ajennings.EnumNotExhaustedException<MyColor>();
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "ENUM001",
                Message = "enum value(s) not referenced in enclosing block: Blue",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 26, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var test = @"
namespace net.ajennings
{
    class EnumNotExhaustedException<T> : System.Exception
    {
    }
}

namespace EnumTests
{
    class Program
    {
        static int TestTwo(MyColor c)
        {
            throw new net.ajennings.EnumNotExhaustedException<int>();
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "ENUM002",
                Message = "EnumNotExhaustedException must be used with enum",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnumAnalyzerAnalyzer();
        }
    }
}