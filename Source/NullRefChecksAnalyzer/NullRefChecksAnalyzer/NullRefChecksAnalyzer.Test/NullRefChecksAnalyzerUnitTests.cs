using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NullRefChecksAnalyzer.Test.CSharpCodeFixVerifier<
    NullRefChecksAnalyzer.NullRefChecksAnalyzer,
    NullRefChecksAnalyzer.NullRefChecksAnalyzerCodeFixProvider>;

namespace NullRefChecksAnalyzer.Test
{
    [TestClass]
    public class NullRefChecksAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"
    using System;
					
    public class Program
    {
	    public static void Main(string[] args, int a)
	    {
		    if (args == null) {}
		    string s = """";
		    if (s == null) {}
	    }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
					
    public class Program
    {
	    public static void Main(string[] args)
	    {
		    if (args == null) {}
		    string s = """";
		    if (s == null) {}
	    }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("NullRefChecksAnalyzer").WithLocation(0).WithArguments("TypeName");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
