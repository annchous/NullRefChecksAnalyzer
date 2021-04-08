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
            var test = @"using System;
					
    public class Program
    {
        public Program(string a)
        {
            if (a == null || a == ""a"") { }
        }

        static void Main(string[] args, int b)
        {
            if (args == null) { }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
