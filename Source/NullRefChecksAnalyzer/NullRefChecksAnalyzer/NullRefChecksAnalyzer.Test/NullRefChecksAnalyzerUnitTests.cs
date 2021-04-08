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
        static void Main(string[] args, int b)
        {
            var s = args.Equals(null);
            if (ReferenceEquals(args, null) { }
            if (ReferenceEquals(null, args) { }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
